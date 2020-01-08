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
using MahApps.Metro.Controls.Dialogs;

namespace RemoteHealthCare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow
    {
        enum ASTRAND_STAGE
        {
            WARMUP,
            TESTING,
            COOLDOWN
        }

        List<IDevice> devices = new List<IDevice>();
        string authkey;
        public StationaryBike bike;
        public HeartRateMonitor hrMonitor;
        int secondsRemaining = 0;

        ASTRAND_STAGE astrandStage;
        bool isAstrand = false;
        Session activeSession;
        bool sessionActive => activeSession != null;
        bool sessionRunning = false;
        int maximumHeartrate = 0;

        public MainWindow(string authkey, string username)
        {
            InitializeComponent();
            DataContext = this;
            this.authkey = authkey;

            astrandStage = ASTRAND_STAGE.WARMUP;

            App.serverClientWrapper.OnReceived += OnReceived;
        }

        private void OnReceived(RHCCore.Networking.IConnection connection, dynamic args)
        {
            string command = args.Command;
            switch (command)
            {
                case "session/create":
                    {
                        isAstrand = (bool)args.Data.IsAstrand;

                        if (isAstrand)
                            maximumHeartrate = (args.Data.Session as JObject).ToObject<AstrandSession>().GetHeartrate();

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

                                resistanceValue = 0;
                                bike?.ChangeBikeResistance((byte)(int)resistanceValue);

                                secondsRemaining = activeSession.SessionDuration;
                                while (secondsRemaining > 0)
                                {
                                    currentTime = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                                    if (currentTime - startTime >= 1000)
                                    {
                                        secondsRemaining--;
                                        startTime = currentTime;

                                        if (isAstrand)
                                        {
                                            AstrandUpdated();
                                        }
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
                                isAstrand = false;
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
                        Dispatcher.Invoke(() => { txtChat.Text += "[DOCTOR]: " + message + "\n"; scrollChat.ScrollToBottom(); });
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

                        resistanceValue = (double)args.data;
                        bike?.ChangeBikeResistance((byte)args.data);

                        Panel.resistance = value;
                        Panel.drawValues();
                    }
                break;
            }
        }

        private int SecondsPassed 
        { 
            get
            {
                return activeSession.SessionDuration - secondsRemaining;
            }
        }

        private double resistanceValue = 0;
        private double currentResistance;

        private void AstrandUpdated()
        {
            if (hrMonitor.HeartRate >= maximumHeartrate)
            {
                secondsRemaining = 0;
                resistanceValue = 0;
                bike?.ChangeBikeResistance((byte)(int)resistanceValue);
            }

            switch (astrandStage)
            {
                case ASTRAND_STAGE.WARMUP:
                    {
                        if (SecondsPassed % 10 == 0)
                        {
                            if (bike.CurrentRPM < 56)
                            {
                                Dispatcher.Invoke(() => { txtChat.Text += "[ASTRAND]: " + "Speed up" + "\n"; scrollChat.ScrollToBottom(); });
                            }
                            else if (bike.CurrentRPM > 64)
                            {
                                Dispatcher.Invoke(() => { txtChat.Text += "[ASTRAND]: " + "Slow down" + "\n"; scrollChat.ScrollToBottom(); });
                            }
                            else
                            {
                                Dispatcher.Invoke(() => { txtChat.Text += "[ASTRAND]: " + "Great job! Keep it up" + "\n"; scrollChat.ScrollToBottom(); });
                            }

                            resistanceValue = (SecondsPassed / 2);
                            bike.ChangeBikeResistance((byte)(int)resistanceValue);
                        }

                        if (SecondsPassed == 119)
                        {
                            astrandStage = ASTRAND_STAGE.TESTING;
                            //this.ShowProgressAsync("Start Testing", "It's game time");
                        }
                    }
                    break;

                case ASTRAND_STAGE.TESTING:
                    {
                        if (SecondsPassed % 10 == 0)
                        {
                            if (bike.CurrentRPM < 56)
                            {
                                Dispatcher.Invoke(() => { txtChat.Text += "[ASTRAND]: " + "Speed up" + "\n"; scrollChat.ScrollToBottom(); });
                            }
                            else if (bike.CurrentRPM > 64)
                            {
                                Dispatcher.Invoke(() => { txtChat.Text += "[ASTRAND]: " + "Slow down" + "\n"; scrollChat.ScrollToBottom(); });
                            }
                            else
                            {
                                Dispatcher.Invoke(() => { txtChat.Text += "[ASTRAND]: " + "Great job! Keep it up" + "\n"; scrollChat.ScrollToBottom(); });
                            }
                        }

                        if (SecondsPassed % 60 == 0 && SecondsPassed < 240)
                        {
                            resistanceValue *= CalculateNextResistance();
                            bike.ChangeBikeResistance((byte)(int)resistanceValue);
                        }

                        if (SecondsPassed % 15 == 0 && SecondsPassed >= 240)
                        {
                            resistanceValue *= CalculateNextResistance();
                            bike.ChangeBikeResistance((byte)(int)resistanceValue);
                        }

                        if (SecondsPassed == 359)
                        {
                            astrandStage = ASTRAND_STAGE.COOLDOWN;
                            currentResistance = resistanceValue;
                            //ProgressDialogController pdc = await this.ShowProgressAsync("Start Cooldown", "Poah nice game gg take some rest");
                        }
                    }
                    break;

                case ASTRAND_STAGE.COOLDOWN:
                    {
                        if (SecondsPassed % 10 == 0)
                        {
                            resistanceValue -= currentResistance / 6;
                            bike.ChangeBikeResistance((byte)(int)resistanceValue);
                        }

                        if (SecondsPassed == 420)
                        {
                            resistanceValue = 0;
                            bike.ChangeBikeResistance((byte)(int)resistanceValue);
                            astrandStage = ASTRAND_STAGE.WARMUP;
                        }
                    }
                    break;
            }
        }

        private double CalculateNextResistance()
        {
            if (hrMonitor.HeartRate == 0)
                return ((130d / 80d) - 1d) * 0.75 + 1d;
            else
                return ((130d / (double)hrMonitor.HeartRate) - 1d) * 0.75 + 1d;
        }

        private void OnConnectToDevice(object sender, RoutedEventArgs e)
        {
            Dialogs.ConnectDevice wndConnect = new Dialogs.ConnectDevice();
            wndConnect.DeviceClicked += (x, y) =>
            {
                if (bike == null || hrMonitor == null)
                {
                    if (((string)x).Contains("Tacx"))
                    {
#if !SIM
                        bike = new Devices.StationaryBike((string)x, "");
                        bike.DeviceDataChanged += SelectedDevice_DeviceDataChanged;
#else
                        IDevice selectedDevice = Simulator.Simulator.Instance.OpenDevice((string)x);
                        bike = selectedDevice as StationaryBike;
                        bike.DeviceDataChanged += SelectedDevice_DeviceDataChanged;
                        hrMonitor = new HeartRateMonitor();
#endif
                        //lvDevices.Children.Add(new UserControls.StationaryBikeControl(ref bike));
                    }


                    if (((string)x).Contains("Decathlon"))
                    {
                        hrMonitor = new Devices.HeartRateMonitor();
                    }
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
                    Dispatcher.Invoke(() =>
                    {
                        string distanceStr = string.Format("{0:00.00}", bike.Distance);
                        lblDistance.Content = $"Distance: {distanceStr}m";
                        lblHr.Content = $"Heartrate: {hrMonitor.HeartRate}";
                        lblResistance.Content = $"Resistance: {resistanceValue.ToString("0.00")}";
                        lblTimeRemaining.Content = $"Time untill next phase: {(astrandStage == ASTRAND_STAGE.WARMUP ? 120 - SecondsPassed : astrandStage == ASTRAND_STAGE.TESTING ? 360 - SecondsPassed : 420 - SecondsPassed).ToString()}";
                        lblCurrentPhase.Content = $"Current phase: {astrandStage.ToString()}";
                        meterRpm.Value = bike.CurrentRPM;
                    });

#if SIM
                    if (hrMonitor != null)
                        hrMonitor.HeartRate = rnd.Next(126, 134);
#endif

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
                                Resistance = bike.resistance,
                                Workload = bike.Workload,
#if !SIM
                                HR = hrMonitor.HeartRate
#else
                                HR = rnd.Next(126, 134)
#endif
                            },
                        }
                    });
                    lastUpdate = currentDelta;
                }
            }
        }

        Random rnd = new Random(DateTime.Now.Millisecond);
    }
}
