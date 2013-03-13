using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Parse;
using ScheduledLocationAgent.Data;
using CitySafe.Resources;
using System.Diagnostics;
using Microsoft.Phone.Shell;
using System.Device.Location;
using System.Threading;
using System.ComponentModel;

namespace CitySafe
{
    /// <summary>
    /// This is the navigation page for user after logging in.
    /// Since it is the first page the user saw after logging,
    /// it also takes care of the setting up of push notifications,
    /// which is only available after user signning in.
    /// </summary>
    public partial class SOSPage : PhoneApplicationPage
    {

        private bool noCancel;

        public SOSPage()
        {
            InitializeComponent();
            noCancel = false;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (noCancel)
            {
                e.Cancel = true;
                return;
            }
            if (App.HideProgressOverlay())
                e.Cancel = true;
        }

        private async void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.IN_DANGER))
            {
                NavigationService.Navigate(new Uri("/SOSSendPage.xaml", UriKind.Relative));
                return;
            }

            string message = AppResources.SOS_SOSCanceledFail;
            CancellationToken tk = App.ShowProgressOverlay(AppResources.SOS_CancelingRequest);
            try
            {
                //TODO: do this in the cloud.
                var requests = from request in ParseObject.GetQuery(ParseContract.SOSRequestTable.TABLE_NAME)
                               where request.Get<ParseUser>(ParseContract.SOSRequestTable.SENDER) == ParseUser.CurrentUser
                               select request;
                IEnumerable<ParseObject> results = await requests.FindAsync(tk);
                foreach (ParseObject p in results)
                {
                    p[ParseContract.SOSRequestTable.RESOLVED] = true;
                    await p.SaveAsync(tk);
                }

                ParseUser.CurrentUser[ParseContract.UserTable.IN_DANGER] = false;
                HelpButton.Content = AppResources.SOS_SendSOS;
                noCancel = true;
                await ParseUser.CurrentUser.SaveAsync(tk);
                message = AppResources.SOS_SOSCanceledSuccess;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            noCancel = false;
            App.HideProgressOverlay();
            MessageBox.Show(message);
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(AreaMapPage.GetResolvedUri());
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(AreaMapPage.GetUnresolvedUri());
        }

        #region UI Listener for Navigation
        private void Settings_Button_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void Manage_List_Button_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/TrackPage.xaml", UriKind.Relative));
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Change the text of the help button if the user is in danger.
            if (ParseUser.CurrentUser.Get<Boolean>(ParseContract.UserTable.IN_DANGER))
                HelpButton.Content = AppResources.SOS_Resolve;
            else
                HelpButton.Content = AppResources.SOS_SendSOS;
        }
    }
}