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

        private bool _notifyByPushStranger;
        private bool _notifyBySMSStranger;
        private bool _notifyByEmailStranger;

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
                if(value.Equals(""))
                    SetProperty(ref _lastUpdate, "");
                else SetProperty(ref _lastUpdate, AppResources.Setting_LastUpdateAt+value);
            }
        }

        [DataMember(Name = "notifyByPushStranger")]
        public bool notifyByPushStranger
        {
            get { return _notifyByPushStranger; }
            set{ SetProperty(ref _notifyByPushStranger, value);}
        }

        [DataMember(Name = "notifyByEmailStranger")]
        public bool notifyByEmailStranger
        {
            get { return _notifyByEmailStranger; }
            set { SetProperty(ref _notifyByEmailStranger, value); }
        }

        [DataMember(Name = "notifyBySMSStranger")]
        public bool notifyBySMSStranger
        {
            get { return _notifyBySMSStranger; }
            set { SetProperty(ref _notifyBySMSStranger, value); }
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

            //Get the data used to populate UI.
            trackingEnabled = ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.TRACKING_ENABLED);
            interval = ParseUser.CurrentUser.Get<int>(ParseContract.UserTable.UPDATE_INTERVAL);
            notifyByEmailStranger = ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.NOTIFY_BY_EMAIL_STRANGER);
            notifyBySMSStranger = ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.NOTIFY_BY_SMS_STRANGER);
            notifyByPushStranger = ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.NOTIFY_BY_PUSH_STRANGER);
            OnPropertyChanged("intervalRadio0");
            OnPropertyChanged("intervalRadio1");
            OnPropertyChanged("intervalRadio2");
            OnPropertyChanged("intervalRadio3");
            OnPropertyChanged("intervalRadio4");
            int lastUpdateIndex = ParseUser.CurrentUser.Get<int>(ParseContract.UserTable.LAST_LOCATION_INDEX);
            queue = new UnsentLocationQueue(ParseUser.CurrentUser.Username);
            if (ParseUser.CurrentUser.ContainsKey(ParseContract.UserTable.LOCATION(lastUpdateIndex)))
            {
                ParseObject lastLocation = ParseUser.CurrentUser.Get<ParseObject>(ParseContract.UserTable.LOCATION(lastUpdateIndex));
                //Need to download the object first.
                await lastLocation.FetchIfNeededAsync(tk);
                lastUpdate = lastLocation.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP).ToString();

                //Sync the local data.
                queue.LastUpdate = lastLocation.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP);
            }
            else
            {
                lastUpdate = "";
                queue.LastUpdate = DateTime.MinValue;
            }
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
            ParseUser.CurrentUser[ParseContract.UserTable.NOTIFY_BY_EMAIL_STRANGER] = notifyByEmailStranger;
            ParseUser.CurrentUser[ParseContract.UserTable.NOTIFY_BY_PUSH_STRANGER] = notifyByPushStranger;
            ParseUser.CurrentUser[ParseContract.UserTable.NOTIFY_BY_SMS_STRANGER] = notifyBySMSStranger;
            await ParseUser.CurrentUser.SaveAsync(tk);
            queue.Save();
        }
    }
}
