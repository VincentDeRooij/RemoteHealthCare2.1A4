using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemoteHealthCare.UserControls
{
    /// <summary>
    /// Interaction logic for DeviceConnection.xaml
    /// </summary>
    public partial class DeviceConnection : UserControl
    {
        private string deviceName;
        public string DeviceName => deviceName;

        public DeviceConnection(string deviceName)
        {
            InitializeComponent();
            this.lblDeviceName.Content = deviceName;
            this.deviceName = deviceName;
        }
    }
}
