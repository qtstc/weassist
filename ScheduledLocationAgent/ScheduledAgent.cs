#define DEBUG_AGENT

using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.IO.IsolatedStorage;
using ScheduledLocationAgent.Data;
using System.Threading;

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

            UserSettings settings = UserSettings.loadUserSettingsFromPhone();
            Debug.WriteLine("Background task invoked. last update:\n" + settings);
            if (isTimeToSendData(settings.interval, settings.lastUpdate))
            {
                sendLocationData();
                settings.lastUpdate = DateTime.Now;
                UserSettings.saveUserSettingsToPhone(settings);
            }
            Debug.WriteLine("Background task finished. last update:\n" + UserSettings.loadUserSettingsFromPhone());
            
#if DEBUG_AGENT
  ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
#endif
            NotifyComplete();
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
            Debug.WriteLine("start sending.");
            Thread.Sleep(3000);
            Debug.WriteLine("sent");
            return true;
        }
    }
}