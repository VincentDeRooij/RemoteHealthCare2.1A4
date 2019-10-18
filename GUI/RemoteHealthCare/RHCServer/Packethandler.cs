using System;
using System.Net;
using System.Text;
using System.Timers;
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

        //Kan pas werken als de fiets goed werkt.
        private void sendBikeData(Object source, ElapsedEventArgs e)
        {
            //if (bike != null)
            //{
            this.clientWrapper.NetworkConnection.Write(createBikeData());
            //}
            //else {
            //    Console.WriteLine("Er is geen fiets.");
            //}
        }

        //Moet nog gedaan worden.
        private void sendHeartRateData(Object source, ElapsedEventArgs e)
        {
            this.clientWrapper.NetworkConnection.Write(createHeartRateData());
        }

        public object createBikeData()
        {
            try
            {
                Console.WriteLine($"{this.bike.DeviceName} {this.bike.AverageSpeed} {this.bike.CurrentSpeed} {this.bike.Distance}");
                return JsonConvert.SerializeObject(new
                {
                    Command = "user/push/bike",
                    Data = new
                    {
                        distance_traversed = $"{this.bike.DeviceName}",
                        average_speed = $"{this.bike.AverageSpeed}",
                        current_speed = $"{this.bike.CurrentSpeed}",
                        distance = $"{this.bike.Distance}",
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
                        distance_traversed = $"5",
                        average_speed = $"5",
                        current_speed = $"5",
                        distance = $"5",
                    }
                });
            }
        }

        //Moet nog data van de heartrate krijgen.
        public static object createHeartRateData()
        {
            try
            {
                return JsonConvert.SerializeObject(new
                {
                    Command = "user/push/heartrate",
                    Data = new
                    {
                        since_session_start = "test",
                        average_hr = "test",
                        current_hr = "test"
                    }
                });
            }
            catch
            {
                return JsonConvert.SerializeObject(new
                {
                    Command = "user/push//nodataavailable",
                });
            }
        }

        private void SetTimer(int time)
        {
            // Create a timer with a two second interval.
            this.timer = new System.Timers.Timer(time);
            // Hook up the Elapsed event for the timer. 
            this.timer.Elapsed += sendBikeData;
            this.timer.Elapsed += sendHeartRateData;
            this.timer.AutoReset = true;
            this.timer.Enabled = true;
        }

        //Dit moet gebeuren als de client iets ontangt. Maar dat doet hij niet.
        public void received(IConnection client, dynamic args)
        {
            Console.WriteLine($"Received: {Encoding.ASCII.GetString(args)}");
        }

        public void login(string username, string password) {
            this.clientWrapper.NetworkConnection.Write(JsonConvert.SerializeObject(new
            {
                Command = "user/push/login",
                Data = new
                {
                    username = $"{username}",
                    password = $"{password}",
                }
            }));
        }

    }
}
