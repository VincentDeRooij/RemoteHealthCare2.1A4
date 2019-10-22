using Newtonsoft.Json.Linq;
using RHCCore.Networking.Models;
using RHCDocter.Models;
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
        private List<Person> listPersons;
        public List<SessionManager> activeSessionWindows { get; }
        public List<PersonProxy> persons { get; set; }

        public MainPage()
        {
            InitializeComponent();
            InitSettings();
            activeSessionWindows = new List<SessionManager>();
            listPersons = new List<Person>();
            persons = new List<PersonProxy>();
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
                List<PersonProxy> personProxies = (args.Data as JArray).ToObject<List<PersonProxy>>();
                bool refreshedUser = false;
                foreach (var proxy in personProxies)
                {
                    if (proxy.Person.IsDoctor)
                        continue;

                    Dispatcher.Invoke(() =>
                    {
                        if (!persons.Any(x => x.Person.Name == proxy.Person.Name))
                        {
                            persons.Add(proxy);
                            AddPersonToView(proxy.Person.Name);
                        }
                        else if (persons.Any(x => x.Person.Username == proxy.Person.Username && x.IsOnline != proxy.IsOnline))
                        {
                            for (int i = 0; i < persons.Count; i++)
                            {
                                PersonProxy p = persons[i];
                                if (p.Person.Username == proxy.Person.Username)
                                {
                                    persons[i] = proxy;
                                    refreshedUser = true;
                                }
                            }
                        }
                    });
                }

                if (refreshedUser)
                {
                    Dispatcher.Invoke(() =>
                    {
                        int selectedIndex = ClientsListBox.SelectedIndex;

                        ClientsListBox.Items.Clear();
                        foreach (PersonProxy item in persons)
                        {
                            AddPersonToView(item.Person.Name);
                        }
                        ClientsListBox.SelectedIndex = selectedIndex;
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
                BTNCreate.IsEnabled = true;
            }

            if (ArchivedSessionsListBox.SelectedIndex < 0)
            {
                BTNConfirm.IsEnabled = false;
            }
        }

        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {
            String message = TXTBoxMessageSend.Text;
            Person p = persons[ClientsListBox.SelectedIndex].Person;
            p.Messages.Add(new ChatMessage(message, true));
            App.TcpClientWrapper.NetworkConnection.Write(new
            {
                Command = "chat/send",
                data = message,
                Key = persons[ClientsListBox.SelectedIndex].Key
            });
            AddMessageToView(true, message);
            TXTBoxMessageSend.Text = "";
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
                Person p = persons[ClientsListBox.SelectedIndex].Person;
                Session session = new Session(TXTBoxNameSession.Text, DateTime.Now, int.Parse(TXTBoxTimeSession.Text));
                SessionManager sw = new SessionManager(persons[ClientsListBox.SelectedIndex], session);
                sw.OnSessionDone += OnSessionManagerClosed;
                sw.Show();
            }
        }

        private void Button_Click_Confirm(object sender, RoutedEventArgs e)
        {
            int index = ArchivedSessionsListBox.SelectedIndex;
            string archivedSession = listPersons[ClientsListBox.SelectedIndex].Sessions[index];
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
                Person p = persons[ClientsListBox.SelectedIndex].Person;
                Session s = new Session("t", DateTime.Now, 60);
                SessionWindow sw = new SessionWindow(ref p, ref s, persons[ClientsListBox.SelectedIndex].Key);
                //sw.OnSessionDone += OnSessionManagerClosed;
                sw.Show();
            }
        }

        private void LB_ArchivedSessions_SelectChanged(object sender, RoutedEventArgs e)
        {
            int index = ArchivedSessionsListBox.SelectedIndex;
            if (index >= 0)
            {
                BTNConfirm.IsEnabled = true;
            }
        }

        private void CB_CreateOrArchive_Changed(object sender, RoutedEventArgs e)
        {
            switch (ComboBoxSelect.SelectedIndex)
            {
                case 0:
                    ArchivedSessionPanel.Visibility = Visibility.Hidden;
                    CreateSessionPanel.Visibility = Visibility.Visible;
                    pnlAstrand.Visibility = Visibility.Hidden;
                    break;
                case 1:
                    CreateSessionPanel.Visibility = Visibility.Hidden;
                    ArchivedSessionPanel.Visibility = Visibility.Visible;
                    pnlAstrand.Visibility = Visibility.Hidden;

                    TXTBoxNameSession.Text = "";
                    TXTBoxTimeSession.Text = "";
                    break;
                case 2:
                    pnlAstrand.Visibility = Visibility.Visible;
                    CreateSessionPanel.Visibility = Visibility.Hidden;
                    ArchivedSessionPanel.Visibility = Visibility.Hidden;
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
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

        private void ClientsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsListBox.Items.Count <= 0)
                return;

            PersonProxy proxy = persons[ClientsListBox.SelectedIndex];
            Person p = proxy.Person;

            if (proxy.IsOnline)
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

            resetArchivedSessionsView();

            foreach (string archivedSession in p.Sessions)
            {
                AddArchivedSessionToView($"{archivedSession}");
            }

            resetMessagesView();
            foreach (ChatMessage message in p.Messages)
            {
                AddMessageToView(message.IsDoctor, message.Message);
            }
        }

        private void OnCreateAstrand(object sender, RoutedEventArgs e)
        {
            if (ClientsListBox.SelectedIndex < 0)
                return;

            PersonProxy person = persons[ClientsListBox.SelectedIndex];
            if (person != null)
            {
                AstrandSession session = new AstrandSession(txtAstrandSessionName.Text, DateTime.Now, int.Parse(txtAstrandAge.Text), (bool)rbMale.IsChecked);
                SessionManager sm = new SessionManager(person, session);
                sm.OnSessionDone += OnSessionManagerClosed;
                sm.Show();
            }
        }

        private void OnSessionManagerClosed(SessionManager manager)
        {
            string sessionId = manager.SessionId;
            manager.Close();
            ArchivedWindow aw = new ArchivedWindow(sessionId, manager.IsAstrand);
            aw.Show();
        }
    }
}
