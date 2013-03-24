using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using CitySafe.Resources;
using System.Diagnostics;
using System.Threading.Tasks;
using CitySafe.ViewModels;
using Microsoft.Phone.Shell;
using System.Threading;
using System.ComponentModel;

namespace CitySafe
{
    /// <summary>
    /// The settings page for the app.
    /// It also controls the PeriodicTask used to 
    /// sending location data to the server in the background.
    /// </summary>
    public partial class SettingsPage : PhoneApplicationPage
    {
        //private PeriodicTask periodicTask;//The background agent used to update user location
        private UserSettings userSettings;//View Model for loading and saving data.

        public SettingsPage()
        {
            InitializeComponent();
            userSettings = new UserSettings();
            SettingsPanel.DataContext = userSettings;
        }

        #region Listeners For UI

        private async void ApplySettingsButton_Click(object sender, EventArgs e)
        {
            //First save the user settings.
            bool savingResult = await SaveUIData();
            if (!savingResult)
                return;

            bool agentStartingResult = true;

            //Then register/unregister the agent.
            if (userSettings.trackingEnabled)
                agentStartingResult = App.StartPeriodicAgent();
            else
                App.RemoveAgent();

            if (!agentStartingResult)//If the period agent was not started successfully,
            {
                userSettings.trackingEnabled = false;
                await SaveUIData();
            }
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            await LoadUIData();
        }

        //private void ChangePersonalInfoBarMenuItem_Click(object sender, EventArgs e)
        //{
        //    WebBrowserTask wbt = new WebBrowserTask();
        //    wbt.Uri = new Uri("http://weassist.azurewebsites.net/changeuserinfo.php");
        //    wbt.Show();
        //}

        #endregion

        #region Helper_Method

        /// <summary>
        /// Load the data for the UI.
        /// </summary>
        private async Task LoadUIData()
        {
            ApplicationBar.IsVisible = false;
            CancellationToken tk = App.ShowProgressOverlay(AppResources.Setting_SyncingUserSettingsWithParseServer);
            string message = "";
            try
            {
                await userSettings.LoadSettings(tk);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Loading UI data canceled.");
                NavigationService.GoBack();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to initialize user settings to the server:\n");
                Debug.WriteLine(e.ToString());
                message = AppResources.Setting_SyncingFailed;
                NavigationService.GoBack();
            }
            App.HideProgressOverlay();
            if (!message.Equals(""))
                MessageBox.Show(message);
            ApplicationBar.IsVisible = true;
        }

        /// <summary>
        /// Save the UI data to the server.
        /// </summary>
        private async Task<bool> SaveUIData()
        {
            bool result = true;
            string message = "";
            ApplicationBar.IsVisible = false;
            CancellationToken tk = App.ShowProgressOverlay(AppResources.Setting_SyncingUserSettingsWithParseServer);
            try
            {
                await userSettings.SaveSettings(tk);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to save user settings to the server:\n");
                Debug.WriteLine(e.ToString());
                message = AppResources.Setting_SyncingFailed;
                result = false;
            }
            ApplicationBar.IsVisible = true;
            App.HideProgressOverlay();
            if (!message.Equals(""))
                MessageBox.Show(message);
            return result;
        }

        #endregion

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (App.HideProgressOverlay())
                e.Cancel = true;
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //Did not use a flag to avoid loading data again here because this page does not lead
            //to any other pages.
            await LoadUIData();
        }
    }
}