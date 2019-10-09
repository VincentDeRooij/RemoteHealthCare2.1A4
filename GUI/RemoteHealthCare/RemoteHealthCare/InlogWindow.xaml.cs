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
        }

        private void LogInClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("BSN: " + bsnInput.Text + "\nPassWord: " + passwordInput.Password);
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
