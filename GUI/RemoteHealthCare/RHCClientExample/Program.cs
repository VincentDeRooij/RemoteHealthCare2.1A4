using Pluralsight.Crypto;
using RHCCore.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RHCClientExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TcpClientWrapper tcpClientWrapper = new TcpClientWrapper();

            //Connecten naar de TcpServer met AUTH
            bool connected = tcpClientWrapper.Connect(new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));

            if (connected)
            {
                //Het verkrijgen van gelezen data van de server naar client
                tcpClientWrapper.NetworkConnection.OnReceived += NetworkConnection_OnReceived;
                tcpClientWrapper.OnError += (x, e) =>
                {
                    Exception ex = (Exception)e;
                    Console.WriteLine(ex.Message);
                };

                //Het schrijven van data naar de server d.m.v. bytes
                tcpClientWrapper.NetworkConnection.Write(Encoding.ASCII.GetBytes("HALLO"));

                //Het schrijven van een string naar de server(overloaded)
                tcpClientWrapper.NetworkConnection.Write("HALLO");

                //Zorgen dat het programma niet stopt met draaien
                Console.ReadKey();

                //Disconnecten van de client
                //tcpClientWrapper.Disconnect();

                //Start met luisteren naar een andere connectie, bijv. 127.0.0.1:20001 ipv 20000
                //tcpClientWrapper.Forward(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20001));
            }
        }

        private static void NetworkConnection_OnReceived(IConnection sender, dynamic args)
        {
            Console.WriteLine(Encoding.ASCII.GetBytes(args));
        }
    }
}
