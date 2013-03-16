using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CitySafe.ViewModels;
using CitySafe.Resources;
using System.Diagnostics;
using System.Threading.Tasks;
using ScheduledLocationAgent.Data;
using System.Threading;
using System.ComponentModel;

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
        private void LoadUIData()
        {
            //CancellationToken tk = App.ShowProgressOverlay(AppResources.TrackedSetting_Loading);
            //string message = "";
            //try
            //{
                //Set the title
                TrackedTitle.Text = App.trackItemModel.user.Get<string>(ParseContract.UserTable.FIRST_NAME);
                trackedSettings.LoadSettings(CancellationToken.None);
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
            CancellationToken tk = App.ShowProgressOverlay(AppResources.TrackedSetting_Saving);
            string message = "";
            try
            {
                await trackedSettings.SaveSettings(tk);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                message = AppResources.Setting_SyncingFailed;
            }
            App.HideProgressOverlay();
            if (!message.Equals(""))
                MessageBox.Show(message);
        }

        private async void Stop_Tracked_Button_Click(object sender, EventArgs e)
        {
            CancellationToken tk = App.ShowProgressOverlay(AppResources.TrackedSetting_RemovingUser);
            string message = "";
            try
            {
                await trackedSettings.RemoveRelation(tk);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                message = AppResources.TrackedSetting_FailToRemove;
            }
            App.HideProgressOverlay();
            if (message.Equals(""))
                NavigationService.GoBack();
            else
                MessageBox.Show(message);
        }

        private async void Apply_Button_Click(object sender, EventArgs e)
        {
            await SaveUIData();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (App.HideProgressOverlay())
                e.Cancel = true;
        }
    }
}