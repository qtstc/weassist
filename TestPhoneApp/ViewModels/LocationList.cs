using CitySafe.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CitySafe.ViewModels
{
    public class LocationList<Pushpin> : List<Pushpin>
    {
        /// <summary>
        /// The Key of this group.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="key">The key for this group.</param>
        public LocationList(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Group a list of pushpins by their timestamp.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="hours"></param>
        /// <returns></returns>
        public static List<LocationList<ViewModels.Pushpin>> GroupByTime(LocationList<ViewModels.Pushpin> l, int hours)
        {
            List<LocationList<ViewModels.Pushpin>> finalList = new List<LocationList<ViewModels.Pushpin>>();
            if (l.Count == 0)
                return finalList;
            int size = 0;
            int currentList = 0;
            DateTime t = DateTime.Now.Date;
            ViewModels.Pushpin p = l.ElementAt<ViewModels.Pushpin>(size);
            while (true)
            {
                while (t > p.position.Timestamp.DateTime)
                    t = t.Subtract(new TimeSpan(hours, 0, 0));
                finalList.Add(new LocationList<ViewModels.Pushpin>(ToRelativeDateTime(t)));
                do
                {
                    finalList.ElementAt(currentList).Add(p);
                    size++;
                    if (size == l.Count)
                        return finalList;
                    p = l.ElementAt<ViewModels.Pushpin>(size);
                }
                while (t <= p.position.Timestamp.DateTime);
                currentList++;
            }
        }

        /// <summary>
        /// Return a string representation of the date.
        /// The string representation is relative to 
        /// the current date.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static string ToRelativeDateTime(DateTime d)
        {
            int n = DateTime.Now.Subtract(d).Days;
            if (n == 0)
                return AppResources.Date_Today;
            if (n == 1)
                return AppResources.Date_Yesterday;
            if (n <= 30)
                return n + AppResources.Date_DaysAgo;
            return d.ToShortDateString();
        }
    }
}
