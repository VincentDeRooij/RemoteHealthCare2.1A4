using System;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Collections.Generic;

namespace TcpClient
{
    class Program
    {
        public static System.Net.Sockets.TcpClient client;
        public static NetworkStream stream;
        public static string sessionId;
        public static string tunnelId;
        public static string routeUuid;
        public static string steveUuid;
        private static IFormatProvider result;
        public static String sceneJson;

        public static void Main(string[] args)
        {
            client = new System.Net.Sockets.TcpClient("145.48.6.10", 6666);
            stream = client.GetStream();

            Thread listenThread = new Thread(ListenThread);
            listenThread.Start();

            sendAction(getSessions());
            while (true)
            {
                if (sessionId != null)
                {
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
                Console.WriteLine(json);

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
                    else if (deserialized.id == "scene/get")
                    {
                        sceneJson = deserialized.data;
                        Console.WriteLine(sceneJson);
                    }
                    else if (deserialized.id == "scene/reset")
                    {
                        //TODO * 
                        Console.WriteLine("Invoked TODO ListenThread()>'(deserialized.id == 'scene/reset'");
                    }
                    else if (deserialized.id == "scene/save")
                    {
                        //TODO * 
                        Console.WriteLine("Invoked TODO ListenThread()>'(deserialized.id == 'scene/save'");
                    }
                    else if (deserialized.id == "scene/load")
                    {
                        //TODO * 
                        Console.WriteLine("Invoked TODO ListenThread()>'(deserialized.id == 'scene/load'");
                    }
                    else if (deserialized.id == "scene/raycast")
                    {
                        //TODO * 
                        Console.WriteLine("Invoked TODO ListenThread()>'(deserialized.id == 'scene/raycast'");
                    }
                    /*TODO: 
                    - node add 
                    - update node 
                    - moveto node 
                    - delete node 
                    - find node
                    - add layer to node 
                    - dellayer

                    - clear panel 
                    - swap panel 
                    - drawlines panel 
                    - set clear color panel 
                    - drawtext panel 
                    - image panel 
                    */


                    else if (deserialized.data.data.id == "route/add")
                    {
                        routeUuid = deserialized.data.data.data.uuid;
                        Console.WriteLine("Route uuid: " + routeUuid);
                    }
                    else if (deserialized.data.data.id == "scene/node/add")
                    {
                        steveUuid = deserialized.data.data.data.uuid;
                        Console.WriteLine("Steve uuid: " + steveUuid);
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
                    Console.WriteLine("Insert scale");
                    double scale = double.Parse(Console.ReadLine());
                    Console.WriteLine("Insert x");
                    double x = double.Parse(Console.ReadLine());
                    Console.WriteLine("Insert y");
                    double y = double.Parse(Console.ReadLine());
                    Console.WriteLine("Insert z");
                    double z = double.Parse(Console.ReadLine());
                    json = encapsulatePacket(EngineInteraction.addEbicMinecraftSteve(scale, x, y, z));
                    break;
                case 'l':
                    json = encapsulatePacket(EngineInteraction.removeLastSteve(steveUuid));
                    Console.WriteLine(json);
                    break;
                case 'm':
                    json = encapsulatePacket(EngineInteraction.followRoute(routeUuid,steveUuid));
                    Console.WriteLine(json);
                    break;
                case 'n':
                    json = encapsulatePacket(EngineInteraction.resetScene());
                    Console.WriteLine(json);
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
            Console.WriteLine("A: Change Skybox Time");
            Console.WriteLine("B: Add flat terrain");
            Console.WriteLine("C: Add random height terrain");
            Console.WriteLine("D: Delete terrain");
            Console.WriteLine("E: Create terrain node");
            Console.WriteLine("H: Add new route");
            Console.WriteLine("I: Create route nodes");
            Console.WriteLine("J: Debug/show current route");
            Console.WriteLine("K: Add epic minecraft steve!");
            Console.WriteLine("L: Remove epic minecraft steve:(");
            Console.WriteLine("M: Make Steve follow the route:(");
            Console.WriteLine("N: Reset scene:(");

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

        public static object drawLinesPanel(string nodeID, int width_)
        {
            return new
            {
                id = "scene/panel/drawlines",
                data = new
                {
                    id = nodeID,
                    width = width_,
                    lines = new
                    {
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

        public static object drawTextPanel(string nodeID, string text_, double[] positionXY, double size_, int[] colorsARGB)
        {
            return new
            {
                id = "scene/panel/drawtext",
                data = new
                {
                    id = nodeID, 
                    text = text_, 
                    position = positionXY, 
                    size = size_, 
                    color = colorsARGB
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
                        dir = new[] { t2,0,t2 } }
                    }
                }
            };
        }

        public static object followRoute(string uuid, string nodeid) // makes a node follow a route
        {
            return new
            {
                id = "route/follow",
                data = new
                {
                    route = uuid, // route id
                    node = nodeid, // this can be any value?
                    speed = 10.0, // the speed of the node
                    offset = 0.0, // the offset of the node, 0.0 means the node moves exactly one the line other values mean its off.
                    rotate = "NONE", // can be set to NONE, XZ or XYZ
                    smoothing = 1.0, // how smooth the node moves on the route?
                    followheigth = true, //set bool to follow the terrain height
                    rotateOffset = new[] { 0, 0, 0 },
                    positionOffset = new[] { 0, 0, 0 }
                }
            };
        }

        public static object updateFollowRouteSpeed(string nodeid, double newSpeed) // changes a given node speed
        {
            return new
            {
                id = "route/follow/speed",
                data = new
                {
                    node = nodeid, // the value of the given node
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
        public static object addEbicMinecraftSteve(double scale, double x, double y, double z)
        {
            return new
            {
                id = "scene/node/add",
                data = new
                {
                    name = "Steve",
                    components = new
                    {
                        transform = new
                        {
                            position = new[] { x, y, z },
                            rotation = new[] { 0, 0, 0 },
                            scale = scale
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

        public static object removeLastSteve(string uuid)
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

        public static object resetScene()
        {
            return new
            {
                id = "scene/reset"
            };
        }
        #endregion

    }
}
