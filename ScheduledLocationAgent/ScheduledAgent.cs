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
            ParseUser user = null;
            //Get the user credentials.
            String[] credential = Utilities.GetParseCredential();
            //Get the location sending info stored in the phone, this does not require internet connection
            UnsentLocationQueue queue = new UnsentLocationQueue(credential[0]);
            DateTime lastUpdate = queue.LastUpdate;//The lastUpdate stored in the phone, should be the same as the one in the server.
            int interval = queue.UpdateInterval;//The update interval stored in the phone, should be the same as the one in the server.
            int unsentSize = queue.QueueSize();

            Debug.WriteLine("Background task invoked:\ninterval: " + interval + "\nlast update: " + lastUpdate+"\nunsent size: "+unsentSize);

            if (unsentSize > 0)//First try to send the unsent locations if there is any.
            {
                try
                {
                    ParseClient.Initialize(ParseContract.applicationID, ParseContract.windowsKey);
                    Debug.WriteLine("Background agent logging in using: " + credential[0] + " " + credential[1]);
                    await ParseUser.LogInAsync(credential[0], credential[1]);
                    user = ParseUser.CurrentUser;
                    await queue.SendLocations(user);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Login failed in the background:");
                    Debug.WriteLine(e.ToString());
                }
            }

            if (queue.QueueSize() > 0)//Display message when there are unsent locations.
            {
                Utilities.ShowToast("CitySafe", "Failed to upload location data.", new Uri("/LoginPage.xaml", UriKind.Relative));
                Debug.WriteLine("Sending unsent location failed");
            }

            if (IsTimeToSendData(interval, lastUpdate))//Then try to update the current location to the server.
            {
                //First get the new location.
                GeoPosition<GeoCoordinate> newLocation = await Utilities.getCurrentGeoPosition();
                if (queue.QueueSize() == 0)//Only send new data when previous data was sent.
                {
                    try
                    {
                        if (user == null)//Only log in when ParseUser is not already logged in
                        {
                            ParseClient.Initialize(ParseContract.applicationID, ParseContract.windowsKey);
                            Debug.WriteLine("Background agent logging in using: " + credential[0] + " " + credential[1]);
                            await ParseUser.LogInAsync(credential[0], credential[1]);
                            user = ParseUser.CurrentUser;
                        }

                        ////Get the update interval and lastUpdate from the server, they should be the same as the ones stored in the phone
                        //int interval = user.Get<int>(ParseContract.UserTable.UPDATE_INTERVAL);
                        ////Querying the database for the time of the last update, used to determine whether we should update now.
                        //ParseObject lastLocation = user.Get<ParseObject>(ParseContract.UserTable.LOCATION(lastLocationIndex));
                        //await lastLocation.FetchIfNeededAsync();
                        //DateTime lastUpdate = lastLocation.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP);

                        Utilities.SaveLocationToParseUser(user, newLocation);
                        await user.SaveAsync();
                        queue.LastUpdate = newLocation.Timestamp.DateTime;
                        Debug.WriteLine("Location updated to the server for user: " + credential[0]);
                    }
                    catch (Exception e)
                    {
                        Utilities.ShowToast("CitySafe", "Failed to upload location data.", new Uri("/LoginPage.xaml", UriKind.Relative));
                        Debug.WriteLine("Failed to send location data to the server, stored it to the local location queue:");
                        Debug.WriteLine(e.ToString());
                        queue.Enqueue(newLocation);
                    }
                }
                else//If the unsent data was not sent, just add the current location to the unsent data.
                queue.Enqueue(newLocation);
            }

            queue.Save();

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
        private bool IsTimeToSendData(int interval, DateTime lastDate)
        {
            DateTime scheduledTime = lastDate.AddMinutes(interval);
            if (scheduledTime <= DateTime.Now)
                return true;
            return false;
        }
    }
}