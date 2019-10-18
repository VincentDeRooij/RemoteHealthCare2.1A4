#define MULTI_DEVICE

using LiveCharts;
using LiveCharts.Wpf;
using RemoteHealthCare.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemoteHealthCare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        List<IDevice> devices = new List<IDevice>();
        string authkey;

        public MainWindow(string authkey)
        {
            InitializeComponent();
            DataContext = this;
            this.authkey = authkey;
        }

        private void OnConnectToDevice(object sender, RoutedEventArgs e)
        {
            Dialogs.ConnectDevice wndConnect = new Dialogs.ConnectDevice();
            wndConnect.DeviceClicked += (x, y) =>
            {
#if SIM
                IDevice selectedDevice = Simulator.Simulator.Instance.OpenDevice((string)x);
#else
                IDevice selectedDevice = ((string)x).Contains("Tacx") ? new Devices.StationaryBike((string)x) : null;
#endif
                
                if (!devices.Contains(selectedDevice))
                {
                    devices.Add(selectedDevice);
                    StationaryBike bike = selectedDevice as StationaryBike;
                    lvDevices.Children.Add(new UserControls.StationaryBikeControl(ref bike));
                }
            };
            wndConnect.ShowDialog();
        }

        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {
            //String message = TXTBoxMessageSend.Text;
            //MainWindow.Person p = listPersons[ClientsListBox.SelectedIndex];
            //p.messages.Add(new MainWindow.Person.Message(true, message));
            //AddMessageToView(true, message);
            //TODO: Message to Server 

        }
    }
}
