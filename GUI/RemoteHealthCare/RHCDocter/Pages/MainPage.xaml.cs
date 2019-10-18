using Newtonsoft.Json.Linq;
using RHCCore.Networking.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace RHCDocter.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private List<Person> listPersons; //TODO: get docter his clients 
        //private List<MainWindow.Session> listSession; //TODO: get archived sessions 
        private bool userOnline; //aka currentSelectedUserIsOnline 
        public List<SessionWindow> activeSessionWindows { get; }
        public static List<Person> persons { get; set; }

        public MainPage()
        {
            InitializeComponent();
            InitSettings();
            activeSessionWindows = new List<SessionWindow>();
            listPersons = new List<Person>();
            App.TcpClientWrapper.OnReceived += OnReceived;
            App.TcpClientWrapper.NetworkConnection.Write(new
            {
                Command = "clients/get",
            });

            System.Timers.Timer timer = new System.Timers.Timer(250);
            timer.Elapsed += (x, y) =>
            {
                App.TcpClientWrapper.NetworkConnection.Write(new
                {
                    Command = "clients/get"
                });
            };
            timer.Start();
        }

        private void OnReceived(RHCCore.Networking.IConnection connection, dynamic args)
        {
            string command = args.Command;
            if (command == "clients/list")
            {
                List<dynamic> persons = (args.Data as JArray).ToObject<List<dynamic>>();
                foreach (var item in persons)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Person p = (item.Person as JObject).ToObject<Person>();
                        if (!(listPersons.Where(x => x.Username == p.Username).Count() > 0) && !p.IsDoctor)
                        {
                            AddPersonToView(p.Name);
                            listPersons.Add(p);
                        }
                    });
                }
            }
        }

        private void InitSettings()
        {
            ClientsListBox.SelectionMode = SelectionMode.Single;
            ArchivedSessionsListBox.SelectionMode = SelectionMode.Single;

            //AddMessageToView(true, "DokterMessage");
            //AddMessageToView(false, "ClientMSG");

            if (ClientsListBox.SelectedIndex < 0)
            {
                BTNCreate.IsEnabled = false;
            }

            if (ArchivedSessionsListBox.SelectedIndex < 0)
            {
                BTNConfirm.IsEnabled = false;
            }
        }

        private void generatePersons()
        {
            ////Generate static persons 
            //listPersons = new List<MainWindow.Person>()
            //{
            //    new MainWindow.Person("Jaap", "BSN01234567"),
            //    new MainWindow.Person("Piet", "BSN12345678"),
            //    new MainWindow.Person("Peter", "BSN23456789")
            //};
            ////Add archived sessions 
            //listPersons[0].archivedSessions.Add(new MainWindow.Session("SessionName1", 27, true));
            //listPersons[0].archivedSessions.Add(new MainWindow.Session("SessionName2", 27, true));
            //listPersons[1].archivedSessions.Add(new MainWindow.Session("SessionName3", 27, true));
            //listPersons[1].archivedSessions.Add(new MainWindow.Session("SessionName4", 27, true));
            ////Add messages  
            //listPersons[0].messages.Add(new MainWindow.Person.Message(true, "Hallo Jaap"));
            //listPersons[0].messages.Add(new MainWindow.Person.Message(false, "Hallo Pannenkoek"));
            //listPersons[0].messages.Add(new MainWindow.Person.Message(true, "HJB Mongool"));
            //listPersons[0].messages.Add(new MainWindow.Person.Message(true, "Sterf RN"));
            //listPersons[1].messages.Add(new MainWindow.Person.Message(true, "Ik start de sessie zo, dus ga maar op de fiets zitten"));
            //listPersons[1].messages.Add(new MainWindow.Person.Message(false, "Okay ik neem plaats"));
            //listPersons[1].messages.Add(new MainWindow.Person.Message(true, "Top! Dan start ik hem nu!"));


        }

        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {
            String message = TXTBoxMessageSend.Text;
            Person p = listPersons[ClientsListBox.SelectedIndex];
            p.Messages.Add(new ChatMessage(message, true));
            AddMessageToView(true, message);
            //TODO: Message to Server 

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
            else
            {

                //ClientsListBox.SelectedIndex
                Person p = listPersons[ClientsListBox.SelectedIndex];
                Session session = new Session(TXTBoxNameSession.Text, DateTime.Now, int.Parse(TXTBoxTimeSession.Text));
                SessionWindow sw = new SessionWindow(ref p, ref session);
                activeSessionWindows.Add(sw);
                sw.Show();

                TXTBoxNameSession.Text = "";
                TXTBoxTimeSession.Text = "";

                BTNCreate.IsEnabled = false;

                (new Thread(() =>
                {
                    bool isClosed = false;
                    Dispatcher.Invoke(() => { isClosed = sw.IsClosed; });
                    Console.Out.WriteLine("Started Session Window Closed ThreadListener");

                    while (!isClosed)
                    {
                        Dispatcher.Invoke(() => { isClosed = sw.IsClosed; });


                        Person selectedPerson = null;
                        Dispatcher.Invoke(() => { selectedPerson = listPersons[ClientsListBox.SelectedIndex]; });

                        foreach (SessionWindow s in activeSessionWindows)
                        {
                            if (selectedPerson.Equals(s.person))
                            {
                                Dispatcher.Invoke(() => { BTNCreate.IsEnabled = false; });
                            }
                            else
                            {
                                //Dispatcher.Invoke(() => { BTNCreate.IsEnabled = true; });
                            }
                        }
                    }
                    activeSessionWindows.Remove(sw);
                    if (activeSessionWindows.Count == 0)
                    {
                        Dispatcher.Invoke(() => { BTNCreate.IsEnabled = true; });
                    }

                })).Start();
            }
        }

        private void Button_Click_Confirm(object sender, RoutedEventArgs e)
        {
            int index = ArchivedSessionsListBox.SelectedIndex;
            Session archivedSession = listPersons[ClientsListBox.SelectedIndex].Sessions[index];
            Console.Out.WriteLine($"index: {index}");

            if (ClientsListBox.SelectedIndex < 0)
            {
                MessageBox.Show("Select a user");
            }
            else if (ArchivedSessionsListBox.SelectedIndex < 0)
            {
                MessageBox.Show("Select a archived session");
            }
            else
            {
                Person p = listPersons[ClientsListBox.SelectedIndex];
                SessionWindow sw = new SessionWindow(ref p, ref archivedSession);
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

            Person p = listPersons[ClientsListBox.SelectedIndex];

            //Archived Sessions reset 
            resetArchivedSessionsView();

            foreach (Session archivedSession in p.Sessions)
            {
                AddArchivedSessionToView($"{archivedSession.Name} - {archivedSession.StartDate}");
            }

            //ChatMessages 
            resetMessagesView();
            foreach (ChatMessage message in p.Messages)
            {
                AddMessageToView(message.IsDoctor, message.Message);
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
            if (text.Length >= 5)
            {
                TXTBoxTimeSession.Text = text.Substring(0, 4);
            }
            else
            {
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

        private void resetArchivedSessionsView()
        {
            ArchivedSessionsListBox.Items.Clear();
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

        private void resetMessagesView()
        {
            MessagesPanel.Children.Clear();
        }
    }
}
