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
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Media;
using CitySafe.Resources;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using Delay;
using System.Threading;
using System.Threading.Tasks;

namespace CitySafe
{
    /// <summary>
    /// A page used to show the infomation of a sos request.
    /// It has three sections, with the first one being the sos message,
    /// the second one being the sos photo and the last one being the sender info.
    /// This page has two modes, depending on the URI used. In the public mode,
    /// the sender's info will be displayed, where as in the private mode,
    /// the sender's info will not be displayed.
    /// </summary>
    public partial class SOSInfoPage : PhoneApplicationPage
    {
        /// <summary>
        /// Used to identify different modes of this page.
        /// Used in the URI.
        /// </summary>
        private const string MODE_KEY = "sos_mode";
        private const string PUBLIC_MODE = "public";
        private const string PRIVATE_MODE = "private";

        public SOSInfoPage()
        {
            InitializeComponent();
            
            //Show/hide the photo page.
            SOSMessage.Text = App.sosRequestInfo.Get<String>(ParseContract.SOSRequestTable.MESSAGE);
            if (App.sosRequestInfo.ContainsKey(ParseContract.SOSRequestTable.IMAGE))
                LowProfileImageLoader.SetUriSource(SOSImage, App.sosRequestInfo.Get<ParseFile>(ParseContract.SOSRequestTable.IMAGE).Url);
            else
                PhotoPage.Visibility = System.Windows.Visibility.Collapsed;

            //Show/hide the message page.
            if (App.sosRequestInfo.Get<string>(ParseContract.SOSRequestTable.MESSAGE).Equals(""))
                MessagePage.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Load the infomation of the sender.
        /// And change the UI.
        /// To be called only when the sender wants to share his/her info.
        /// </summary>
        /// <returns></returns>
        private async Task LoadSenderInfo()
        {
            CancellationToken tk = App.ShowProgressOverlay(AppResources.SOSInfo_LoadUserInfo);
            try
            {
                ParseUser sender = App.sosRequestInfo.Get<ParseUser>(ParseContract.SOSRequestTable.SENDER);
                await sender.FetchIfNeededAsync(tk);

                if (App.sosRequestInfo.Get<bool>(ParseContract.SOSRequestTable.SHARE_EMAIL))
                    EmailTextBlock.Text = sender.Email;
                else
                    EmailPanel.Visibility = System.Windows.Visibility.Collapsed;

                if (App.sosRequestInfo.Get<bool>(ParseContract.SOSRequestTable.SHARE_NAME))
                    NameTextBlock.Text = sender.Get<string>(ParseContract.UserTable.FIRST_NAME) + " " + sender.Get<string>(ParseContract.UserTable.LAST_NAME);
                else
                    NamePanel.Visibility = System.Windows.Visibility.Collapsed;

                if (App.sosRequestInfo.Get<bool>(ParseContract.SOSRequestTable.SHARE_PHONE))
                    PhoneTextBlock.Text = sender.Get<string>(ParseContract.UserTable.PHONE);
                else
                    PhonePanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch(OperationCanceledException e)
            {
                Debug.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.SOSInfo_PhotoLoadFail);
            }
            App.HideProgressOverlay();
        }

        /// <summary>
        /// Give different URI for different pages.
        /// Public page has the sender info section.
        /// </summary>
        /// <returns></returns>
        public static Uri GetPulicInfoUri()
        {
            return new Uri("/SOSInfoPage.xaml?" + MODE_KEY + "=" + PUBLIC_MODE, UriKind.Relative);
        }

        /// <summary>
        /// Geve different URI for different pages.
        /// Private page does not have sender info section.
        /// </summary>
        /// <returns></returns>
        public static Uri GetPrivateInfoUri()
        {
            return new Uri("/SOSInfoPage.xaml?" + MODE_KEY + "=" + PRIVATE_MODE, UriKind.Relative);
        }


        /// <summary>
        /// Change the UI depending on the data.
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationContext.QueryString[MODE_KEY].Equals(PRIVATE_MODE))//If in private mode.
                InfoPage.Visibility = System.Windows.Visibility.Collapsed;
            else if (!App.sosRequestInfo.Get<bool>(ParseContract.SOSRequestTable.SHARE_EMAIL)//If in public mode and no info is allowed to be shown.
                && !App.sosRequestInfo.Get<bool>(ParseContract.SOSRequestTable.SHARE_PHONE)
                && !App.sosRequestInfo.Get<bool>(ParseContract.SOSRequestTable.SHARE_NAME))
                InfoPage.Visibility = System.Windows.Visibility.Collapsed;
            else//If info is allowed to be shown.
                await LoadSenderInfo();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            App.sosRequestInfo = null;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            App.HideProgressOverlay();
        }
    }
}