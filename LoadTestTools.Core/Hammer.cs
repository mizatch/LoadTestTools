using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp;

namespace LoadTestTools.Core
{
    public class Hammer
    {
        public static async Task<HammerStats> HammerUrl(HammerOptions hammerOptions)
        {
            ServicePointManager.DefaultConnectionLimit = hammerOptions.MaximumConcurrentRequests;

            var concurrentRequestCount = 5;

            var restClient = new RestClient(hammerOptions.Url);

            var hammerSwingResult = new List<HammerSwingResult>();

            var maximumRuntimeStopWatch = Stopwatch.StartNew();

            while (concurrentRequestCount <= hammerOptions.MaximumConcurrentRequests && maximumRuntimeStopWatch.ElapsedMilliseconds <= hammerOptions.MaximumMillisecondsToHammer)
            {
                var requestResultTasks = new List<Task<RequestResult>>();

                for (var i = 0; i < concurrentRequestCount; i++)
                {
                    var hammerTask = Task.Factory.StartNew(() =>
                        SendRequest(restClient, hammerOptions.QueryStringParameters, hammerOptions.RequestHeaders));

                    requestResultTasks.Add(hammerTask);
                }

                var taskResults = await Task.WhenAll(requestResultTasks);

                var hammerSwingResults = taskResults.ToList();

                hammerSwingResult.Add(new HammerSwingResult
                {
                    AverageResponseTime = hammerSwingResults.Average(a => a.ResponseMilliseconds),
                    FailureCount = hammerSwingResults.Count(c => !c.IsSuccessful),
                    TotalRequestCount = concurrentRequestCount,
                    RequestResults = hammerSwingResults
                });

                concurrentRequestCount += 5;
            }

            return new HammerStats
            {
                HammerSwingStats = hammerSwingResult
            };
        }

        private static RequestResult SendRequest(IRestClient restClient,
            Dictionary<string, string> queryStringParameters, Dictionary<string, string> requestHeaders)
        {
            var request = new RestRequest(Method.GET);

            if (queryStringParameters != null && queryStringParameters.Any())
            {
                foreach (var queryStringParameter in queryStringParameters)
                {
                    request.AddQueryParameter(queryStringParameter.Key, queryStringParameter.Value);
                }
            }

            if (requestHeaders != null && requestHeaders.Any())
            {
                foreach (var requestHeader in requestHeaders)
                {
                    request.AddHeader(requestHeader.Key, requestHeader.Value);
                }
            }

            var requestStartDateTime = DateTime.Now;

            var requestStopwatch = Stopwatch.StartNew();

            var restResponse = restClient.Execute(request);

            requestStopwatch.Stop();

            return new RequestResult
            {
                ResponseMilliseconds = requestStopwatch.ElapsedMilliseconds,
                IsSuccessful = restResponse.IsSuccessful,
                RequestStartDateTime = requestStartDateTime
            };
        }
    }
}
