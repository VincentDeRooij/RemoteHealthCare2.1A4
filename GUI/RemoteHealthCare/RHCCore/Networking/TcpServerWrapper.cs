using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RHCCore.Networking
{
    public sealed class TcpServerWrapper
    {
        private TcpListener listener;
        private CancellationTokenSource runtimeToken;
        private X509Certificate2 certificate;

        private IList<IConnection> connectedClients;
        public IEnumerable<IConnection> Connections => connectedClients;

        public delegate void ServerConnectionHandler(IConnection client, dynamic args);
        public event ServerConnectionHandler OnClientConnected;
        public event ServerConnectionHandler OnClientDisconnected;
        public event ServerConnectionHandler OnClientDataReceived;
        public event ServerConnectionHandler OnClientError;

        private bool secure;

        public TcpServerWrapper(IPEndPoint endPoint, bool secure = true)
        {
            this.listener = new TcpListener(endPoint);
            this.connectedClients = new List<IConnection>();
            this.secure = secure;
            if (secure)
                certificate = new X509Certificate2("Networking/cert.pfx", "hallo");
        }

        public async Task<bool> StartAsync()
        {
            if (runtimeToken == null)
            {
                runtimeToken = new CancellationTokenSource();
                var t = Task.Run(AcceptClients);
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
                NetworkConnection connection = new NetworkConnection();
                Stream selectedStream = clientStream;

                bool failed = false;

                SslStream secureStream = null;
                try
                {
                    if (secure)
                    {
                        secureStream = new SslStream(clientStream, false);
                        selectedStream = secureStream;
                        secureStream.AuthenticateAsServer(certificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                    }
                    failed = false;
                }
                catch (AuthenticationException e)
                {
                    client.Close();
                    failed = true;
                }

                if (!failed)
                {
                    if (connection.Init(ref secureStream, (IPEndPoint)client.Client.RemoteEndPoint))
                    {
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
    }
}
