using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Device.Location;

namespace ScheduledLocationAgent.Data
{
    public class Utilities
    {
        public static string convertGeoPositionToJSON(GeoPosition<GeoCoordinate> position)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
#if DEBUG
            string json = JsonConvert.SerializeObject(position, Formatting.Indented, jsonSerializerSettings);
#else
                                string json = JsonConvert.SerializeObject(objectToSave, Formatting.None, jsonSerializerSettings);
#endif
            return json;
        }

        public static GeoPosition<GeoCoordinate> convertJSONToGeoPosition(string json)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };

            GeoPosition<GeoCoordinate> result = JsonConvert.DeserializeObject<GeoPosition<GeoCoordinate>>(json, jsonSerializerSettings);

            return result;
        }
    }
}
