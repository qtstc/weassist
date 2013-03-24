using System;
using System.Windows;
using System.Windows.Controls;
using CitySafe.Resources;
using Parse;
using System.Diagnostics;

namespace CitySafe
{
    public partial class PasswordResetDialog : UserControl
    {
        public PasswordResetDialog()
        {
            InitializeComponent();
        }

        private void ok_Button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowProgressOverlay(AppResources.Login_SendingResetEmail);
            try
            {
                ParseUser.RequestPasswordResetAsync(email_TextBox.Text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                App.HideProgressOverlay();
                MessageBox.Show(AppResources.Login_FailSendResetEmail);
                return;
            }
            m_Popup.IsOpen = false;
        }

        private void cancel_Buton_Click(object sender, RoutedEventArgs e)
        {
            m_Popup.IsOpen = false;
        }
    }
}
