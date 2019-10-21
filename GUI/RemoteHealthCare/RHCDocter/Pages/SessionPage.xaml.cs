using RHCCore.Networking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using LiveCharts;
using LiveCharts.Configurations;

namespace RHCDocter.Pages
{
    /// <summary>
    /// Interaction logic for SessionPage.xaml
    /// </summary>
    public partial class SessionPage : Page
    {
        private Thread threadPB;
        private Person person;
        private Session session;
        private string key;

        public SessionPage(ref Person person_, ref Session session_, string key)
        {
            this.key = key;
            InitializeComponent();
            person = person_;
            session = session_;
            ProgressBar.Maximum = session.SessionDuration;
            SliderResistance.IsEnabled = !session.IsArchived;
            BTNStart.IsEnabled = false;
            threadPB = new Thread(HandleProgressBarThread);

            App.TcpClientWrapper.OnReceived += OnReceived;
        }

        private void OnReceived(RHCCore.Networking.IConnection connection, dynamic args)
        {
            string command = (string)args.Command;
            if (command == $"session/{session.SessionId}/ready")
            {
                Dispatcher.Invoke(() => BTNStart.IsEnabled = true);
            }

            if (command == $"session/{session.SessionId}/updated")
            {
                Console.WriteLine(args);
            }
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            threadPB.Start();
            BTNStart.IsEnabled = false;
            App.TcpClientWrapper.NetworkConnection.Write(new
            {
                Command = "session/start",
                Data = new
                {
                    SessionId = session.SessionId,
                    Key = key
                }
            });
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            threadPB.Abort();
            BTNStart.IsEnabled = false;
            BTNStop.IsEnabled = false;
            App.TcpClientWrapper.NetworkConnection.Write(new
            {
                Command = "session/stop",
                Data = new
                {
                    SessionId = session.SessionId
                }
            });
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;

            double value = slider.Value;
            Console.Out.WriteLine($"Slider Value: {value}");
            App.TcpClientWrapper.NetworkConnection.Write(new
            {
                Command = "resistance/send",
                data = value,
                Key = key

            });
        }

        private void setSlider_Value(double value)
        {
            SliderResistance.Value = value;
        }


        public void HandleProgressBarThread()
        {
            Console.Out.WriteLine($"Time to progress: {session.SessionDuration}");

            for (int i = 0; i < session.SessionDuration; i++)
            {
                Dispatcher.Invoke(() => { ProgressBar.Value++; });
                Thread.Sleep(1000);
            }
            
            Dispatcher.Invoke(() => { BTNStop.IsEnabled = false; });
        }
    }
}
