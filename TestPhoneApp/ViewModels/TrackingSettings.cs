using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CitySafe.ViewModels
{
    [DataContract]
    public class TrackingSettings : Settings
    {
        private bool _usePushNotification;
        private bool _useSMS;
        private bool _useEmail;

        public TrackingSettings(bool usePushNotification, bool useSMS, bool useEmail)
        {
            this.usePushNotification = usePushNotification;
            this.useSMS = useSMS;
            this.useEmail = useEmail;
        }

        [DataMember(Name = "usePushNotification")]
        public bool usePushNotification
        {
            get { return _usePushNotification; }
            set { SetProperty(ref _usePushNotification, value); }
        }

        [DataMember(Name = "useSMS")]
        public bool useSMS
        {
            get { return _useSMS; }
            set { SetProperty(ref _useSMS, value); }
        }

        [DataMember(Name = "useEmail")]
        public bool useEmail
        {
            get { return _useEmail; }
            set { SetProperty(ref _useEmail, value); }
        }
    }
}
