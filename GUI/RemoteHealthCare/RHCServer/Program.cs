using RHCCore.Networking;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RHCServer
{
    class Program
    {
        private static TcpServerWrapper server;

        static async Task Main(string[] args)
        {
            server = new TcpServerWrapper(new IPEndPoint(IPAddress.Any, 20000));
            await server.StartAsync();
            server.OnClientConnected += OnNewClient;
            server.OnClientDataReceived += OnDataReceived;
            server.OnClientDisconnected += OnClientDisconnected;
            server.OnClientError += OnClientError;

            while (true)
            {

            }
        }

        private static void OnClientError(IConnection client, dynamic args)
        {
            Console.WriteLine($"ERROR: {args.Message}");
        }

        private static void OnClientDisconnected(IConnection client, dynamic args)
        {
            Console.WriteLine($"CLIENT {client.RemoteEndPoint.Address} DISCONNECTED");
        }

        private static void OnDataReceived(IConnection client, dynamic args)
        {
            Console.WriteLine($"CLIENT {client.RemoteEndPoint.Address} SENT {Encoding.UTF8.GetString(args)}");
        }

        private static void OnNewClient(IConnection client, dynamic args)
        {
            Console.WriteLine($"CLIENT {client.RemoteEndPoint.Address} CONNECTED");
        }
    }
}
