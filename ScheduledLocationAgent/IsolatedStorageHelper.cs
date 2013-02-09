using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using Newtonsoft.Json;

namespace ScheduledLocationAgent.Data
{
    /// <summary>
    /// Original from http://dotnetapp.com/blog/2012/11/25/how-to-avoid-application-settings-corruption-when-a-background-agent-is-used/?utm_source=rss&utm_medium=rss&utm_campaign=how-to-avoid-application-settings-corruption-when-a-background-agent-is-used.
    /// Added save string methods for ease of use.
    /// The methods in this class are used because saving data using IsolatedStorageSettings is not thread-safe when both the foreground app and background agent are accessing the data.
    /// </summary>
    public static class IsolatedStorageHelper
    {
        #region Fields

        private static Mutex _mutex;

        #endregion

        #region Methods

        public static void WriteStringToFile(string fileName, String stringToSave, string mutexName = null)
        {
            if (mutexName != null && _mutex == null)
            {
                _mutex = new Mutex(false, mutexName);
            }

            if (_mutex != null)
            {
                _mutex.WaitOne();
            }

            try
            {
                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isolatedStorageFileStream = isolatedStorageFile.CreateFile(fileName))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(isolatedStorageFileStream))
                        {
                            try
                            {
                                streamWriter.Write(stringToSave);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            finally
            {
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex();
                }
            }
        }



        public static string ReadStringFromFile(string fileName, string mutexName = null)
        {
            if (mutexName != null && _mutex == null)
            {
                _mutex = new Mutex(false, mutexName);
            }

            if (_mutex != null)
            {
                _mutex.WaitOne();
            }

            string result = "";

            try
            {
                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isolatedStorageFileStream = isolatedStorageFile.OpenFile(fileName, FileMode.Open))
                    {
                        using (StreamReader streamReader = new StreamReader(isolatedStorageFileStream))
                        {
                            try
                            {
                                result = streamReader.ReadToEnd();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            finally
            {
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex();
                }
            }

            return result;
        }

        public static void WriteObjectToFileUsingJson<T>(string fileName, T objectToSave, string mutexName = null) where T : class
        {
            if (mutexName != null && _mutex == null)
            {
                _mutex = new Mutex(false, mutexName);
            }

            if (_mutex != null)
            {
                _mutex.WaitOne();
            }

            try
            {
                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isolatedStorageFileStream = isolatedStorageFile.CreateFile(fileName))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(isolatedStorageFileStream))
                        {
                            try
                            {
                                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All };


#if DEBUG
                                string json = JsonConvert.SerializeObject(objectToSave, Formatting.Indented, jsonSerializerSettings);
#else
                                string json = JsonConvert.SerializeObject(objectToSave, Formatting.None, jsonSerializerSettings);
#endif
                                streamWriter.Write(json);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            finally
            {
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex();
                }
            }
        }



        public static T ReadObjectFromFileUsingJson<T>(string fileName, string mutexName = null) where T : class
        {
            if (mutexName != null && _mutex == null)
            {
                _mutex = new Mutex(false, mutexName);
            }

            if (_mutex != null)
            {
                _mutex.WaitOne();
            }

            T result = default(T);

            try
            {
                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isolatedStorageFileStream = isolatedStorageFile.OpenFile(fileName, FileMode.Open))
                    {
                        using (StreamReader streamReader = new StreamReader(isolatedStorageFileStream))
                        {
                            try
                            {
                                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All };

                                string json = streamReader.ReadToEnd();

                                result = JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            finally
            {
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex();
                }
            }

            return result;
        }

        public static bool IsFileExist(string fileName)
        {
            bool isExist;

            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                isExist = isolatedStorageFile.FileExists(fileName);
            }

            return isExist;
        }

        #endregion
    }
}