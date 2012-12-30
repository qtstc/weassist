using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TestPhoneApp
{
    public partial class LoginPage : PhoneApplicationPage
    {

        private OverLay progressOverlay;

        public LoginPage()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string storedUsername = PhoneApplicationService.Current.State[App.APP_USERNAME_KEY] as string;
            string storedPassword = PhoneApplicationService.Current.State[App.APP_PASSWORD_KEY] as string;
            if (!storedUsername.Equals(string.Empty) && !storedPassword.Equals(string.Empty))
            {
                //Populate UI with previous data.
                login_username_textbox.Text = storedUsername;
                login_password_textbox.Password = storedPassword;
                login();
            }
        }

        private void Login_Signin_Button_Click(object sender, RoutedEventArgs e)
        {
            login();
        }

        /// <summary>
        /// Get user credentials from UI and try to log in.
        /// If successful, user credentials will be stored in PhoneApplicationService.
        /// User will than be redirected to the settings page.
        /// </summary>
        private void login()
        {
            string username = login_username_textbox.Text;
            string password = login_password_textbox.Password;
            UserManager.loginState validationResult = UserManager.validateUser(username, password);
            if (validationResult == UserManager.loginState.VALIDATED)
            {
                PhoneApplicationService.Current.State[App.APP_USERNAME_KEY] = username;
                PhoneApplicationService.Current.State[App.APP_PASSWORD_KEY] = password;
                PhoneApplicationService.Current.State[App.APP_VALIDATED_KEY] = true;
                NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
                NavigationService.RemoveBackEntry();
            }
        }
    }
}