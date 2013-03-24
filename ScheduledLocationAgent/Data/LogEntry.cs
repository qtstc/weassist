using System;

namespace ScheduledLocationAgent.Data
{
    /// <summary>
    /// Used to store an entry in the phone storage.
    /// Used for recording exceptions during runtime.
    /// </summary>
    public class LogEntry
    {
        public LogEntry(string tag, DateTime time, string exceptionString, string stackTrace)
        {
            this.tag = tag;
            this.time = time;
            this.exceptionString = exceptionString;
            this.stackTrace = stackTrace;
        }

        public string tag;
        public DateTime time;
        public string exceptionString;
        public string stackTrace;
    }
}