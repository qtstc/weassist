#define DEBUG_AGENT

using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.IO.IsolatedStorage;
using ScheduledLocationAgent.Data;

namespace ScheduledLocationAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            //TODO: Add code to perform your task in background
            //string toastMessage = "default message";
            //ShellToast toast = new ShellToast();
            //toast.Title = "toast title";
            //toast.Content = toastMessage;
            //toast.Show();

            UserSettings userSettings = getUserSettings();
            Debug.WriteLine("Background task invoked. Current user settings:\n" + userSettings);
            Debug.Assert(userSettings.trackingEnabled);
            if (isTimeToSendData(userSettings.interval, userSettings.lastUpdate))
            {
                sendLocationData();
                userSettings.lastUpdate = DateTime.Now;
                IsolatedStorageSettings.ApplicationSettings.Save();
                Debug.WriteLine("Just sent.");
            }
            Debug.WriteLine("Background task finished. Current user settings:\n" + getUserSettings());
            
#if DEBUG_AGENT
  ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
#endif
            NotifyComplete();
        }

        /// <summary>
        /// Get the current user settings
        /// </summary>
        /// <returns>the current user settings</returns>
        private UserSettings getUserSettings()
        {
            Debug.Assert(IsolatedStorageSettings.ApplicationSettings.Contains(UserSettings.USER_SETTINGS_ISOLATED_STORAGE_KEY));
            return IsolatedStorageSettings.ApplicationSettings[UserSettings.USER_SETTINGS_ISOLATED_STORAGE_KEY] as UserSettings;
        }

        /// <summary>
        /// Decide whether it's time to update the location data to the server.
        /// </summary>
        /// <param name="interval">the interval set by the user, in minutes</param>
        /// <param name="lastDate">the last time when the location data was sent</param>
        /// <returns>true if the location data should be sent, false otherwise</returns>
        private bool isTimeToSendData(int interval, DateTime lastDate)
        {
            DateTime scheduledTime = lastDate.AddMinutes(interval);
            if (scheduledTime <= DateTime.Now)
                return true;
            return false;
        }

        /// <summary>
        /// Send the location data to the server.
        /// </summary>
        /// <returns>true if succeeded</returns>
        private bool sendLocationData()
        {
            return true;
        }
    }
}