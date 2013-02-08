#define DEBUG_AGENT

using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.IO.IsolatedStorage;
using ScheduledLocationAgent.Data;
using System.Threading;
using Parse;
using System.Device.Location;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        protected override async void OnInvoke(ScheduledTask task)
        {
            ParseClient.Initialize("eJ2fw5ZYIDLUUbmBer2QIC1142uFXrTfJEwcVFfd", "ngWRcVLvVb7NSdg1UVceMTyDfjpXoR9vJklGu9Eg");
           
            await ParseUser.LogInAsync("tao", "p");
            ParseUser user = ParseUser.CurrentUser;

            int lastLocationIndex = user.Get<int>(ParseContract.UserTable.LAST_LOCATION_INDEX);

            //Querying the database for the time of the last update, used to determine whether we should update now.
            ParseObject lastLocation = user.Get<ParseObject>(ParseContract.UserTable.LOCATION(lastLocationIndex));
            DateTime lastUpdate = lastLocation.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP);
            //Get the update interval.
            int interval = user.Get<int>(ParseContract.UserTable.UPDATE_INTERVAL);

            Debug.WriteLine("Background task invoked:\nlast location index "+lastLocationIndex+"\ninterval: "+interval+"\nlast update: "+lastUpdate);
            if (isTimeToSendData(interval, lastUpdate))
            {
                lastLocationIndex++;
                user[ParseContract.UserTable.LAST_LOCATION_INDEX] = (lastLocationIndex) % user.Get<int>(ParseContract.UserTable.LOCATION_DATA_SIZE);
                user[ParseContract.UserTable.LOCATION(lastLocationIndex)] = ParseContract.LocationTable.GeoPositionToParseObject(getCurrentGeoPosition());
                await user.SaveAsync();
            }
            Debug.WriteLine("Background task finished.");
            
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
        /// Get the curent geo position
        /// </summary>
        /// <returns></returns>
        private GeoPosition<GeoCoordinate> getCurrentGeoPosition()
        {
            return new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.Now), new GeoCoordinate(1.1, 2.2));
        }
    }
}