using Newtonsoft.Json;
using RemoteHealthCare.Devices;
using RHCCore.Networking;
using RHCFileIO;
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

        private static DataWriter dataWriter;
        private static LogWriter logWriter;

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
                        logWriter.WriteLogText("Server received loggin request");
                        string username = (string)args.Data.Username;
                        string password = (string)args.Data.Password;

                        if (UserList.UserExists(username, password))
                        {
                            dynamic user = UserList.GetUser(username);
                            string authKey = Guid.NewGuid().ToString();
                            validAuthKeys.Add(new Tuple<IConnection, string, string>(client, authKey.ToString(), user.Name));
                            logWriter.WriteLogText($"Server accepted login, added client, {user.name}, with {authKey}");
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
                            logWriter.WriteLogText("Server refused login connection, username and password incorrect");
                            client.Write(new
                            {
                                Command = "login/refused",
                            });
                        }
                    }
                    break;

                case "user/push/bike":
                    {
                        logWriter.WriteLogText($"Server got bike data, from bike {args.Data.bike_name}");
                        dynamic data = args.Data;

                        data.bike_name = bike.DeviceName;
                        data.average_speed = bike.AverageSpeed;
                        data.current_speed = bike.CurrentSpeed;
                        data.distance = bike.Distance;
                        break;
                    }

                case "user/push/heart":
                    {
                        logWriter.WriteLogText("Server got heart data");
                        dynamic data = args.data;

                        data.current_hr = hrMonitor.HeartRate;
                        break;
                    }

                case "client/add":
                    {
                        logWriter.WriteLogText($"Server added client, {args.Data.Name}");
                        UserList.AddUser(args.Data.Name, args.Data.Username, args.Data.Password);
                    }
                    break;

                case "clients/get":
                    {
                        logWriter.WriteLogText($"Server got client request");
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
                        logWriter.WriteLogText($"Server got dokter login request");
                        if (UserList.UserExists((string)args.Data.Username, (string)args.Data.Password, true))
                        {
                            logWriter.WriteLogText($"Server accepted dokter login request from {args.Data.Username}");
                            client.Write(new
                            {
                                Command = "login/accepted"
                            });
                        }
                        else
                        {
                            logWriter.WriteLogText($"Server refused connection from {args.Data.Username}");
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
            logWriter.WriteLogText($"Server got new client connection request from {client.RemoteEndPoint.Address}");
            Console.WriteLine($"CLIENT {client.RemoteEndPoint.Address} CONNECTED");
        }
    }
}
