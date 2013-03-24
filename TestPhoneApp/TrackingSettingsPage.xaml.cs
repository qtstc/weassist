using CitySafe.Resources;
using CitySafe.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ScheduledLocationAgent.Data;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

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
        private void LoadUIData()
        {
            //CancellationToken tk = App.ShowProgressOverlay(AppResources.TrackingSetting_Loading);
            //string message = "";
            //try
            //{
            //Set the title
            TrackingTitle.Text = App.trackItemModel.user.Get<string>(ParseContract.UserTable.FIRST_NAME);
            trackingSettings.LoadSettings(CancellationToken.None);
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.ToString());
            //    message = AppResources.Setting_SyncingFailed;
            //}
            //App.HideProgressOverlay();
            //if (!message.Equals(""))
            //    MessageBox.Show(message);
        }

        /// <summary>
        /// Save the UI data to the server.
        /// </summary>
        private async Task SaveUIData()
        {
            string message = "";
            ApplicationBar.IsVisible = false;
            CancellationToken tk = App.ShowProgressOverlay(AppResources.TrackingSetting_Saving);
            try
            {
                await trackingSettings.SaveSettings(tk);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                message = AppResources.Setting_SyncingFailed;
            }
            App.HideProgressOverlay();
            ApplicationBar.IsVisible = true;
            if (!message.Equals(""))
                MessageBox.Show(message);
        }

        private async void Apply_Button_Click(object sender, EventArgs e)
        {
            await SaveUIData();
        }

        private async void Stop_Tracking_Button_Click(object sender, EventArgs e)
        {
            ApplicationBar.IsVisible = false;
            CancellationToken tk = App.ShowProgressOverlay(AppResources.TrackingSetting_RemovingUser);
            try
            {
                await trackingSettings.RemoveRelation(tk);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(AppResources.Setting_SyncingFailed);
            }
            App.HideProgressOverlay();
            ApplicationBar.IsVisible = true;
        }

        private void Check_Previous_Location_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.trackItemModel.user.Get<Boolean>(ParseContract.UserTable.IN_DANGER) || App.trackItemModel.relation.Get<bool>(ParseContract.TrackRelationTable.ALLOW_LOCATION_ACCESS))
                {
                    NavigationService.Navigate(new Uri("/TrackMapPage.xaml", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show(AppResources.TrackingSetting_LocationRequestDenied);
                }
            }
            catch
            {
                MessageBox.Show(AppResources.TrackingSetting_LocationRequestDenied);
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (App.HideProgressOverlay())
                e.Cancel = true;
        }
    }
}