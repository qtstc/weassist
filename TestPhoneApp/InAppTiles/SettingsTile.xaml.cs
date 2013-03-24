using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Animation;

namespace CitySafe
{
    public partial class SettingsTile : UserControl
    {
        public SettingsTile()
        {
            InitializeComponent();
        }

        public void SetLocationUpdateText(string info)
        {
            locationEnabledTextBlock.Text = info;
        }

        public void SetReceivingNotification(string info)
        {
            notificationTextBlock.Text = info;
        }
    }
}
