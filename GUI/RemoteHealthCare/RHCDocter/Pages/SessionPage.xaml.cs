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
        private MainWindow.Person person;
        private MainWindow.Session session;

        public SessionPage(ref MainWindow.Person person_, ref MainWindow.Session session_)
        {
            InitializeComponent();
            person = person_;
            session = session_;
            ProgressBar.Maximum = session.sessionDurationTime;
            SliderResistance.IsEnabled = !session.isArchived;
            threadPB = new Thread(HandleProgressBarThread);
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            threadPB.Start();
            BTNStart.IsEnabled = false;
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            threadPB.Abort();
            BTNStart.IsEnabled = false;
            BTNStop.IsEnabled = false;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;

            double value = slider.Value;
            Console.Out.WriteLine($"Slider Value: {value}");
            //Set Window Title.
            //this.Title = "Value: " + value.ToString("0.0") + "/" + slider.Maximum;
        }

        //Double values 0.0-10 
        private void setSlider_Value(double value)
        {
            SliderResistance.Value = value;
        }


        public void HandleProgressBarThread()
        {
            Console.Out.WriteLine($"Time to progress: {session.sessionDurationTime}");

            for (int i = 0; i < session.sessionDurationTime; i++)
            {
                Dispatcher.Invoke(() => { ProgressBar.Value++; });
                //Dispatcher.Invoke(new Action(() => { ProgressBar.Value++; }));
                Thread.Sleep(1000);
            }
            
            Dispatcher.Invoke(() => { BTNStop.IsEnabled = false; });
            //TODO: When Session is done: 
            //
        }
    }
}
