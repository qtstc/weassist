using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CitySafe.ViewModels;
using System.Diagnostics;

namespace CitySafe
{
    public partial class TrackPage : PhoneApplicationPage
    {
        private TrackingViewModel trackingModel;
        public TrackPage()
        {
            InitializeComponent();
            trackingModel = new TrackingViewModel();
            TrackingList.DataContext = trackingModel;
            trackingModel.LoadData();
        }

        private void TrackingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected item is null (no selection) do nothing
            if (TrackingList.SelectedItem == null)
                return;

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/TrackingSettingsPage.xaml", UriKind.Relative));

            // Reset selected item to null (no selection)
            TrackingList.SelectedItem = null;
        }

    }
}