using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using LoadTestTools.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace LoadTestTools.Recording.AppInsights
{
    public class AppInsightsRecorder : IRecorder
    {
        private readonly TelemetryClient _telemetryClient;

        public AppInsightsRecorder()
        {
            var instrumentationKey = ConfigurationManager.AppSettings["Recorder.AppInsights.InstrumentationKey"];

            if (instrumentationKey == null)
            {
                throw new ConfigurationErrorsException(
                    "In order to use the App Insights recorder, you must supply an Instrumentation Key in your configuration using Key of Recorder.AppInsights.InstrumentationKey");
            }

            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = instrumentationKey;

            _telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        public void RecordDrill(DrillOptions drillOptions, DrillStats drillStats)
        {
            var properties = new Dictionary<string, string>
            {
                {"Url", drillOptions.Url},
                {"ConnectionCount", drillOptions.ConnectionCount.ToString()},
                {"MillisecondsToDrill", drillOptions.MillisecondsToDrill.ToString()},
                {"MillisecondsToWaitAfterRequest", drillOptions.MillisecondsToWaitAfterRequest.ToString()},
                {"QueryStringParameters", CombineQueryStringParameters(drillOptions.QueryStringParameters) }
            };

            var metrics = new Dictionary<string, double>
            {
                { "TotalRequestCount", drillStats.TotalRequestCount},
                { "ConnectionCount", drillStats.ConnectionCount},
                { "FailureCount", drillStats.FailureCount},
                { "AverageResponseTime", (double) drillStats.AverageResponseTime }
            };

            _telemetryClient.TrackEvent("Drill", properties, metrics);

            _telemetryClient.Flush();

            System.Threading.Thread.Sleep(5000);
        }

        public void RecordHammer(HammerOptions hammerOptions, HammerStats hammerStats)
        {
            var properties = new Dictionary<string, string>
            {
                {"Url", hammerOptions.Url},
                {"MaximumConcurrentRequests", hammerOptions.MaximumConcurrentRequests.ToString()},
                {"MaximumMillisecondsToHammer", hammerOptions.MaximumMillisecondsToHammer.ToString()},
                {"QueryStringParameters", CombineQueryStringParameters(hammerOptions.QueryStringParameters)}
            };

            foreach (var hammerSwingResult in hammerStats.HammerSwingStats)
            {
                var metrics = new Dictionary<string, double>
                {
                    { "FailureCount", hammerSwingResult.FailureCount},
                    { "ConnectionCount", hammerSwingResult.TotalRequestCount},
                    { "AverageResponseTime", (double) hammerSwingResult.AverageResponseTime}
                };

                _telemetryClient.TrackEvent("Hammer", properties, metrics);
            }

            _telemetryClient.Flush();

            System.Threading.Thread.Sleep(5000);
        }

        private static string CombineQueryStringParameters(Dictionary<string, string> queryStrings)
        {
            if (queryStrings == null)
            {
                return string.Empty;
            }

            if (!queryStrings.Any())
            {
                return string.Empty;
            }

            return string.Join(", ", queryStrings
                .Select(s => $"{s.Key}: {s.Value}").ToList());
        }
    }
}