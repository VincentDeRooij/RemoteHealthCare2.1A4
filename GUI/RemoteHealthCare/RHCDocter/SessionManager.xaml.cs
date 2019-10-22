using LiveCharts;
using LiveCharts.Wpf;
using RHCCore.Networking;
using RHCCore.Networking.Models;
using RHCDocter.Models;
using System;
using System.Collections.Concurrent;
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
using System.Windows.Threading;

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

        private ConcurrentQueue<Tuple<IConnection, dynamic>> networkMessages;
        
        private DateTime startedAt;
        private int oldValue = 0;

        public SessionManager(PersonProxy client, Session session)
        {
            InitializeComponent();

            this.networkMessages = new ConcurrentQueue<Tuple<IConnection, dynamic>>();

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

                new LineSeries
                {
                    Title = "HR",
                    Values = new ChartValues<double> { },
                    PointGeometry = DefaultGeometries.Diamond,
                    PointGeometrySize = 8,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 10, 10))
                },
            };

            Labels = new List<string>();
            YFormatter = value => value.ToString();
            DataContext = this;
            DispatcherTimer updateTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(0.5),
            };

            updateTimer.Tick += async (x, y) =>
            {
                Tuple<IConnection, dynamic> networkMessage;
                while (networkMessages.TryDequeue(out networkMessage))
                {
                    string command = (string)networkMessage.Item2.Command;
                    if (command == $"session/{sessionId}/ready")
                    {
                        bttnStart.IsEnabled = true;
                    }

                    if (command == $"session/{sessionId}/updated")
                    {
                        int currentValue = (int)networkMessage.Item2.Data.SecondsPassed;
                        if (currentValue != oldValue)
                        {
                            if (currentValue % 60 == 0)
                                minutesPassed++;

                            pbDuration.Value = currentValue;
                            oldValue = currentValue;

                            Labels.Add(string.Format("{0:D2}:{1:D2}", minutesPassed, currentValue % 60));
                            int RPM = (int)networkMessage.Item2.Data.BikeData.RPM;
                            meterRpm.Value = RPM;
                            this.SeriesCollection[0].Values.Add((double)RPM);
                            this.SeriesCollection[1].Values.Add((double)networkMessage.Item2.Data.HR);
                        }
                    }

                    if (command == $"session/{sessionId}/done")
                    {
                        bttnStart.IsEnabled = false;
                        bttnStop.IsEnabled = false;
                    }
                }
            };

            updateTimer.Start();
        }

        private int minutesPassed = 0;
        private void OnReceived(RHCCore.Networking.IConnection connection, dynamic args)
        {
            networkMessages.Enqueue(new Tuple<IConnection, dynamic>(connection, args));
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
