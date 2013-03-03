using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using ScheduledLocationAgent.Data;
using System.Diagnostics;

namespace CitySafePushWebService
{
    public partial class CitySafePush : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            NameValueCollection nvc = Request.Form;

            LiteralControl lt = new LiteralControl();
            this.Controls.Add(lt);

            if (string.IsNullOrEmpty(nvc[AzureContract.PushNotificationPost.TITLE]) ||
                string.IsNullOrEmpty(nvc[AzureContract.PushNotificationPost.CONTENT]) ||
                string.IsNullOrEmpty(nvc[AzureContract.PushNotificationPost.SUBSCRIPTION_URI]) ||
                string.IsNullOrEmpty(nvc[AzureContract.PushNotificationPost.NAVIGATION_URI]))
            {
                lt.Text = "Error, one of the required post fields is empty.";
                return;
            }
            
            string title, content, subscriptionUri, navigationUri;

            title = nvc[AzureContract.PushNotificationPost.TITLE];
            content = nvc[AzureContract.PushNotificationPost.CONTENT];
            subscriptionUri = nvc[AzureContract.PushNotificationPost.SUBSCRIPTION_URI];
            navigationUri = nvc[AzureContract.PushNotificationPost.NAVIGATION_URI];
            lt.Text = "Toast title:" + title + "\ncontent:" + content + "\nnavigation uri:" + navigationUri + "\nphone uri:" + subscriptionUri;

            sendPushNotification(subscriptionUri, title, content, navigationUri);
        }

        /// <summary>
        /// Send a toast push notification to the phone with the given uri.
        /// </summary>
        /// <param name="subscriptionUri">the uri of the phone receiving the push notification</param>
        /// <param name="title">title of the toast message</param>
        /// <param name="content">content of the toast message</param>
        /// <param name="navigationUri">relative navigation uri of the toast,starts with a "/"</param>
        /// <returns>string indicating the result of the sending push notification</returns>
        private String sendPushNotification(String subscriptionUri, String title, String content, String navigationUri)
        {
            try
            {
                HttpWebRequest sendNotificationRequest = (HttpWebRequest)WebRequest.Create(subscriptionUri);

                // Create an HTTPWebRequest that posts the toast notification to the Microsoft Push Notification Service.
                // HTTP POST is the only method allowed to send the notification.
                sendNotificationRequest.Method = "POST";

                // The optional custom header X-MessageID uniquely identifies a notification message. 
                // If it is present, the same value is returned in the notification response. It must be a string that contains a UUID.
                // sendNotificationRequest.Headers.Add("X-MessageID", "<UUID>");

                // Create the toast message.
                string toastMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<wp:Notification xmlns:wp=\"WPNotification\">" +
                   "<wp:Toast>" +
                        "<wp:Text1>" + title + "</wp:Text1>" +
                        "<wp:Text2>" + content + "</wp:Text2>" +
                        "<wp:Param>" + navigationUri + "</wp:Param>" +
                   "</wp:Toast> " +
                "</wp:Notification>";

                // Set the notification payload to send.
                byte[] notificationMessage = Encoding.Default.GetBytes(toastMessage);

                // Set the web request content length.
                sendNotificationRequest.ContentLength = notificationMessage.Length;
                sendNotificationRequest.ContentType = "text/xml";
                sendNotificationRequest.Headers.Add("X-WindowsPhone-Target", "toast");
                sendNotificationRequest.Headers.Add("X-NotificationClass", "2");


                using (Stream requestStream = sendNotificationRequest.GetRequestStream())
                {
                    requestStream.Write(notificationMessage, 0, notificationMessage.Length);
                }

                // Send the notification and get the response.
                HttpWebResponse response = (HttpWebResponse)sendNotificationRequest.GetResponse();
                string notificationStatus = response.Headers["X-NotificationStatus"];
                string notificationChannelStatus = response.Headers["X-SubscriptionStatus"];
                string deviceConnectionStatus = response.Headers["X-DeviceConnectionStatus"];

                // return the response from the Microsoft Push Notification Service.  
                // Normally, error handling code would be here. In the real world, because data connections are not always available,
                // notifications may need to be throttled back if the device cannot be reached.
                return notificationStatus + " | " + deviceConnectionStatus + " | " + notificationChannelStatus;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }
}