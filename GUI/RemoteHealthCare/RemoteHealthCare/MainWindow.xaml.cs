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
        public float lastDistance;
        public static StationaryBike bike;
        public static int counter = 0;
        public static float totalspeed = 0;
        public static Stopwatch stopwatch = new Stopwatch();
        Session activeSession;
        bool sessionActive => activeSession != null;

        public MainWindow(string authkey, string username)
        {
            InitializeComponent();
            DataContext = this;
            this.authkey = authkey;

            App.serverClientWrapper.OnReceived += OnReceived;

            //bike = new StationaryBike(username, username);
            //BikeConnection();
        }

        private void OnReceived(RHCCore.Networking.IConnection connection, dynamic args)
        {
            string command = args.Command;
            switch (command)
            {
                case "session/start":
                    {
                        Session newSession = (args.Data.Session as JObject).ToObject<Session>();
                        if (activeSession == null)
                        {
                            activeSession = newSession;
                            new Thread(() =>
                            {
                                double startTime = newSession.StartDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                                double currentTime = startTime;
                                while (currentTime - startTime < newSession.SessionDuration)
                                {
                                    Thread.Sleep(10);
                                    currentTime = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                                }

                                connection.Write(new
                                {
                                    Command = "session/done",
                                });

                                activeSession = null;
                            }).Start();
                        }
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

        private void SelectedDevice_DeviceDataChanged(object sender, EventArgs e)
        {
            //bike = (StationaryBike)sender;
            //if (bike.Distance != lastDistance)
            //{
            //    float distanceDifference = bike.Distance - lastDistance;
            //    currentSpeed = (distanceDifference / 0.02f);
            //    speedList.Add(currentSpeed);

            //    if (speedList.Count == 10) {
            //        float totalspeed = 0;
            //        foreach (float value in speedList) {
            //            totalspeed += value;
            //        }
            //        speedList.Clear();
            //        //Console.WriteLine($"Distance: {bike.Distance}\n Speed: {totalspeed/10}");
            //    }
            //}
            //lastDistance = bike.Distance;
            //Console.WriteLine($"Distance: {bike.Distance}");

        }

        static async void BikeConnection()
        {
            int errorCode = 0;
            BLE bleBike = new BLE();
            BLE bleHeart = new BLE();
            Thread.Sleep(1000); // We need some time to list available devices

            // List available devices
            List<String> bleBikeList = bleBike.ListDevices();
            Console.WriteLine("Devices found: ");
            foreach (var name in bleBikeList)
            {
                Console.WriteLine($"Device: {name}");
            }

            // Connecting
            errorCode = errorCode = await bleBike.OpenDevice("Tacx Flux 24517");
            // __TODO__ Error check

            var services = bleBike.GetServices;
            foreach (var service in services)
            {
                Console.WriteLine($"Service: {service}");
            }

            // Set service
            errorCode = await bleBike.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
            // __TODO__ error check

            stopwatch.Start();

            // Subscribe
            bleBike.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            errorCode = await bleBike.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");

            // Heart rate
            errorCode = await bleHeart.OpenDevice("Decathlon Dual HR");

            await bleHeart.SetService("HeartRate");

            bleHeart.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            await bleHeart.SubscribeToCharacteristic("HeartRateMeasurement");


            Console.Read();
        }

        private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            //Console.WriteLine("Received from {0}: {1}, {2}", e.ServiceName,
            //    BitConverter.ToString(e.Data).Replace("-", " "),
            //    Encoding.UTF8.GetString(e.Data));
            bike.PushDataChange(e.Data);

            Thread.Sleep(5);
            counter++;

            bike.currentSpeedData = bike.CurrentSpeed;
            bike.averageSpeedData = bike.AverageSpeed / 22;
            bike.distanceData = bike.Distance * 1000;

            if (counter % 10 == 0)
            {
                Console.WriteLine($"De current speed={bike.currentSpeedData}\nAverage Speed:{bike.averageSpeedData}");
                EngineInteraction.updateFollowRouteSpeed(bike.CurrentSpeed);
            }

            //Console.WriteLine($"Distance: {bike.Distance}\nSpeed: {bike.CurrentSpeed}");
        }
    }
}
