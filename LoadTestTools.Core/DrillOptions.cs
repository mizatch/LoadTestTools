using System;
using System.Collections.Generic;

namespace LoadTestTools.Core
{
    public class DrillOptions
    {
        public DrillOptions()
        {
            RequestMethod = RequestMethod.Get;
        }

        public RequestMethod RequestMethod { get; set; }
        public object Body { get; set; }
        public BodyType? BodyType { get; set; }
        public string Url { get; set; }
        public int ConnectionCount { get; set; }
        public int MillisecondsToDrill { get; set; }
        public int? MillisecondsToWaitAfterRequest { get; set; }
        public Dictionary<string, string> QueryStringParameters { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
        public IRecorder Recorder { get; set; }
        public IEnumerable<IPreDrillProcess> PreDrillProcesses { get; set; }
    }
}
