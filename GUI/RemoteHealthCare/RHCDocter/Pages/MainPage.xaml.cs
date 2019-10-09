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
        private bool userOnline; //aka currentSelectedUserIsOnline 

        public MainPage()
        {
            InitializeComponent();
            InitSettings();
        }

        private void InitSettings()
        {
            ClientsListBox.SelectionMode = SelectionMode.Single;
            ArchivedSessionsListBox.SelectionMode = SelectionMode.Single;

            userOnline = false;
            //AddMessageToView(true, "DokterMessage");
            //AddMessageToView(false, "ClientMSG");

            if (ArchivedSessionsListBox.SelectedIndex < 0)
            {
                BTNConfirm.IsEnabled = false;
            }
        }

        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {
            String message = TXTBoxMessageSend.Text;

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
            int index = ArchivedSessionsListBox.SelectedIndex;
            Console.Out.WriteLine($"index: {index}");

            if (ClientsListBox.SelectedIndex < 0)
            {
                MessageBox.Show("Select a user");
            } else if (ArchivedSessionsListBox.SelectedIndex < 0)
            {
                MessageBox.Show("Select a archived session");
            }
            else
            {
                SessionWindow sw = new SessionWindow(ClientsListBox.SelectedItem.ToString().Remove(0, 37));
                sw.Show();
            }
        }

        private void LB_Clients_SelectChanged(object sender, RoutedEventArgs e)
        {
            int index = ClientsListBox.SelectedIndex;
            //TODO: check if user is online, and set bool 'userOnline' 
            //Console.Out.WriteLine(index);
            ClientUserName.Content = ClientsListBox.SelectedItem.ToString().Remove(0, 37);

            if (userOnline)
            {
                ClientUserStatus.Content = "Online";
                BTNSend.IsEnabled = true;
                BTNCreate.IsEnabled = true;
            }
            else
            {
                ClientUserStatus.Content = "Offline";
                BTNSend.IsEnabled = false;
                BTNCreate.IsEnabled = false;
            }
        }

        private void LB_ArchivedSessions_SelectChanged(object sender, RoutedEventArgs e)
        {
            int index = ArchivedSessionsListBox.SelectedIndex;
            if (index >= 0)
            {
                BTNConfirm.IsEnabled = true;
            }

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

        private void AddArchivedSessionToView(String sessionTitle)
        {
            //TODO: add GUI layout for archived session 
            ListBoxItem lbi = new ListBoxItem();
            lbi.HorizontalContentAlignment = HorizontalAlignment.Left;
            lbi.Height = 30;
            lbi.Width = 260;
            lbi.Background = Brushes.DarkGray;
            lbi.Padding = new Thickness(5);
            lbi.Content = sessionTitle;

            ArchivedSessionsListBox.Items.Add(lbi);
        }

        private void AddPersonToView(String clientUserName)
        {
            //TODO: add GUI layout per person 
            ListBoxItem lbi = new ListBoxItem();
            lbi.HorizontalContentAlignment = HorizontalAlignment.Left;
            lbi.Height = 30;
            lbi.Width = 260;
            lbi.Background = Brushes.LightGray;
            lbi.Padding = new Thickness(5);
            lbi.Content = clientUserName;

            ClientsListBox.Items.Add(lbi);
        }

        private void AddMessageToView(bool isDokterMessage, string message)
        {
            Label lbl = new Label();
            TextBlock txtb = new TextBlock();

            txtb.TextWrapping = System.Windows.TextWrapping.WrapWithOverflow;
            txtb.Padding = new Thickness(5);
            txtb.Text = message;

            lbl.MaxWidth = 250;
            lbl.HorizontalAlignment = HorizontalAlignment.Left;
            
            lbl.BorderThickness = new Thickness(1);
            lbl.BorderBrush = Brushes.DarkGray;
            lbl.Margin = new Thickness(0, 5, 0, 5);

            if (isDokterMessage)
            {
                lbl.HorizontalAlignment = HorizontalAlignment.Right;
                lbl.Background = Brushes.LightGray;
            }
            else
            {
                lbl.HorizontalAlignment = HorizontalAlignment.Left;
                lbl.Background = Brushes.GhostWhite;
            }
                
            lbl.Content = txtb;
            MessagesPanel.Children.Add(lbl);
        }
    }
}
