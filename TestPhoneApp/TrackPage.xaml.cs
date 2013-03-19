using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using CitySafe.ViewModels;
using System.Diagnostics;
using ScheduledLocationAgent.Data;
using System.Text.RegularExpressions;
using CitySafe.Resources;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using System.Threading;
using System.ComponentModel;

namespace CitySafe
{
    /// <summary>
    /// This the page that contains the tracking and tracked lists.
    /// </summary>
    public partial class TrackPage : PhoneApplicationPage
    {
        public TrackPage()
        {
            InitializeComponent();
            App.trackingModel = new TrackViewModel();
            App.trackedModel = new TrackViewModel();
            TrackingList.DataContext = App.trackingModel;
            TrackedList.DataContext = App.trackedModel;
        }

        /// <summary>
        /// Load the UI data from the server.
        /// </summary>
        private async Task LoadUIData()
        {
            CancellationToken tk = App.ShowProgressOverlay(AppResources.Tracker_LoadList);
            string message = "";
            try
            {
                await App.trackingModel.LoadData(ParseContract.TrackRelationTable.TRACKING, tk);
                await App.trackedModel.LoadData(ParseContract.TrackRelationTable.TRACKED, tk);
            }
            catch(OperationCanceledException)
            {
                Debug.WriteLine("Load track page canceled");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                message = AppResources.Tracker_FailToLoadList;
                NavigationService.GoBack();
            }
            App.HideProgressOverlay();

            if (!message.Equals(""))
                MessageBox.Show(message);
        }

        #region track list setup
        private void TrackingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NavigateToTrackSettingsPage(TrackingList, "/TrackingSettingsPage.xaml");
        }

        private void TrackedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NavigateToTrackSettingsPage(TrackedList, "/TrackedSettingsPage.xaml");
        }

        /// <summary>
        /// Helper method used to navigate to a track settings page.
        /// </summary>
        /// <param name="trackList">the list from which the selection happens</param>
        /// <param name="pageUri">URI of the page navigating to</param>
        private void NavigateToTrackSettingsPage(LongListSelector trackList, String pageUri)
        {
            // If selected item is null (no selection) do nothing
            if (trackList.SelectedItem == null)
                return;

            // Save the selected model as a global variable.
            // It contains the info of the other user and the track relation.
            // So the track settings page does not need to query the server again for them.
            TrackItemModel model = trackList.SelectedItem as TrackItemModel;
            App.trackItemModel = model;

            // Navigate to the new page
            NavigationService.Navigate(new Uri(pageUri, UriKind.Relative));

            // Reset selected item to null (no selection)
            trackList.SelectedItem = null;
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

            CancellationToken tk = App.ShowProgressOverlay(AppResources.Tracker_SendingInvitation);

            string message = AppResources.Tracker_InvitationSuccess;

            try
            {
                if (TrackPivot.SelectedIndex == 0)//If adding to the tracking list. The current user is tracking, the other user is tracked.
                {
                    Debug.WriteLine("Adding user to the tracking table...");
                    message = await App.trackingModel.AddNewUser(newEmail, ParseContract.TrackRelationTable.TRACKING, ParseContract.TrackRelationTable.TRACKING_VERIFIED, tk);
                }
                else if (TrackPivot.SelectedIndex == 1)//If adding to the tracked list. The current user is tracked, the other user is tracking.
                {
                    Debug.WriteLine("Adding user to the tracked table...");
                    message = await App.trackedModel.AddNewUser(newEmail, ParseContract.TrackRelationTable.TRACKED, ParseContract.TrackRelationTable.TRACKED_VERIFIED, tk);
                }

            }
            catch (Exception ex)
            {
                message = AppResources.Tracker_AddFailed;
                Debug.WriteLine(ex.ToString());
            }
            App.HideProgressOverlay();
            MessageBox.Show(message);
        }

        #endregion

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

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            App.HideProgressOverlay();
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            await LoadUIData();
        }
    }
}