using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TestPhoneApp
{
    public partial class SOSPage : PhoneApplicationPage
    {
        public SOSPage()
        {
            InitializeComponent();
        }

        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Manage_List_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/TrackPage.xaml", UriKind.Relative));
        }
    }
}