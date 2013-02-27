using System;
using System.Text;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using Windows.Devices.Geolocation;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;

namespace ScheduledLocationAgent.Data
{
    /// <summary>
    /// A class to store static functions.
    /// </summary>
    public class Utilities
    {
        #region Parse Credential Storage

        private const String SETTINGS_USERNAME_KEY = "settings_username_key";//Key used to store username in the IsolatedStorageSettings.
        private const String SETTINGS_PASSWORD_KEY = "settings_password_key";//Key used to store password in the IsolatedStorageSettings.

        /// <summary>
        /// Store the usernmae and password to the IsolatedStorageSettings.ApplicationSettings.
        /// The password will be encrypted.
        /// </summary>
        /// <param name="username">The username to be stored</param>
        /// <param name="password">The password to be stored</param>
        public static void SaveParseCredential(String username, String password)
        {
             var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), null);
             IsolatedStorageSettings.ApplicationSettings[SETTINGS_PASSWORD_KEY] = encrypted;
             IsolatedStorageSettings.ApplicationSettings[SETTINGS_USERNAME_KEY] = username;
             IsolatedStorageSettings.ApplicationSettings.Save();
        }

        /// <summary>
        /// Get the username and password from the IsolatedStorageSettings.ApplicationSettings.
        /// </summary>
        /// <returns>A String array with the first element being the username and the second being the password.
        /// Both elements will be empty string if nothing is found.
        /// </returns>
        public static String[] GetParseCredential()
        {
            String storedUsername = "";
            String storedPassword = "";
            if (IsolatedStorageSettings.ApplicationSettings.Contains(SETTINGS_USERNAME_KEY))
                 storedUsername = IsolatedStorageSettings.ApplicationSettings[SETTINGS_USERNAME_KEY] as string;
             if(IsolatedStorageSettings.ApplicationSettings.Contains(SETTINGS_USERNAME_KEY))
             {
                 var bytes = IsolatedStorageSettings.ApplicationSettings[SETTINGS_PASSWORD_KEY] as byte[];
                 var unEncrypted = ProtectedData.Unprotect(bytes, null);
                 storedPassword = Encoding.UTF8.GetString(unEncrypted, 0, unEncrypted.Length);
             }
             return new String[] { storedUsername, storedPassword };
        }

        #endregion

        #region Location
        /// <summary>
        /// Get the curent geo position
        /// </summary>
        /// <returns></returns>
        public static async Task<GeoPosition<GeoCoordinate>> getCurrentGeoPosition()
        {
            return new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.Now), new GeoCoordinate(1.1, 2.2));

            //Geolocator locator = new Geolocator();
            //locator.DesiredAccuracy = PositionAccuracy.High;
            //Geoposition position = await locator.GetGeopositionAsync();
            //GeoCoordinate coor = ConvertGeocoordinate(position.Coordinate);
            //return new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.Now),coor);
        }

        /// <summary>
        /// Convert a Geocoordinate instance to a GeoCoordinate instance
        /// </summary>
        /// <param name="geocoordinate">the Geocoordinate to be converted</param>
        /// <returns>the resulted GeoCoordinate instance</returns>
        public static GeoCoordinate ConvertGeocoordinate(Geocoordinate geocoordinate)
        {
            return new GeoCoordinate
                (
                geocoordinate.Latitude,
                geocoordinate.Longitude,
                geocoordinate.Altitude ?? Double.NaN,
                geocoordinate.Accuracy,
                geocoordinate.AltitudeAccuracy ?? Double.NaN,
                geocoordinate.Speed ?? Double.NaN,
                geocoordinate.Heading ?? Double.NaN
                );
        }
        #endregion

        /// <summary>
        /// Show a toast to the user.
        /// Cannot be used in the foreground app.
        /// </summary>
        /// <param name="title">the title of the toast</param>
        /// <param name="content">the content of the toast</param>
        /// <param name="uri">the navigation uri</param>
        public static void ShowToast(String title, String content, Uri uri)
        {
            ShellToast s = new ShellToast();
            s.Title = title;
            s.NavigationUri = uri;
            s.Content = content;
            s.Show();
        }
    }
}
