using System;
using System.Collections.Generic;
using System.Diagnostics;
using RestSharp;

namespace LoadTestTools.Core
{
    public class Drill
    {
        public static List<DrillResult> DrillUrl(string url, int drillTime)
        {
            var restClient = new RestClient(url);

            var drillResults = new List<DrillResult>();

            var sessionStopWatch = new Stopwatch();
            sessionStopWatch.Start();

            while (sessionStopWatch.Elapsed < TimeSpan.FromMilliseconds(drillTime))
            {
                var request = new RestRequest(Method.GET);

                var requestStopwatch = Stopwatch.StartNew();

                var restResponse = restClient.Execute(request);

                requestStopwatch.Stop();

                drillResults.Add(new DrillResult
                {
                    ResponseMilliseconds = requestStopwatch.ElapsedMilliseconds,
                    IsSuccessful = restResponse.IsSuccessful
                });
            }

            return drillResults;
        }
    }

}
