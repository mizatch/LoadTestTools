using System.Collections.Generic;

namespace LoadTestTools.Core
{
    public class HammerOptions
    {
        public string Url { get; set; }
        public int MaximumConcurrentRequests { get; set; }
        public int MaximumMillisecondsToHammer { get; set; }
        public Dictionary<string, string> QueryStringParameters { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
        public IRecorder Recorder { get; set; }
        public IEnumerable<IPreHammerProcess> PreHammerProcesses { get; set; }
    }
}
