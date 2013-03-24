using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CitySafe
{
    public partial class HelpTile : UserControl
    {
        public HelpTile()
        {
            InitializeComponent();
            Storyboard anim = (Storyboard)FindName("liveTileAnimTop");
            anim.Begin();
        }

        public void SetSOSText(String s)
        {
            SOSTextblock.Text = s;
        }

        private void liveTileAnimTop_Completed_1(object sender, EventArgs e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnimBottom");
            anim.Begin();
        }

        private void liveTileAnimBottom_Completed_1(object sender, EventArgs e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnimTop");
            anim.Begin();
        }
    }
}
