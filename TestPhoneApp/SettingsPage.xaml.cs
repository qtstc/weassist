#define DEBUG_AGENT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TestPhoneApp.Resources;
using Parse;
using Microsoft.Phone.Scheduler;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using ScheduledLocationAgent.Data;
using System.Threading;
using System.Threading.Tasks;

namespace TestPhoneApp
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private PeriodicTask periodicTask;
        private UserSettings userSettings;

        //Used to identify the periodic task
        private const string periodicTaskName = "PeriodicAgent";

        // Constructor
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
        }

        /// <summary>
        /// Load user settings from the isolated storage.
        /// If there is no previous user setting,
        /// a new one will be loaded from the web site.
        /// </summary>
        /// <returns>true if there is a previous user setting</returns>
        private async Task<bool> loadUserSettings()
        {
            userSettings = UserSettings.loadUserSettingsFromPhone();
            if (userSettings != null)
            {
                Debug.WriteLine(userSettings);
                UserSettingsPanel.DataContext = userSettings;
                return true;
            }
            //If no user setting is stored in the phone, load user setting from the web. Default user setting will be created when the user creates account on the website.
            userSettings = await loadUserSettingsFromServerWithProgressOverlay();
            UserSettings.saveUserSettingsToPhone(userSettings);
            UserSettingsPanel.DataContext = userSettings;
            return false;
        }

        #region Background Agent
        /// <summary>
        /// Start the background periodic agent.
        /// The agent will be scheduled to run every 30 minutes.
        /// However, in debug mode, this agent will not run,
        /// and we use ScheduledActionService.LaunchForTest for testing.
        /// </summary>
        private void StartPeriodicAgent()
        {
            // Obtain a reference to the period task, if one exists
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

            // If the task already exists and the IsEnabled property is false, background
            // agents have been disabled by the user
            if (periodicTask != null && !periodicTask.IsEnabled)
            {
                MessageBox.Show("Background agents for this application have been disabled by the user.");
                return;
            }

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            if (periodicTask != null && periodicTask.IsEnabled)
            {
                RemoveAgent();
            }

            periodicTask = new PeriodicTask(periodicTaskName);

            // The description is required for periodic agents. This is the string that the user
            // will see in the background services Settings page on the device.
            periodicTask.Description = "This demonstrates a periodic task.";

            ScheduledActionService.Add(periodicTask);

                // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
#if(DEBUG_AGENT)
    ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(10));
#endif
        }

        /// <summary>
        /// Remove the background agent with the given name
        /// </summary>
        /// <param name="name">the name of the background agent to be removed</param>
        private void RemoveAgent()
        {
            try
            {
                ScheduledActionService.Remove(periodicTaskName);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Listeners For UI
        /// <summary>
        /// Lisenter for the ChangeUserButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeUserButton_Click(object sender, RoutedEventArgs e)
        {
            ParseUser.LogOut();
            RemoveAgent();
            UserSettings.clearUserSettingsInPhone();
            //Go back to login page
            NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            //Remove back entry. Prevent user from coming back to settings page by pressing back button
            //when he or she is on the login page
            NavigationService.RemoveBackEntry();
        }

        private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            saveUserSettingsToServerWithProgressOverlay(new UserSettings(false,134,DateTime.Today));
            //UserSettings.saveUserSettingsToPhone(userSettings);
            //if (userSettings.trackingEnabled)
            //    StartPeriodicAgent();
            //else
            //    RemoveAgent();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //await loadUserSettings();
            Debug.WriteLine("Interval from saved!:"+ParseUser.CurrentUser.Get<int>("update_interval"));
        }

        #endregion

        /// <summary>
        /// Load user settings from the server. The progress overlay will be shown.
        /// </summary>
        /// <returns>the user settings loaded from the server. null if nothing.</returns>
        private async Task<UserSettings> loadUserSettingsFromServerWithProgressOverlay()
        {
            App.showProgressOverlay(AppResources.Setting_SyncingUserSettingsWithParseServer);
            UserSettings result = null;
            try
            {
                result = await UserSettings.loadUserSettingsFromParseServer();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to load user settings from the server:\n");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Setting_SyncingFailed);
            }
            App.hideProgressOverlay();
            return result;
        }

        /// <summary>
        /// Save user settings to the server
        /// </summary>
        /// <param name="toBeSaved">the UserSettings instance to be saved</param>
        private async void saveUserSettingsToServerWithProgressOverlay(UserSettings toBeSaved)
        {
            App.showProgressOverlay(AppResources.Setting_SyncingUserSettingsWithParseServer);
            try
            {
                await UserSettings.saveUserSettingsToParseServer(toBeSaved);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to save user settings to the server:\n");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Setting_SyncingFailed);
            }
            App.hideProgressOverlay();

        }
    }
}