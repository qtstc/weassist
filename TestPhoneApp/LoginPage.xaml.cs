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
using TestPhoneApp.Resources;
using Parse;
using System.Diagnostics;

namespace TestPhoneApp
{
    public partial class LoginPage : PhoneApplicationPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ParseUser.CurrentUser != null)
            {
                navigateToSettingsPage();
            }
        }

        private void Login_Signin_Button_Click(object sender, RoutedEventArgs e)
        {
            loginWithProgressOverlay();
        }

        private void Login_Signup_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SignupPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Getting username and password from UI and attemp to log in with parse.
        /// If login succeeded, navigate the user to the settings page.
        /// Show the progress overlay before calling this async method.
        /// </summary>
        private async void loginWithProgressOverlay()
        {
            App.showProgressOverlay(AppResources.ProgressBar_VerifyingUser);
            string username = login_username_textbox.Text;
            string password = login_password_textbox.Password;
            try
            {
                await ParseUser.LogInAsync(username, password);
                Debug.WriteLine("Log in with " + username + " and " + password + " succeeded.");
                //Move to change user page if logged in successfully.
                navigateToSettingsPage();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Log in with " + username + " and " + password + " failed.");
                Debug.WriteLine(e.ToString());
            }
            //Remove the progress overlay.
            App.hideProgressOverlay();
        }

        /// <summary>
        /// Navigate to the settings page after the user is logged in.
        /// It also remove the log in page from the stack.
        /// So when user tap the back button in the settings page,
        /// the application will exit instead of coming back to the login page.
        /// </summary>
        private void navigateToSettingsPage()
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
            NavigationService.RemoveBackEntry();
        }
    }
}