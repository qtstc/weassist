using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledLocationAgent.Data
{

    /// <summary>
    /// A class used to store unsent location data.
    /// It is only used for JSON conversion.
    /// Do not create instances of this class directly.
    /// It is meaned to be used by the UnsentLocationQueue class.
    /// </summary>
    class UnsentLocations
    {
        public GeoPosition<GeoCoordinate>[] queue;
        public int begin;
        public int end;
        public int updateInterval;
        public DateTime lastUpdate;
    }
}
