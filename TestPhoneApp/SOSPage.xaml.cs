using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Parse;
using ScheduledLocationAgent.Data;
using CitySafe.Resources;
using System.Diagnostics;

namespace CitySafe
{
    public partial class SOSPage : PhoneApplicationPage
    {
        public SOSPage()
        {
            InitializeComponent();
        }

        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private async void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            try
            {
                App.ShowProgressOverlay(AppResources.SOS_SendingRequest);
                if (!ParseUser.CurrentUser.Get<bool>(ParseContract.UserTable.IN_DANGER))
                {
                    message = AppResources.SOS_SOSSentFail;
                    ParseObject sos = new ParseObject(ParseContract.SOSRequestTable.TABLE_NAME);
                    sos[ParseContract.SOSRequestTable.SENDER] = ParseUser.CurrentUser;
                    sos[ParseContract.SOSRequestTable.RESOLVED] = false;
                    sos[ParseContract.SOSRequestTable.sentLocation] = ParseContract.LocationTable.GeoPositionToParseObject(Utilities.getCurrentGeoPosition());
                    await sos.SaveAsync();

                    ParseUser.CurrentUser[ParseContract.UserTable.IN_DANGER] = true;

                    HelpButton.Content = AppResources.SOS_Resolved;
                    await ParseUser.CurrentUser.SaveAsync();
                    message = AppResources.SOS_SOSSentSuccess;
                }
                else
                {
                    App.ShowProgressOverlay(AppResources.SOS_CancelingRequest);
                    message = AppResources.SOS_SOSCanceledFail;
                    var requests = from request in ParseObject.GetQuery(ParseContract.SOSRequestTable.TABLE_NAME)
                                   where request.Get<ParseUser>(ParseContract.SOSRequestTable.SENDER) == ParseUser.CurrentUser
                                   select request;
                    IEnumerable<ParseObject> results = await requests.FindAsync();
                    foreach (ParseObject p in results)
                    {
                        p[ParseContract.SOSRequestTable.RESOLVED] = true;
                        await p.SaveAsync();

                    }

                    HelpButton.Content = AppResources.SOS_SendSOS;

                    ParseUser.CurrentUser[ParseContract.UserTable.IN_DANGER] = false;
                    await ParseUser.CurrentUser.SaveAsync();
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

        private void Manage_List_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/TrackPage.xaml", UriKind.Relative));
        }
    }
}