using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace RHCCore.Networking
{
    public interface IConnection : IDisposable
    {
        delegate void ConnectionEventHandler(IConnection sender, dynamic args);
        event ConnectionEventHandler OnReceived;
        event ConnectionEventHandler OnSuccessfulConnection;
        event ConnectionEventHandler OnDisconnected;
        event ConnectionEventHandler OnError;
        byte[] LastMessage { get; }
        IPEndPoint RemoteEndPoint { get; }
        bool Init(ref SslStream networkStream, IPEndPoint remoteEndPoint);
        void Write(byte[] data);
        void Write(string data);
        void Write(dynamic data);
        void Shutdown();
    }
}
