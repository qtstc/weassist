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

namespace CitySafe.ViewModels
{
    /// <summary>
    /// This class is used as the data context for the UserSettingsPanel of the SettingsPage.
    /// </summary>
    [DataContract]
    public class UserSettings : Settings
    {
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
    }
}
