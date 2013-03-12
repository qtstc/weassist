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

namespace CitySafe
{
    public partial class SOSInfoPage : PhoneApplicationPage
    {
        public SOSInfoPage()
        {
            InitializeComponent();
            SOSMessage.Text = App.sosRequestInfo.Get<String>(ParseContract.SOSRequestTable.MESSAGE);
            if (App.sosRequestInfo.ContainsKey(ParseContract.SOSRequestTable.IMAGE))
            LowProfileImageLoader.SetUriSource(SOSImage, App.sosRequestInfo.Get<ParseFile>(ParseContract.SOSRequestTable.IMAGE).Url);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            App.sosRequestInfo = null;
        }
    }
}