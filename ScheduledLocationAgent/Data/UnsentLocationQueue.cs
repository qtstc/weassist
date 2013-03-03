using Parse;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledLocationAgent.Data
{
    /// <summary>
    /// A class used to store and manage unsent locations.
    /// It is used in the background agent to store the location data
    /// that was not sent to the Parse server because of connection issues.
    /// </summary>
    public class UnsentLocationQueue
    {
        private UnsentLocations locationQueue;
        private const string MUTEX_SUFFIX = "_location_queue_mutex";
        private const string FILE_NAME_SUFFIX = "_location_queue.dat";
        string username;

        /// <summary>
        /// Create an instance of a queue used to store unsent locations for a specific user.
        /// If a previous copy already exists, it will be loaded instead of creating a new one.
        /// </summary>
        /// <param name="username">the ParseUser username used to identify the queue.</param>
        public UnsentLocationQueue(string username)
        {
            this.username = username;
            if (IsolatedStorageHelper.IsFileExist(username + FILE_NAME_SUFFIX))
            {
                locationQueue = IsolatedStorageHelper.ReadObjectFromFileUsingJson<UnsentLocations>(username + FILE_NAME_SUFFIX, username + MUTEX_SUFFIX);
            }
            else
            {
                locationQueue = new UnsentLocations();
                locationQueue.queue = new GeoPosition<GeoCoordinate>[ParseContract.UserTable.DEFAULT_DATA_SIZE];
                locationQueue.begin = 0;
                locationQueue.end = 0;
                for (int i = 0; i < locationQueue.queue.Length; i++)
                    locationQueue.queue[i] = null;
            }
        }

        /// <summary>
        /// Save the unsent locations to the phone.
        /// Must be called at the end of operations.
        /// </summary>
        /// <returns>true if save succeeded.</returns>
        public Boolean Save()
        {
            try
            {
                IsolatedStorageHelper.WriteObjectToFileUsingJson(username + FILE_NAME_SUFFIX, locationQueue, username + MUTEX_SUFFIX);
            }
            catch {
                return false;
            }
            return true;
        }

        /// <summary>
        /// The interval at which location data is taken.
        /// This value is synced with the updateInterval column in the Locations table of Parse.
        /// </summary>
        public int UpdateInterval { set { locationQueue.updateInterval = value; } get { return locationQueue.updateInterval; } }

        /// <summary>
        /// The last time when the location is updated.
        /// This value is synced with the lastUpdate column in the Locations table of Parse.
        /// </summary>
        public DateTime LastUpdate { set { locationQueue.lastUpdate = value; } get { return locationQueue.lastUpdate; } }

        /// <summary>
        /// Return the number of locations in the queue.
        /// The maximum value is the size of the queue.
        /// </summary>
        /// <returns>the number of locations in the queue</returns>
        public int QueueSize()
        {
            int result = 0;
            if (locationQueue.queue[locationQueue.begin] == null)
                result = 0;
            else
            {
                result = locationQueue.end - locationQueue.begin;
                if (result <= 0)
                    result += locationQueue.queue.Length;
            }
            Debug.WriteLine("Queue size is: " + result);
            return result;
        }

        /// <summary>
        /// Add a location to the queue.
        /// </summary>
        /// <param name="item">the location to be added</param>
        public void Enqueue(GeoPosition<GeoCoordinate> item)
        {
            if (QueueSize() == locationQueue.queue.Length)//If the queue is full.
                Dequeue();
            locationQueue.queue[locationQueue.end] = item;
            LastUpdate = item.Timestamp.DateTime;
            locationQueue.end++;
            if (locationQueue.end == locationQueue.queue.Length)
                locationQueue.end = 0;
        }

        /// <summary>
        /// Remove a location from the queue.
        /// If the queue is empty an exception will be thrown.
        /// </summary>
        /// <returns>the removed item.</returns>
        private GeoPosition<GeoCoordinate> Dequeue()
        {
            GeoPosition<GeoCoordinate> first = locationQueue.queue[locationQueue.begin];
            if (first == null)//If queue is empty
                throw new ArgumentNullException("The queue is empty");
            locationQueue.queue[locationQueue.begin] = null;
            locationQueue.begin++;
            if (locationQueue.begin == locationQueue.queue.Length)
                locationQueue.begin = 0;
            return first;
        }

        /// <summary>
        /// Send the unsent locations to the server.
        /// </summary>
        /// <param name="user">the user whose location data is to be sent</param>
        /// <returns>true if successful</returns>
        public async Task<bool> SendLocations(ParseUser user)
        {
            Queue<GeoPosition<GeoCoordinate>> temp = new Queue<GeoPosition<GeoCoordinate>>();
            if (QueueSize() == 0)
                return true;
            try
            {
                await user.FetchIfNeededAsync();
                while (QueueSize() > 0)
                {
                    GeoPosition<GeoCoordinate> last = Dequeue();
                    temp.Enqueue(last);
                    Utilities.SaveLocationToParseUser(user, last);
                }
                await user.SaveAsync();
            }
            catch(Exception e)
            {
                Debug.WriteLine("Failed to send the unsent locations");
                Debug.WriteLine(e.ToString());
                while (temp.Count > 0)
                {
                    Enqueue(temp.Dequeue());
                }
                return false;
            }
            return true;
        }
    }
}
