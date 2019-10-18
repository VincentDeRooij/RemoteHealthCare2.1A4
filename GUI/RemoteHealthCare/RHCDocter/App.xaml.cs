using RHCCore.Networking;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace RHCDocter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static TcpClientWrapper TcpClientWrapper;

        public App()
        {
            TcpClientWrapper = new TcpClientWrapper();
            TcpClientWrapper.Connect(new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));
        }
    }
}
