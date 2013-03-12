using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using CitySafe.Resources;
using Parse;
using System.Diagnostics;
using ScheduledLocationAgent.Data;
using Microsoft.Phone.Notification;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.ComponentModel;

namespace CitySafe
{
    /// <summary>
    /// The login page for the application.
    /// Only shown when the user is not logged in.
    /// </summary>
    public partial class LoginPage : PhoneApplicationPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private const string DUMMY_PASSWORD = "DUMMY_PASSWORD";

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
          //Navigate driectly to the settings page if logged in.
            if (ParseUser.CurrentUser != null)
            {
                 await NavigateToSOSPage();
            } 
        }

        #region listener for UI.
        private void Login_Signin_Button_Click(object sender, RoutedEventArgs e)
        {
            loginWithProgressOverlay();
        }

        private void Login_Signup_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SignupPage.xaml", UriKind.Relative));
            //WebBrowserTask wbt = new WebBrowserTask();
            //wbt.Uri = new Uri("http://citysafe.azurewebsites.net/signup.php");
            //wbt.Show();
        }

        private void Forget_Password_Button_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.Uri = new Uri("http://citysafe.azurewebsites.net/forgetpassword.php");
            wbt.Show();
        }
        #endregion

        #region helper methods

        /// <summary>
        /// Getting username and password from UI and attemp to log in with parse.
        /// If login succeeded, navigate the user to the settings page.
        /// Also stores the user credential in the phone so the
        /// background agent can log in.
        /// </summary>
        private async void loginWithProgressOverlay()
        {
            if (ParseUser.CurrentUser != null && login_password_textbox.Password.Equals(DUMMY_PASSWORD)&& login_username_textbox.Text.Equals(ParseUser.CurrentUser.Username))
            {
                await NavigateToSOSPage();
                return;
            } 

            App.ShowProgressOverlay(AppResources.ProgressBar_VerifyingUser);
            string username = login_username_textbox.Text;
            string password = login_password_textbox.Password;
            try
            {
                await ParseUser.LogInAsync(username, password);
                Debug.WriteLine("Log in with " + username + " and " + password + " succeeded.");
                //Save the username and password to the app so the background agent can log in.
                Utilities.SaveParseCredential(username, password);
                //Remove the progress overlay.
                App.HideProgressOverlay();
                //Move to change user page if logged in successfully.
                await NavigateToSOSPage();
            }
            catch (ParseException e)//When login failed because of parse exception
            {
                Debug.WriteLine("Log in with " + username + " and " + password + " failed.");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Login_LoginWrongCredentials);
                //Remove the progress overlay.
                App.HideProgressOverlay();
            }
            catch (Exception e)//When login failed because of other exceptions(mostly network problem)
            {
                Debug.WriteLine("Log in with " + username + " and " + password + " failed.");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Login_LoginFailMessage);
                //Remove the progress overlay.
                App.HideProgressOverlay();
            }
        }


        /// <summary>
        /// Navigate to the SOS page after the user is logged in.
        /// It also remove the log in page from the stack.
        /// So when user tap the back button in the settings page,
        /// the application will exit instead of coming back to the login page.
        /// </summary>
        private async Task NavigateToSOSPage()
        {
            if (await SetUpAfterLogin())
            {
                NavigationService.Navigate(new Uri("/SOSPage.xaml", UriKind.Relative));
                NavigationService.RemoveBackEntry();
            }
        }

        /// <summary>
        /// Set up push notification and periodagent.
        /// </summary>
        private async Task<bool> SetUpAfterLogin()
        {
            App.ShowProgressOverlay(AppResources.Login_SettingUp);
            bool result = true;

            try
            {
                await ParseUser.CurrentUser.FetchAsync();
                //If does not contain one of the keys, initialize the user settings
                if (!ParseUser.CurrentUser.ContainsKey(ParseContract.UserTable.TRACKING_ENABLED))
                {
                    await InitializeUserSettings();
                }
                //Start the periodic agent even when the user does not navigate to the settings page.
                if (ParseUser.CurrentUser.Get<Boolean>(ParseContract.UserTable.TRACKING_ENABLED))
                    App.StartPeriodicAgent();
                //Start the push notification channel.
                try
                {
                    await SetUpChannel();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    MessageBox.Show(AppResources.SOS_FailToSetUpPush);
                }
                //Send the unsent location data to the server.
                UnsentLocationQueue queue = new UnsentLocationQueue(ParseUser.CurrentUser.Username);
                await queue.SendLocations(ParseUser.CurrentUser);
            }
            catch (Exception e)
            {
                login_username_textbox.Text = ParseUser.CurrentUser.Username;
                login_password_textbox.Password = DUMMY_PASSWORD;
                MessageBox.Show(AppResources.App_ConnectionError);
                result = false;
            }
            App.HideProgressOverlay();
            return result;
        }

        /// <summary>
        /// Helper method used to initialize user settings.
        /// Only called when the user uses the app for the first time.
        /// No exception handling.
        /// </summary>
        private async Task InitializeUserSettings()
        {
            ParseUser.CurrentUser[ParseContract.UserTable.TRACKING_ENABLED] = false;
            ParseUser.CurrentUser[ParseContract.UserTable.UPDATE_INTERVAL] = ParseContract.UserTable.DEFAULT_INTERVAL;
            ParseUser.CurrentUser[ParseContract.UserTable.LOCATION_DATA_SIZE] = ParseContract.UserTable.DEFAULT_DATA_SIZE;
            ParseUser.CurrentUser[ParseContract.UserTable.LAST_LOCATION_INDEX] = ParseContract.UserTable.DEFAULT_DATA_SIZE - 1;
            ParseUser.CurrentUser[ParseContract.UserTable.IN_DANGER] = false;
            ParseUser.CurrentUser[ParseContract.UserTable.WIN_PNONE_PUSH_URI] = "";
            ParseUser.CurrentUser[ParseContract.UserTable.NOTIFY_BY_EMAIL_STRANGER] = false;
            ParseUser.CurrentUser[ParseContract.UserTable.NOTIFY_BY_SMS_STRANGER] = false;
            ParseUser.CurrentUser[ParseContract.UserTable.NOTIFY_BY_PUSH_STRANGER] = false;
            await ParseUser.CurrentUser.SaveAsync();
        }

        #endregion

        #region Push Notification Code

        public const string CHANNEL_NAME = "CitySafeChannel";// The name of our push channel.

        private async Task SetUpChannel()
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
                ParseUser.CurrentUser[ParseContract.UserTable.WIN_PNONE_PUSH_URI] = pushChannel.ChannelUri.ToString();
                await ParseUser.CurrentUser.SaveAsync();
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
            //StringBuilder message = new StringBuilder();

            //message.AppendFormat("Received Toast {0}:\n", DateTime.Now.ToShortTimeString());

            //string relativeUri = string.Empty;
            // //Parse out the information that was part of the message.
            //foreach (string key in e.Collection.Keys)
            //{
            //    message.AppendFormat("{0}: {1}\n", key, e.Collection[key]);

            //    if (string.Compare(
            //        key,
            //        "wp:Param",
            //        System.Globalization.CultureInfo.InvariantCulture,
            //        System.Globalization.CompareOptions.IgnoreCase) == 0)
            //    {
            //        relativeUri = e.Collection[key];
            //    }
            //}

             //Display a dialog of all the fields in the toast.
            Dispatcher.BeginInvoke(() => MessageBox.Show(e.Collection["wp:Text2"]));
        }

        #endregion

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (App.HideProgressOverlay())
                e.Cancel = true;
        }
    }
}