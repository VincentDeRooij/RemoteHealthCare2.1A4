using System;
using System.Collections.Generic;
using System.Net;
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

        IPEndPoint RemoteEndPoint { get; }

        void Write(byte[] data);
        void Write(string data);
        void Shutdown();
    }
}
