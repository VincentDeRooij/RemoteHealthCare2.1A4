using Newtonsoft.Json;
using RemoteHealthCare.Devices;
using RHCCore.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RHCServer
{
    class Program
    {
        private static TcpServerWrapper server;
        private static List<Tuple<IConnection, string, string>> validAuthKeys;

        private static StationaryBike bike;
        private static HeartRateMonitor hrMonitor;

        static async Task Main(string[] args)
        {
            validAuthKeys = new List<Tuple<IConnection, string, string>>();
            server = new TcpServerWrapper(new IPEndPoint(IPAddress.Any, 20000));
            await server.StartAsync();

            new UserList();
            server.OnClientConnected += OnNewClient;
            server.OnClientDataReceived += OnDataReceived;
            server.OnClientDisconnected += OnClientDisconnected;
            server.OnClientError += OnClientError;

            Console.ReadKey();
        }

        private static void OnClientError(IConnection client, dynamic args)
        {
            Console.WriteLine($"ERROR: {args.Message}");
        }

        private static void OnClientDisconnected(IConnection client, dynamic args)
        {
            if (validAuthKeys.Where(x => x.Item1 == client).Count() > 0)
                validAuthKeys.Remove(validAuthKeys.Where(x => x.Item1 == client).First());
        }

        private static void OnDataReceived(IConnection client, dynamic args)
        {
            string command = args.Command;
            switch (command)
            {
                case "login/try":
                    {
                        string username = (string)args.Data.Username;
                        string password = (string)args.Data.Password;

                        if (UserList.UserExists(username, password))
                        {
                            dynamic user = UserList.GetUser(username);
                            string authKey = Guid.NewGuid().ToString();
                            validAuthKeys.Add(new Tuple<IConnection, string, string>(client, authKey.ToString(), user.Name));
                            client.Write(new
                            {
                                Command = "login/authenticated",
                                Data = new
                                {
                                    Key = authKey
                                }
                            });
                        }
                        else
                        {
                            client.Write(new
                            {
                                Command = "login/refused",
                            });
                        }
                    }
                    break;

                case "user/push/heartrate":
                    {                   
                        dynamic data = args.data;
                        hrMonitor.HeartRate = data.current_hr;
                        break;
                    }

                case "user/push/bike":
                    {
                        dynamic data = args.data;
                        bike.deviceName = data.bike_name;
                        bike.averageSpeed = data.average_speed;
                        bike.currentSpeed = data.current_speed;
                        bike.Distance = data.Distance;
                        break;
                    }

                case "client/add":
                    {
                        UserList.AddUser(args.Data.Name, args.Data.Username, args.Data.Password);
                    }
                    break;

                case "clients/get":
                    {
                        client.Write(new
                        {
                            Command = "clients/sent",
                            Data = new 
                            {
                                Users = validAuthKeys
                            }
                        });
                    }
                    break;

                case "doctor/login":
                    {
                        if (UserList.UserExists((string)args.Data.Username, (string)args.Data.Password, true))
                        {
                            client.Write(new
                            {
                                Command = "login/accepted"
                            });
                        }
                        else
                        {
                            client.Write(new
                            {
                                Command = "login/refused"
                            });
                        }
                    }
                break;
            }
        }

        private static void OnNewClient(IConnection client, dynamic args)
        {
            Console.WriteLine($"CLIENT {client.RemoteEndPoint.Address} CONNECTED");
        }
    }
}
