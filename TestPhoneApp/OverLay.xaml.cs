using System.Windows.Controls;

namespace CitySafe
{
    public partial class OverLay : UserControl
    {
        public OverLay()
        {
            InitializeComponent();
        }

        public void setText(string text)
        {
            Overlay_Textblock.Text = text;
        }
    }
}
