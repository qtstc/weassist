using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ScheduledLocationAgent.Data;

namespace CitySafe.ViewModels
{
    /// <summary>
    /// The data context for the tracking settings page.
    /// </summary>
    [DataContract]
    public class TrackingSettings : Settings
    {
        private bool _usePushNotification;
        private bool _useSMS;
        private bool _useEmail;

        private TrackingSettings(bool usePushNotification, bool useSMS, bool useEmail)
        {
            this.usePushNotification = usePushNotification;
            this.useSMS = useSMS;
            this.useEmail = useEmail;
        }

        /// <summary>
        /// Default constructor that does not do anything.
        /// Need to call LoadTrackingSettings before 
        /// setting an instance of this class as the data context.
        /// </summary>
        public TrackingSettings()
        {

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

        /// <summary>
        /// Load tracking settings from the server.
        /// Does not have exception handling.
        /// </summary>
        public async override Task LoadSettings()
        {
                //Use the data to populate UI.
                usePushNotification = App.trackItemModel.relation.Get<bool>(ParseContract.TrackRelationTable.NOTIFY_BY_PUSH);
                useSMS = App.trackItemModel.relation.Get<bool>(ParseContract.TrackRelationTable.NOTIFY_BY_SMS);
                useEmail = App.trackItemModel.relation.Get<bool>(ParseContract.TrackRelationTable.NOTIFY_BY_EMAIL);
        }

        /// <summary>
        /// Save tracking settings to the servr.
        /// Does not have exception handling.
        /// </summary>
        public async override Task SaveSettings()
        {
                //Not all fields are saved.
                App.trackItemModel.relation[ParseContract.TrackRelationTable.NOTIFY_BY_PUSH] = usePushNotification;
                App.trackItemModel.relation[ParseContract.TrackRelationTable.NOTIFY_BY_SMS] = useSMS;
                App.trackItemModel.relation[ParseContract.TrackRelationTable.NOTIFY_BY_EMAIL] = useEmail;
                await App.trackItemModel.relation.SaveAsync();
        }

        /// <summary>
        /// Delete the track relation from the server.
        /// Also remove the item from the list.
        /// </summary>
        /// <returns></returns>
        public async Task RemoveRelation()
        {
            //TODO: notify the other user that the other user has deleted the tracking relation.
            await App.trackItemModel.relation.DeleteAsync();
            App.trackingModel.remove(App.trackItemModel);
            App.trackItemModel = null;
        }
    }
}
