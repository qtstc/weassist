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
using System.Device.Location;
using TestPhoneApp.Data;

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
            loadUserSettings();
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
            //Go back to login page
            NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            //Remove back entry. Prevent user from coming back to settings page by pressing back button
            //when he or she is on the login page
            NavigationService.RemoveBackEntry();
        }

        private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            //First save the user settings.
            saveUserSettingsWithProgressOverlay();
            //Then register/unregister the agent.
            if (userSettings.trackingEnabled)
                StartPeriodicAgent();
            else
                RemoveAgent();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            loadUserSettings();
        }

        #endregion

        #region Helper_Method

        /// <summary>
        /// Load user settings.
        /// If there is no previous user setting,
        /// a new one will be loaded from the web site.
        /// </summary>
        private void loadUserSettings()
        {
            ParseUser.CurrentUser.FetchAsync();//Sync first because settings can be updated in the background.

            //If does not contain one of the keys, initialize the user settings
            if (!ParseUser.CurrentUser.ContainsKey(ParseContract.TRACKING_ENABLED_KEY))
                initializeUserSettingsWithProgressOverlay();

            //Use the data to populate UI.
            bool trackingEnabled = ParseUser.CurrentUser.Get<bool>(ParseContract.TRACKING_ENABLED_KEY);
            int interval = ParseUser.CurrentUser.Get<int>(ParseContract.UPDATE_INTERVAL_KEY);
            int lastUpdateIndex = ParseUser.CurrentUser.Get<int>(ParseContract.LAST_LOCATION_INDEX_KEY);
            DateTime lastUpdate = Utilities.convertJSONToGeoPosition(ParseUser.CurrentUser.Get<string>(ParseContract.LOCATION(lastUpdateIndex))).Timestamp.DateTime;
            userSettings = new UserSettings(trackingEnabled, interval, lastUpdate);
            UserSettingsPanel.DataContext = userSettings;

            Debug.WriteLine(userSettings);
        }

        /// <summary>
        /// Helper method used to save user settings.
        /// </summary>
        private async void saveUserSettingsWithProgressOverlay()
        {
            App.showProgressOverlay(AppResources.Setting_SyncingUserSettingsWithParseServer);
            try
            {
                //Not all fields are saved.
                ParseUser.CurrentUser[ParseContract.UPDATE_INTERVAL_KEY] = userSettings.interval;
                ParseUser.CurrentUser[ParseContract.TRACKING_ENABLED_KEY] = userSettings.trackingEnabled;
                await ParseUser.CurrentUser.SaveAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to save user settings to the server:\n");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Setting_SyncingFailed);
            }
            App.hideProgressOverlay();
        }

        /// <summary>
        /// Helper method used to initialize user settings.
        /// Only called when the user uses the app for the first time.
        /// </summary>
        private async void initializeUserSettingsWithProgressOverlay()
        {
            const int DEFAULT_INTERVAL = 60;
            const int DEFAULT_DATA_SIZE = 48;

            ParseUser.CurrentUser[ParseContract.TRACKING_ENABLED_KEY] = false;
            ParseUser.CurrentUser[ParseContract.UPDATE_INTERVAL_KEY] = DEFAULT_INTERVAL;
            ParseUser.CurrentUser[ParseContract.LOCATION_DATA_SIZE_KEY] = DEFAULT_DATA_SIZE;
            ParseUser.CurrentUser[ParseContract.LAST_LOCATION_INDEX_KEY] = 0;
            for (int i = 0; i < DEFAULT_DATA_SIZE; i++)
                ParseUser.CurrentUser[ParseContract.LOCATION(i)] = Utilities.convertGeoPositionToJSON(new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.MinValue), GeoCoordinate.Unknown));
             App.showProgressOverlay(AppResources.Setting_SyncingUserSettingsWithParseServer);
             try
             {
                 await ParseUser.CurrentUser.SaveAsync();
             }
             catch (Exception e)
             {
                 Debug.WriteLine("Fail to initialize user settings to the server:\n");
                 Debug.WriteLine(e.ToString());
                 MessageBox.Show(AppResources.Setting_InitializeSettings);
             }
            App.hideProgressOverlay();
        }
        #endregion
    }
}