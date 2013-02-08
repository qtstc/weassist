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
    public static class ParseContract
    {
        public static class UserTable
        {
            public static string UPDATE_INTERVAL { get { return "updateInterval"; } }
            private static string LOCATION_HISTORY { get { return "location_"; } }
            public static string TRACKING_ENABLED { get { return "trackingEnabled"; } }
            public static string LAST_LOCATION_INDEX { get { return "lastLocationIndex"; } }
            public static string LOCATION_DATA_SIZE { get { return "locationDataSize"; } }
            public static string LOCATION(int key) { return LOCATION_HISTORY + key.ToString("000"); }
        }

        public static class LocationTable
        {
            public static string TABLE_NAME { get { return "Locations"; } }

            public static string LOCATION { get { return "Locations";} }
            public static string TIME_STAMP { get { return "timeStamp"; } }
            public static string ALTITUDE { get { return "altitude"; } }
            public static string HORIZONTAL_ACCURACY { get { return "horizontalAccuracy"; } }
            public static string VERTICAL_ACCURACY { get { return "verticalAccuracy"; } }
            public static string SPEED { get { return "speed"; } }
            public static string COURSE { get { return "course"; } }

            public static string DUMMY_LOCATION { get { return "CaKfUVexoN"; } }

            public static double NaNforParse = -1;

            public static ParseObject GeoPositionToParseObject(GeoPosition<GeoCoordinate> g)
            { 
                ParseObject Locations = new ParseObject(TABLE_NAME);
                Locations[LOCATION] = new ParseGeoPoint(toParseNaN(g.Location.Latitude), toParseNaN(g.Location.Longitude));
                Locations[TIME_STAMP] = g.Timestamp.DateTime;
                Locations[ALTITUDE] = toParseNaN(g.Location.Altitude);
                Locations[HORIZONTAL_ACCURACY] = toParseNaN(g.Location.HorizontalAccuracy);
                Locations[VERTICAL_ACCURACY] = toParseNaN(g.Location.VerticalAccuracy);
                Locations[SPEED] = toParseNaN(g.Location.Speed);
                Locations[COURSE] = toParseNaN(g.Location.Course);
                return Locations;
            }

            private static double toParseNaN(double n)
            {
                if (Double.IsNaN(n))
                    return NaNforParse;
                else return n;
            }
        }

        public static class TrackTable
        {
            public static string TRACKING { get { return "tracking"; } }
            public static string TRACKED { get { return "tracked"; } }
        }
    }
}
