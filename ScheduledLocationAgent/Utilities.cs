using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Device.Location;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;

namespace ScheduledLocationAgent.Data
{
    public class Utilities
    {
        private const String SETTINGS_USERNAME_KEY = "settings_username_key";
        private const String SETTINGS_PASSWORD_KEY = "settings_password_key";

        public static void SaveParseCredential(String username, String password)
        {
             var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), null);
             IsolatedStorageSettings.ApplicationSettings[SETTINGS_PASSWORD_KEY] = encrypted;
             IsolatedStorageSettings.ApplicationSettings[SETTINGS_USERNAME_KEY] = username;
             IsolatedStorageSettings.ApplicationSettings.Save();
        }

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

        /// <summary>
        /// Get the curent geo position
        /// </summary>
        /// <returns></returns>
        public static GeoPosition<GeoCoordinate> getCurrentGeoPosition()
        {
            return new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.Now), new GeoCoordinate(1.1, 2.2));
        }
    }
}
