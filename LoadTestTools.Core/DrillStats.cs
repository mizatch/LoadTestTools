namespace LoadTestTools.Core
{
    public class DrillStats
    {
        public decimal AverageResponseTime { get; set; }
        public int TotalRequestCount { get; set; }
        public int ConnectionCount { get; set; }
        public int FailureCount { get; set; }
    }
}