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
    public partial class ConnectionsTile : UserControl
    {
         public ConnectionsTile()
        {
            InitializeComponent();
            Storyboard anim = (Storyboard)FindName("liveTileAnim1");
            anim.Begin();
        }

        private void liveTileAnim1_Completed_1(object sender, EventArgs e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnim1_Inverse");
            anim.Begin();
        }

        private void liveTileAnim1_Inverse_Completed_1(object sender, EventArgs e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnim1");
            anim.Begin();
        }
    }
}
