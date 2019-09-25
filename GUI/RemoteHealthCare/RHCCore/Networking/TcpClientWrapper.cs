using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RHCCore.Networking
{
    public sealed class TcpClientWrapper
    {
        private TcpClient client;
        private bool active;

        private NetworkConnection networkConnection;
        public NetworkConnection NetworkConnection => networkConnection;

        public delegate void ClientConnectionHandler(IConnection connection, dynamic args);
        public event ClientConnectionHandler OnClientConnected;
        public event ClientConnectionHandler OnClientDisconnected;
        public event ClientConnectionHandler OnClientForwarded;
        public event ClientConnectionHandler OnReceived;
        public event ClientConnectionHandler OnError;

        public TcpClientWrapper()
        {
            this.active = false;
        }

        public bool Connect(IPEndPoint endPoint)
        {
            if (!active)
            {
                try
                {
                    this.client = new TcpClient();
                    this.client.Connect(endPoint);
                    NetworkStream networkStream = this.client.GetStream();
                    this.networkConnection = new NetworkConnection();
                    this.networkConnection.OnSuccessfulConnection   += (x, y) => OnClientConnected?.Invoke(x, y);
                    this.networkConnection.OnError                  += (x, y) => OnError?.Invoke(x, y);
                    this.networkConnection.OnReceived               += (x, y) => OnReceived?.Invoke(x, y);
                    this.networkConnection.OnDisconnected           += (x, y) => OnClientDisconnected?.Invoke(x, y);

                    bool authrequested = false;
                    ManualResetEvent resetEvent = new ManualResetEvent(false);
                    this.networkConnection.OnReceived += (x, y) =>
                    {
                        if (Encoding.UTF8.GetString(y) == "AUTH")
                        {
                            x.Write(new byte[] { 0x1, 0x32, 0x11, 0x42, 0x11, 0x11, 0x9, 0x29 });
                            authrequested = true;
                            resetEvent.Set();
                        }
                    };
                    this.networkConnection.Init(ref networkStream, (IPEndPoint)client.Client.RemoteEndPoint);
                    try
                    {
                        resetEvent.WaitOne();
                        if (authrequested)
                        {
                            resetEvent.Reset();
                            this.networkConnection.OnReceived += (x, y) =>
                            {
                                if (Encoding.UTF8.GetString(y) == "AUTH-OK")
                                {
                                    active = true;
                                    resetEvent.Set();
                                }
                            };
                            resetEvent.WaitOne();
                        }
                        else
                            this.Disconnect();
                    }
                    catch (AbandonedMutexException e)
                    {

                    }                    
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
                    this.networkConnection?.Shutdown();
                    this.client?.Close();
                    this.networkConnection = null;
                    this.client = null;
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
