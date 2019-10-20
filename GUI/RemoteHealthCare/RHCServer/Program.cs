using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RHCCore.Networking;
using RHCCore.Networking.Models;
using RHCFileIO;

namespace RHCServer
{
    class Program
    {
        private static TcpServerWrapper server;
        private static List<Tuple<IConnection, string, string>> authkeys;
        private static List<Session> activeSessions = new List<Session>();

        private static PatientOverview PatientOverview;
        private static DataWriter dataWriter;
        private static LogWriter logWriter;

        static async Task Main(string[] args)
        {
            authkeys = new List<Tuple<IConnection, string, string>>();
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
            if (authkeys.Where(x => x.Item1 == client).Count() > 0)
                authkeys.Remove(authkeys.Where(x => x.Item1 == client).First());
        }

        private static void OnDataReceived(IConnection client, dynamic args)
        {
            string command = args.Command;
            switch (command)
            {
                case "session/create":
                {
                    string authkey = args.Data.Key;
                    Session session = args.Data.Session;

                    activeSessions.Add(session);
                    IConnection clientConnection = authkeys.Where(x => x.Item2 == authkey).FirstOrDefault()?.Item1;
                    if (clientConnection != null)
                    {
                        clientConnection.Write(new
                        {
                            Command = "session/start",
                            Data = new
                            {
                                Session = session
                            }
                        });
                    }
                }
                break;

                case "login/try":
                    {
                        //logWriter.WriteLogText("Server received loggin request");
                        string username = (string)args.Data.Username;
                        string password = (string)args.Data.Password;

                        if (UserList.UserExists(username, password))
                        {
                            Person user = UserList.GetUser(username);
                            string authKey = Guid.NewGuid().ToString();
                            authkeys.Add(new Tuple<IConnection, string, string>(client, authKey.ToString(), user.Username));
                            //logWriter.WriteLogText($"Server accepted login, added client, {user.Username}, with {authKey}");
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
                            //logWriter.WriteLogText("Server refused login connection, username and password incorrect");
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

                        foreach (PatientData patientData in PatientOverview.PatientDataBase)
                        {
                            if (patientData.Equals(args.ID))
                            {
                                SaveDataHeartData(args.ID, data.current_hr);
                            }
                        }

                        break;
                    }

                case "user/push/bike":
                    {
                        //logWriter.WriteLogText("Server got heart data");
                        dynamic data = args.data;

                        foreach (PatientData patientData in PatientOverview.PatientDataBase)
                        {
                            if (patientData.Equals(args.ID))
                            {
                                SaveDataBikeData(args.ID, (string)data.bike_name, (int)data.average_speed, (int)data.current_speed, (float)data.distance);
                            }
                        }

                        break;
                    }

                case "client/add":
                    {
                        //logWriter.WriteLogText($"Server added client, {args.Data.Name}");
                        UserList.AddUser(args.Data.Name, args.Data.Username, args.Data.Password);
                    }
                    break;

                case "clients/get":
                    {
                        //logWriter.WriteLogText($"Server got client request");

                        List<dynamic> personsList = new List<dynamic>();
                        foreach (Person iperson in UserList.GetPersons())
                        {
                            bool online = authkeys.Where(x => x.Item3 == iperson.Username).Count() > 0;
                            personsList.Add(new
                            {
                                person = iperson,
                                is_online = online,
                                auth_key = (online ? authkeys.Where(x => x.Item3 == iperson.Username).First().Item2 : "")
                            });
                        }

                        client.Write(new
                        {
                            Command = "clients/list",
                            Data = personsList
                        });
                    }
                    break;

                case "doctor/login":
                    {
                        //logWriter.WriteLogText($"Server got dokter login request");
                        if (UserList.UserExists((string)args.Data.Username, (string)args.Data.Password, true))
                        {
                            //logWriter.WriteLogText($"Server accepted dokter login request from {args.Data.Username}");
                            client.Write(new
                            {
                                Command = "login/accepted"
                            });
                        }
                        else
                        {
                            //logWriter.WriteLogText($"Server refused connection from {args.Data.Username}");
                            client.Write(new
                            {
                                Command = "login/refused"
                            });
                        }
                    }
                break;

                case "dokter/history/request":
                    {
                        //logWriter.WriteLogText($"dokter request received for history data {args.Data.Patient}");
                        PatientData data = dataWriter.GetPatientData(args.Data.Patient);
                        dynamic json = JsonConvert.SerializeObject(data);

                        client.Write(new
                        {
                            Command = "history/patient",
                            Data = new 
                            { 
                                patient = json
                            }
                        });
                    }
                break;
            }
        }

        public static void SaveDataBikeData(string patientID, string bikeName, int avgSpeed, int curSpeed, float distance)
        {
            
            foreach (PatientData patient in PatientOverview.PatientDataBase)
            {
                if (patient.patientID.Equals(patientID))
                {
                    BikeData bikeData;
                    if (patient.bikeData == null)
                    {
                        bikeData = new BikeData(bikeName);
                    }
                    bikeData = patient.bikeData;
                    bikeData.averageSpeed.Add(avgSpeed);
                    bikeData.currentSpeed.Add(curSpeed);
                    bikeData.distanceTraversed.Add(distance);
                    //logWriter.WriteLogText($"Bike data {bikeName} saved, {patientID}");
                }
            }
        }

        public static void SaveDataHeartData(string patientID, int hrRate)
        {
            foreach (PatientData patient in PatientOverview.PatientDataBase)
            {
                if (patient.patientID.Equals(patientID))
                {
                    HeartData heartData;
                    if (patient.bikeData == null)
                    {
                        heartData = new HeartData();
                    }
                    heartData = patient.heartData;
                    heartData.currentHRTRate.Add(hrRate);
                    heartData.averageHRTRate.Add(heartData.CalcTotalAverageHR());
                    //logWriter.WriteLogText($"Heartrate data saved, {patientID}");
                }
            }
        }

        private static void OnNewClient(IConnection client, dynamic args)
        {
            //logWriter.WriteLogText($"Server got new client connection request from {client.RemoteEndPoint.Address}");
            Console.WriteLine($"CLIENT {client.RemoteEndPoint.Address} CONNECTED");
        }
    }
}
