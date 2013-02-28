using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Threading;
using Parse;
using ScheduledLocationAgent.Data;

namespace CitySafe.ViewModels
{
    /// <summary>
    /// This class is used as the data context for the user settings page.
    /// It also takes care of the loading and saving of data from the parse
    /// server. Note that the data stored in this class and the data stored
    /// in ParseUser.currentUser is not synced.
    /// </summary>
    [DataContract]
    public class UserSettings : Settings
    {
        private bool _trackingEnabled;
        private int _interval;
        private DateTime _lastUpdate;

        private UserSettings(bool trackingEnabled, int interval, DateTime lastUpdate)
        {
            _trackingEnabled = trackingEnabled;
            _interval = interval;
            _lastUpdate = lastUpdate;
        }
        
        /// <summary>
        /// Default constructor. Does not do anything.
        /// Remember to call loadUserSettings() before set an instance of 
        /// this class as the data context.
        /// </summary>
        public UserSettings()
        {
 
        }

        [DataMember(Name="trackingEnabled")]
        public bool trackingEnabled
        {
            get { return _trackingEnabled; }
            set { SetProperty(ref _trackingEnabled, value); }
        }

        [DataMember(Name = "interval")]
        public int interval
        {
            get { return _interval; }
            set { SetProperty(ref _interval, value); }
        }

        [DataMember(Name = "lastUpdate")]
        public DateTime lastUpdate
        {
            get { return _lastUpdate; }
            set { SetProperty(ref _lastUpdate, value); }
        }

        /// <summary>
        /// Load user settings from the server.
        /// Does not take care of exception handling.
        /// This method will intialize the user setting
        /// on the server if no user setting data exist
        /// on the server.
        /// </summary>
        public async override Task LoadSettings()
        {
                await ParseUser.CurrentUser.FetchAsync();//Sync first because settings can be updated in the background.

                //If does not contain one of the keys, initialize the user settings
                if (!ParseUser.CurrentUser.ContainsKey(ParseContract.UserTable.TRACKING_ENABLED))
                {
                    await InitializeUserSettings();
                }

                //Get the data used to populate UI.
                trackingEnabled = ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.TRACKING_ENABLED);
                interval = ParseUser.CurrentUser.Get<int>(ParseContract.UserTable.UPDATE_INTERVAL);
                int lastUpdateIndex = ParseUser.CurrentUser.Get<int>(ParseContract.UserTable.LAST_LOCATION_INDEX);
                ParseObject lastLocation = ParseUser.CurrentUser.Get<ParseObject>(ParseContract.UserTable.LOCATION(lastUpdateIndex));
                //Need to download the object first.
                await lastLocation.FetchIfNeededAsync();
                lastUpdate = lastLocation.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP);
        }

        /// <summary>
        /// Save user settings to the server.
        /// No exception handling.
        /// </summary>
        public async override Task SaveSettings()
        {
                ParseUser.CurrentUser[ParseContract.UserTable.UPDATE_INTERVAL] = interval;
                ParseUser.CurrentUser[ParseContract.UserTable.TRACKING_ENABLED] = trackingEnabled;
                await ParseUser.CurrentUser.SaveAsync();
        }

        /// <summary>
        /// Helper method used to initialize user settings.
        /// Only called when the user uses the app for the first time.
        /// No exception handling.
        /// </summary>
        private async Task InitializeUserSettings()
        {
            const int DEFAULT_INTERVAL = 30;
            const int DEFAULT_DATA_SIZE = 96;
            ParseUser.CurrentUser[ParseContract.UserTable.TRACKING_ENABLED] = false;
            ParseUser.CurrentUser[ParseContract.UserTable.UPDATE_INTERVAL] = DEFAULT_INTERVAL;
            ParseUser.CurrentUser[ParseContract.UserTable.LOCATION_DATA_SIZE] = DEFAULT_DATA_SIZE;
            ParseUser.CurrentUser[ParseContract.UserTable.LAST_LOCATION_INDEX] = DEFAULT_DATA_SIZE - 1;
            ParseUser.CurrentUser[ParseContract.UserTable.IN_DANGER] = false;
            ParseUser.CurrentUser[ParseContract.UserTable.WIN_PNONE_PUSH_URI] = "";

            //Get the null location used to mark un
            ParseQuery<ParseObject> nullLocationQuery = ParseObject.GetQuery(ParseContract.LocationTable.TABLE_NAME);
            ParseObject nullLocation = await nullLocationQuery.GetAsync(ParseContract.LocationTable.DUMMY_LOCATION);

            for (int i = 0; i < DEFAULT_DATA_SIZE; i++)
            {
                ParseUser.CurrentUser[ParseContract.UserTable.LOCATION(i)] = nullLocation;
            }

            await ParseUser.CurrentUser.SaveAsync();
        }
    }
}
