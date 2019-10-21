using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Diagnostics;
using RHCCore.Networking;
using System.Net;

namespace RemoteHealthCare
{

    /// <summary>w
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static System.Net.Sockets.TcpClient clientVR;
        public static NetworkStream streamVR;
        public static System.Net.Sockets.TcpClient clientData;
        public static NetworkStream streamData;
        public static string sessionId;
        public static string tunnelId;
        public static String sceneJson;
        public static Dictionary<String, String> Uuids = new Dictionary<string, string>();
        public static ArrayList chatList = new ArrayList();
        public static Stopwatch stopwatch = new Stopwatch();
        public static Panel panel { get; set; }


        public static TcpClientWrapper serverClientWrapper;

        public App()
            : base()
        {
#if SIM
            Simulator.Simulator s = Simulator.Simulator.Instance;
#endif
            serverClientWrapper = new TcpClientWrapper();
            bool result = serverClientWrapper.Connect(new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));
            
            if (result == false)
            {
                MessageBox.Show("Could not connect to the server");
                Environment.Exit(0);
                return;
            }

            Thread vrThread = new Thread(() =>
            {
                clientVR = new System.Net.Sockets.TcpClient("145.48.6.10", 6666);
                streamVR = clientVR.GetStream();
                //clientData = new System.Net.Sockets.TcpClient("127.0.0.1", 20000);
                //streamData = clientData.GetStream();

                Thread listenThreadVR = new Thread(() => ListenThread(streamVR));
                listenThreadVR.Start();

                //Thread listenThreadData = new Thread(() => ListenThread(streamData));
                //listenThreadData.Start();

                stopwatch.Start();

                new Thread(() =>
                {
                    string command = string.Format(@"C:\Users\brand\Downloads\NetworkEngine.18.10.10.1\NetworkEngine\");
                    Process process = new Process();
                    process.StartInfo.WorkingDirectory = command;
                    process.StartInfo.FileName = "sim.bat";
                    process.StartInfo.CreateNoWindow = false;
                    process.Start();
                    process.WaitForExit();
                    process.Close();
                }).Start();

                Thread.Sleep(3000);

                sendAction(getSessions());
                int setupTimer = 0;
                while (true)
                {
                    if (sessionId != null)
                    {
                        sendAction(tunnelCreate());
                    }

                    if (tunnelId != null)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                    setupTimer++;
                    if (setupTimer == 100)
                    {
                        sendAction(getSessions());
                        setupTimer = 0;
                    }
                }

                setupSimulator();
                Console.WriteLine("Enter a character to send a command");
                while (true)
                {
                    printMenu();
                    //char henk = Console.ReadLine().ToString().ToCharArray()[0];
                    char input = Console.ReadKey().KeyChar;
                    Console.WriteLine("");
                    chooseAction(input);
                }
            });
            //vrThread.Start();
        }

        static void ListenThread(NetworkStream stream)
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
                Console.WriteLine(json);

                dynamic deserialized = JsonConvert.DeserializeObject(json);

                if (deserialized != null)
                {
                    switch ((string)deserialized.id)
                    {
                        case "session/list":
                            foreach (var item in deserialized.data)
                            {
                                if (item.clientinfo.user == Environment.UserName)
                                {
                                    Console.WriteLine("-------------------");
                                    sessionId = item.id;
                                    Console.WriteLine(item.id);
                                }
                            }
                            break;
                        case "tunnel/create":
                            tunnelId = deserialized.data.id;
                            Console.WriteLine(tunnelId);
                            break;
                        case "tunnel/send":
                            if (deserialized.data.data.id == "scene/get")
                            {
                                Uuids.Clear();
                                foreach (var item in deserialized.data.data.data.children)
                                {
                                    if (!Uuids.ContainsKey((string)item.name))
                                        Uuids.Add((string)item.name, (string)item.uuid);
                                }
                                foreach (KeyValuePair<string, string> kvp in Uuids)
                                {
                                    //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                                    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                                }
                            }
                            else if (deserialized.data.data.id == "scene/node/add")
                            {
                                if (!Uuids.ContainsKey((string)deserialized.data.data.data.name))
                                    Uuids.Add((string)deserialized.data.data.data.name, (string)deserialized.data.data.data.uuid);
                            }
                            else if (deserialized.data.data.id == "route/add")
                            {
                                if (!Uuids.ContainsKey("Route"))
                                    Uuids.Add("Route", (string)deserialized.data.data.data.uuid);
                            }
                            break;
                        case "scene/get":
                            sceneJson = deserialized.data;
                            Console.WriteLine(sceneJson);
                            break;
                        case "scene/node/add":
                            Uuids.Add(deserialized.data.name, deserialized.data.uuid);
                            foreach (KeyValuePair<string, string> kvp in Uuids)
                            {
                                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                            }
                            break;
                        case "bike/resistance":
                            var resistance = deserialized.data;
                            Panel.resistance = (int)resistance;
                            Panel.drawValues();
                            break;
                        case "bike/message":
                            var message = deserialized.data;
                            Panel.chatList.Add((string)message);
                            Panel.drawValues();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

            private static void setupSimulator()
        {
            //Get Scene
            sendAction(encapsulatePacket(EngineInteraction.getScene()));
            Thread.Sleep(1000);

            //Draw Panel
            Uuids.TryGetValue("Camera", out string cameraUUID);
            sendAction(encapsulatePacket(EngineInteraction.createPanel("panel", cameraUUID)));
            Thread.Sleep(1000);
            Uuids.TryGetValue("panel", out string panelUuid);
            panel = new Panel(panelUuid);
            Panel.drawPanel();

            //Add Route
            sendAction(encapsulatePacket(EngineInteraction.createRoute()));
            Thread.Sleep(1000);
            Uuids.TryGetValue("Route", out string routeUUID);
            sendAction(encapsulatePacket(EngineInteraction.addRoad(routeUUID, 0.5)));

            //Remove Groundplane
            if (Uuids.TryGetValue("GroundPlane", out string groundUUID))
            {
                sendAction(encapsulatePacket(EngineInteraction.removeObject(groundUUID)));
                Uuids.Remove("GroundPlane");
            }

            //Change SkyBox
            sendAction(encapsulatePacket(EngineInteraction.changeSkyBoxTexture()));
            sendAction(encapsulatePacket(EngineInteraction.addObject(1, 1, 1, 1, "Bike", cameraUUID)));
            sendAction(encapsulatePacket(EngineInteraction.followRoute(routeUUID, cameraUUID)));
        }

        private static void chooseAction(char character)
        {
            character = char.ToLower(character);
            string json = "temp";
            bool sendMessage = true;
            string objectName, routeUUID, objectUUID;


            switch (character)
            {
                case 'a':
                    {
                        Console.WriteLine("Enter a time between 0 - 24");
                        double time = double.Parse(Console.ReadLine());
                        json = encapsulatePacket(EngineInteraction.setSkyBoxTime(time));
                        Console.WriteLine(json);
                        break;
                    }
                case 'b':
                    {
                        json = encapsulatePacket(EngineInteraction.addFlatTerrain());
                        break;
                    }
                case 'c':
                    {
                        json = encapsulatePacket(EngineInteraction.addRandomTerrain());
                        break;
                    }
                case 'd':
                    {
                        json = encapsulatePacket(EngineInteraction.deleteTerrain());
                        break;
                    }
                case 'e':
                    {
                        json = encapsulatePacket(EngineInteraction.addTerrainNode());
                        break;
                    }
                case 'f':
                    {
                        Uuids.TryGetValue("Camera", out string cameraUUID);
                        json = encapsulatePacket(EngineInteraction.createPanel("panel", cameraUUID));
                        break;
                    }
                case 'g':
                    {
                        Uuids.TryGetValue("panel", out string panelUUID);
                        json = encapsulatePacket(EngineInteraction.clearPanel(panelUUID));
                        break;
                    }
                case 'k':
                    {
                        Console.WriteLine("Insert name");
                        string name = Console.ReadLine();
                        Console.WriteLine("Insert scale");
                        double scale = double.Parse(Console.ReadLine());
                        Console.WriteLine("Insert x");
                        double x = double.Parse(Console.ReadLine());
                        Console.WriteLine("Insert y");
                        double y = double.Parse(Console.ReadLine());
                        Console.WriteLine("Insert z");
                        double z = double.Parse(Console.ReadLine());

                        Uuids.TryGetValue("Camera", out string cameraUUID);
                        json = encapsulatePacket(EngineInteraction.addObject(scale, x, y, z, name, cameraUUID));
                        break;
                    }
                case 'l':
                    {
                        Console.WriteLine("Name of Object");
                        objectName = Console.ReadLine();
                        if (Uuids.TryGetValue(objectName, out objectUUID))
                        {
                            json = encapsulatePacket(EngineInteraction.removeObject(objectUUID));
                            Uuids.Remove(objectName);
                            Console.WriteLine(json);
                        }
                        break;
                    }
                case 'm':
                    {
                        Uuids.TryGetValue("Camera", out objectUUID);
                        Uuids.TryGetValue("Route", out routeUUID);
                        json = encapsulatePacket(EngineInteraction.followRoute(routeUUID, objectUUID));
                        Console.WriteLine(json);
                        break;
                    }
                case 'n':
                    {
                        json = encapsulatePacket(EngineInteraction.resetScene());
                        Console.WriteLine(json);
                        break;
                    }
                case 'o':
                    {
                        Console.WriteLine("What object?");
                        objectName = Console.ReadLine();
                        Console.WriteLine("Insert speed");
                        double speed = double.Parse(Console.ReadLine());
                        Uuids.TryGetValue(objectName, out objectUUID);
                        //json = encapsulatePacket(EngineInteraction.updateFollowRouteSpeed(objectUUID, speed));
                        Console.WriteLine(json);
                        break;
                    }
                case 'p':
                    {
                        json = encapsulatePacket(EngineInteraction.getScene());
                        break;
                    }
                case 'q':
                    {
                        Uuids.TryGetValue("panel", out string panelUUID);
                        json = encapsulatePacket(EngineInteraction.swapPanel(panelUUID));
                        break;
                    }
                case 'r':
                    {
                        json = encapsulatePacket(EngineInteraction.changeSkyBoxTexture());
                        break;
                    }
                case 't':
                    {
                        Uuids.TryGetValue("panel", out string panelUUID);
                        Panel.drawPanel();
                        sendMessage = false;
                        break;
                    }
                case 'v':
                    {
                        Console.WriteLine("What text?");
                        string text = ": " + Console.ReadLine();
                        Console.WriteLine("What resistance?");
                        int resistance = int.Parse(Console.ReadLine());
                        TimeSpan elapsedTime = stopwatch.Elapsed;
                        string time2 = "" + elapsedTime.Minutes.ToString("00") + ":" + elapsedTime.Seconds.ToString("00") + " ";
                        Console.WriteLine("What speed?");
                        float speed = float.Parse(Console.ReadLine());
                        Panel.chatList.Add(time2 + text);
                        Panel.speed = speed;
                        Panel.resistance = resistance;

                        Panel.drawValues();

                        sendMessage = false;
                        break;
                    }
                default:
                    {
                        sendMessage = false;
                        Console.WriteLine("Wrong char");
                        break;
                    }
            }
            Console.WriteLine(json);
            if (sendMessage)
            {
                sendAction(json);
            }
        }

        private static void printMenu()
        {
            //Add your own commands
            Console.WriteLine("======================================");
            Console.WriteLine("A: Change Skybox Time");
            Console.WriteLine("B: Add flat terrain");
            Console.WriteLine("C: Add random height terrain");
            Console.WriteLine("D: Delete terrain");
            Console.WriteLine("E: Create terrain node");
            Console.WriteLine("F: Create panel");
            Console.WriteLine("G: Clear panel");
            Console.WriteLine("H: Add new route");
            Console.WriteLine("I: Create route nodes");
            Console.WriteLine("J: Debug/show current route");
            Console.WriteLine("K: Add object (steve)");
            Console.WriteLine("L: Remove object");
            Console.WriteLine("M: Make object follow the route");
            Console.WriteLine("N: Reset scene");
            Console.WriteLine("O: Change route speed");

            Console.WriteLine("======================================");
        }

        public static void sendAction(string json)
        {
            Console.WriteLine(json);
            byte[] prependBytes = BitConverter.GetBytes(json.Length);
            byte[] databytes = System.Text.Encoding.UTF8.GetBytes(json);

            streamVR.Write(prependBytes, 0, prependBytes.Length);
            streamVR.Write(databytes, 0, databytes.Length);
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
    }
    public class EngineInteraction
    {

        #region Scene 
        public static object getScene()
        {
            return new
            {
                id = "scene/get"
            };
        }



        public static object resetScene()
        {
            return new
            {
                id = "scene/reset"
            };
        }

        public static object saveScene(bool overwrite_)
        {
            return new
            {
                id = "scene/save",
                data = new
                {
                    filename = "cookie.json",
                    overwrite = overwrite_
                }
            };
        }

        public static object loadScene()
        {
            return new
            {
                id = "scene/load",
                data = new
                {
                    filename = "cookie.json"
                }
            };
        }

        public static object raycastScene(int[] startPosition, int[] directionPoint, bool physicsCheck)
        {
            return new
            {
                id = "scene/raycast",
                data = new
                {
                    start = startPosition,
                    direction = directionPoint,
                    physics = physicsCheck.ToString().ToLower()
                }
            };
        }

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
                            position = new[] { -128, 0, -128 },
                            scale = 1,
                            rotation = new[] { 0, 0, 0 }
                        },
                        terrain = new
                        {
                            smoothnormals = true

                        },
                        model = new
                        {
                            file = "data/NetworkEngine/models/minecraft/minecraft-steve.obj",
                            animated = false,
                            animation = "animationname"
                        }
                    }
                }
            };
        }

        public static object updateNode(string nodeID, string parentID, int[] position_, int scale_, int[] rotation_,
            string animationName, double animationSpeed)
        {
            return new
            {
                id = "scene/node/update",
                data = new
                {
                    id = nodeID,
                    parent = parentID,
                    transform = new
                    {
                        position = position_,
                        scale = scale_,
                        rotation = rotation_
                    },
                    animation = new
                    {
                        name = animationName,
                        speed = animationSpeed
                    }
                }
            };
        }

        public static object movetoNode(string nodeID, string stopMovement, int[] destinationPosition, string rotate_,
            string interpolate_, bool followheight_, double speed_)
        {
            return new
            {
                id = "scene/node/moveto",
                data = new
                {
                    id = nodeID,
                    stop = stopMovement,
                    position = destinationPosition,
                    rotate = rotate_,
                    interpolate = interpolate_,
                    followheight = followheight_,
                    speed = speed_
                }
            };
        }

        public static object deleteNode(string nodeID)
        {
            return new
            {
                id = "scene/node/delete",
                data = new
                {
                    id = nodeID
                }
            };
        }

        public static object findNode(string nodeName)
        {
            return new
            {
                id = "scene/node/find",
                data = new
                {
                    name = nodeName
                }
            };
        }

        public static object addLayer(string nodeID, string diffuseTexturePNG, string normalTexturePNG,
            int minHeight_, int maxHeight_, int fadeDist_)
        {
            return new
            {
                id = "scene/node/addlayer",
                data = new
                {
                    id = nodeID,
                    diffuse = diffuseTexturePNG,
                    normal = normalTexturePNG,
                    minHeight = minHeight_,
                    maxHeight = maxHeight_,
                    fadeDist = fadeDist_
                }
            };
        }

        public static object dellayer()
        {
            return new
            {
                id = "scene/node/dellayer",
                data = new
                {

                }
            };
        }

        #endregion

        #region Panel

        public static object createPanel(string name, string parent)
        {
            return new
            {
                id = "scene/node/add",
                data = new
                {
                    name = name,
                    parent = parent,
                    components = new
                    {
                        transform = new
                        {
                            position = new[] { 0, 1.05, -0.65 },
                            rotation = new[] { 285, 0, 0 },
                            scale = 1
                        },
                        panel = new

                        {
                            size = new[] { 0.5, 0.5 },
                            resolution = new[] { 512, 512 },
                            background = new[] { 0, 0, 0, 1 },
                            castShadow = true
                        }

                    }
                }

            };

        }
        public static object clearPanel(string nodeID)
        {
            return new
            {
                id = "scene/panel/clear",
                data = new
                {
                    id = nodeID
                }
            };
        }

        public static object swapPanel(string nodeID)
        {
            return new
            {
                id = "scene/panel/swap",
                data = new
                {
                    id = nodeID
                }
            };
        }

        public static object drawLinesPanel(string nodeID)
        {
            return new
            {
                id = "scene/panel/drawlines",
                data = new
                {
                    id = nodeID,
                    width = 10,
                    lines = new[]
                    {
                        new [] { 0,0, 512,512, 1,1,1,1 },
                        new [] { 0, 512, 512, 0, 1,1,1,1 },
                        new [] { 256, 0, 256, 512, 1,1,1,1 },
                        new [] { 0, 256, 512, 256, 1,1,1,1 },
                        /*
                        TODO:
                        [ 0,0, 10,10, 0,0,0,1, // x1,y1, x2,y2, r,g,b,a ],
                        [0, 0, 100, 10, 0, 0, 0, 1, // x1,y1, x2,y2, r,g,b,a ]
                        */
                    }
                }
            };
        }

        public static object setClearColorPanel(string nodeID, int[] colorsARGB)
        {
            return new
            {
                id = "scene/panel/setclearcolor",
                data = new
                {
                    id = nodeID,
                    color = colorsARGB
                }
            };
        }

        public static object drawTextPanel(string nodeID)
        {
            return new
            {
                id = "scene/panel/drawtext",
                data = new
                {
                    id = nodeID,
                    text = "Pascal is gay",
                    position = new[] { 100, 180 },
                    size = 70,
                    color = new[] { 0.8f, 0.2f, 0.8f, 1 }
                }
            };
        }

        public static object imagePanel(string nodeID, string imagePNG, double[] positionXY, double[] sizeXY)
        {
            return new
            {
                id = "scene/panel/image",
                data = new
                {
                    id = nodeID,
                    image = imagePNG,
                    position = positionXY,
                    size = sizeXY
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
                heightMap[i] = random.NextDouble() * 0.2;
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

        #region Skybox 
        public static object setSkyBoxTime(double time_)
        {
            return new
            {
                id = "scene/skybox/settime",
                data = new
                {
                    time = time_
                }
            };
        }

        public static object updateSkyBoxTime(double t)
        {
            //TODO *
            return null;
            Console.WriteLine("Invoked TODO Method>'updateSkyBoxTime(double t)'");
        }

        public static object changeSkyBoxTexture()
        {
            string filepath = "data/NetworkEngine/textures/SkyBoxes/mp_mandaris/";
            return new
            {
                id = "scene/skybox/update",
                data = new
                {
                    type = "static",
                    files = new
                    {
                        xpos = filepath + "mandaris_rt.tga",
                        xneg = filepath + "mandaris_lf.tga",
                        ypos = filepath + "mandaris_up.tga",
                        yneg = filepath + "mandaris_dn.tga",
                        zpos = filepath + "mandaris_bk.tga",
                        zneg = filepath + "mandaris_ft.tga"
                    }
                }
            };
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
        // Creates 4 route nodes according to the parameters set
        // where p1 & p2 are positions so p1=50 & p2=50 means you get a square shaped route 
        // where t1 & t2 are direction, the directions mean the shaping of the route, if the directions are t1 = 0 & t2 = 0. You get straight lines
        // so when you want a smoother route tweak the t1 & t2 parameters.
        public static object createRoute()
        {
            int tr = 15;
            return new
            {
                id = "route/add",
                data = new
                {
                    nodes = new[]
                    {
                        new { pos = new[] { 0,0,0 },
                        dir = new[] { tr, 0, tr } },

                        new { pos = new[] { 50,0,0 },
                        dir = new[] { tr, 0, tr } },

                        new { pos = new[] { 50,0,25 },
                        dir = new[] { -tr, 0, tr } },

                        new { pos = new[] { 25,0,25 },
                        dir = new[] { -tr, 0,-tr } },

                        new { pos = new[] { 25,0,-25 },
                        dir = new[] { -tr, 0,-tr } },

                        new { pos = new[] { 0,0,-25 },
                        dir = new[] { -tr, 0, tr } },


                        //new { pos = new[] { 0,0,0 },
                        //dir = new[] { 45,-90,45 } },

                        //new { pos = new[] { p1,0,0 },
                        //dir = new[] { t1,0,t1 } },

                        //new { pos = new[] { p1,0,p2 },
                        //dir = new[] { t2,0,t1 } },

                        //new { pos = new[] { 0,0,p2 },
                        //dir = new[] { t2,0,t2 } }
                    }
                }
            };
        }
        // updates the 4 route nodes according to the parameters set
        // where p1 & p2 are positions so p1=50 & p2=50 means you get a square shaped route 
        // where t1 & t2 are direction, the directions mean the shaping of the route, if the directions are t1 = 0 & t2 = 0. You get straight lines
        // so when you want a smoother route tweak the t1 & t2 parameters.
        //  
        // the uuid is the route id. 
        public static object updateRoute(string uuid, int p1, int p2, int t1, int t2)
        {
            return new
            {
                id = "route/update",
                data = new
                {
                    id = uuid,
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
                        dir = new[] { t2,0,t2 }

                        }
                }
                }
            };
        }

        public static object followRoute(string routeUuid, string nodeid) // makes a node follow a route
        {
            return new
            {
                id = "route/follow",
                data = new
                {
                    route = routeUuid, // route id
                    node = nodeid, // this can be any value?
                    speed = 0.0, // the speed of the node
                    offset = 0.0, // the offset of the node, 0.0 means the node moves exactly one the line other values mean its off.
                    rotate = "XYZ", // can be set to NONE, XZ or XYZ
                    smoothing = 1.0, // how smooth the node moves on the route?
                    followheigth = true, //set bool to follow the terrain height
                    rotateOffset = new[] { 0, 0, 0 },
                    positionOffset = new[] { 0, 0, 0 }
                }
            };
        }

        public static object updateFollowRouteSpeed(double newSpeed) // changes a given node speed
        {

            App.Uuids.TryGetValue("Camera", out string objectUUID);
            return new
            {
                id = "route/follow/speed",
                data = new
                {
                    node = objectUUID, // the value of the given node
                    speed = newSpeed // the speed of the node
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
                    id = uuid // deletes the route with the given uuid
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
        public static object addObject(double scale, double x, double y, double z, string name, string parent)
        {
            return new
            {
                id = "scene/node/add",
                data = new
                {
                    name = name,
                    parent = parent,
                    components = new
                    {
                        transform = new
                        {
                            position = new[] { 0, 0, 0 },
                            rotation = new[] { 0, -90, 0 },
                            scale = scale
                        },
                        model = new
                        {
                            file = "data/NetworkEngine/models/bike/bike.fbx",
                            animated = false,
                            animation = "animationname"
                        }

                    }
                }

            };
        }

        public static object removeObject(string uuid)
        {
            return new
            {
                id = "scene/node/delete",
                data = new
                {
                    id = uuid
                }
            };
        }
        #endregion

    }

    #endregion

    public class Panel
    {
        public static string nodeID { get; set; }

        public static float speed;
        public static int resistance;
        public static ArrayList chatList { get; set; }
        public Panel(string nodeID)
        {
            Panel.nodeID = nodeID;
            chatList = new ArrayList();

            App.sendAction(App.encapsulatePacket(EngineInteraction.clearPanel(nodeID)));
            App.sendAction(App.encapsulatePacket(EngineInteraction.swapPanel(nodeID)));
            App.sendAction(App.encapsulatePacket(EngineInteraction.clearPanel(nodeID)));
            App.sendAction(App.encapsulatePacket(EngineInteraction.swapPanel(nodeID)));
        }

        public static void drawPanel()
        {

            App.sendAction(App.encapsulatePacket(EngineInteraction.swapPanel(nodeID)));
            //Background
            App.sendAction(App.encapsulatePacket(EngineInteraction.clearPanel(nodeID)));

            //Outlines
            App.sendAction(App.encapsulatePacket(drawOutLines()));

            //Text
            App.sendAction(App.encapsulatePacket(drawText("Speed", 2, 0, 40)));
            App.sendAction(App.encapsulatePacket(drawText("m/s", 3, 10, 60)));

            App.sendAction(App.encapsulatePacket(drawText("Resistance", 6, 0, 40)));

            App.sendAction(App.encapsulatePacket(drawText("Chat", 11, 0, 40)));

            App.sendAction(App.encapsulatePacket(EngineInteraction.swapPanel(nodeID)));
        }


        public static void drawValues()
        {
            drawPanel();
            App.sendAction(App.encapsulatePacket(EngineInteraction.swapPanel(nodeID)));
            App.sendAction(App.encapsulatePacket(drawText("" + speed, 3, 7, 60)));
            int counter = 0;

            ArrayList temp = new ArrayList();
            if (chatList.Count >= 5)
            {
                temp.Add(chatList[chatList.Count - 4]);
                temp.Add(chatList[chatList.Count - 3]);
                temp.Add(chatList[chatList.Count - 2]);
                temp.Add(chatList[chatList.Count - 1]);
            }
            else
            {
                temp = chatList;
            }

            foreach (string text in temp)
            {
                App.sendAction(App.encapsulatePacket(drawText(text, 12 + counter, 0, 40)));
                counter++;
            }

            drawResistance(resistance);
            App.sendAction(App.encapsulatePacket(EngineInteraction.swapPanel(nodeID)));
        }

        public static void drawResistance(int value)
        {
            if (value > 7)
            {
                value = 7;
            }
            for (int i = 0; i < value; i++)
            {
                App.sendAction(App.encapsulatePacket(drawResistanceBlock(i)));
            }

        }

        public static void addText(string text) {
            TimeSpan elapsedTime = App.stopwatch.Elapsed;
            string time2 = "" + elapsedTime.Minutes.ToString("00") + ":" + elapsedTime.Seconds.ToString("00") + " ";
            chatList.Add(time2 + text);
        }

        public static object drawText(string text, int row, int column, int size)
        {
            int s = 32;
            return new
            {
                id = "scene/panel/drawtext",
                data = new
                {
                    id = nodeID,
                    text = text,
                    position = new[] { s * (column + 1.15), s * (row - 0.25) },
                    size = size,
                    color = new[] { 1, 1, 1, 1 }
                }
            };
        }

        public static object drawOutLines()
        {
            int s = 32;
            int sH = 16;
            return new
            {
                id = "scene/panel/drawlines",
                data = new
                {
                    id = nodeID,
                    width = 2,
                    lines = new[]
                    {
                        //Outer box
                        new [] { 0,0, 0,512, 1,1,1,1 },
                        new [] { 0,512, 512,512, 1,1,1,1 },
                        new [] { 0, 0, 512, 0, 1,1,1,1 },
                        new [] { 512, 0, 512, 512, 1,1,1,1 },

                        //Speed Box
                        new [] { s, s, s* 15, s, 1,1,1,1 },
                        new [] { s*15, s, s*15, s* 4 , 1,1,1,1 },
                        new [] { s, s*4, s*15, s* 4 , 1,1,1,1 },
                        new [] { s, s*4, s, s, 1,1,1,1 },

                        //Speed Text Box
                        new [] { s*4, s, s*4, s*2, 1,1,1,1 },
                        new [] { s, s*2, s*4, s*2, 1,1,1,1 },

                        //Resistance Box
                        new [] { s, s*5,s*15, s*5, 1,1,1,1 },
                        new [] { s*15, s*9,s*15, s*5, 1,1,1,1 },
                        new [] { s*15, s*9,s, s*9, 1,1,1,1 },
                        new [] { s, s*5,s, s*9, 1,1,1,1 },

                        //Resistance Text Box
                        new [] { s*6, s*5,s*6, s*6, 1,1,1,1 },
                        new [] { s, s*6,s*6, s*6, 1,1,1,1 },

                        //Resistance Value Box
                        new [] { s*1 + sH, s* 6 + sH, s* 14 + sH -3, s* 6 + sH, 1,1,1,1 },
                        new [] { s*14 + sH -3, s* 8 + sH, s* 14 + sH -3, s* 6 + sH, 1,1,1,1 },
                        new [] { s*14 + sH -3, s* 8 + sH, s* 1 + sH, s* 8 + sH, 1,1,1,1 },
                        new [] { s*1 + sH, s* 6 + sH, s* 1 + sH, s* 8 + sH, 1,1,1,1 },

                        //Chat Box
                        new [] { s,s*10,s*15,s*10, 1,1,1,1 },
                        new [] { s*15,s*15,s*15,s*10, 1,1,1,1 },
                        new [] { s*15,s*15,s,s*15, 1,1,1,1 },
                        new [] { s,s*10,s,s*15, 1,1,1,1 },

                        //Chat Text Box
                        new [] { s*3 + s/2, s*11, s*3 + s/2, s*10, 1,1,1,1 },
                        new [] { s*3 + s/2, s*11, s, s*11, 1,1,1,1 },




                        /*
                        TODO:
                        [ 0,0, 10,10, 0,0,0,1, // x1,y1, x2,y2, r,g,b,a ],
                        [0, 0, 100, 10, 0, 0, 0, 1, // x1,y1, x2,y2, r,g,b,a ]
                        */
                    }
                }
            };
        }

        public static object drawResistanceBlock(int value)
        {
            int s = 32;
            float x = 13 * 32 / 7;

            return new
            {
                id = "scene/panel/drawlines",
                data = new
                {
                    id = nodeID,
                    width = 8,
                    lines = new[]
                    {
                        new [] {
                            1.5 * s + value*x,
                            6.625 * s,
                            1.5 * s + (value + 1) * x,
                            6.625 *s,
                            value /7f,(7-value)/7f,0,1
                        },
                        new [] {
                            1.5 * s + value*x,
                            6.875 * s,
                            1.5 * s + (value + 1) * x,
                            6.875 *s,
                            value /7f,(7-value)/7f,0,1
                        },
                        new [] {
                            1.5 * s + value*x,
                            7.125 * s,
                            1.5 * s + (value + 1) * x,
                            7.125 *s,
                            value /7f,(7-value)/7f,0,1
                        },
                        new [] {
                            1.5 * s + value*x,
                            7.375 * s,
                            1.5 * s + (value + 1) * x,
                            7.375 *s,
                            value /7f,(7-value)/7f,0,1
                        },
                        new [] {
                            1.5 * s + value*x,
                            7.625 * s,
                            1.5 * s + (value + 1) * x,
                            7.625 *s,
                            value /7f,(7-value)/7f,0,1
                        },
                        new [] {
                            1.5 * s + value*x,
                            7.875 * s,
                            1.5 * s + (value + 1) * x,
                            7.875 *s,
                            value /7f,(7-value)/7f,0,1
                        },
                        new [] {
                            1.5 * s + value*x,
                            8.125 * s,
                            1.5 * s + (value + 1) * x,
                            8.125 *s,
                            value /7f,(7-value)/7f,0,1
                        },
                        new [] {
                            1.5 * s + value*x,
                            8.375 * s,
                            1.5 * s + (value + 1) * x,
                            8.375 *s,
                            value /7f,(7-value)/7f,0,1
                        }
                        /*
                        [0, 0, 100, 10, 0, 0, 0, 1, // x1,y1, x2,y2, r,g,b,a ]
                        */
                    }
                }
            };
        }

    }
}