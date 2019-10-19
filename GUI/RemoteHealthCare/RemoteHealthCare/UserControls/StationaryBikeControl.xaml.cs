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
    public class MeasureModel
    {
        public DateTime DateTime { get; set; }
        public double Value { get; set; }
    }

    /// <summary>
    /// Interaction logic for StationaryBikeView.xaml
    /// </summary>
    public partial class StationaryBikeControl : UserControl, INotifyPropertyChanged
    {
        private StationaryBike stationaryBike;

        private double _axisMax;
        private double _axisMin;

        public ChartValues<MeasureModel> ChartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }

        public bool IsReading { get; set; }

        public StationaryBikeControl(ref StationaryBike bike)
        {
            InitializeComponent();
            this.stationaryBike = bike;
            this.stationaryBike.DeviceDataChanged += OnDeviceDataChanged;

            var mapper = Mappers.Xy<MeasureModel>()
                        .X(model => model.DateTime.Ticks)
                        .Y(model => model.Value);

            Charting.For<MeasureModel>(mapper);

            ChartValues = new ChartValues<MeasureModel>();

            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            AxisUnit = TimeSpan.TicksPerSecond;

            SetAxisLimits(DateTime.Now);
            IsReading = false;

            this.DataContext = this;
        }

        private void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks;
            AxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks;
        }

        private void OnDeviceDataChanged(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            ChartValues.Add(new MeasureModel
            {
                DateTime = now,
                Value = stationaryBike.Distance
            });

            SetAxisLimits(now);

            Dispatcher.InvokeAsync(() =>
            {
                lblDistance.Content = string.Format("{1} -> Distance: {0:0.##} KM", stationaryBike.Distance, stationaryBike.deviceName);
            });

            if (ChartValues.Count > 150) ChartValues.RemoveAt(0);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
