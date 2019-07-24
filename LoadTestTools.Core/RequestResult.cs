using System;

namespace LoadTestTools.Core
{
    public class RequestResult
    {
        public decimal ResponseMilliseconds { get; set; }
        public bool IsSuccessful { get; set; }
        public DateTime RequestStartDateTime { get; set; }
    }
}
