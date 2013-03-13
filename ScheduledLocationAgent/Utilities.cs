using System;
using System.Text;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using Windows.Devices.Geolocation;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using Parse;

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
            //return new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.Now), new GeoCoordinate(1.1, 2.2));

            try
            {
                Geolocator locator = new Geolocator();
                locator.DesiredAccuracy = PositionAccuracy.High;
                Geoposition position = await locator.GetGeopositionAsync(
                maximumAge: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(10)
                );
                GeoCoordinate coor = ConvertGeocoordinate(position.Coordinate);
                return new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.Now), coor);
            }
            catch
            {
                return null;
            }
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

        /// <summary>
        /// Save a new location data to the parse user.
        /// This method does not save the data to the server.
        /// Need to call SaveAsync on the user afterwards.
        /// </summary>
        /// <param name="user">the user whose location is to be saved</param>
        /// <param name="newData">the new location data</param>
        public static void SaveLocationToParseUser(ParseUser user, GeoPosition<GeoCoordinate> newData)
        {
            int lastLocationIndex = user.Get<int>(ParseContract.UserTable.LAST_LOCATION_INDEX);
            int newLocationIndex = lastLocationIndex + 1;
            newLocationIndex %= user.Get<int>(ParseContract.UserTable.LOCATION_DATA_SIZE);
            user[ParseContract.UserTable.LAST_LOCATION_INDEX] = newLocationIndex;


            if (!user.ContainsKey(ParseContract.UserTable.LOCATION(newLocationIndex)))//If the new slot was not filled.
            {
                //Create a new location.
                user[ParseContract.UserTable.LOCATION(newLocationIndex)] = ParseContract.LocationTable.GeoPositionToParseObject(newData);
            }
            else//If the new slot contains a valid location
            {
                ParseObject newLocation = user.Get<ParseObject>(ParseContract.UserTable.LOCATION(newLocationIndex));
                //Change the location entry without creating a new one.
                ParseContract.LocationTable.GeoPositionSetParseObject(newData, newLocation);
            }
            user[ParseContract.UserTable.LAST_LOCATION] = user.Get<ParseObject>(ParseContract.UserTable.LOCATION(newLocationIndex));
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


        #region Debugging Log
        public const string LOG_FILE_NAME = "ExceptionLog.dat";
        public static void WriteToExceptionLog(String tag, DateTime time, Exception e)
        {
            LogEntry log = new LogEntry(tag, time, e.ToString(), e.StackTrace);
            IsolatedStorageHelper.WriteObjectToFileUsingJson<LogEntry>(true,LOG_FILE_NAME, log,"randomMutex");
        }
        #endregion
    }
}
