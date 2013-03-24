using System.Windows.Controls;

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
