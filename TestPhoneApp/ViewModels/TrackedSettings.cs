﻿using ScheduledLocationAgent.Data;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CitySafe.ViewModels
{
    /// <summary>
    /// The data context for the tracked settings page.
    /// </summary>
    [DataContract]
    public class TrackedSettings : Settings
    {
        private bool _allowLocationAccess;

        [DataMember(Name = "allowLocationAccess")]
        public bool allowLocationAccess
        {
            get { return _allowLocationAccess; }
            set { SetProperty(ref _allowLocationAccess, value); }
        }

        public async override Task LoadSettings(CancellationToken tk)
        {
            allowLocationAccess = App.trackItemModel.relation.Get<bool>(ParseContract.TrackRelationTable.ALLOW_LOCATION_ACCESS);
        }

        public async override Task SaveSettings(CancellationToken tk)
        {
            App.trackItemModel.relation[ParseContract.TrackRelationTable.ALLOW_LOCATION_ACCESS] = allowLocationAccess;
            await App.trackItemModel.relation.SaveAsync(tk);
        }

        /// <summary>
        /// Delete the track relation from the server.
        /// Also remove the item from the list.
        /// </summary>
        /// <returns></returns>
        public async Task RemoveRelation(CancellationToken tk)
        {
            //TODO: notify the other user that the other user has deleted the tracking relation.
            await App.trackItemModel.relation.DeleteAsync(tk);
            App.trackedModel.remove(App.trackItemModel);
            App.trackItemModel = null;
        }
    }
}
