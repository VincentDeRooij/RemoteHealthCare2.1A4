using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RHCCore.Networking;
using RHCCore.Networking.Models;

namespace RHCServer
{
    class Program
    {
        private static TcpServerWrapper server;
        private static List<Tuple<IConnection, string, string>> authkeys;
        private static List<IConnection> doctors;

        static async Task Main(string[] args)
        {
            authkeys = new List<Tuple<IConnection, string, string>>();
            server = new TcpServerWrapper(new IPEndPoint(IPAddress.Any, 20000));
            doctors = new List<IConnection>();
            await server.StartAsync();

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

            if (doctors.Contains(client))
                doctors.Remove(client);
        }

        private static void OnDataReceived(IConnection client, dynamic args)
        {
            string command = args.Command;
            switch (command)
            {
                case "session/create":
                    {
                        string authkey = (string)args.Data.Key;
                        Session session = null;
                        if ((bool)args.Data.IsAstrand)
                            session = (args.Data.Session as JObject).ToObject<AstrandSession>();
                        else
                            session = (args.Data.Session as JObject).ToObject<Session>();

                        IConnection clientConnection = authkeys.Where(x => x.Item2 == authkey).FirstOrDefault()?.Item1;
                        if (SessionStorage.Instance.CreateSession(session))
                        {
                            var tuple = authkeys.Where(x => x.Item1 == clientConnection).FirstOrDefault();
                            if (tuple != null)
                            {
                                Person person = AccountStorage.Instance.GetPerson(tuple.Item3);
                                person.Sessions.Add(session.SessionId);
                                AccountStorage.Instance.SyncToFile();
                            }

                            if (clientConnection != null)
                            {
                                clientConnection.Write(new
                                {
                                    Command = "session/create",
                                    Data = new
                                    {
                                        Session = session,
                                        IsAstrand = args.Data.IsAstrand
                                    }
                                });
                            }
                        }
                    }
                break;

                case "session/update":
                    {
                        Session session = SessionStorage.Instance.RetrieveSession((string)args.Data.SessionId);
                        session.BikeData.Add(args.Data.BikeData);
                        SessionStorage.Instance.SyncSession(session);

                        doctors.ForEach(x => x.Write(new
                        {
                            Command = $"session/{session.SessionId}/updated",
                            Data = args.Data
                        }));
                    }
                break;

                case "session/start":
                    {
                        string authkey = (string)args.Data.Key;
                        IConnection clientConnection = authkeys.Where(x => x.Item2 == authkey).FirstOrDefault()?.Item1;
                        clientConnection.Write(new
                        {
                            Command = "session/start",
                        });
                    }
                    break;

                case "session/ready":
                    {
                        Session session = SessionStorage.Instance.RetrieveSession((string)args.Data.SessionId);
                        doctors.ForEach(x => x.Write(new
                        {
                            Command = $"session/{session.SessionId}/ready",
                            Data = args.Data
                        }));
                    }
                break;

                case "session/done":
                    {
                        Session session = SessionStorage.Instance.RetrieveSession((string)args.Data.SessionId);
                        session.IsArchived = true;
                        SessionStorage.Instance.SyncSession(session);

                        doctors.ForEach(x => x.Write(new
                        {
                            Command = $"session/{session.SessionId}/done",
                            Data = args.Data
                        }));
                    }
                break;

                case "login/try":
                    {
                        string username = (string)args.Data.Username;
                        string password = (string)args.Data.Password;

                        if (AccountStorage.Instance.UserExists(username, password))
                        {
                            Person user = AccountStorage.Instance.GetPerson(username);
                            string authKey = Guid.NewGuid().ToString();
                            authkeys.Add(new Tuple<IConnection, string, string>(client, authKey.ToString(), user.Username));
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

                case "client/add":
                    {
                        AccountStorage.Instance.AddPerson(new Person(args.Data.Name, args.Data.Username, args.Data.Password));
                    }
                break;

                case "clients/get":
                    {
                        List<dynamic> personsList = new List<dynamic>();
                        foreach (Person iperson in AccountStorage.Instance.AccountList)
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
                        if (AccountStorage.Instance.UserExists((string)args.Data.Username, (string)args.Data.Password, true))
                        {
                            client.Write(new
                            {
                                Command = "login/accepted"
                            });

                            doctors.Add(client);
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

                case "chat/send":
                    {
                        string key = (string)args.Key;
                        authkeys.Where(x => x.Item2.Equals(key)).FirstOrDefault() ? .Item1.Write(args);
                    }
                break;

                case "resistance/send":
                    {
                        string key = (string)args.Key;
                        authkeys.Where(x => x.Item2.Equals(key)).FirstOrDefault()?.Item1.Write(args);
                    }
                break;

                case "doctor/session/done":
                    {
                        AstrandSession astrandSession = SessionStorage.Instance.RetrieveAstrandSession((string)args.Data.SessionId);
                        astrandSession.ReachedSteady = (bool)args.Data.SteadyState;
                        SessionStorage.Instance.SyncSession(astrandSession);
                    }
                break;

                case "history/request":
                    {
                        string json = JsonConvert.SerializeObject(((bool)args.Data.IsAstrand ? SessionStorage.Instance.RetrieveAstrandSession((string)args.Data.SessionId) : SessionStorage.Instance.RetrieveSession((string)args.Data.SessionId)));
                        byte[] jsonBuffer = Encoding.ASCII.GetBytes(json);
                        List<byte> buffer = new List<byte>(jsonBuffer);
                        client.Write(new
                        {
                            Command = $"history/{(string)args.Data.SessionId}/start",
                        });

                        while (buffer.Count > 0)
                        {
                            int remaining = buffer.Count >= 4096 ? 4096 : buffer.Count;
                            client.Write(new
                            {
                                Command = $"history/{(string)args.Data.SessionId}/upload",
                                Data = new
                                {
                                    Session = buffer.GetRange(0, remaining)
                                }
                            });

                            buffer.RemoveRange(0, remaining);
                        }

                        client.Write(new
                        {
                            Command = $"history/{(string)args.Data.SessionId}/done",
                        });
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
