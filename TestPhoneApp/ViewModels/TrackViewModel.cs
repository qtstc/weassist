using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Parse;
using ScheduledLocationAgent.Data;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace CitySafe.ViewModels
{
    public class TrackViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<TrackItemModel> trackItems { get; private set; }

        private bool loaded;

        public TrackViewModel()
        {
            trackItems = new ObservableCollection<TrackItemModel>();
            loaded = false;
        }


        /// <summary>
        /// Load data from the parse server. Only work when called for the first time.
        /// </summary>
        /// <param name="mode">can be either ParseContract.TrackRelationTable.TRACKED or ParseContract.TrackRelationTable.TRACKING</param>
        public async Task LoadData(String mode)
        {
            if (loaded)
                return;
            trackItems.Clear();
            var confirmedRelation = from relation in ParseObject.GetQuery(ParseContract.TrackRelationTable.TABLE_NAME)
                                    where relation.Get<bool>(ParseContract.TrackRelationTable.TRACKED_VERIFIED) == true
                                     where relation.Get<bool>(ParseContract.TrackRelationTable.TRACKING_VERIFIED) == true
                                     where relation.Get<ParseUser>(mode) == ParseUser.CurrentUser
                                     select relation;

            IEnumerable<ParseObject> results = await confirmedRelation.FindAsync();
            foreach (ParseObject u in results)
            {
                ParseUser user = u.Get<ParseUser>(ParseContract.TrackRelationTable.OtherRole(mode));
                await user.FetchAsync();
                trackItems.Add(new TrackItemModel(user,u));
            }
            loaded = true;
        }

        public void add(TrackItemModel item)
        {
            trackItems.Add(item);
            NotifyPropertyChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
