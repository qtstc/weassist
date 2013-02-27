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
    /// <summary>
    /// The settings page for the app.
    /// It also controls the PeriodicTask used to 
    /// sending location data to the server in the background.
    /// </summary>
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
            userSettings = new UserSettings();
            SettingsPanel.DataContext = userSettings;
            LoadUIData();
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
            periodicTask.Description = AppResources.Background_Description;

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
            Utilities.SaveParseCredential("", "");//Also clear the user credential stored in the phone.
            RemoveAgent();
            //Go back to login page
            NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            //Remove back entry. Prevent user from coming back to settings page by pressing back button
            //when he or she is on the login page
            NavigationService.RemoveBackEntry();
        }

        private async void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            //First save the user settings.
            await SaveUIData();
            //Then register/unregister the agent.
            if (userSettings.trackingEnabled)
                StartPeriodicAgent();
            else
                RemoveAgent();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadUIData();
        }

        #endregion

        #region Helper_Method

        /// <summary>
        /// Load the data for the UI.
        /// </summary>
        private async Task LoadUIData()
        {
            App.ShowProgressOverlay(AppResources.Setting_SyncingUserSettingsWithParseServer);
            try
            {
                await userSettings.LoadSettings();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to initialize user settings to the server:\n");
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
            App.ShowProgressOverlay(AppResources.Setting_SyncingUserSettingsWithParseServer);
            try
            {
                await userSettings.SaveSettings();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to save user settings to the server:\n");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Setting_SyncingFailed);
            }
            App.HideProgressOverlay();
        }

        #endregion
    }
}