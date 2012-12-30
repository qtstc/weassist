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

namespace TestPhoneApp
{
    public partial class SignupPage : PhoneApplicationPage
    {
        public SignupPage()
        {
            InitializeComponent();
            signupWithProgressOverlay();
        }

        private async void signupWithProgressOverlay()
        {
            var user = new ParseUser()
            {
                Username = "tao",
                Password = "p",
                Email = "email@example.com"
            };

            // other fields can be set just like with ParseObject
            user["phone"] = "415-392-0202";

            await user.SignUpAsync();
        }
    }
}