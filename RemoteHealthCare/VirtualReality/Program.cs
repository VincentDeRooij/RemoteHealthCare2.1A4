using System;
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
        private static IFormatProvider result;

        public static void Main(string[] args)
        {
            client = new System.Net.Sockets.TcpClient("145.48.6.10", 6666);
            stream = client.GetStream();

            Thread listenThread = new Thread(ListenThread);
            listenThread.Start();
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
                }
            }
        }

        private static void chooseAction(char character)
        {
            String cToLow = character.ToString().ToLower();
            character = cToLow.ToCharArray()[0];

            string json = "temp";
            bool sendMessage = true;


            switch (character)
            {
                case 'a':
                    json = getSessions();
                    break;
                case 'b':
                    if (sessionId != null)
                        json = tunnelCreate();
                    else
                        Console.WriteLine("No SessionId Found, try 'a'");
                    break;
                case 'c':
                    Console.WriteLine("Enter a time between 0 - 24");
                    double time = double.Parse(Console.ReadLine());
                    json = encapsulatePacket(EngineInteraction.convertSkyBoxTime(time));
                    Console.WriteLine(json);
                    break;
                case 'd':
                    json = encapsulatePacket(EngineInteraction.addFlatTerrain());
                    break;
                case 'e':
                    json = encapsulatePacket(EngineInteraction.addRandomTerrain());
                    break;
                case 'f':
                    json = encapsulatePacket(EngineInteraction.deleteTerrain());
                    break;
                case 'g':
                    json = encapsulatePacket(EngineInteraction.addTerrainNode());
                    break;
                case 'i':
                    json = encapsulatePacket(EngineInteraction.CreateRoute(50,50, 5, -5));
                    break;
                case 'j':
                    json = encapsulatePacket(EngineInteraction.DebugRoute(false));
                    break;
                //case 'h':
                //    json = encapsulatePacket(EngineInteraction.addRoad());
                //    break;
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
    }

    //Add your methods here
    public class EngineInteraction
    {
        public static object convertSkyBoxTime(double time)
        {
            return new
            {
                id = "scene/skybox/settime",
                data = new
                {
                    time = time
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
                id = "scene/terain/delete",
                data = new
                {

                }
            };
        }

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

        public static object addRoad(string routeUuid, double hightOffset)
        {
            return new
            {
                id = "scene/terain/delete",
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

        public static object CreateRoute(int p1, int p2, int t1, int t2) 
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

        public static object RemoveRoute(string uuid)
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

        public static object DebugRoute(bool show) 
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


    }    
}
