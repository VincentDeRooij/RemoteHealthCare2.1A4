﻿using Newtonsoft.Json;
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
            this.Write(JsonConvert.SerializeObject(data));
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
                    int read = networkStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    int packetLength = BitConverter.ToInt32(lengthBuffer, 0);

                    if (packetLength <= 0 || read <= 0)
                        break;

                    byte[] networkMessage = new byte[packetLength];
                    read = networkStream.Read(networkMessage, 0, networkMessage.Length);

                    if (read <= 0)
                        break;

                    lastMessage = networkMessage;
                    OnReceived?.Invoke(this, JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(networkMessage)));
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