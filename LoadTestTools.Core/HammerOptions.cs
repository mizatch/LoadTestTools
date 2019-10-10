using System.Collections.Generic;

namespace LoadTestTools.Core
{
    public class HammerOptions
    {
        public HammerOptions()
        {
            RequestMethod = RequestMethod.Get;
        }

        public RequestMethod RequestMethod { get; set; }
        public object Body { get; set; }
        public BodyType? BodyType { get; set; }
        public string Url { get; set; }
        public int MaximumConcurrentRequests { get; set; }
        public int MaximumMillisecondsToHammer { get; set; }
        public Dictionary<string, string> QueryStringParameters { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
        public IRecorder Recorder { get; set; }
        public IEnumerable<IPreHammerProcess> PreHammerProcesses { get; set; }
    }
}
