using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace RHCCore.Networking
{
    public sealed class TcpClientWrapper
    {
        private TcpClient client;
        private bool active;
        private bool secure;

        private NetworkConnection networkConnection;
        public NetworkConnection NetworkConnection => networkConnection;

        public delegate void ClientConnectionHandler(IConnection connection, dynamic args);
        public event ClientConnectionHandler OnClientConnected;
        public event ClientConnectionHandler OnClientDisconnected;
        public event ClientConnectionHandler OnClientForwarded;
        public event ClientConnectionHandler OnReceived;
        public event ClientConnectionHandler OnError;

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
        }

        public TcpClientWrapper(bool secure = true)
        {
            this.active = false;
            this.secure = secure;
        }

        public bool Connect(IPEndPoint endPoint)
        {
            if (!active)
            {
                try
                {
                    this.client = new TcpClient();
                    this.client.Connect(endPoint);
                    NetworkStream clientNetworkStream = this.client.GetStream();
                    this.networkConnection = new NetworkConnection();
                    this.networkConnection.OnSuccessfulConnection += (x, y) => OnClientConnected?.Invoke(x, y);
                    this.networkConnection.OnError += (x, y) => OnError?.Invoke(x, y);
                    this.networkConnection.OnReceived += (x, y) => OnReceived?.Invoke(x, y);
                    this.networkConnection.OnDisconnected += (x, y) => OnClientDisconnected?.Invoke(x, y);

                    SslStream secureStream = null;
                    if (secure)
                    {
                        secureStream = new SslStream(clientNetworkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                        secureStream.AuthenticateAsClient("localhost");
                    }

                    Stream selectedStream = null;
                    if (secure)
                        selectedStream = secureStream;
                    else
                        selectedStream = clientNetworkStream;

                    this.active = this.networkConnection.Init(ref secureStream, (IPEndPoint)client.Client.RemoteEndPoint);                   
                }
                catch (Exception e)
                {
                    OnError?.Invoke(networkConnection, e);
                }
                finally
                {
                    if (!active)
                    {
                        this.networkConnection?.Shutdown();
                        this.client?.Close();
                        this.networkConnection = null;
                        this.client = null;
                    }
                }
            }
            return active;
        }

        public bool Forward(IPEndPoint newEndPoint)
        {
            if (active)
            {
                try
                {
                    Disconnect();
                    active = Connect(newEndPoint);
                    OnClientForwarded?.Invoke(networkConnection, null);
                }
                catch (Exception e)
                {
                    OnError?.Invoke(networkConnection, e);
                }
            }
            else
            {
                active = Connect(newEndPoint);
            }
            return active;
        }

        public void Disconnect()
        {
            if (active)
            {
                this.networkConnection?.Shutdown();
                this.client?.Close();
                this.networkConnection = null;
                this.client = null;
                active = false;
            }
        }
    }
}
