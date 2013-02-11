using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CitySafe.Resources;
using Parse;

namespace CitySafe
{
    /// <summary>
    /// This is the page for signing up. Currently not used.
    /// The web page sign up interface will be used.
    /// </summary>
    public partial class SignupPage : PhoneApplicationPage
    {
        public SignupPage()
        {
            InitializeComponent();
            signupWithProgressOverlay();
        }

        private async void signupWithProgressOverlay()
        {
            App.showProgressOverlay(AppResources.ProgressBar_SigningUpUser);
            var user = new ParseUser()
            {
                Username = "dummyuser",
                Password = "dummypassword",
                Email = "dummyuser@depauw.edu"
            };

            await user.SignUpAsync();
            App.hideProgressOverlay();
        }
    }
}