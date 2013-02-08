using Parse;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledLocationAgent.Data
{
    public static class ParseContract
    {
        public static class UserTable
        {
            public static string UPDATE_INTERVAL { get { return "update_interval"; } }
            private static string LOCATION_HISTORY { get { return "location_"; } }
            public static string TRACKING_ENABLED { get { return "tracking_enabled"; } }
            public static string LAST_LOCATION_INDEX { get { return "last_location_index"; } }
            public static string LOCATION_DATA_SIZE { get { return "location_data_size"; } }
            public static string LOCATION(int key) { return LOCATION_HISTORY + key; }
        }

        public static class LocationTable
        {
            public static string TABLE_NAME { get { return "locations"; } }

            public static string LOCATION { get { return "location";} }
            public static string TIME_STAMP { get { return "time_stamp"; } }
            public static string ALTITUDE { get { return "altitude"; } }
            public static string HORIZONTAL_ACCURACY { get { return "horizontal_accuracy"; } }
            public static string VERTICAL_ACCURACY { get { return "vertical_accuracy"; } }
            public static string SPEED { get { return "speed"; } }
            public static string COURSE { get { return "course"; } }

            public static ParseObject GeoPositionToParseObject(GeoPosition<GeoCoordinate> g)
            { 
                ParseObject location = new ParseObject(TABLE_NAME);
                location[LOCATION] = new ParseGeoPoint(g.Location.Latitude, g.Location.Longitude);
                location[TIME_STAMP] = g.Timestamp;
                location[ALTITUDE] = g.Location.Altitude;
                location[HORIZONTAL_ACCURACY] = g.Location.HorizontalAccuracy;
                location[VERTICAL_ACCURACY] = g.Location.VerticalAccuracy;
                location[SPEED] = g.Location.Speed;
                location[COURSE] = g.Location.Course;
                return location;
            }
        }

        public static class TrackTable
        {
            public static string TRACKING { get { return "tracking"; } }
            public static string TRACKED { get { return "tracked"; } }
        }
    }
}
