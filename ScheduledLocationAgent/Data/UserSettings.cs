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
        //Used to identify the user setting stored in isolated storage
        public const string USER_SETTINGS_ISOLATED_STORAGE_KEY = "user_settings_isolated_storage_key";

        private bool _trackingEnabled;
        private int _interval;
        private DateTime _lastUpdate;

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
