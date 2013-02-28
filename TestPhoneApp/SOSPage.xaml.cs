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
using Microsoft.Phone.Notification;
using System.Text;

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
            App.ShowProgressOverlay(AppResources.SOS_SettingUpPush);
            try
            {
                SetUpChannel();
            }
            catch (Exception e)
            {
                MessageBox.Show(AppResources.SOS_FailToSetUpPush);
            }
            App.HideProgressOverlay();
        }

        #region Push Notification Code

        public const string CHANNEL_NAME = "CitySafeChannel";// The name of our push channel.

        private void SetUpChannel()
        {
            /// Holds the push channel that is created or found.
            HttpNotificationChannel pushChannel;

            // Try to find the push channel.
            pushChannel = HttpNotificationChannel.Find(CHANNEL_NAME);

            // If the channel was not found, then create a new connection to the push service.
            if (pushChannel == null)
            {
                pushChannel = new HttpNotificationChannel(CHANNEL_NAME);

                // Register for all the events before attempting to open the channel.
                pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);

                // Register for this notification only if you need to receive the notifications while your application is running.
                pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

                pushChannel.Open();

                // Bind this new channel for toast events.
                pushChannel.BindToShellToast();

            }
            else
            {
                // The channel was already open, so just register for all the events.
                pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);

                // Register for this notification only if you need to receive the notifications while your application is running.
                pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);
                Debug.WriteLine(String.Format("Channel Uri is {0}", pushChannel.ChannelUri.ToString()));
            }
        }

        /// <summary>
        /// Called every time the channel uri is changed.
        /// It basically updates the uri stored in the parse server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                // Display the new URI for testing purposes.   Normally, the URI would be passed back to your web service at this point.
                Debug.WriteLine(String.Format("New Channel Uri is {0}", e.ChannelUri.ToString()));
                ParseUser.CurrentUser[ParseContract.UserTable.WIN_PNONE_PUSH_URI] = e.ChannelUri.ToString();
                ParseUser.CurrentUser.SaveAsync();
            });
        }

        /// <summary>
        /// Error handling code.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            // Error handling logic for your particular application would be here.
            Dispatcher.BeginInvoke(() =>
                MessageBox.Show(String.Format("A push notification {0} error occurred.  {1} ({2}) {3}",
                    e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData))
                    );
        }

        /// <summary>
        /// Listener for receiving toast notification while the app is in the foreground.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            StringBuilder message = new StringBuilder();
            string relativeUri = string.Empty;

            message.AppendFormat("Received Toast {0}:\n", DateTime.Now.ToShortTimeString());

            // Parse out the information that was part of the message.
            foreach (string key in e.Collection.Keys)
            {
                message.AppendFormat("{0}: {1}\n", key, e.Collection[key]);

                if (string.Compare(
                    key,
                    "wp:Param",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    relativeUri = e.Collection[key];
                }
            }

            // Display a dialog of all the fields in the toast.
            Dispatcher.BeginInvoke(() => MessageBox.Show(message.ToString()));
        }

        #endregion

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
                    sos[ParseContract.SOSRequestTable.sentLocation] = ParseContract.LocationTable.GeoPositionToParseObject(await Utilities.getCurrentGeoPosition());
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