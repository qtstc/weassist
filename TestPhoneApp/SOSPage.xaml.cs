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


        public SOSPage()
        {
            InitializeComponent();

            //Change the text of the help button if the user is in danger.
            if (ParseUser.CurrentUser.Get<Boolean>(ParseContract.UserTable.IN_DANGER))
                HelpButton.Content = AppResources.SOS_Resolve;
        }


        private async void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            CancellationToken tk = App.ShowProgressOverlay(AppResources.SOS_SendingRequest);
            try
            {
                if (!ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.IN_DANGER))
                {
                    message = AppResources.SOS_SOSSentFail;
                    ParseObject sos = new ParseObject(ParseContract.SOSRequestTable.TABLE_NAME);
                    sos[ParseContract.SOSRequestTable.SENDER] = ParseUser.CurrentUser;
                    sos[ParseContract.SOSRequestTable.RESOLVED] = false;
                    GeoPosition<GeoCoordinate> current = await Utilities.getCurrentGeoPosition();
                    if (current == null)
                    {
                        message = AppResources.Map_CannotObtainLocation;
                        throw new InvalidOperationException("Cannot access location");
                    }
                    sos[ParseContract.SOSRequestTable.sentLocation] = ParseContract.LocationTable.GeoPositionToParseObject(current);
                    await sos.SaveAsync(tk);

                    string result = await ParseContract.CloudFunction.NewSOSCall(sos.ObjectId,tk);
                    Debug.WriteLine("string returned " + result);

                    ParseUser.CurrentUser[ParseContract.UserTable.IN_DANGER] = true;
                    HelpButton.Content = AppResources.SOS_Resolve;

                    await ParseUser.CurrentUser.SaveAsync(tk);
                    message = AppResources.SOS_SOSSentSuccess;
                }
                else
                {
                    App.ShowProgressOverlay(AppResources.SOS_CancelingRequest);
                    message = AppResources.SOS_SOSCanceledFail;
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

                    await ParseUser.CurrentUser.SaveAsync(tk);
                    message = AppResources.SOS_SOSCanceledSuccess;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            App.HideProgressOverlay();
            MessageBox.Show(message);
        }


        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (App.HideProgressOverlay())
                e.Cancel = true;
        }

        #region UI Listener for Navigation
        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void Manage_List_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/TrackPage.xaml", UriKind.Relative));
        }
        #endregion
    }
}