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

        /// <summary>
        /// Load user settings from the isolated storage.
        /// If there is no previous user setting,
        /// a new one will be created and saved in the isolated storage.
        /// </summary>
        /// <returns>true if there is a previous user setting</returns>
        private bool loadUserSettings()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(UserSettings.USER_SETTINGS_ISOLATED_STORAGE_KEY))
            {
                userSettings = IsolatedStorageSettings.ApplicationSettings[UserSettings.USER_SETTINGS_ISOLATED_STORAGE_KEY] as UserSettings;
                Debug.WriteLine(userSettings);
                UserSettingsPanel.DataContext = userSettings;
                return true;
            }
            //Default user setting
            userSettings = new UserSettings(false, 12, DateTime.Today);
            IsolatedStorageSettings.ApplicationSettings[UserSettings.USER_SETTINGS_ISOLATED_STORAGE_KEY] = userSettings;
            IsolatedStorageSettings.ApplicationSettings.Save();
            UserSettingsPanel.DataContext = userSettings;
            return false;
        }

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

        /// <summary>
        /// Lisenter for the ChangeUserButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeUserButton_Click(object sender, RoutedEventArgs e)
        {
            //ParseUser.LogOut();
            ////Go back to login page
            //NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            ////Remove back entry. Prevent user from coming back to settings page by pressing back button
            ////when he or she is on the login page
            //NavigationService.RemoveBackEntry();
            Debug.WriteLine(userSettings);
            Debug.WriteLine(IsolatedStorageSettings.ApplicationSettings[UserSettings.USER_SETTINGS_ISOLATED_STORAGE_KEY]);
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings.Save();
            if (userSettings.trackingEnabled)
                StartPeriodicAgent();
            else
                RemoveAgent();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            loadUserSettings();
        }

        /* DELETED OnNavigatedFrom
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //No need to save userSettings to the isolated storage
            //because userSettings and the settings currently stored in isolated storage point to the save object.
            Debug.WriteLine(IsolatedStorageSettings.ApplicationSettings[USER_SETTINGS_ISOLATED_STORAGE_KEY] as UserSettings);
        }
         * */
    }
}