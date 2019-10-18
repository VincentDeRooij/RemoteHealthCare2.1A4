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
using System.Windows.Shapes;

namespace RemoteHealthCare
{
    /// <summary>
    /// Interaction logic for InlogWindow.xaml
    /// </summary>
    public partial class InlogWindow : Window
    {
        public InlogWindow()
        {
            InitializeComponent();
            //App.serverClientWrapper.OnReceived += OnMessageReceived;
        }

        private void OnMessageReceived(RHCCore.Networking.IConnection connection, dynamic args)
        {
            Console.WriteLine(args);
            if (args.Command == "login/authenticated")
            {
                Dispatcher.Invoke(() =>
                {
                    MainWindow main = new MainWindow((string)args.Data.Key,"username");
                    main.Show();
                    this.Close(); 
                });
            }

            if (args.Command == "login/refused")
            {
                MessageBox.Show("Incorrect username or password", "Refused", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LogInClick(object sender, RoutedEventArgs e)
        {
            string username = bsnInput.Text;
            MainWindow main = new MainWindow("henk", "username");
            main.Show();
            this.Close();

            //App.serverClientWrapper.NetworkConnection.Write(new
            //{
            //    Command = "login/try",
            //    Data = new
            //    {
            //        Username = RHCCore.Security.Hashing.EncryptSHA256(bsnInput.Text),
            //        Password = RHCCore.Security.Hashing.EncryptSHA256(passwordInput.Password)
            //    }
            //});
        }
    }
}
