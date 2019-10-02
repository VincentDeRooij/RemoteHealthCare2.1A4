using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RHCCore.Networking
{
    public class NetworkConnection : IConnection
    {
        private SslStream networkStream;
        private object networkLock;

        private byte[] lastMessage;
        public byte[] LastMessage => lastMessage;

        private bool active = false;

        private IPEndPoint remoteEndPoint;
        public IPEndPoint RemoteEndPoint => remoteEndPoint;

        public event IConnection.ConnectionEventHandler OnReceived;
        public event IConnection.ConnectionEventHandler OnSuccessfulConnection;
        public event IConnection.ConnectionEventHandler OnDisconnected;
        public event IConnection.ConnectionEventHandler OnError;

        public bool Init(ref SslStream networkStream, IPEndPoint remoteEndPoint)
        {
            try
            {
                this.networkLock = new object();
                this.networkStream = networkStream;
                active = true;
                new Thread(Receive).Start();
                this.remoteEndPoint = remoteEndPoint;
                OnSuccessfulConnection?.Invoke(this, null);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return active;
        }

        public void Write(string data)
        {
            this.Write(Encoding.ASCII.GetBytes(data));
        }

        public void Write(dynamic data)
        {
            this.Write(data);
        }

        public void Write(byte[] data)
        {
            try
            {
                byte[] messageLength = BitConverter.GetBytes(data.Length);
                networkStream.Write(messageLength, 0, messageLength.Length);
                networkStream.Write(data, 0, data.Length);
            }                               
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                active = false;
            }
        }

        private void Receive()
        {
            try
            {
                while (active)
                {
                    byte[] lengthBuffer = new byte[4];
                    networkStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    int receivingByteSize = BitConverter.ToInt32(lengthBuffer, 0);

                    if (receivingByteSize <= 0)
                        break;

                    Thread.Sleep(100);

                    byte[] networkMessage = new byte[receivingByteSize];
                    networkStream.Read(networkMessage, 0, networkMessage.Length);

                    lastMessage = networkMessage;
                    OnReceived?.Invoke(this, networkMessage);
                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(this, e);
            }
            finally
            {
                OnDisconnected?.Invoke(this, null);
            }
        }

        private void WriteConfirmation()
        {
            byte[] receiving = new byte[] { 0 };
            networkStream.Write(receiving, 0, receiving.Length);
        }

        private void AwaitConfirmation()
        {
            byte[] msg = new byte[1];
            networkStream.Read(msg, 0, msg.Length);
        }

        public void Shutdown()
        {
            if (active)
                active = false;
        }

        public void Dispose()
        {
            
        }
    }
}
