﻿using Parse;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Threading;
using System.Threading.Tasks;

namespace ScheduledLocationAgent.Data
{
    /// <summary>
    /// The contract used to manage the data stored in the parse server.
    /// </summary>
    public static class ParseContract
    {
        //Credentials necessary for connecting to the Parse server.
        public const string applicationID = "JCShhMq7PAo4ds86p1aFqD8moEV61yO9tm7kYJw2";
        public const string windowsKey = "ucvkkkm6IXAtcnNleST3BPbxOVsBgrP3u986c9P4";

        /// <summary>
        /// Data contract for the User table.
        /// </summary>
        public static class UserTable
        {
            public static string FIRST_NAME { get { return "firstName"; } }
            public static string LAST_NAME { get { return "lastName"; } }
            public static string UPDATE_INTERVAL { get { return "updateInterval"; } }
            private static string LOCATION_HISTORY { get { return "location_"; } }
            public static string TRACKING_ENABLED { get { return "trackingEnabled"; } }
            public static string LAST_LOCATION_INDEX { get { return "lastLocationIndex"; } }
            public static string LAST_GEO_POINT { get { return "lastGeoPoint"; } }
            public static string LOCATION_DATA_SIZE { get { return "locationDataSize"; } }
            public static string LOCATION(int key) { return LOCATION_HISTORY + key.ToString("000"); }
            public static string IN_DANGER { get { return "inDanger"; } }
            public static string WIN_PNONE_PUSH_URI { get { return "winPhonePushURI"; } }
            public static string PHONE { get { return "phone"; } }
            public static string AGREEMENT_CONFIRMED { get { return "agreementConfirmed"; } }

            public static string NOTIFY_BY_SMS_STRANGER { get { return "notifyBySMSStranger"; } }
            public static string NOTIFY_BY_EMAIL_STRANGER { get { return "notifyByEmailStranger"; } }
            public static string NOTIFY_BY_PUSH_STRANGER { get { return "notifyByPushStranger"; } }

            public const int DEFAULT_INTERVAL = 30;
            public const int DEFAULT_DATA_SIZE = 96;
        }

        /// <summary>
        /// Data contract for the Locations table.
        /// </summary>
        public static class LocationTable
        {
            public static string TABLE_NAME { get { return "Locations"; } }

            public static string LOCATION { get { return "geoLocation"; } }
            public static string TIME_STAMP { get { return "timeStamp"; } }
            public static string ALTITUDE { get { return "altitude"; } }
            public static string HORIZONTAL_ACCURACY { get { return "horizontalAccuracy"; } }
            public static string VERTICAL_ACCURACY { get { return "verticalAccuracy"; } }
            public static string SPEED { get { return "speed"; } }
            public static string COURSE { get { return "course"; } }

            public static string IS_SOS_REQUEST { get { return "isSOSRequest"; } }

            public static double NaNforParse = -1;

            /// <summary>
            /// Converts a GeoPosition instance to a ParseObject.
            /// </summary>
            /// <param name="g">the GeoPosition instance</param>
            /// <returns>the ParseObject</returns>
            public static ParseObject GeoPositionToParseObject(GeoPosition<GeoCoordinate> g)
            {
                ParseObject Locations = new ParseObject(TABLE_NAME);
                GeoPositionSetParseObject(g, Locations);
                return Locations;
            }

            /// <summary>
            /// Use a GeoPosition instance to populate a ParseObject
            /// </summary>
            /// <param name="g">the GeoPosition</param>
            /// <param name="p">the ParseObject</param>
            public static void GeoPositionSetParseObject(GeoPosition<GeoCoordinate> g, ParseObject p)
            {
                p[LOCATION] = new ParseGeoPoint(ToParseNaN(g.Location.Latitude), ToParseNaN(g.Location.Longitude));
                p[TIME_STAMP] = g.Timestamp.DateTime;
                p[ALTITUDE] = ToParseNaN(g.Location.Altitude);
                p[HORIZONTAL_ACCURACY] = ToParseNaN(g.Location.HorizontalAccuracy);
                p[VERTICAL_ACCURACY] = ToParseNaN(g.Location.VerticalAccuracy);
                p[SPEED] = ToParseNaN(g.Location.Speed);
                p[COURSE] = ToParseNaN(g.Location.Course);
            }

            /// <summary>
            /// Convert a ParseObject to its GeoPosition counterpart
            /// </summary>
            /// <param name="p">the ParseObject instance</param>
            /// <returns>the GeoPosition object</returns>
            public static GeoPosition<GeoCoordinate> ParseObjectToGeoPosition(ParseObject p)
            {
                GeoPosition<GeoCoordinate> g = new GeoPosition<GeoCoordinate>();

                g.Timestamp = new DateTimeOffset(p.Get<DateTime>(TIME_STAMP));
                ParseGeoPoint pGeoPoint = p.Get<ParseGeoPoint>(LOCATION);

                g.Location = new GeoCoordinate();
                if (pGeoPoint.Latitude != NaNforParse)
                    g.Location.Latitude = pGeoPoint.Latitude;
                if (pGeoPoint.Longitude != NaNforParse)
                    g.Location.Longitude = pGeoPoint.Longitude;

                Double pAltitude = p.Get<Double>(ALTITUDE);
                if (pAltitude != NaNforParse)
                    g.Location.Altitude = pAltitude;

                Double pHAccuracy = p.Get<Double>(HORIZONTAL_ACCURACY);
                if (pHAccuracy != NaNforParse)
                    g.Location.HorizontalAccuracy = pHAccuracy;

                Double pVAccuracy = p.Get<Double>(VERTICAL_ACCURACY);
                if (pVAccuracy != NaNforParse)
                    g.Location.VerticalAccuracy = pVAccuracy;

                Double pSpeed = p.Get<Double>(SPEED);
                if (pSpeed != NaNforParse)
                    g.Location.Speed = pSpeed;

                Double pCourse = p.Get<Double>(COURSE);
                if (pCourse != NaNforParse)
                    g.Location.Course = pCourse;

                return g;
            }

            /// <summary>
            /// Because Parse does not accept the Double.NaN in its table,
            /// we convert it to a special value.
            /// </summary>
            /// <param name="n">the original value if n is not a NaN, otherwise NaNforParse</param>
            /// <returns></returns>
            private static double ToParseNaN(double n)
            {
                if (Double.IsNaN(n))
                    return NaNforParse;
                else return n;
            }
        }

        /// <summary>
        /// DataContract for the SOSRequest table.
        /// </summary>
        public static class SOSRequestTable
        {
            private const int IMAGE_WIDTH = 200;

            public static string TABLE_NAME { get { return "SOSRequest"; } }

            public static string SENDER { get { return "sender"; } }
            public static string RESOLVED { get { return "resolved"; } }
            public static string SENT_LOCATION { get { return "sentLocation"; } }
            public static string MESSAGE { get { return "message"; } }
            public static string IMAGE { get { return "image"; } }

            public static string SOS_IMAGE_FILE_NAME { get { return "sosimage.jpg"; } }
            public const int MAX_MESSAGE_LENGTH = 200;

            public static string SHARE_NAME { get { return "shareName"; } }
            public static string SHARE_EMAIL { get { return "shareEmail"; } }
            public static string SHARE_PHONE { get { return "sharePhone"; } }
            public static string SHARE_REQUEST { get { return "shareRequest"; } }

            public static int[] GetImageWidthHeight(int width, int height)
            {
                int newHeight = height * IMAGE_WIDTH / width;
                return new int[] { IMAGE_WIDTH, newHeight };
            }
        }

        /// <summary>
        /// DataContract for the TrackRelation table.
        /// </summary>
        public static class TrackRelationTable
        {
            public static string TABLE_NAME { get { return "TrackRelation"; } }

            public static string TRACKING { get { return "tracking"; } }
            public static string TRACKED { get { return "tracked"; } }
            public static string TRACKED_VERIFIED { get { return "trackedVerified"; } }
            public static string TRACKING_VERIFIED { get { return "trackingVerified"; } }
            public static string UNREGISTERED_USER_EMAIL { get { return "unregisteredUserEmail"; } }
            public static string NOTIFY_BY_SMS { get { return "notifyBySMS"; } }
            public static string NOTIFY_BY_EMAIL { get { return "notifyByEmail"; } }
            public static string NOTIFY_BY_PUSH { get { return "notifyByPush"; } }
            public static string ALLOW_LOCATION_ACCESS { get { return "allowLocationAccess"; } }

            /// <summary>
            /// Get the other role.
            /// </summary>
            /// <param name="track">either TRACKED or TRACKING</param>
            /// <returns>the other one</returns>
            public static string OtherRole(string track)
            {
                if (track == TRACKING) return TRACKED;
                else if (track == TRACKED) return TRACKING;
                else return "Error";
            }

            /// <summary>
            /// Get the other verified
            /// </summary>
            /// <param name="verified">either TRACKING_VERIFIED or TRACKED_VERIFIED</param>
            /// <returns>the other one</returns>
            public static string OtherVerified(string verified)
            {
                if (verified == TRACKED_VERIFIED) return TRACKING_VERIFIED;
                else if (verified == TRACKING_VERIFIED) return TRACKED_VERIFIED;
                else return "Error";
            }
        }

        /// <summary>
        /// A class used to store class functions.
        /// It also defines the interface used for each cloud function.
        /// </summary>
        public static class CloudFunction
        {
            /// <summary>
            /// Send track invitaion to a user.
            /// </summary>
            /// <param name="userID"></param>
            /// <param name="role"></param>
            /// <param name="relationID"></param>
            /// <param name="tk"></param>
            /// <returns></returns>
            public static async Task<string> SendTrackInvitation(string userID, string role, string relationID, CancellationToken tk)
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("userID", userID);
                parameters.Add("relation", role);
                parameters.Add("relationID", relationID);
                string result;
                if (tk != CancellationToken.None)
                    result = await ParseCloud.CallFunctionAsync<string>("InviteExistingUser", parameters, tk);
                else
                    result = await ParseCloud.CallFunctionAsync<string>("InviteExistingUser", parameters);
                return result;
            }

            /// <summary>
            /// Invite a new user.
            /// </summary>
            /// <param name="newEmail"></param>
            /// <param name="role"></param>
            /// <param name="tk"></param>
            /// <returns></returns>
            public static async Task<string> InviteNewUser(string newEmail, string role, CancellationToken tk)
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("email", newEmail);
                parameters.Add("relation", role);
                string result;
                if (tk != CancellationToken.None)
                    result = await ParseCloud.CallFunctionAsync<string>("InviteNewUser", parameters, tk);
                else
                    result = await ParseCloud.CallFunctionAsync<string>("InviteNewUser", parameters);
                return result;
            }

            /// <summary>
            /// Make a new SOS Call.
            /// </summary>
            /// <param name="requestTableRowID"></param>
            /// <param name="tk"></param>
            /// <returns></returns>
            public static async Task<string> NewSOSCall(string requestTableRowID, CancellationToken tk)
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("requestTableRowID", requestTableRowID);
                string result;
                if (tk != CancellationToken.None)
                    result = await ParseCloud.CallFunctionAsync<string>("NewSOSCall", parameters, tk);
                else
                    result = await ParseCloud.CallFunctionAsync<string>("NewSOSCall", parameters);
                return result;
            }
        }

    }
}
