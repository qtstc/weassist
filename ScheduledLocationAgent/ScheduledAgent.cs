#define DEBUG_AGENT

using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System;
using ScheduledLocationAgent.Data;
using Parse;
using System.Device.Location;
using Microsoft.Phone.Shell;

namespace ScheduledLocationAgent
{
    /// <summary>
    /// This class runs in the background to update the user's location to the server.
    /// </summary>
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
            try
            {
                ParseClient.Initialize(ParseContract.applicationID, ParseContract.windowsKey);
                String[] credential = Utilities.GetParseCredential();
                Debug.WriteLine("Background agent logging in using: " + credential[0] + " " + credential[1]);
                await ParseUser.LogInAsync(credential[0], credential[1]);
                ParseUser user = ParseUser.CurrentUser;

                int lastLocationIndex = user.Get<int>(ParseContract.UserTable.LAST_LOCATION_INDEX);

                //Querying the database for the time of the last update, used to determine whether we should update now.
                ParseObject lastLocation = user.Get<ParseObject>(ParseContract.UserTable.LOCATION(lastLocationIndex));
                await lastLocation.FetchIfNeededAsync();

                DateTime lastUpdate = lastLocation.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP);
                //Get the update interval.
                int interval = user.Get<int>(ParseContract.UserTable.UPDATE_INTERVAL);

                Debug.WriteLine("Background task invoked:\nlast Locations index " + lastLocationIndex + "\ninterval: " + interval + "\nlast update: " + lastUpdate);
                if (IsTimeToSendData(interval, lastUpdate))
                {
                    int newLocationIndex = lastLocationIndex + 1;
                    newLocationIndex %= user.Get<int>(ParseContract.UserTable.LOCATION_DATA_SIZE);
                    user[ParseContract.UserTable.LAST_LOCATION_INDEX] = newLocationIndex;

                    ParseObject newLocation = user.Get<ParseObject>(ParseContract.UserTable.LOCATION(newLocationIndex));

                    GeoPosition<GeoCoordinate> newData = await Utilities.getCurrentGeoPosition();
                    if (newLocation.ObjectId == ParseContract.LocationTable.DUMMY_LOCATION)//If the new location is a dummy
                    {
                        //Create a new location.
                        user[ParseContract.UserTable.LOCATION(newLocationIndex)] = ParseContract.LocationTable.GeoPositionToParseObject(newData);
                    }
                    else//If the new location contains a valid location
                    {
                        //Change the location entry without creating a new one.
                        ParseContract.LocationTable.GeoPositionSetParseObject(newData, newLocation);
                    }
                    await user.SaveAsync();
                }

#if DEBUG_AGENT
                ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
#endif
                NotifyComplete();
            }
            catch(Exception e)
            {
                Utilities.ShowToast("CitySafe", "Failed to upload location data.", new Uri("/SettingsPage.xaml", UriKind.Relative));
                Debug.WriteLine(e.ToString());
                Debug.WriteLine("Error in background agent.");
            }
        }

        /// <summary>
        /// Decide whether it's time to update the location data to the server.
        /// </summary>
        /// <param name="interval">the interval set by the user, in minutes</param>
        /// <param name="lastDate">the last time when the location data was sent</param>
        /// <returns>true if the location data should be sent, false otherwise</returns>
        private bool IsTimeToSendData(int interval, DateTime lastDate)
        {
            DateTime scheduledTime = lastDate.AddMinutes(interval);
            if (scheduledTime <= DateTime.Now)
                return true;
            return false;
        }
    }
}