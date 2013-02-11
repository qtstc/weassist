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

namespace CitySafe
{
    public partial class TrackPage : PhoneApplicationPage
    {
        public TrackPage()
        {
            InitializeComponent();
            App.trakcingModel = new TrackViewModel();
            TrackingList.DataContext = App.trakcingModel;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            await App.trakcingModel.LoadData(ParseContract.TrackRelationTable.TRACKING);
        }

        private void TrackingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected item is null (no selection) do nothing
            if (TrackingList.SelectedItem == null)
                return;

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/TrackingSettingsPage.xaml", UriKind.Relative));

            // Reset selected item to null (no selection)
            TrackingList.SelectedItem = null;
        }

        private async void TrackListAddButton_Click(object sender, RoutedEventArgs e)
        {
            //First check email address to see whether it is valid.
            String newEmail = TrackAddTextBox.Text;

            String resultMessage = AppResources.Tracker_InvitationSuccess;

            if (!IsValidEmail(newEmail))
            {
                MessageBox.Show(AppResources.Tracker_InvalidEmailMessage);
                return;
            }

            App.showProgressOverlay(AppResources.Tracker_SendingInvitation);

            try
            {
                var query = from user in ParseUser.Query
                            where user.Get<string>("email") == newEmail
                            select user;
                IEnumerable<ParseObject> results = await query.FindAsync();

                ParseObject invited = null;
                ParseObject trackRelation = new ParseObject(ParseContract.TrackRelationTable.TABLE_NAME);

                //When the user is registered.
                if (results.Count() == 1)
                    invited = results.First<ParseObject>();
                else if (results.Count() == 0)//When the user is not registered
                {
                    //Use a place holder as the user
                    invited = await ParseUser.Query.GetAsync(ParseContract.UserTable.DUMMY_USER);
                    //Store the email
                    trackRelation[ParseContract.TrackRelationTable.UNREGISTERED_USER_EMAIL] = newEmail;
                }

                //TODO: check whether a user is adding himself

                if (invited == null)
                    throw new InvalidOperationException("More than one user have the same email address " + newEmail);

                //Put all notification method as true as default.
                trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_PUSH] = true;
                trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_SMS] = true;
                trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_EMAIL] = true;

                if (TrackPivot.SelectedIndex == 0)//If adding to the tracking list. The current user is tracking, the other user is tracked.
                {
                    Debug.WriteLine("Adding user to the tracking table...");

                    bool needNewRecord = true;

                    //Check whether the other user has already sent an invitation to the current user.
                    //This only happens when the other user is registered.
                    if(invited.ObjectId != ParseContract.UserTable.DUMMY_USER)//If registered
                    {
                        var relationQuery = from relation in ParseObject.GetQuery(ParseContract.TrackRelationTable.TABLE_NAME)
                                            where relation.Get<ParseObject>(ParseContract.TrackRelationTable.TRACKING) == ParseUser.CurrentUser
                                            && relation.Get<ParseObject>(ParseContract.TrackRelationTable.TRACKED) == invited 
                                            select relation; 
                        IEnumerable<ParseObject> relationResult = await relationQuery.FindAsync();
                        
                        if(relationResult.Count()>0)//If a record already exists.
                        {
                            needNewRecord = false;
                            trackRelation = relationResult.First();
                            Debug.WriteLine("id:\n\n" + trackRelation.ObjectId);
                            trackRelation[ParseContract.TrackRelationTable.TRACKING_VERIFIED] = true;
                            if(trackRelation.Get<bool>(ParseContract.TrackRelationTable.TRACKED_VERIFIED))//If the other user has already confirmed
                            {
                                ParseUser confirmTracked = await ParseUser.Query.GetAsync(trackRelation.Get<String>(ParseContract.TrackRelationTable.TRACKED));
                                App.trakcingModel.add(new TrackItemModel(confirmTracked));
                            }
                            else
                            {
                                //TODO: send the invitation from the current user.
                                Debug.WriteLine("Send membership invitation to the new tracked");
                            }
                        }
                    }
                    else
                    {
                        //TODO: send membership invitation that includes the invitation from the current user.
                        Debug.WriteLine("Send membership invitation to the new tracked");
                    }
                    //TODO:send invitation
                    if(needNewRecord)
                    {
                        //Create a new record.
                        trackRelation[ParseContract.TrackRelationTable.TRACKING] = ParseUser.CurrentUser;
                        trackRelation[ParseContract.TrackRelationTable.TRACKED] = invited;
                        trackRelation[ParseContract.TrackRelationTable.TRACKING_VERIFIED] = true;
                        trackRelation[ParseContract.TrackRelationTable.TRACKED_VERIFIED] = false;
                    }
                }
                else if (TrackPivot.SelectedIndex == 1)//If adding to the tracked list. The current user is tracked, the other user is tracking.
                {
                    Debug.WriteLine("Adding user to the tracked table...");

                    bool needNewRecord = true;

                    //Check whether the other user has already sent an invitation to the current user.
                    //This only happens when the other user is registered.
                    if (invited.ObjectId != ParseContract.UserTable.DUMMY_USER)//If registered
                    {
                        var relationQuery = from relation in ParseObject.GetQuery(ParseContract.TrackRelationTable.TABLE_NAME)
                                            where relation.Get<ParseObject>(ParseContract.TrackRelationTable.TRACKED) == ParseUser.CurrentUser
                                            && relation.Get<ParseObject>(ParseContract.TrackRelationTable.TRACKING) == invited
                                            select relation;
                        IEnumerable<ParseObject> relationResult = await relationQuery.FindAsync();
                        if (relationResult.Count() > 0)//If a record already exists.
                        {
                            needNewRecord = false;
                            trackRelation = relationResult.First();
                            trackRelation[ParseContract.TrackRelationTable.TRACKED_VERIFIED] = true;
                            if (trackRelation.Get<bool>(ParseContract.TrackRelationTable.TRACKING_VERIFIED))//If the other user has already confirmed
                            {
                                ParseUser confirmTracking = await ParseUser.Query.GetAsync(trackRelation.Get<String>(ParseContract.TrackRelationTable.TRACKING));
                                App.trakcingModel.add(new TrackItemModel(confirmTracking));
                            }
                            else
                            {
                                //TODO: send the invitation from the current user.
                                Debug.WriteLine("Send invitation to the new tracking");
                            }
                        }
                    }
                    else
                    {
                        //TODO: send membership invitation that includes the invitation from the current user.
                        Debug.WriteLine("Send membership invitation to the new tracking");
                    }

                    if (needNewRecord)
                    {
                        //Create a new record.
                        trackRelation[ParseContract.TrackRelationTable.TRACKING] = ParseUser.CurrentUser;
                        trackRelation[ParseContract.TrackRelationTable.TRACKED] = invited;
                        trackRelation[ParseContract.TrackRelationTable.TRACKED_VERIFIED] = true;
                        trackRelation[ParseContract.TrackRelationTable.TRACKING_VERIFIED] = false;
                    }
                }

                await trackRelation.SaveAsync();
            }
            catch (Exception ex)
            {
                resultMessage = AppResources.Tracker_AddFailed;
                Debug.WriteLine("{"+ex.StackTrace);
                Debug.WriteLine(ex.ToString()+"}");
            }
            App.hideProgressOverlay();
            MessageBox.Show(resultMessage);
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