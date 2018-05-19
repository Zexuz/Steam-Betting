using System;
using System.Collections.Generic;

namespace Serilog.Core
{
    public class LogDetail
    {
        public DateTime Timestamp { get; private set; }

        //WHARE
        public string Message  { get; set; }
        public string Product  { get; set; }
        public string Layer    { get; set; }
        public string Location { get; set; }
        public string Hostname { get; set; }


        //WHO
        public string UserId { get; set; }

        //EVERYTING ELSE
        public long?     ElapsedMilliseconds { get; set; }
        public Exception Exception           { get; set; }
        public string    CorrelationId       { get; set; }

        public Dictionary<string, object> AdditionalInfo { get; set; }

        public LogDetail()
        {
            Timestamp = DateTime.Now;
        }
    }
}