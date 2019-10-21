#define MULTI_DEVICE

using Avans.TI.BLE;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json.Linq;
using RemoteHealthCare.Devices;
using RHCCore.Networking.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemoteHealthCare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow
    {
        List<IDevice> devices = new List<IDevice>();
        string authkey;
        public StationaryBike bike;
        public HeartRateMonitor hrMonitor;
        int secondsRemaining = 0;

        Session activeSession;
        bool sessionActive => activeSession != null;
        bool sessionRunning = false;

        public MainWindow(string authkey, string username)
        {
            InitializeComponent();
            DataContext = this;
            this.authkey = authkey;

            App.serverClientWrapper.OnReceived += OnReceived;
        }

        private void OnReceived(RHCCore.Networking.IConnection connection, dynamic args)
        {
            string command = args.Command;
            switch (command)
            {
                case "session/create":
                    {
                        Session newSession = (args.Data.Session as JObject).ToObject<Session>();
                        if (activeSession == null)
                        {
                            activeSession = newSession;
                            connection.Write(new
                            {
                                Command = $"session/ready",
                                Data = new
                                {
                                    SessionId = newSession.SessionId
                                }
                            });
                        }
                    }
                break;

                case "session/start":
                    {
                        if (activeSession != null)
                        {
                            new Thread(() =>
                            {
                                sessionRunning = true;
                                
                                double startTime = activeSession.StartDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                                double currentTime = startTime;

                                lastUpdate = 0;

                                secondsRemaining = activeSession.SessionDuration;
                                while (secondsRemaining > 0)
                                {
                                    currentTime = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                                    if (currentTime - startTime >= 1000)
                                    {
                                        secondsRemaining--;
                                        startTime = currentTime;
                                    }
                                }

                                connection.Write(new
                                {
                                    Command = "session/done",
                                    Data = new
                                    {
                                        SessionId = activeSession.SessionId,
                                    }
                                });

                                activeSession = null;
                            }).Start();
                        }
                    }
                break;

                case "session/pause":
                    {
                        sessionRunning = false;
                    }
                break;

                case "chat/send":
                    {
                        string message = (string)args.data;
                        if (Panel.chatList != null)
                        {
                            Panel.addText(message);
                            Panel.drawValues();
                        }
                    }
                break;

                case "resistance/send":
                    {
                        int value = (int)args.data;
                        Panel.resistance = value;
                        Panel.drawValues();
                    }
                break;
            }
        }

        private void OnConnectToDevice(object sender, RoutedEventArgs e)
        {
            Dialogs.ConnectDevice wndConnect = new Dialogs.ConnectDevice();
            wndConnect.DeviceClicked += (x, y) =>
            {
#if SIM
                IDevice selectedDevice = Simulator.Simulator.Instance.OpenDevice((string)x);
                bike = selectedDevice as StationaryBike;
                hrMonitor = new HeartRateMonitor();
#else
                IDevice selectedDevice = ((string)x).Contains("Tacx") || ((string)x).Contains("Decathlon") ? new Devices.StationaryBike((string)x, "") : null;
#endif
                Console.WriteLine(selectedDevice);
                selectedDevice.DeviceDataChanged += SelectedDevice_DeviceDataChanged;
                if (!devices.Contains(selectedDevice))
                {
                    devices.Add(selectedDevice);
                    StationaryBike bike = selectedDevice as StationaryBike;
                    lvDevices.Children.Add(new UserControls.StationaryBikeControl(ref bike));
                }
            };
            wndConnect.ShowDialog();
        }

        double lastUpdate = 0;
        double currentDelta = 0;
        private void SelectedDevice_DeviceDataChanged(object sender, EventArgs e)
        {
            if (sessionActive && sessionRunning)
            {
                if (lastUpdate == 0)
                {
                    lastUpdate = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                }

                currentDelta = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                if (currentDelta - lastUpdate >= 125)
                {
                    hrMonitor.HeartRate = 100;
                    App.serverClientWrapper.NetworkConnection.Write(new
                    {
                        Command = "session/update",
                        Data = new
                        {
                            SecondsPassed = activeSession.SessionDuration - secondsRemaining + 1,
                            SessionId = activeSession.SessionId,
                            BikeData = new
                            {
                                Distance = bike.Distance,
                                RPM = bike.CurrentRPM,
                                Speed = bike.CurrentSpeed,
                                AvgSpeed = bike.AverageSpeed,
                            }
                        }
                    });
                    lastUpdate = currentDelta;
                }
            }
        }
    }
}
