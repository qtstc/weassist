using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledLocationAgent.Data
{

        public class LogEntry
        {
            public LogEntry(string tag, DateTime time, string exceptionString,string stackTrace)
            {
                this.tag =tag;
                this.time = time;
                this.exceptionString = exceptionString;
                this.stackTrace =stackTrace;
            }

            public string tag;
            public DateTime time;
            public string exceptionString;
            public string stackTrace;
        }
}
