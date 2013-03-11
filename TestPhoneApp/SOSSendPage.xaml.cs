using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Parse;
using ScheduledLocationAgent.Data;
using CitySafe.Resources;
using System.Threading;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.IO.IsolatedStorage;

namespace CitySafe
{
    public partial class SOSSendPage : PhoneApplicationPage
    {
        private CameraCaptureTask camera;
        private BitmapImage defaultAddPhoto;
        private MemoryStream imageStream;
        private bool noCancel;

        public SOSSendPage()
        {
            InitializeComponent();
            camera = new CameraCaptureTask();
            camera.Completed += camera_Completed;

            defaultAddPhoto = new BitmapImage(new Uri("/Assets/addphoto.png", UriKind.Relative));
            AddPhotoImage.Source = defaultAddPhoto;
            imageStream = null;
            noCancel = false;
            SOSMessage.MaxLength = ParseContract.SOSRequestTable.MAX_MESSAGE_LENGTH;
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            this.Focus();//Get rid of the keyboard if there is any.
            String message = AppResources.SOS_SOSSentFail;
            CancellationToken tk = App.ShowProgressOverlay(AppResources.SOS_SendingRequest);
            ApplicationBar.IsVisible = false;

            try
            {
                ParseObject sos = new ParseObject(ParseContract.SOSRequestTable.TABLE_NAME);
                sos[ParseContract.SOSRequestTable.SENDER] = ParseUser.CurrentUser;
                sos[ParseContract.SOSRequestTable.RESOLVED] = false;
                sos[ParseContract.SOSRequestTable.MESSAGE] = SOSMessage.Text;

                if (imageStream != null)
                {
                    ParseFile image = new ParseFile(ParseContract.SOSRequestTable.SOS_IMAGE_FILE_NAME, imageStream.ToArray());
                    await image.SaveAsync(tk);
                    Debug.WriteLine(image.Name + " is saved to the server.");
                    sos[ParseContract.SOSRequestTable.IMAGE] = image;
                }

                GeoPosition<GeoCoordinate> current = await Utilities.getCurrentGeoPosition();
                if (current == null)
                {
                    message = AppResources.Map_CannotObtainLocation;
                    throw new InvalidOperationException("Cannot access location");
                }
                sos[ParseContract.SOSRequestTable.SENT_LOCATION] = ParseContract.LocationTable.GeoPositionToParseObject(current);
                await sos.SaveAsync(tk);
                noCancel = true;
                string result = await ParseContract.CloudFunction.NewSOSCall(sos.ObjectId,tk);
                Debug.WriteLine("string returned " + result);
                ParseUser.CurrentUser[ParseContract.UserTable.IN_DANGER] = true;

                await ParseUser.CurrentUser.SaveAsync(tk);//No cancellation because the sos request is already sent.
                message = AppResources.SOS_SOSSentSuccess;
                NavigationService.GoBack();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                ApplicationBar.IsVisible = true;
            }
            App.HideProgressOverlay();
            MessageBox.Show(message);
            noCancel = false;
        }

        private void DeletePhotoImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AddPhotoImage.Source = defaultAddPhoto;
            imageStream = null;
            DeletePhotoImage.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void AddPhotoImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            camera.Show();
        }

        void camera_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage tmp = new BitmapImage();
                tmp.SetSource(e.ChosenPhoto);
                WriteableBitmap bmp = new WriteableBitmap(tmp);
                int[] dimension = ParseContract.SOSRequestTable.GetImageWidthHeight(bmp.PixelWidth, bmp.PixelHeight);

                Debug.WriteLine(dimension[0] + " " + dimension[1]);
                imageStream = new MemoryStream();
                bmp.SaveJpeg(imageStream, dimension[0], dimension[1], 0,100);//Resize and save the image to the stream

                bmp.SetSource(imageStream);

                AddPhotoImage.Source = bmp;

                DeletePhotoImage.Visibility = System.Windows.Visibility.Visible;
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //Don't cancel when can't cancel
            if (noCancel)
            {
                e.Cancel = true;
                return;
            }
            if (App.HideProgressOverlay())
                e.Cancel = true;
        }

        #region save and load sos message
        private const string SOS_MESSAGE_KEY_SUFFIX = "_sos_message";

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(ParseUser.CurrentUser.ObjectId + SOS_MESSAGE_KEY_SUFFIX))
                SOSMessage.Text = IsolatedStorageSettings.ApplicationSettings[ParseUser.CurrentUser.ObjectId + SOS_MESSAGE_KEY_SUFFIX] as string;
            else
                SOSMessage.Text = AppResources.SOSSend_DefaultMessage;
            CharCount.Text = SOSMessage.Text.Length + "/" + ParseContract.SOSRequestTable.MAX_MESSAGE_LENGTH;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings[ParseUser.CurrentUser.ObjectId + SOS_MESSAGE_KEY_SUFFIX] = SOSMessage.Text;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
        #endregion

        /// <summary>
        /// Event handler used to change the char count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SOSMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            CharCount.Text = SOSMessage.Text.Length + "/" + ParseContract.SOSRequestTable.MAX_MESSAGE_LENGTH;
        }
    }
}