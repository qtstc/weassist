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
using CitySafe.Resources;
using Parse;
using ScheduledLocationAgent.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CitySafe
{
    public partial class TrackingSettingsPage : PhoneApplicationPage
    {
        private TrackingSettings trackingSettings;

        public TrackingSettingsPage()
        {
            InitializeComponent();
            trackingSettings = new TrackingSettings();
            LoadUIData();
            TrackingSettingsPanel.DataContext = trackingSettings;
        }

        /// <summary>
        /// Load UI data from the server
        /// </summary>
        private async Task LoadUIData()
        {
            App.ShowProgressOverlay(AppResources.TrackingSetting_Loading);
            try
            {
               //Set the title
                TrackingTitle.Text = App.trackItemModel.user.Username;
                await trackingSettings.LoadSettings();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to initialize tracking settings to the server:\n");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.TrackingSetting_FailToSync);
            }
            App.HideProgressOverlay();
        }

        /// <summary>
        /// Save the UI data to the server.
        /// </summary>
        private async Task SaveUIData()
        {
            App.ShowProgressOverlay(AppResources.TrackingSetting_Saving);
            try
            {
                await trackingSettings.SaveSettings();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to save user settings to the server:\n");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.TrackingSetting_FailToSync);
            }
            App.HideProgressOverlay();
        }

        private async void Apply_Button_Click(object sender, RoutedEventArgs e)
        {
            await SaveUIData();
        }

        private async void Stop_Tracking_Button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowProgressOverlay(AppResources.TrackingSetting_RemovingUser);
            try
            {
                await trackingSettings.RemoveRelation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(AppResources.TrackingSetting_FailToSync);
            }
            NavigationService.Navigate(new Uri("/TrackPage.xaml", UriKind.Relative));
            NavigationService.RemoveBackEntry();
        }
    }
}