using System;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Parse;
using ScheduledLocationAgent.Data;
using System.Diagnostics;
using CitySafe.Resources;
using System.Threading;

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
        private String _lastUpdate;
        private UnsentLocationQueue queue;

        private int interval;
        private int[] intervalRadioMultiplier = new int[5] { 30, 60, 180, 360, 1440 };

        /// <summary>
        /// Default constructor. Does not do anything.
        /// Remember to call loadUserSettings() before set an instance of 
        /// this class as the data context.
        /// </summary>
        public UserSettings()
        {
        }

        [DataMember(Name = "trackingEnabled")]
        public bool trackingEnabled
        {
            get { return _trackingEnabled; }
            set { SetProperty(ref _trackingEnabled, value); }
        }

        [DataMember(Name = "intervalRadio0")]
        public bool intervalRadio0
        {
            get {
                return interval == intervalRadioMultiplier[0]; }
            set
            {
                if (value)
                {
                    queue.UpdateInterval = intervalRadioMultiplier[0];
                    SetProperty(ref interval, intervalRadioMultiplier[0]);
                }
            }
        }

        [DataMember(Name = "intervalRadio1")]
        public bool intervalRadio1
        {
            get { return interval == intervalRadioMultiplier[1]; }
            set
            {
                if (value)
                {
                    queue.UpdateInterval = intervalRadioMultiplier[1];
                    SetProperty(ref interval, intervalRadioMultiplier[1]);
                }
            }
        }

        [DataMember(Name = "intervalRadio2")]
        public bool intervalRadio2
        {
            get { return interval == intervalRadioMultiplier[2]; }
            set
            {
                if (value)
                {
                    queue.UpdateInterval = intervalRadioMultiplier[2];
                    SetProperty(ref interval, intervalRadioMultiplier[2]);
                }
            }
        }

        [DataMember(Name = "intervalRadio3")]
        public bool intervalRadio3
        {
            get { return interval == intervalRadioMultiplier[3]; }
            set
            {
                if (value)
                {
                    queue.UpdateInterval = intervalRadioMultiplier[3];
                    SetProperty(ref interval, intervalRadioMultiplier[3]);
                }
            }
        }

        [DataMember(Name = "intervalRadio4")]
        public bool intervalRadio4
        {
            get {
                return interval == intervalRadioMultiplier[4]; }
            set
            {
                if (value)
                {
                    queue.UpdateInterval = intervalRadioMultiplier[4];
                    SetProperty(ref interval, intervalRadioMultiplier[4]);
                }
            }
        }

        [DataMember(Name = "lastUpdate")]
        public String lastUpdate
        {
            get { return _lastUpdate; }
            set {
                if(value.Equals(DateTime.MinValue.ToString()))
                    SetProperty(ref _lastUpdate, "");
                else SetProperty(ref _lastUpdate, AppResources.Setting_LastUpdateAt+value);
            }
        }

        /// <summary>
        /// Load user settings from the server.
        /// Does not take care of exception handling.
        /// This method will intialize the user setting
        /// on the server if no user setting data exist
        /// on the server.
        /// </summary>
        public async override Task LoadSettings(CancellationToken tk)
        {
            await ParseUser.CurrentUser.FetchAsync(tk);//Sync first because settings can be updated in the background.

            //If does not contain one of the keys, initialize the user settings
            if (!ParseUser.CurrentUser.ContainsKey(ParseContract.UserTable.TRACKING_ENABLED))
            {
                await InitializeUserSettings(tk);
            }

            //Get the data used to populate UI.
            trackingEnabled = ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.TRACKING_ENABLED);
            interval = ParseUser.CurrentUser.Get<int>(ParseContract.UserTable.UPDATE_INTERVAL);
            OnPropertyChanged("intervalRadio0");
            OnPropertyChanged("intervalRadio1");
            OnPropertyChanged("intervalRadio2");
            OnPropertyChanged("intervalRadio3");
            OnPropertyChanged("intervalRadio4");
            int lastUpdateIndex = ParseUser.CurrentUser.Get<int>(ParseContract.UserTable.LAST_LOCATION_INDEX);
            ParseObject lastLocation = ParseUser.CurrentUser.Get<ParseObject>(ParseContract.UserTable.LOCATION(lastUpdateIndex));
            //Need to download the object first.
            await lastLocation.FetchIfNeededAsync(tk);
            lastUpdate = lastLocation.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP).ToString();

            //Sync the local data.
            queue = new UnsentLocationQueue(ParseUser.CurrentUser.Username);
            queue.LastUpdate = lastLocation.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP);
            queue.UpdateInterval = interval;
            queue.Save();
        }

        /// <summary>
        /// Save user settings to the server.
        /// No exception handling.
        /// </summary>
        public async override Task SaveSettings(CancellationToken tk)
        {
            ParseUser.CurrentUser[ParseContract.UserTable.UPDATE_INTERVAL] = interval;
            ParseUser.CurrentUser[ParseContract.UserTable.TRACKING_ENABLED] = trackingEnabled;
            await ParseUser.CurrentUser.SaveAsync(tk);
            queue.Save();
        }

        /// <summary>
        /// Helper method used to initialize user settings.
        /// Only called when the user uses the app for the first time.
        /// No exception handling.
        /// </summary>
        private async Task InitializeUserSettings(CancellationToken tk)
        {
            ParseUser.CurrentUser[ParseContract.UserTable.TRACKING_ENABLED] = false;
            ParseUser.CurrentUser[ParseContract.UserTable.UPDATE_INTERVAL] = ParseContract.UserTable.DEFAULT_INTERVAL;
            ParseUser.CurrentUser[ParseContract.UserTable.LOCATION_DATA_SIZE] = ParseContract.UserTable.DEFAULT_DATA_SIZE;
            ParseUser.CurrentUser[ParseContract.UserTable.LAST_LOCATION_INDEX] = ParseContract.UserTable.DEFAULT_DATA_SIZE - 1;
            ParseUser.CurrentUser[ParseContract.UserTable.IN_DANGER] = false;
            ParseUser.CurrentUser[ParseContract.UserTable.WIN_PNONE_PUSH_URI] = "";

            //Get the null location used to mark unintialized location
            ParseQuery<ParseObject> nullLocationQuery = ParseObject.GetQuery(ParseContract.LocationTable.TABLE_NAME);
            ParseObject nullLocation = await nullLocationQuery.GetAsync(ParseContract.LocationTable.DUMMY_LOCATION,tk);

            for (int i = 0; i < ParseContract.UserTable.DEFAULT_DATA_SIZE; i++)
            {
                ParseUser.CurrentUser[ParseContract.UserTable.LOCATION(i)] = nullLocation;
            }

            await ParseUser.CurrentUser.SaveAsync(tk);
        }
    }
}
