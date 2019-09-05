using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RemoteHealthCare.Windows
{
    /// <summary>
    /// Interaction logic for ConnectDevice.xaml
    /// </summary>
    public partial class ConnectDevice
    {
        private BLE connectionLister;
        private List<string> foundConnections;

        public ConnectDevice()
        {
            InitializeComponent();
            connectionLister = new BLE();
            foundConnections = new List<string>();
            Timer timer = new Timer();
            timer.Interval = 50;
            timer.Elapsed += OnRefresh;
            timer.Start();
        }

        private void OnRefresh(object sender, ElapsedEventArgs e)
        {
#if !SIM
            List<string> foundDevices = connectionLister.ListDevices();
#else
            List<string> foundDevices = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                foundDevices.Add($"Tacx Flux {i}");
            }
#endif
            for (int i = 0; i < foundDevices.Count; i++)
            {
                string deviceName = foundDevices[i];
                if (!foundConnections.Contains(deviceName))
                {
                    foundConnections.Add(deviceName);
                    Dispatcher.Invoke(() =>
                    {
                        lbConnections.Items.Add(new UserControls.DeviceConnection(deviceName));
                    });
                }
            }
        }
    }
}
