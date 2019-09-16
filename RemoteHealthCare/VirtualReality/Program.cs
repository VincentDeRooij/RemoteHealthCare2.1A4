using System;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

namespace TcpClient
{
    class Program
    {
        public static void Main(string[] args)
        {


            string id = "session/list";
            Message message = new Message();

            string json = JsonConvert.SerializeObject(message);
            Console.WriteLine(JsonConvert.SerializeObject(message));

            byte[] prependBytes = BitConverter.GetBytes(json.Length);
            byte[] databytes = System.Text.Encoding.UTF8.GetBytes(json);

            System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient("145.48.6.10", 6666);

            NetworkStream stream = client.GetStream();

            //stream.Write(prependBytes, 0, prependBytes.Length);
            //stream.Write(databytes, 0, databytes.Length);



            
            var henk = new {
                id = "tunnel/create",
                data = new {
                    session = "0f7ab30a-67fe-4ad5-8c13-72fa8bd9228f", key = "" },

            };

            var henkJson = JsonConvert.SerializeObject(henk);


            stream.Write(System.Text.Encoding.UTF8.GetBytes(henkJson),0, henkJson.Length);

            Thread.Sleep(1000);
            var messageBytes = new byte[2048];

            int receivedCount = stream.Read(messageBytes, 0, messageBytes.Length);

            int datalength = BitConverter.ToInt32(messageBytes, 0);

            Console.WriteLine(datalength);
            Console.WriteLine(receivedCount);
            foreach (byte b in messageBytes)
            {
                Console.WriteLine(b);
            }

            Console.WriteLine(System.Text.Encoding.UTF8.GetString(messageBytes, 4, receivedCount - 4));
            Console.ReadKey();

        }


        private static byte[] sendMessage(byte[] messageBytes)
        {
            const int bytesize = 1024;
            try
            {
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient("127.0.0.1", 1234); // Create a new connection
                NetworkStream stream = client.GetStream();

                stream.Write(messageBytes, 0, messageBytes.Length); // Write the bytes
                Console.WriteLine(@"
            ================================
            =   Connected to the server    =
            ================================

                Waiting for response...");

                messageBytes = new byte[bytesize];

                stream.Read(messageBytes, 0, messageBytes.Length);

                // Clean up
                stream.Dispose();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return messageBytes;
        }

    }

    class Message
    {

        public string id { get; set; }
        public Message()
        {
            this.id = "tunnel/create";
            var data = new { data = new { session = "0f7ab30a-67fe-4ad5-8c13-72fa8bd9228f", key = "" } };

        }

        public string delete()
            {
                var delete = new
                {
                    id ="scene/terain/delete",
                    data = new
                    {

                    }
                };
                return JsonConvert.SerializeObject(delete);
            }

    }

}
