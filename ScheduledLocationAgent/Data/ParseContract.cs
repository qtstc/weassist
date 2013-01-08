using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledLocationAgent.Data
{
    public static class ParseContract
    {
        public static string UPDATE_INTERVAL_KEY { get { return "update_interval_key"; } }
        private static string LOCATION_HISTORY_KEY { get { return "location_key_"; } }
        public static string TRACKING_ENABLED_KEY { get { return "tracking_enabled_key"; } }
        public static string LAST_LOCATION_INDEX_KEY { get { return "last_location_index_key"; } }
        public static string LOCATION_DATA_SIZE_KEY { get { return "location_data_size_key"; } }

        public static string LOCATION(int key) { return LOCATION_HISTORY_KEY + key; }
    }
}
