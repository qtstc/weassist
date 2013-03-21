using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Parse;
using ScheduledLocationAgent.Data;
using System.Diagnostics;
using CitySafe.Resources;
using System.Threading;

namespace CitySafe.ViewModels
{
    /// <summary>
    /// This is the data context for the track page.
    /// It also takes care of the loading and 
    /// saving of data from and to the server.
    /// </summary>
    public class TrackViewModel : INotifyPropertyChanged
    {
        // A collection for ItemViewModel objects.
        public ObservableCollection<TrackItemModel> trackItems { get; private set; }

        // A bool used to indicate whether data was already loaded.
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
        public async Task LoadData(String mode,CancellationToken tk)
        {
            if (loaded)
                return;
            //First remove all previous data.
            trackItems.Clear();
            //Query the server for the tracking relations.
            var confirmedRelation = from relation in ParseObject.GetQuery(ParseContract.TrackRelationTable.TABLE_NAME).Include(ParseContract.TrackRelationTable.OtherRole(mode))
                                    where relation.Get<bool>(ParseContract.TrackRelationTable.TRACKED_VERIFIED) == true
                                     where relation.Get<bool>(ParseContract.TrackRelationTable.TRACKING_VERIFIED) == true
                                     where relation.Get<ParseUser>(mode) == ParseUser.CurrentUser
                                     select relation;
            IEnumerable<ParseObject> results = await confirmedRelation.FindAsync(tk);

            //For each tracking relations, get the user.
            foreach (ParseObject u in results)
            {
                ParseUser user = u.Get<ParseUser>(ParseContract.TrackRelationTable.OtherRole(mode));
                //await user.FetchAsync(tk);
                trackItems.Add(new TrackItemModel(user,u));
            }

            loaded = true;
        }

        /// <summary>
        /// Add a TrackItemModel to the list.
        /// </summary>
        /// <param name="item"></param>
        private void addToList(TrackItemModel item)
        {
            trackItems.Add(item);
            NotifyPropertyChanged();
        }

        /// <summary>
        /// Remove a TrackItemModel from the list
        /// </summary>
        /// <param name="relation"></param>
        public void remove(TrackItemModel item)
        {
            trackItems.Remove(item);
            NotifyPropertyChanged();
        }

        /// <summary>
        /// Add a user to the list.
        /// </summary>
        /// <param name="newEmail"> the email of the user, used to identify the user</param>
        /// <param name="role">the role of the current user</param>
        /// <returns>the result message</returns>
        public async Task<String> AddNewUser(string newEmail, string role, string verified,CancellationToken tk)
        {
            var query = from user in ParseUser.Query
                        where user.Get<string>("email") == newEmail
                        select user;
            IEnumerable<ParseObject> results = await query.FindAsync(tk);

            ParseObject trackRelation = new ParseObject(ParseContract.TrackRelationTable.TABLE_NAME);

            //TODO: check whether a user is adding himself

            bool needNewRecord = true;
            bool needTrackInvitation = true;
            String resultMessage = AppResources.Tracker_InvitationSuccess;
            ParseObject invited = null;

            //When the user is registered.
            if (results.Count() == 1)
            {
                invited = results.First<ParseObject>();
                var relationQuery = from relation in ParseObject.GetQuery(ParseContract.TrackRelationTable.TABLE_NAME)
                                    where relation.Get<ParseObject>(role) == ParseUser.CurrentUser
                                    && relation.Get<ParseObject>(ParseContract.TrackRelationTable.OtherRole(role)) == invited
                                    select relation;
                IEnumerable<ParseObject> relationResult = await relationQuery.FindAsync(tk);

                if (relationResult.Count() > 0)//If a record already exists.
                {
                    needNewRecord = false;
                    trackRelation = relationResult.First();
                    if (trackRelation.Get<bool>(ParseContract.TrackRelationTable.OtherVerified(verified)))//If the other user has already confirmed
                    {
                        needTrackInvitation = false;
                        if (trackRelation.Get<bool>(verified))
                        {
                            resultMessage = AppResources.Tracker_AlreadyInList;
                        }
                        else
                        {
                            ParseUser confirmed = trackRelation.Get<ParseUser>(ParseContract.TrackRelationTable.OtherRole(role));
                            await confirmed.FetchIfNeededAsync(tk);
                            addToList(new TrackItemModel(confirmed, trackRelation));
                            resultMessage = AppResources.Tracker_AddSuccess;
                        }
                    }
                    trackRelation[verified] = true;
                }
            }
            else if (results.Count() == 0)//When the user is not registered
            {
                //First check whether there is a record.
                var relationQuery = from relation in ParseObject.GetQuery(ParseContract.TrackRelationTable.TABLE_NAME)
                                    where relation.Get<ParseObject>(role) == ParseUser.CurrentUser
                                    && relation.Get<string>(ParseContract.TrackRelationTable.UNREGISTERED_USER_EMAIL) == newEmail
                                    select relation;
                IEnumerable<ParseObject> relationResult = await relationQuery.FindAsync(tk);

                if (relationResult.Count() > 0)//If old result exists
                {
                    needNewRecord = false;
                    trackRelation = relationResult.First();
                }
                else//If old result does not exist.
                {
                    //Use a place holder as the user
                    invited = null;
                    //Store the email
                    trackRelation[ParseContract.TrackRelationTable.UNREGISTERED_USER_EMAIL] = newEmail;
                }
                
                Debug.WriteLine("Send membership invitation");
                needTrackInvitation = false;
                string result = await ParseContract.CloudFunction.InviteNewUser(newEmail, role,tk);
                Debug.WriteLine("string returned " + result);
            }
            else
                throw new InvalidOperationException("Duplicate emails!");

            if (needNewRecord)
            {
                //Create a new record.
                trackRelation[role] = ParseUser.CurrentUser;
                if(invited != null)
                    trackRelation[ParseContract.TrackRelationTable.OtherRole(role)] = invited;
                trackRelation[verified] = true;
                trackRelation[ParseContract.TrackRelationTable.OtherVerified(verified)] = false;

                //Put all notification method as true as default.
                trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_PUSH] = true;
                trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_SMS] = true;
                trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_EMAIL] = true;

                //Put default location access as false
                trackRelation[ParseContract.TrackRelationTable.ALLOW_LOCATION_ACCESS] = false;
            }
            await trackRelation.SaveAsync(tk);

            if (needTrackInvitation)
            {
                Debug.WriteLine("Send track invitation");
                string result = await ParseContract.CloudFunction.SendTrackInvitation(trackRelation.Get<ParseUser>(ParseContract.TrackRelationTable.OtherRole(role)).ObjectId, role,trackRelation.ObjectId,tk);
                Debug.WriteLine("string returned " + result);
            }
            return resultMessage;
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
