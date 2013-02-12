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
            public static string TABLE_NAME { get { return "User"; } }

            public static string UPDATE_INTERVAL { get { return "updateInterval"; } }
            private static string LOCATION_HISTORY { get { return "location_"; } }
            public static string TRACKING_ENABLED { get { return "trackingEnabled"; } }
            public static string LAST_LOCATION_INDEX { get { return "lastLocationIndex"; } }
            public static string LOCATION_DATA_SIZE { get { return "locationDataSize"; } }
            public static string LOCATION(int key) { return LOCATION_HISTORY + key.ToString("000"); }

            public static string DUMMY_USER { get { return "WNrCdVZZ48"; } }
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
                Locations[LOCATION] = new ParseGeoPoint(ToParseNaN(g.Location.Latitude), ToParseNaN(g.Location.Longitude));
                Locations[TIME_STAMP] = g.Timestamp.DateTime;
                Locations[ALTITUDE] = ToParseNaN(g.Location.Altitude);
                Locations[HORIZONTAL_ACCURACY] = ToParseNaN(g.Location.HorizontalAccuracy);
                Locations[VERTICAL_ACCURACY] = ToParseNaN(g.Location.VerticalAccuracy);
                Locations[SPEED] = ToParseNaN(g.Location.Speed);
                Locations[COURSE] = ToParseNaN(g.Location.Course);
                return Locations;
            }

            private static double ToParseNaN(double n)
            {
                if (Double.IsNaN(n))
                    return NaNforParse;
                else return n;
            }
        }

        public static class TrackRelationTable
        {
            public static string TABLE_NAME { get { return "TrackRelation"; } }

            public static string TRACKING { get { return "tracking"; } }
            public static string TRACKED { get { return "tracked"; } }
            public static string TRACKED_VERIFIED { get {return "trackedVerified";}}
            public static string TRACKING_VERIFIED {get {return "trackingVerified";}}
            public static string UNREGISTERED_USER_EMAIL { get { return "unregisteredUserEmail"; } }
            public static string NOTIFY_BY_SMS { get { return "notifyBySMS"; } }
            public static string NOTIFY_BY_EMAIL { get { return "notifyByEmail"; } }
            public static string NOTIFY_BY_PUSH { get { return "notifyByPush"; } }

            public static string OtherRole(string track)
            {
                if (track == TRACKING) return TRACKED;
                else if (track == TRACKED) return TRACKING;
                else return "Error";
            }

            public static string OtherVerified(string verified)
            {
                if (verified == TRACKED_VERIFIED) return TRACKING_VERIFIED;
                else if (verified == TRACKING_VERIFIED) return TRACKED_VERIFIED;
                else return "Error";
            }
        }
    }
}
