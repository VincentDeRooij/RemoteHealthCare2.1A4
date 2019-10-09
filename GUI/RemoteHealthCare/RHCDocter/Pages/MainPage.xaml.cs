using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace RHCDocter.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private List<MainWindow.Person> Persons; //TODO: get docter his clients 
        private List<MainWindow.Session> ArchivedSessions; //TODO: get archived sessions 

        public MainPage()
        {
            InitializeComponent();
            ClientsListBox.SelectionMode = SelectionMode.Single;
            ArchivedSessionsListBox.SelectionMode = SelectionMode.Single;
        }

        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_Create(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TXTBoxNameSession.Text) || String.IsNullOrWhiteSpace(TXTBoxNameSession.Text) || TXTBoxNameSession.Text.Contains(" "))
            {
                MessageBox.Show("Please fill in a name without spaces.");
            }
            else if (String.IsNullOrEmpty(TXTBoxTimeSession.Text))
            {
                MessageBox.Show("Please fill in a time.");
            }
        }

        private void Button_Click_Confirm(object sender, RoutedEventArgs e)
        {

        }

        private void LB_Clients_SelectChanged(object sender, RoutedEventArgs e)
        {
            int index = ClientsListBox.SelectedIndex;
            //Console.Out.WriteLine(index);

        }



        private void LB_ArchivedSessions_SelectChanged(object sender, RoutedEventArgs e)
        {
            int index = ArchivedSessionsListBox.SelectedIndex;
            //Console.Out.WriteLine(index);
        }

        private void CB_CreateOrArchive_Changed(object sender, RoutedEventArgs e)
        {
            switch (ComboBoxSelect.SelectedIndex)
            {
                case 0:
                    ArchivedSessionPanel.Visibility = Visibility.Hidden;
                    CreateSessionPanel.Visibility = Visibility.Visible;
                    break;
                case 1:
                    CreateSessionPanel.Visibility = Visibility.Hidden;
                    ArchivedSessionPanel.Visibility = Visibility.Visible;
                    TXTBoxNameSession.Text = "";
                    TXTBoxTimeSession.Text = "";
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
            //Console.Out.WriteLine(ComboBoxSelect.SelectedIndex);
            //Console.Out.WriteLine(ComboBoxSelect.Items[ComboBoxSelect.SelectedIndex].ToString());
        }

        private void TXTBoxTimeSession_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            String text = TXTBoxTimeSession.Text;
            //Console.Out.WriteLine($"length: {text.Length}");
            if (text.Length != 0)
            {
                string[] numbers = Regex.Split(text, @"\D+");

                string result = "";
                foreach (string value in numbers)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        //Console.Out.WriteLine($"value= {value}");
                        result += value;
                    }
                }
                Console.Out.WriteLine($"Result = {result}");
                TXTBoxTimeSession.Text = result;
                TXTBoxTimeSession.CaretIndex = result.Length;
            }
        }
    }
}
