using LiveCharts;
using LiveCharts.Configurations;
using RemoteHealthCare.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for StationaryBikeView.xaml
    /// </summary>
    public partial class StationaryBikeControl : UserControl, INotifyPropertyChanged
    {
        private StationaryBike stationaryBike;

        public StationaryBikeControl(ref StationaryBike bike)
        {
            InitializeComponent();
            this.lblDeviceName.Content = bike.DeviceName;
            this.stationaryBike = bike;
            this.stationaryBike.DeviceDataChanged += OnDeviceDataChanged;
            this.DataContext = this;
        }

        private void OnDeviceDataChanged(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            Dispatcher.InvokeAsync(() =>
            {
                chartRPM.Value = stationaryBike.RPM;
                lblDistance.Content = string.Format("{0} m", (int)(stationaryBike.Distance * 1000));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
