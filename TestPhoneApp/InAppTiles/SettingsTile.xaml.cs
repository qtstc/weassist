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
            Storyboard anim = (Storyboard)FindName("liveTileAnim1_Part1");
            anim.Begin();
        }

        public void SetLocationUpdateText(string info)
        {
            locationEnabledTextBlock.Text = info;
        }

        public void SetReceivingNotification(string info)
        {
            notificationTextBlock.Text = info;
        }

        private void liveTileAnim1_Part1_Completed_1(object sender, EventArgs e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnim1_Part2");
            anim.Begin();
        }

        private void liveTileAnim1_Part2_Completed_1(object sender, EventArgs e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnim2_Part1");
            anim.Begin();
        }
        private void liveTileAnim2_Part1_Completed_1(object sender, EventArgs e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnim2_Part2");
            anim.Begin();
        }

        private void liveTileAnim2_Part2_Completed_1(object sender, EventArgs e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnim1_Part1");
            anim.Begin();
        }
    }
}
