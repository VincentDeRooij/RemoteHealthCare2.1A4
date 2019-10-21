using LiveCharts;
using LiveCharts.Wpf;
using RHCCore.Networking.Models;
using RHCDocter.Models;
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
    /// Interaction logic for SessionManager.xaml
    /// </summary>
    public partial class SessionManager : Window
    {
        private string sessionId;
        private PersonProxy client;
        private Session session;

        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private DateTime startedAt;
        private int oldValue = 0;

        public SessionManager(PersonProxy client, Session session)
        {
            InitializeComponent();

            App.TcpClientWrapper.OnReceived += OnReceived;

            App.TcpClientWrapper.NetworkConnection.Write(new
            {
                Command = "session/create",
                Data = new
                {
                    Session = session,
                    Key = client.Key
                }
            });

            this.sessionId = session.SessionId;
            this.client = client;
            this.session = session;

            pbDuration.Maximum = session.SessionDuration;
            pbDuration.Minimum = 0;
            pbDuration.Value = 0;

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "RPM",
                    Values = new ChartValues<double> { },
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 8
                },
            };

            Labels = new List<string>();
            YFormatter = value => value.ToString();
            DataContext = this;
        }

        private void OnReceived(RHCCore.Networking.IConnection connection, dynamic args)
        {
            string command = (string)args.Command;
            if (command == $"session/{sessionId}/ready")
            {
                Dispatcher.Invoke(() => bttnStart.IsEnabled = true);
            }

            if (command == $"session/{sessionId}/updated")
            {
                Console.WriteLine(args);
                Dispatcher.Invoke(() =>
                {
                    int currentValue = (int)args.Data.SecondsPassed;
                    if (currentValue != oldValue)
                    {
                        pbDuration.Value = currentValue;
                        oldValue = currentValue;
                        Labels.Add(startedAt.AddSeconds(currentValue).ToString("mm:ss"));

                        int RPM = (int)args.Data.BikeData.RPM;
                        meterRpm.Value = RPM;
                        this.SeriesCollection[0].Values.Add((double)RPM);
                    }
                });
            }

            if (command == $"session/{sessionId}/done")
            {
                Dispatcher.Invoke(() =>
                {
                    bttnStart.IsEnabled = false;
                    bttnStop.IsEnabled = false;
                });
            }
        }

        private void OnStartClicked(object sender, RoutedEventArgs e)
        {
            bttnStart.IsEnabled = false;
            bttnStop.IsEnabled = true;
            startedAt = DateTime.Now;
            App.TcpClientWrapper.NetworkConnection.Write(new
            {
                Command = $"session/start",
                Data = new
                {
                    Key = client.Key
                }
            });
        }

        private void OnStopClicked(object sender, RoutedEventArgs e)
        {
            bttnStop.IsEnabled = false;
            App.TcpClientWrapper.NetworkConnection.Write(new
            {
                Command = $"session/stop",
                Data = new
                {
                    Key = client.Key
                }
            });
        }
    }
}
