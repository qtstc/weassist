using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Controls.Primitives;
using CitySafe.Resources;
using Parse;
using System.Diagnostics;
using Microsoft.Phone.Tasks;
using ScheduledLocationAgent.Data;

namespace CitySafe
{
    public partial class LoginPage : PhoneApplicationPage
    {

        private const string LOGIN_PAGE_URL = "http://www.google.com";

        public LoginPage()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Navigate driectly to the settings page if logged in.
            if (ParseUser.CurrentUser != null)
            {
                navigateToSOSPage();
            } 
        }

        private void Login_Signin_Button_Click(object sender, RoutedEventArgs e)
        {
            loginWithProgressOverlay();
        }

        private void Login_Signup_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SignupPage.xaml", UriKind.Relative));
            //WebBrowserTask wbt = new WebBrowserTask();
            //wbt.Uri = new Uri(LOGIN_PAGE_URL);
            //wbt.Show();
        }

        /// <summary>
        /// Getting username and password from UI and attemp to log in with parse.
        /// If login succeeded, navigate the user to the settings page.
        /// Show the progress overlay before calling this async method.
        /// </summary>
        private async void loginWithProgressOverlay()
        {
            App.ShowProgressOverlay(AppResources.ProgressBar_VerifyingUser);
            string username = login_username_textbox.Text;
            string password = login_password_textbox.Password;
            try
            {
                await ParseUser.LogInAsync(username, password);
                Debug.WriteLine("Log in with " + username + " and " + password + " succeeded.");
                //Save the username and password to the app so the background agent can log in.
                Utilities.SaveParseCredential(username, password);
                //Move to change user page if logged in successfully.
                navigateToSOSPage();
            }
            catch (ParseException e)//When login failed because of parse exception
            {
                Debug.WriteLine("Log in with " + username + " and " + password + " failed.");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Login_LoginWrongCredentials);
            }
            catch (Exception e)//When login failed because of other exceptions(mostly network problem)
            {
                Debug.WriteLine("Log in with " + username + " and " + password + " failed.");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Login_LoginFailMessage);
            }
            //Remove the progress overlay.
            App.HideProgressOverlay();
        }

        /// <summary>
        /// Navigate to the SOS page after the user is logged in.
        /// It also remove the log in page from the stack.
        /// So when user tap the back button in the settings page,
        /// the application will exit instead of coming back to the login page.
        /// </summary>
        private void navigateToSOSPage()
        {
            NavigationService.Navigate(new Uri("/SOSPage.xaml", UriKind.Relative));
            NavigationService.RemoveBackEntry();
        }
    }
}