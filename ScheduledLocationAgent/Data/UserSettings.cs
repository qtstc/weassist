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

namespace ScheduledLocationAgent.Data
{
    /// <summary>
    /// This class is used as the data context for the UserSettingsPanel of the SettingsPage.
    /// It is also used to store user settings in the isolated storage.
    /// It was not supposed to be in the solution of ScheduledLocationAgent.
    /// However, because we cannot add a reference to the main solution in the soluion of ScheduledLocationAgent
    /// ("Unable to add the selected reference because it is not supported by background agents."),
    /// there is no way to refer to this class if it is in the main solution.
    /// So we moved it here.
    /// </summary>
    [DataContract]
    public class UserSettings : INotifyPropertyChanged
    {
        private bool _trackingEnabled;
        private int _interval;
        private DateTime _lastUpdate;

        private const string USER_SETTINGS_FILE_NAME = "user_settings.dat";//Name of the file where the user setting is saved.
        private const string USER_SETTINGS_MUTEX_NAME = "user_settings_mutex";//The name of the mutex used to manage the user setting file.


        public UserSettings(bool trackingEnabled, int interval, DateTime lastUpdate)
        {
            _trackingEnabled = trackingEnabled;
            _interval = interval;
            _lastUpdate = lastUpdate;
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
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, 
            [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Save the user settings to the phone's local storage.
        /// </summary>
        /// <param name="newSettings">the user setting to be saved</param>
        public static void saveUserSettings(UserSettings newSettings)
        {
            IsolatedStorageHelper.WriteObjectToFileUsingJson(USER_SETTINGS_FILE_NAME, newSettings, USER_SETTINGS_MUTEX_NAME) ;
        }

        public static void clearUserSettings()
        {
            saveUserSettings(null);
        }

        /// <summary>
        /// Load user setting from the phone's local storage.
        /// </summary>
        /// <returns>the user setting loaded. null if no setting is found.</returns>
        public static UserSettings loadUserSettings()
        {
            if (IsolatedStorageHelper.IsFileExist(USER_SETTINGS_FILE_NAME))
            {
                UserSettings settings = IsolatedStorageHelper.ReadObjectFromFileUsingJson<UserSettings>(USER_SETTINGS_FILE_NAME, USER_SETTINGS_MUTEX_NAME);
                return settings;
            }
            return null;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// String representation. For debug purpose.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Enabled: " + trackingEnabled + "\nInterval: " + interval + "\nLast Update :" + lastUpdate;
        }
    }
}
