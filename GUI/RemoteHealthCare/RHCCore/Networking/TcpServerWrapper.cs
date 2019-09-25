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
                NetworkConnection connection = new NetworkConnection(ref clientStream, (IPEndPoint)client.Client.RemoteEndPoint);
                connection.OnDisconnected       += (x, y) => OnClientDisconnected?.Invoke(x, y);
                connection.OnError              += (x, y) => OnClientError?.Invoke(x, y);
                connection.OnReceived           += (x, y) => OnClientDataReceived?.Invoke(x, y);
                OnClientConnected?.Invoke(connection, null);
                connectedClients.Add(connection);
            }
            listener.Stop();
        }
    }
}
