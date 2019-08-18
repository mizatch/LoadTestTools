using System.Collections.Generic;

namespace LoadTestTools.Core
{
    public class DrillOptions
    {
        public string Url { get; set; }
        public int ConnectionCount { get; set; }
        public int MillisecondsToDrill { get; set; }
        public int? MillisecondsToWaitAfterRequest { get; set; }
        public Dictionary<string, string> QueryStringParameters { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
    }
}
