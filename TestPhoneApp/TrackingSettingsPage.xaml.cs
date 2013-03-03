using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CitySafe.ViewModels;
using CitySafe.Resources;
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
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Setting_SyncingFailed);
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
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Setting_SyncingFailed);
            }
            App.HideProgressOverlay();
        }

        private async void Apply_Button_Click(object sender, EventArgs e)
        {
            await SaveUIData();
        }

        private async void Stop_Tracking_Button_Click(object sender, EventArgs e)
        {
            App.ShowProgressOverlay(AppResources.TrackingSetting_RemovingUser);
            try
            {
                await trackingSettings.RemoveRelation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(AppResources.Setting_SyncingFailed);
            }
            NavigationService.Navigate(new Uri("/TrackPage.xaml", UriKind.Relative));
            NavigationService.RemoveBackEntry();
        }

        private void Check_Previous_Location_Button_Click(object sender, RoutedEventArgs e)
        {
            if (App.trackItemModel.user.Get<Boolean>(ParseContract.UserTable.IN_DANGER) || App.trackItemModel.relation.Get<bool>(ParseContract.TrackRelationTable.ALLOW_LOCATION_ACCESS))
            {
                NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show(AppResources.TrackingSetting_LocationRequestDenied);
            }
        }
    }
}