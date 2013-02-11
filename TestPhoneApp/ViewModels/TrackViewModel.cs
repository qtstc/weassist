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

namespace CitySafe.ViewModels
{
    public class TrackViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<TrackItemModel> trackItems { get; private set; }

        public TrackViewModel()
        {
            trackItems = new ObservableCollection<TrackItemModel>();
        }


        /// <summary>
        /// Load data from the parse server.
        /// </summary>
        /// <param name="mode">can be either ParseContract.TrackRelationTable.TRACKED or ParseContract.TrackRelationTable.TRACKING</param>
        public async Task LoadData(String mode)
        {
            ParseUser user = await ParseUser.Query.GetAsync(ParseContract.UserTable.DUMMY_USER);
            trackItems.Add(new TrackItemModel(user));

            //var confirmedRelation = from relation in ParseObject.GetQuery(ParseContract.TrackRelationTable.TABLE_NAME)
            //                        where relation.Get<bool>(ParseContract.TrackRelationTable.TRACKED_VERIFIED) == true 
            //                        && relation.Get<bool>(ParseContract.TrackRelationTable.TRACKING_VERIFIED) == true 
            //                        select relation;
            //var allQualifiedUsers = from user in ParseUser.Query
            //                        join relation in confirmedRelation on user equals relation.Get<ParseUser>(mode)
            //                        select user;
            //var users = from user in allQualifiedUsers
            //            where user == ParseUser.CurrentUser
            //            select user;

            //IEnumerable<ParseUser> relationResult = await users.FindAsync();
            //trackItems = new ObservableCollection<TrackItemModel>();
            //foreach (ParseUser u in relationResult)
            //{
            //    trackItems.Add(new TrackItemModel(u));
            //}
            //Debug.WriteLine(trackItems.Count + "Haha");
        }

        public void add(TrackItemModel item)
        {
            trackItems.Add(item);
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
    }
}
