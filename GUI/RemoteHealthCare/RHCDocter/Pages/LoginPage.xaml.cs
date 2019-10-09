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
using RHCDocter.Pages;

namespace RHCDocter.pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();

        }

        private void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Button Login Clicked");

            //usernameTXTBox.Text.Equals(null) || usernameTXTBox.Text.Equals("") || usernameTXTBox.Text.Contains(" ") || (usernameTXTBox.Text?.Equals("") ? true : false)  
            if (String.IsNullOrEmpty(usernameTXTBox.Text) || String.IsNullOrWhiteSpace(usernameTXTBox.Text) || usernameTXTBox.Text.Contains(" "))
            {
                
                System.Windows.MessageBox.Show("Please fill in a username without spaces.");

                
            }
            else if (String.IsNullOrEmpty(passwordBox.Password) || String.IsNullOrWhiteSpace(passwordBox.Password) || passwordBox.Password.Contains(" "))
            {
                System.Windows.MessageBox.Show("Please fill in a password without spaces.");
            }
            else
            {
                Console.WriteLine($"Login Credentials: {usernameTXTBox.Text} : {passwordBox.Password}");
                //TODO: Login Check 
                MainPage mainPage = new MainPage();

                //Window.GetWindow(this)
                //this.ShowsNavigationUI.
                this.NavigationService.Navigate(mainPage);
            }

        }
    }
}
