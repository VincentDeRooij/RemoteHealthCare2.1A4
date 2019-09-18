﻿using System;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

namespace TcpClient
{
    class Program
    {
        public static System.Net.Sockets.TcpClient client;
        public static NetworkStream stream;
        public static string sessionId;
        public static string tunnelId;
        public static string routeUuid;
        private static IFormatProvider result;

        public static void Main(string[] args)
        {
            client = new System.Net.Sockets.TcpClient("145.48.6.10", 6666);
            stream = client.GetStream();

            Thread listenThread = new Thread(ListenThread);
            listenThread.Start();

            sendAction(getSessions());
            while (true) {
                if (sessionId != null) {
                    break;
                }
                Thread.Sleep(100);
            }

            sendAction(tunnelCreate());
            while (true)
            {
                if (tunnelId != null)
                {
                    break;
                }
                Thread.Sleep(100);
            }

            Console.WriteLine("Enter a character to send a command");
            while (true)
            {
                printMenu();
                char henk = Console.ReadLine().ToString().ToCharArray()[0];
                chooseAction(henk);
            }
        }

        static void ListenThread()
        {
            while (true)
            {
                byte[] buffer = new byte[4];

                int incomingBytes = stream.Read(buffer, 0, buffer.Length);
                //Console.WriteLine($"Bytes from server: {incomingBytes}");
                int packetLength = BitConverter.ToInt32(buffer, 0);
                //Console.WriteLine($"PacketLength: {packetLength}");

                byte[] totalBuffer = new byte[packetLength];
                int msgPosition = 0;
                while (msgPosition < packetLength)
                {
                    incomingBytes = stream.Read(totalBuffer, msgPosition, packetLength - msgPosition);
                    msgPosition += incomingBytes;
                }
                string json = System.Text.Encoding.UTF8.GetString(totalBuffer, 0, packetLength);
                //Console.WriteLine(json);

                dynamic deserialized = JsonConvert.DeserializeObject(json);

                if (deserialized != null)
                {
                    if (deserialized.id == "session/list")
                    {
                        foreach (var item in deserialized.data)
                        {

                            if (item.clientinfo.user == Environment.UserName)
                            {
                                Console.WriteLine("-------------------");
                                sessionId = item.id;
                                Console.WriteLine(item.id);
                            }
                        }
                    }
                    else if (deserialized.id == "tunnel/create")
                    {
                        tunnelId = deserialized.data.id;
                        Console.WriteLine(tunnelId);
                    }
                    else if (deserialized.data.data.id == "route/add")
                    {
                        routeUuid = deserialized.data.data.data.uuid;
                        Console.WriteLine(routeUuid);
                    }
                }
            }
        }

        private static void chooseAction(char character)
        {
            character = char.ToLower(character);

            string json = "temp";
            bool sendMessage = true;


            switch (character)
            {
                case 'a':
                    Console.WriteLine("Enter a time between 0 - 24");
                    double time = double.Parse(Console.ReadLine());
                    json = encapsulatePacket(EngineInteraction.setSkyBoxTime(time));
                    Console.WriteLine(json);
                    break;
                case 'b':
                    json = encapsulatePacket(EngineInteraction.addFlatTerrain());
                    break;
                case 'c':
                    json = encapsulatePacket(EngineInteraction.addRandomTerrain());
                    break;
                case 'd':
                    json = encapsulatePacket(EngineInteraction.deleteTerrain());
                    break;
                case 'e':
                    json = encapsulatePacket(EngineInteraction.addTerrainNode());
                    break;
                case 'i':
                    json = encapsulatePacket(EngineInteraction.createRoute(50, 50, 5, -5));
                    break;
                case 'j':
                    json = encapsulatePacket(EngineInteraction.debugRoute(true));
                    break;
                case 'h':
                    json = encapsulatePacket(EngineInteraction.addRoad(routeUuid, 0.01));
                    break;

                case 'k':
                    json = encapsulatePacket(EngineInteraction.addEbicMinecraftSteve(""));
                    break;

                default:
                    sendMessage = false;
                    Console.WriteLine("Wrong char");
                    break;
            }

            if (sendMessage)
            {
                sendAction(json);
            }
        }

        private static void printMenu()
        {
            //Add your own commands
            Console.WriteLine("======================================");
<<<<<<< Updated upstream
            Console.WriteLine("A: Session List(Use to get Session ID)");
            Console.WriteLine("B: Create Tunnel");
            Console.WriteLine("C: Change Skybox Time");
            Console.WriteLine("D: Add flat terrain");
            Console.WriteLine("E: Add random height terrain");
            Console.WriteLine("F: Delete terrain");
            Console.WriteLine("G: Create terrain node");
            //Console.WriteLine("H:");
            Console.WriteLine("I: Create route nodes");
            Console.WriteLine("J: Debug/show current route");

            Console.WriteLine("K: Add epic minecraft steve!");

=======
            Console.WriteLine("A: Change Skybox Time");
            Console.WriteLine("B: Add flat terrain");
            Console.WriteLine("C: Add random height terrain");
            Console.WriteLine("D: Delete terrain");
            Console.WriteLine("E: Create terrain node");
            //Console.WriteLine("H: ");
            //Console.WriteLine("I: ");
            //Console.WriteLine("J: ");
>>>>>>> Stashed changes
            //Console.WriteLine("K: ");
            //Console.WriteLine("L: ");
            //Console.WriteLine("M: ");
            //Console.WriteLine("N: ");
            //Console.WriteLine("O: ");
            //Console.WriteLine("P: ");
            //Console.WriteLine("Q: ");
            Console.WriteLine("======================================");
        }

        private static void sendAction(string json)
        {
            byte[] prependBytes = BitConverter.GetBytes(json.Length);
            byte[] databytes = System.Text.Encoding.UTF8.GetBytes(json);

            stream.Write(prependBytes, 0, prependBytes.Length);
            stream.Write(databytes, 0, databytes.Length);
        }
        public static string getSessions()
        {
            return JsonConvert.SerializeObject(new
            {
                id = "session/list"
            });
        }

        #region Tunnel 
        public static string tunnelCreate()
        {
            return JsonConvert.SerializeObject(new
            {
                id = "tunnel/create",
                data = new
                {
                    session = sessionId,
                    key = ""
                }

            });
        }

        public static string encapsulatePacket(object json)
        {
            return JsonConvert.SerializeObject(new
            {
                id = "tunnel/send",
                data = new
                {
                    dest = tunnelId,
                    data =
                        json

                }
            });
        }
        #endregion

    }

    //Add your methods here
    public class EngineInteraction
    {

        #region Scene 

        #region Node
        public static object addTerrainNode()
        {
            return new
            {
                id = "scene/node/add",
                data = new
                {
                    name = "terrain",
                    components = new
                    {
                        transform = new
                        {
                            position = new[] { 0, 0, 0 },
                            scale = 1,
                            rotation = new[] { 0, 0, 0 }
                        },
                        terrain = new
                        {
                            smoothnormals = true

                        }
                    }
                }
            };
        }

        #endregion

        #region Terrain 
        public static object addRandomTerrain()
        {
            double[] heightMap = new double[65536];
            Random random = new Random();

            for (int i = 0; i < 65536; i++)
            {
                heightMap[i] = random.Next(0, 2);
            }
            return new
            {
                id = "scene/terrain/add",
                data = new
                {
                    size = new[] { 256, 256 },
                    heights = heightMap
                }
            };
        }

        public static object addFlatTerrain()
        {
            int[] heightMap = new int[65536];

            for (int i = 0; i < 65536; i++)
            {
                heightMap[i] = 0;
            }
            return new
            {
                id = "scene/terrain/add",
                data = new
                {
                    size = new[] { 256, 256 },
                    heights = heightMap
                }
            };
        }

        public static object updateTerrain()
        {

            return new
            {
                id = "scene/terrain/update",
                data = new
                {
                }
            };
        }

        public static object deleteTerrain()
        {
            return new
            {
                id = "scene/terrain/delete",
                data = new
                {

                }
            };
        }



        #endregion

        #region Panel

        #endregion

        #region Skybox 
        public static object setSkyBoxTime(double t)
        {
            return new
            {
                id = "scene/skybox/settime",
                data = new
                {
                    time = t
                }
            };
        }

        public static object updateSkyBoxTime(double t)
        {
            //TODO 
            return null;
        }

        #endregion

        #region Road 

        public static object addRoad(string routeUuid, double hightOffset)
        {
            return new
            {
                id = "scene/road/add",
                data = new
                {
                    route = routeUuid,
                    diffuse = "data/NetworkEngine/textures/tarmac_diffuse.png",
                    normal = "data/NetworkEngine/textures/tarmac_normale.png",
                    specular = "data/NetworkEngine/textures/tarmac_specular.png",
                    heightoffset = hightOffset
                }
            };
        }

        public static object updateRoad(string routeUuid, double hightOffset)
        {
            return new
            {
                id = "scene/road/update",
                data = new
                {
                    route = routeUuid,
                    diffuse = "data/NetworkEngine/textures/tarmac_diffuse.png",
                    normal = "data/NetworkEngine/textures/tarmac_normale.png",
                    specular = "data/NetworkEngine/textures/tarmac_specular.png",
                    heightoffset = hightOffset
                }
            };
        }



        #endregion

        #endregion

        #region Route 
        public static object createRoute(int p1, int p2, int t1, int t2)
        {
            return new
            {
                id = "route/add",
                data = new
                {
                    nodes = new[]
                    {
                        new { pos = new[] { 0,0,0 },
                            dir = new[] { t1,0,t2 } },

                        new { pos = new[] { p1,0,0 },
                            dir = new[] { t1,0,t1 } },

                        new { pos = new[] { p1,0,p2 },
                            dir = new[] { t2,0,t1 } },

                        new { pos = new[] { 0,0,p2 },
                            dir = new[] { t2,0,t2 } }
                    }
                }
            };
        }

        public static object updateRoute(int p1, int p2, int t1, int t2)
        {
            return new
            {
                id = "route/add",
                data = new
                {
                    nodes = new[]
                    {
                        new { index = 0,
                            pos = new[] { 0,0,0 },
                            dir = new[] { t1,0,t2 } },

                        new { index = 1,
                            pos = new[] { p1,0,0 },
                            dir = new[] { t1,0,t1 } },

                        new { index = 2,
                            pos = new[] { p1,0,p2 },
                            dir = new[] { t2,0,t1 } },

                        new { index = 3,
                            pos = new[] { 0,0,p2 },
                            dir = new[] { t2,0,t2 } }
                    }
                }
            };
        }

        public static object removeRoute(string uuid)
        {
            return new
            {
                id = "route/delete",
                data = new
                {
                    id = uuid
                }
            };
        }

        public static object debugRoute(bool show)
        {
            return new
            {
                id = "route/show",
                data = new
                {
                    show = show
                }
            };
        }

        #endregion

        #region Other 
        public static object addEbicMinecraftSteve(string uuid)
        {
            return new
            {
                id = "scene/node/add",
                data = new
                {
                    name = "Steve",
                    parent = "GEEN ROOT UUID WANT IK SNAP ME GOD NIET HOE IK ALLES OP MOET VRAGEN ZONDER RETURN WAARDES maar het werkt wel met Johan's code",
                    components = new
                    {
                        transform = new
                        {
                            position = new[] { 0, 0, 0 },
                            rotation = new[] { 0, 0, 0 },
                            scale = 10
                        },
                        model = new
                        {
                            file = "data/NetworkEngine/models/minecraft/minecraft-steve.obj",
                            cullbackfaces = false
                        }
                    }
                }
            };
        }
        #endregion

    }
}
