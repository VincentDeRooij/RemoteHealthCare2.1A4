﻿using System;
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

namespace RHCDocter
{
    /// <summary>
    /// Interaction logic for SessionWindow.xaml
    /// </summary>
    public partial class SessionWindow : Window
    {
        public bool IsClosed { get; private set; }
        public MainWindow.Person person { get; set; }

        public SessionWindow(ref MainWindow.Person person_, ref MainWindow.Session session_)
        {
            InitializeComponent();
            person = person_;
            Title = $"Session '{person_.name} : {person_.BSN}' at {session_.sessionDate} Session";

            SessionPage sessionPage = new SessionPage(ref person_, ref session_);
            SessionMainFrame.Navigate(sessionPage);

            SessionMainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;

            SessionMainFrame.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, OnBrowseForward));
            SessionMainFrame.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, OnBrowseBack));

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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }
    }
}