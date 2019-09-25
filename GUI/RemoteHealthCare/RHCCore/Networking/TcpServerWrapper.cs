using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RHCCore.Networking
{
    public sealed class TcpServerWrapper
    {
        private TcpListener listener;
        private CancellationTokenSource runtimeToken;

        private IList<IConnection> connectedClients;
        public IEnumerable<IConnection> Connections => connectedClients;

        public delegate void ServerConnectionHandler(IConnection client, dynamic args);
        public event ServerConnectionHandler OnClientConnected;
        public event ServerConnectionHandler OnClientDisconnected;
        public event ServerConnectionHandler OnClientDataReceived;
        public event ServerConnectionHandler OnClientError;

        public TcpServerWrapper(IPEndPoint endPoint)
        {
            this.listener = new TcpListener(endPoint);
            this.connectedClients = new List<IConnection>();
        }

        public async Task<bool> StartAsync()
        {
            if (runtimeToken == null)
            {
                runtimeToken = new CancellationTokenSource();
                Task.Run(AcceptClients);
                return true;
            }
            return false;
        }

        public async Task<bool> ShutdownAsync()
        {
            if (runtimeToken != null)
            {
                runtimeToken.Cancel();
                for (int i = 0; i < connectedClients.Count; i++)
                    connectedClients[i].Shutdown();
                runtimeToken = null;
                return true;
            }
            return false;
        }

        private async void AcceptClients()
        {
            listener.Start();
            while (!runtimeToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                NetworkStream clientStream = client.GetStream();

                ManualResetEvent resetEvent = new ManualResetEvent(false);
                NetworkConnection connection = new NetworkConnection();
                if (connection.Init(ref clientStream, (IPEndPoint)client.Client.RemoteEndPoint))
                {
                    connection.Write("AUTH");
                    bool authenticated = false;
                    connection.OnReceived += (x, y) => AuthenticateUser(x, y, ref resetEvent, out authenticated);
                    resetEvent.WaitOne(1000);

                    //Clean up event
                    connection.OnReceived -= (x, y) => AuthenticateUser(x, y, ref resetEvent, out authenticated);

                    if (authenticated)
                    {
                        connection.Write(Encoding.UTF8.GetBytes("AUTH-OK"));
                        connection.OnDisconnected += (x, y) => OnClientDisconnected?.Invoke(x, y);
                        connection.OnError += (x, y) => OnClientError?.Invoke(x, y);
                        connection.OnReceived += (x, y) => OnClientDataReceived?.Invoke(x, y);
                        OnClientConnected?.Invoke(connection, null);
                        connectedClients.Add(connection);
                    }
                }
            }
            listener.Stop();
        }

        private void AuthenticateUser(IConnection connection, dynamic args, ref ManualResetEvent resetEvent, out bool authenticated)
        {
            try
            {
                byte[] authMessage = { 0x1, 0x32, 0x11, 0x42, 0x11, 0x11, 0x9, 0x29 };
                if (Encoding.UTF8.GetString(args) == Encoding.UTF8.GetString(authMessage))
                {
                    authenticated = true;
                    resetEvent.Set();
                }
                else authenticated = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                authenticated = false;
            }
        }
    }
}
