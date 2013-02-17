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
using System.Diagnostics;
using System.Threading.Tasks;

namespace CitySafe
{
    public partial class TrackedSettingsPage : PhoneApplicationPage
    {
        private TrackedSettings trackedSettings;

        public TrackedSettingsPage()
        {
            InitializeComponent();
            trackedSettings = new TrackedSettings();
            LoadUIData();
            TrackedSettingsPanel.DataContext = trackedSettings;
        }

        /// <summary>
        /// Load UI data from the server
        /// </summary>
        private async Task LoadUIData()
        {
            App.ShowProgressOverlay(AppResources.TrackedSetting_Loading);
            try
            {
                //Set the title
                TrackedTitle.Text = App.trackItemModel.user.Username;
                await trackedSettings.LoadSettings();
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
            App.ShowProgressOverlay(AppResources.TrackedSetting_Saving);
            try
            {
                await trackedSettings.SaveSettings();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Setting_SyncingFailed);
            }
            App.HideProgressOverlay();
        }

        private async void Stop_Tracked_Button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowProgressOverlay(AppResources.TrackedSetting_RemovingUser);
            try
            {
                await trackedSettings.RemoveRelation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(AppResources.Setting_SyncingFailed);
            }
            NavigationService.Navigate(new Uri("/TrackPage.xaml", UriKind.Relative));
            NavigationService.RemoveBackEntry();
        }

        private async void Apply_Button_Click(object sender, RoutedEventArgs e)
        {
            await SaveUIData();
        }
    }
}