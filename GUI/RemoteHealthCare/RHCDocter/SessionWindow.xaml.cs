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
using RHCCore.Networking.Models;
using RHCDocter.Pages;

namespace RHCDocter
{
    /// <summary>
    /// Interaction logic for SessionWindow.xaml
    /// </summary>
    public partial class SessionWindow : Window
    {
        public bool IsClosed { get; private set; }
        public Person person { get; set; }
        public string key { get; set; }
        private Session session;

        public SessionWindow(ref Person person_, ref Session session_, string key)
        {
            this.session = session_;
            this.key = key;
            InitializeComponent();
            person = person_;
            Title = $"Session '{person_.Name} : {person_.Username}' at {session_.StartDate} Session";
            App.TcpClientWrapper.OnReceived += OnReceived;

            SessionPage sessionPage = new SessionPage(ref person_, ref session_,this.key);
            SessionMainFrame.Navigate(sessionPage);

            SessionMainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;

            SessionMainFrame.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, OnBrowseForward));
            SessionMainFrame.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, OnBrowseBack));

            void OnBrowseForward(object sender, ExecutedRoutedEventArgs args)
            {
                Console.WriteLine($"Forward Mainframe");
            }

            void OnBrowseBack(object sender, ExecutedRoutedEventArgs args)
            {
                Console.WriteLine($"Back Mainframe");
            }
        }

        private void OnReceived(RHCCore.Networking.IConnection connection, dynamic args)
        {
            string command = (string)args.Command;
            if (command == $"session/{session.SessionId}/done")
            {
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }
    }
}
