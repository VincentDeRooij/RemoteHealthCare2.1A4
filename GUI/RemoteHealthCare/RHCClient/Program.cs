using System;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows;
using Newtonsoft.Json;
using RemoteHealthCare.Devices;
using RHCCore.Networking;


namespace RemoteHealthCare
{
    class Packethandler
    {
        private TcpClientWrapper clientWrapper;
        private Timer timer;
        private StationaryBike bike;
        private HeartRateMonitor hRMonitor;

        private int authKey;

        public Packethandler(string ip)
        {
            this.clientWrapper = new TcpClientWrapper();
            try
            {
                this.clientWrapper.Connect(new System.Net.IPEndPoint(IPAddress.Parse(ip), 20000));
                this.clientWrapper.OnReceived += received;
                SetTimer(500);
            }
            catch
            {
                Console.WriteLine("No connection available.");
            }
        }

        public void setBike(StationaryBike bike)
        {
            login("login", "password");
            this.bike = bike;
        }

        public void SetHRMonitor(HeartRateMonitor HeartMonitor) 
        {
            login("login", "password");
            this.hRMonitor = HeartMonitor;
        }

        //Kan pas werken als de fiets goed werkt.
        private void sendBikeData(Object source, ElapsedEventArgs e)
        {
            //if (bike != null)
            //{
            this.clientWrapper.NetworkConnection.Write(createBikeData());
            //}
            //else {
            //    Console.WriteLine("Er is geen fiets aangesloten");
            //}
        }

        private void sendHeartData(Object source, ElapsedEventArgs e)
        {
            //if (bike != null)
            //{
            this.clientWrapper.NetworkConnection.Write(createHeartRateData());
            //}
            //else {
            //    Console.WriteLine("Er is geen HR monitor aangesloten.");
            //}
        }

        public object createBikeData()
        {
            try
            {
                Console.WriteLine($"{this.bike.deviceName} {this.bike.AverageSpeed} {this.bike.CurrentSpeed} {this.bike.Distance}");
                return JsonConvert.SerializeObject(new
                {
                    Command = "user/push/bike",
                    Data = new
                    {
                        bike_name = $"{this.bike.deviceName}",
                        average_speed = $"{this.bike.AverageSpeed}",
                        current_speed = $"{this.bike.CurrentSpeed}",
                        distance = $"{this.bike.Distance}"
                    }
                });
            }
            catch
            {
                return JsonConvert.SerializeObject(new
                {
                    Command = "user/push/bike",
                    Data = new
                    {
                        average_speed = $"5",
                        current_speed = $"5",
                        distance = $"5"
                    }
                });
            }
        }

        //Moet nog data van de heartrate krijgen.
        public object createHeartRateData()
        {
            try
            {
                return JsonConvert.SerializeObject(new
                {
                    Command = "user/push/heartrate",
                    Data = new
                    {
                        current_hr = $"{this.hRMonitor.HeartRate}"
                    }
                });
            }
            catch
            {
                return JsonConvert.SerializeObject(new
                {
                    Command = "user/push/nodataavailable",
                });
            }
        }

        private void SetTimer(int time)
        {
            // Create a timer with a two second interval.
            this.timer = new System.Timers.Timer(time);
            // Hook up the Elapsed event for the timer. 
            this.timer.Elapsed += sendBikeData;
            this.timer.Elapsed += sendHeartData;
            this.timer.AutoReset = true;
            this.timer.Enabled = true;
        }

        //Dit moet gebeuren als de client iets ontangt. Maar dat doet hij niet.
        public void received(IConnection server, dynamic args)
        {
            string command = args.Command;
            switch (command)
            {
                case "login/authenticated":
                    {
                        this.authKey = args.Command.data.key;
                        break;
                    }
                case "login/refused":
                    {
                        MessageBox.Show("Login Error!");
                        break;
                    }
                case "clients/sent": 
                    {
                        Console.WriteLine("?");
                        break;
                    }
                default: 
                    {
                        Error();
                        break;
                    }   
            }
        }

        public void Error() 
        {
            this.clientWrapper.NetworkConnection.Write(JsonConvert.SerializeObject(new
            {
                Command = "error",
            }));
        }

        public void login(string username, string password)
        {
            this.clientWrapper.NetworkConnection.Write(JsonConvert.SerializeObject(new
            {
                Command = "login/try",
                Data = new
                {
                    username = $"{username}",
                    password = $"{password}",
                }
            }));
        }

    }
}
