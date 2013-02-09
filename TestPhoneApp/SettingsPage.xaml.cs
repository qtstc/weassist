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
using CitySafe.Resources;
using Parse;
using Microsoft.Phone.Scheduler;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using ScheduledLocationAgent.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Device.Location;
using CitySafe.ViewModels;

namespace CitySafe
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
        private async void loadUserSettings()
        {
            App.showProgressOverlay(AppResources.Setting_SyncingUserSettingsWithParseServer);
            try
            {
                await ParseUser.CurrentUser.FetchAsync();//Sync first because settings can be updated in the background.
                //If does not contain one of the keys, initialize the user settings
                if (!ParseUser.CurrentUser.ContainsKey(ParseContract.UserTable.TRACKING_ENABLED))
                {
                    await initializeUserSettings();
                }

                //Use the data to populate UI.
                bool trackingEnabled = ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.TRACKING_ENABLED);
                int interval = ParseUser.CurrentUser.Get<int>(ParseContract.UserTable.UPDATE_INTERVAL);
                int lastUpdateIndex = ParseUser.CurrentUser.Get<int>(ParseContract.UserTable.LAST_LOCATION_INDEX);
                ParseObject lastLocation = ParseUser.CurrentUser.Get<ParseObject>(ParseContract.UserTable.LOCATION(lastUpdateIndex));
                //Need to download the object first.
                await lastLocation.FetchIfNeededAsync();
                userSettings = new UserSettings(trackingEnabled, interval, lastLocation.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP));
                UserSettingsPanel.DataContext = userSettings;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to initialize user settings to the server:\n");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Setting_SyncingFailed);
            }
            App.hideProgressOverlay();
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
                ParseUser.CurrentUser[ParseContract.UserTable.UPDATE_INTERVAL] = userSettings.interval;
                ParseUser.CurrentUser[ParseContract.UserTable.TRACKING_ENABLED] = userSettings.trackingEnabled;
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
        private async Task initializeUserSettings()
        {
            const int DEFAULT_INTERVAL = 30;
            const int DEFAULT_DATA_SIZE = 96;
            ParseUser.CurrentUser[ParseContract.UserTable.TRACKING_ENABLED] = false;
            ParseUser.CurrentUser[ParseContract.UserTable.UPDATE_INTERVAL] = DEFAULT_INTERVAL;
            ParseUser.CurrentUser[ParseContract.UserTable.LOCATION_DATA_SIZE] = DEFAULT_DATA_SIZE;
            ParseUser.CurrentUser[ParseContract.UserTable.LAST_LOCATION_INDEX] = DEFAULT_DATA_SIZE-1;

            //Get the null location used to mark un
            ParseQuery<ParseObject> nullLocationQuery = ParseObject.GetQuery(ParseContract.LocationTable.TABLE_NAME);
            ParseObject nullLocation = await nullLocationQuery.GetAsync(ParseContract.LocationTable.DUMMY_LOCATION);

            for (int i = 0; i < DEFAULT_DATA_SIZE; i++)
            {
                ParseUser.CurrentUser[ParseContract.UserTable.LOCATION(i)] = nullLocation;
            }

            await ParseUser.CurrentUser.SaveAsync();
        }
        #endregion
    }
}