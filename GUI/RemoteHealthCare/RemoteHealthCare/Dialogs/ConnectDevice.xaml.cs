using Avans.TI.BLE;
using MahApps.Metro.Controls;
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
using System.Windows.Threading;

namespace RemoteHealthCare.Dialogs
{
    /// <summary>
    /// Interaction logic for ConnectDevice.xaml
    /// </summary>
    public partial class ConnectDevice
    {
        private BLE connectionLister;
        private List<string> foundConnections;
        public event EventHandler DeviceClicked;

        public ConnectDevice()
        {
            InitializeComponent();
            connectionLister = new BLE();
            foundConnections = new List<string>();
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += OnRefresh;
            timer.Start();
            lbConnections.MouseDoubleClick += OnDeviceTryConnect;
        }

        private void OnRefresh(object sender, ElapsedEventArgs e)
        {
#if !SIM
            List<string> foundDevices = connectionLister.ListDevices();
#else
            List<string> foundDevices = Simulator.Simulator.Instance.ListDevices();
#endif
            for (int i = 0; i < foundDevices.Count; i++)
            {
                string deviceName = foundDevices[i];
                if (!foundConnections.Contains(deviceName))
                {
                    if (deviceName.Contains("Tacx Flux"))
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

        private void OnDeviceTryConnect(object sender, MouseButtonEventArgs e)
        {
            if (lbConnections.SelectedItem != null)
            {
                OnDeviceClicked((lbConnections.SelectedItem as UserControls.DeviceConnection).DeviceName);
            }
        }

        protected virtual void OnDeviceClicked(string deviceName)
        {
            this.DeviceClicked?.Invoke(deviceName, null);
        }
    }
}
