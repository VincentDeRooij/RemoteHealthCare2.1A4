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
    }
}
