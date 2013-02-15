using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CitySafe.ViewModels;
using System.Diagnostics;
using ScheduledLocationAgent.Data;
using System.Text.RegularExpressions;
using CitySafe.Resources;
using Parse;
using System.Threading.Tasks;

namespace CitySafe
{
    public partial class TrackPage : PhoneApplicationPage
    {
        public TrackPage()
        {
            InitializeComponent();
            App.trackingModel = new TrackViewModel();
            App.trackedModel = new TrackViewModel();
            TrackingList.DataContext = App.trackingModel;
            TrackedList.DataContext = App.trackedModel;
            loadLists();
        }

        //protected async override void OnNavigatedTo(NavigationEventArgs e)
        private async void loadLists()
        {
            App.ShowProgressOverlay(AppResources.Tracker_LoadList);
            try
            {
                await App.trackingModel.LoadData(ParseContract.TrackRelationTable.TRACKING);
                await App.trackedModel.LoadData(ParseContract.TrackRelationTable.TRACKED);
            }
            catch
            {
                MessageBox.Show(AppResources.Tracker_FailToLoadList);
            }
            App.HideProgressOverlay();
        }

        private void TrackingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NavigateToTrackSettingsPage(TrackingList, "/TrackingSettingsPage.xaml");
        }

        private void TrackedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NavigateToTrackSettingsPage(TrackedList, "/TrackedSettingsPage.xaml");
        }

        private void NavigateToTrackSettingsPage(LongListSelector trackList, String pageUri)
        {
            // If selected item is null (no selection) do nothing
            if (trackList.SelectedItem == null)
                return;

            //Save the info of the other user and the track relation to global variable.
            //So the track settings page does not need to query the server again for them.
            TrackItemModel model = trackList.SelectedItem as TrackItemModel;
            App.trackUser = model.user;
            App.trackRelation = model.relation;

            // Navigate to the new page
            NavigationService.Navigate(new Uri(pageUri, UriKind.Relative));

            // Reset selected item to null (no selection)
            trackList.SelectedItem = null;
        }

        /// <summary>
        /// Add a user to either tracked list or tracking list.
        /// </summary>
        /// <param name="newEmail"> the email of the user, used to identify the user</param>
        /// <param name="list">the list to be add</param>
        /// <param name="role">the role of the current user</param>
        /// <returns>the result message</returns>
        private async Task<String> AddtoList(String newEmail, TrackViewModel list, string role, string verified)
        {
            var query = from user in ParseUser.Query
                        where user.Get<string>("email") == newEmail
                        select user;
            IEnumerable<ParseObject> results = await query.FindAsync();

            ParseObject trackRelation = new ParseObject(ParseContract.TrackRelationTable.TABLE_NAME);

            //Put all notification method as true as default.
            trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_PUSH] = true;
            trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_SMS] = true;
            trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_EMAIL] = true;

            //TODO: check whether a user is adding himself

            bool needNewRecord = true;
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
                IEnumerable<ParseObject> relationResult = await relationQuery.FindAsync();

                if (relationResult.Count() > 0)//If a record already exists.
                {
                    needNewRecord = false;
                    trackRelation = relationResult.First();
                    trackRelation[verified] = true;
                    if (trackRelation.Get<bool>(ParseContract.TrackRelationTable.OtherVerified(verified)))//If the other user has already confirmed
                    {
                        ParseUser confirmed = trackRelation.Get<ParseUser>(ParseContract.TrackRelationTable.OtherRole(role));
                        await confirmed.FetchIfNeededAsync();
                        list.add(new TrackItemModel(confirmed, trackRelation));
                        resultMessage = AppResources.Tracker_AddSuccess;
                    }
                    else
                    {
                        //TODO: send the invitation from the current user.
                        Debug.WriteLine("Send membership invitation");
                    }
                }
            }
            else if (results.Count() == 0)//When the user is not registered
            {
                //Use a place holder as the user
                invited = await ParseUser.Query.GetAsync(ParseContract.UserTable.DUMMY_USER);
                //Store the email
                trackRelation[ParseContract.TrackRelationTable.UNREGISTERED_USER_EMAIL] = newEmail;

                //TODO: send membership invitation that includes the invitation from the current user.
                Debug.WriteLine("Send membership invitation");
            }
            else
                throw new InvalidOperationException("Duplicate emails!");

            if (needNewRecord)
            {
                //Create a new record.
                trackRelation[role] = ParseUser.CurrentUser;
                trackRelation[ParseContract.TrackRelationTable.OtherRole(role)] = invited;
                trackRelation[verified] = true;
                trackRelation[ParseContract.TrackRelationTable.OtherVerified(verified)] = false;
            }

            await trackRelation.SaveAsync();
            return resultMessage;
        }

        private async void TrackListAddButton_Click(object sender, RoutedEventArgs e)
        {
            //First check email address to see whether it is valid.
            String newEmail = TrackAddTextBox.Text;

            if (!IsValidEmail(newEmail))
            {
                MessageBox.Show(AppResources.Tracker_InvalidEmailMessage);
                return;
            }

            App.ShowProgressOverlay(AppResources.Tracker_SendingInvitation);

            string message = AppResources.Tracker_InvitationSuccess;

            try
            {
                 if (TrackPivot.SelectedIndex == 0)//If adding to the tracking list. The current user is tracking, the other user is tracked.
                {
                    Debug.WriteLine("Adding user to the tracking table...");
                    message = await AddtoList(newEmail, App.trackingModel, ParseContract.TrackRelationTable.TRACKING, ParseContract.TrackRelationTable.TRACKING_VERIFIED);
                 }
                 else if (TrackPivot.SelectedIndex == 1)//If adding to the tracked list. The current user is tracked, the other user is tracking.
                 {
                     Debug.WriteLine("Adding user to the tracked table...");
                     message = await AddtoList(newEmail, App.trackedModel, ParseContract.TrackRelationTable.TRACKED, ParseContract.TrackRelationTable.TRACKED_VERIFIED);
                 }

            }
            catch (Exception ex)
            {
                message = AppResources.Tracker_AddFailed;
                Debug.WriteLine("{" + ex.StackTrace);
                Debug.WriteLine(ex.ToString() + "}");
            }
            App.HideProgressOverlay();
            MessageBox.Show(message);
        }


        /// <summary>
        /// Validate email address
        /// </summary>
        /// <param name="strIn">the email address</param>
        /// <returns>true if it is valid</returns>
        public static bool IsValidEmail(string strIn)
        {
            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn,
                   @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$");
        }

    }
}