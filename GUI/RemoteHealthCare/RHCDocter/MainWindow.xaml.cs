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

namespace RHCDocter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;

            MainFrame.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, OnBrowseForward));
            MainFrame.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, OnBrowseBack));

            void OnBrowseForward(object sender, ExecutedRoutedEventArgs args)
            {
                Console.WriteLine($"Forward Mainframe");
                //Do Nothing 
            }
            void OnBrowseBack(object sender, ExecutedRoutedEventArgs args)
            {
                Console.WriteLine($"Back Mainframe");
                //Do Nothing 
            }
        }

        public class Person
        {
            public bool isOnline { get; set; }
            public String name { get; }
            public String BSN { get;  }
            public List<Session> archivedSessions { get; }
            public List<Message> messages { get; }

            public Person(string name_, string BSN_)
            {
                isOnline = true;
                name = name_;
                BSN = BSN_;
                archivedSessions = new List<Session>();
                messages = new List<Message>();
            }

            public class Message
            {
                public bool isDocter { get; }
                public String message { get; }

                public Message(bool isDoctor_, string message_)
                {
                    isDocter = isDoctor_;
                    message = message_;
                }
            }
        }

        public class Session
        {
            public bool isArchived { get; set; }
            public String name { get;  }
            public int sessionDurationTime { get; }
            public DateTime sessionDate { get; }

            public Session(string name_, int sessionTimeInSeconds)
            {
                name = name_;
                sessionDurationTime = sessionTimeInSeconds;
                isArchived = false;
                sessionDate = DateTime.Now;
            }

            public Session(string name_, int sessionTimeInSeconds, bool isArchived_)
            {
                name = name_;
                sessionDurationTime = sessionTimeInSeconds;
                isArchived = isArchived_;
                sessionDate = DateTime.Now;
            }

        }
    }
}
