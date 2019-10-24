using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RHCCore.Networking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RHCDocter
{
    /// <summary>
    /// Interaction logic for ArchivedWindow.xaml
    /// </summary>
    public partial class ArchivedWindow : Window
    {
        private string sessionId;
        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private bool IsAstrand;
        private StringBuilder jsonBuilder = new StringBuilder();

        public ArchivedWindow(string sessionId, bool isAstrand)
        {
            this.IsAstrand = isAstrand;
            this.sessionId = sessionId;
            InitializeComponent();

            App.TcpClientWrapper.NetworkConnection.OnReceived += OnReceived;
            App.TcpClientWrapper.NetworkConnection.Write(new
            {
                Command = "history/request",
                Data = new
                {
                    SessionId = sessionId,
                    IsAstrand = isAstrand
                }
            });

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "RPM",
                    Values = new ChartValues<double> { },
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 8,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 10, 10, 255))
                },

                new LineSeries
                {
                    Title = "Heartrate",
                    Values = new ChartValues<double> { },
                    PointGeometry = DefaultGeometries.Diamond,
                    PointGeometrySize = 8,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 10, 10))
                },

                new LineSeries
                {
                    Title = "Resistance",
                    Values = new ChartValues<double> { },
                    PointGeometry = DefaultGeometries.Triangle,
                    PointGeometrySize = 8,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 10, 255, 10))
                },
            };

            Labels = new List<string>();
            YFormatter = value => value.ToString();

            DataContext = this;
        }

        private void OnReceived(RHCCore.Networking.IConnection sender, dynamic args)
        {
            string command = (string)args.Command;
            if (command == $"history/{sessionId}/start")
            {
                jsonBuilder = new StringBuilder();
            }

            if (command == $"history/{sessionId}/upload")
            {
                byte[] buffer = (args.Data.Session as JArray).ToObject<List<byte>>().ToArray();
                string packet = Encoding.ASCII.GetString(buffer);
                jsonBuilder.Append(packet);
            }

            if (command == $"history/{sessionId}/done")
            {
                Console.WriteLine(jsonBuilder.ToString());
                dynamic dataObject;
                dataObject = JsonConvert.DeserializeObject<dynamic>(jsonBuilder.ToString());

                Session rootSession = JsonConvert.DeserializeObject<Session>(jsonBuilder.ToString());

                if (IsAstrand)
                {
                    AstrandSession astrandSession = JsonConvert.DeserializeObject<AstrandSession>(jsonBuilder.ToString());
                    Dispatcher.Invoke(() =>
                    {
                        lblVO2.Visibility = Visibility.Visible;
                        if (astrandSession.ReachedSteady && astrandSession.BikeData.Count > 2800)
                            lblVO2.Content = $"Client did reach steady-state, calculated VO2max: { CalculateVO2Max((int)130, (int)astrandSession.BikeData[2800].Workload, (bool)astrandSession.IsMale, (double)astrandSession.GetFactor())} L/min";
                        else
                            lblVO2.Content = "Client did not reach steady-state";
                    });
                }

                List<dynamic> collection = (dataObject.BikeData as JArray).ToObject<List<dynamic>>();
                List<object> heartData = new List<object>();
                List<object> rpmData = new List<object>();
                List<object> resistanceData = new List<object>();

                for (int i = 0; i < collection.Count; i += 16)
                {
                    int currentSecond = (i / 8);
                    heartData.Add((double)collection[i].HR);
                    rpmData.Add((double)collection[i].RPM);
                    resistanceData.Add((double)collection[i].Resistance);
                    Labels.Add(string.Format("{0:D2}", currentSecond));
                }

                this.SeriesCollection[1].Values.AddRange(heartData);
                this.SeriesCollection[0].Values.AddRange(rpmData);
                this.SeriesCollection[2].Values.AddRange(resistanceData);
            }
        }

        private double CalculateVO2Max(int heartRate, int workload, bool isMale, double factor)
        {
            if (isMale)
                return ((0.00212 * workload + 0.299) / (0.769 * heartRate - 48.5) * 100) * factor;
            else
                return ((0.00193 * workload + 0.326) / (0.769 * heartRate - 56.1) * 100) * factor;
        }
    }
}
