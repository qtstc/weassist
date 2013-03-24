using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Parse;
using ScheduledLocationAgent.Data;
using CitySafe.Resources;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Phone.Notification;
using System.Windows.Input;
using Microsoft.Phone.Tasks;

namespace CitySafe
{
    public partial class SOSPage : PhoneApplicationPage
    {
        private bool noCancel;//Flag used to prevent cancellation when sending request.

        public SOSPage()
        {
            InitializeComponent();
            noCancel = false;
        }


        private async void HelpButton_Tap(object sender, GestureEventArgs e)
        {
            if (!ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.IN_DANGER))
            {
                NavigationService.Navigate(new Uri("/SOSSendPage.xaml", UriKind.Relative));
                return;
            }

            string message = AppResources.SOS_SOSCanceledFail;
            ApplicationBar.IsVisible = false;
            CancellationToken tk = App.ShowProgressOverlay(AppResources.SOS_CancelingRequest);
            try
            {
                //TODO: do this in the cloud.
                var requests = from request in ParseObject.GetQuery(ParseContract.SOSRequestTable.TABLE_NAME)
                               where request.Get<ParseUser>(ParseContract.SOSRequestTable.SENDER) == ParseUser.CurrentUser
                               where request.Get<Boolean>(ParseContract.SOSRequestTable.RESOLVED) == false
                               select request;
                IEnumerable<ParseObject> results = await requests.FindAsync(tk);
                foreach (ParseObject p in results)
                {
                    p[ParseContract.SOSRequestTable.RESOLVED] = true;
                    await p.SaveAsync(tk);
                }

                ParseUser.CurrentUser[ParseContract.UserTable.IN_DANGER] = false;
                helpTile.SetSOSText(AppResources.SOS_SendSOS);
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
            ApplicationBar.IsVisible = true;
            MessageBox.Show(message);
        }

        #region UI Listener for Navigation
        private void CheckButton_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(AreaMapPage.GetResolvedUri());
        }

        private void Settings_Button_Click(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void Manage_List_Button_Click(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/TrackPage.xaml", UriKind.Relative));
        }

        private void SaveButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(AreaMapPage.GetUnresolvedUri());
        }
        #endregion

        /// <summary>
        /// Lisenter for the change user appbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeUserButton_Click(object sender, EventArgs e)
        {
            CancellationToken tk = App.ShowProgressOverlay(AppResources.Setting_Loggingout);
            try
            {
                HttpNotificationChannel.Find(LoginPage.CHANNEL_NAME).Close();
                ParseUser.CurrentUser[ParseContract.UserTable.WIN_PNONE_PUSH_URI] = "";
                await ParseUser.CurrentUser.SaveAsync(tk);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Log out cancelled");
            }
            catch
            {
                App.HideProgressOverlay();
                MessageBox.Show(AppResources.Setting_FailToLogOut);
                return;
            }
            ParseUser.LogOut();
            Utilities.SaveParseCredential("", "");//Also clear the user credential stored in the phone.
            App.RemoveAgent();
            //Go back to login page
            NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            //Remove back entry. Prevent user from coming back to settings page by pressing back button
            //when he or she is on the login page
            NavigationService.RemoveBackEntry();
            NavigationService.RemoveBackEntry();
            App.HideProgressOverlay();
        }

        private void PrivacyStatementButton_Click(object sender, EventArgs e)
        {
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.Uri = new Uri("http://weassist.azurewebsites.net/privacystatement.php");
            wbt.Show();
        }

        private void InstructionsButton_Click(object sender, EventArgs e)
        {
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.Uri = new Uri("http://weassist.azurewebsites.net/instructions.php");
            wbt.Show();
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(AppResources.SOS_About);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            //Change the text of the help button if the user is in danger.
            if (ParseUser.CurrentUser.Get<Boolean>(ParseContract.UserTable.IN_DANGER))
                helpTile.SetSOSText(AppResources.SOS_Resolve);
            else
                helpTile.SetSOSText(AppResources.SOS_SendSOS);

            if (ParseUser.CurrentUser.Get<Boolean>(ParseContract.UserTable.TRACKING_ENABLED))
                settingsTile.SetLocationUpdateText(AppResources.SOS_TrackingOn);
            else
                settingsTile.SetLocationUpdateText(AppResources.SOS_TrackingOff);

            if (ParseUser.CurrentUser.Get<Boolean>(ParseContract.UserTable.NOTIFY_BY_EMAIL_STRANGER)
                || ParseUser.CurrentUser.Get<Boolean>(ParseContract.UserTable.NOTIFY_BY_PUSH_STRANGER)
                || ParseUser.CurrentUser.Get<Boolean>(ParseContract.UserTable.NOTIFY_BY_SMS_STRANGER))
                settingsTile.SetReceivingNotification(AppResources.SOS_NotificationOn);
            else
                settingsTile.SetReceivingNotification(AppResources.SOS_NotificationOff);
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
    }
}