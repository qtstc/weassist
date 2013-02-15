using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CitySafe.ViewModels;
using CitySafe.Resources;
using Parse;
using ScheduledLocationAgent.Data;
using System.Diagnostics;

namespace CitySafe
{
    public partial class TrackingSettingsPage : PhoneApplicationPage
    {
        private TrackingSettings trackingSettings;

        public TrackingSettingsPage()
        {
            InitializeComponent();
            LoadTrackingSettings();
        }

        /// <summary>
        /// Load tracking settings.
        /// </summary>
        private async void LoadTrackingSettings()
        {
            App.ShowProgressOverlay(AppResources.TrackingSetting_Loading);
            try
            {
                //Use the data to populate UI.
                bool usePushNotification = App.trackRelation.Get<bool>(ParseContract.TrackRelationTable.NOTIFY_BY_PUSH);
                bool useSMS = App.trackRelation.Get<bool>(ParseContract.TrackRelationTable.NOTIFY_BY_SMS);
                bool useEmail = App.trackRelation.Get<bool>(ParseContract.TrackRelationTable.NOTIFY_BY_EMAIL);
                TrackingTitle.Text = App.trackUser.Username;
                trackingSettings = new TrackingSettings(usePushNotification, useSMS, useEmail);
                TrackingSettingsPanel.DataContext = trackingSettings;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to initialize tracking settings to the server:\n");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.TrackingSetting_FailToSync);
            }
            App.HideProgressOverlay();
        }

        /// <summary>
        /// Helper method used to save tracking settings.
        /// </summary>
        private async void SaveTrackingSettings()
        {
            App.ShowProgressOverlay(AppResources.TrackingSetting_Saving);
            try
            {
                //Not all fields are saved.
                App.trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_PUSH] = trackingSettings.usePushNotification;
                App.trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_SMS] = trackingSettings.useSMS;
                App.trackRelation[ParseContract.TrackRelationTable.NOTIFY_BY_EMAIL] = trackingSettings.useEmail;
                await App.trackRelation.SaveAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fail to save user settings to the server:\n");
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.TrackingSetting_FailToSync);
            }
            App.HideProgressOverlay();
        }

        private void Apply_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveTrackingSettings();
        }
    }
}