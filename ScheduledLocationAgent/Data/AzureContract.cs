
namespace ScheduledLocationAgent.Data
{
    /// <summary>
    /// The contract used for doing push notification on Azure,
    /// </summary>
    public static class AzureContract
    {
        public static class PushNotificationPost
        {
            public static string SUBSCRIPTION_URI { get { return "push_subscription_uri"; } }
            public static string TITLE { get { return "push_title"; } }
            public static string CONTENT { get { return "push_content"; } }
            public static string NAVIGATION_URI { get { return "push_navigation_uri"; } }
        }
    }
}
