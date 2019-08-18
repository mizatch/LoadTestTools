using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using RestSharp;

namespace LoadTestTools.Core
{
    public class Hammer
    {
        public HammerStats HammerUrl(HammerOptions hammerOptions)
        {
            return ExecuteConnections(hammerOptions);
        }

        private HammerStats ExecuteConnections(HammerOptions hammerOptions)
        {
            ThreadPool.SetMinThreads(hammerOptions.MaximumConcurrentRequests, hammerOptions.MaximumConcurrentRequests);

            ServicePointManager.DefaultConnectionLimit = hammerOptions.MaximumConcurrentRequests;

            var concurrentRequestCount = 1;

            var hammerSwingResults = new List<HammerSwingResult>();

            RequestResult RequestFunc(HammerOptions options)
            {
                return ExecuteConnection(options);
            }

            var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = hammerOptions.MaximumConcurrentRequests
            };

            var transformBlock = new TransformBlock<HammerOptions, RequestResult>((Func<HammerOptions, RequestResult>)RequestFunc, executionDataflowBlockOptions);

            var maximumRuntimeStopWatch = Stopwatch.StartNew();

            while (concurrentRequestCount <= hammerOptions.MaximumConcurrentRequests 
                   && maximumRuntimeStopWatch.ElapsedMilliseconds <= hammerOptions.MaximumMillisecondsToHammer)
            {
                for (var i = 0; i < concurrentRequestCount; i++)
                {
                    transformBlock.Post(hammerOptions);
                }

                var aggregatedResults = new List<RequestResult>();

                for (var i = 0; i < concurrentRequestCount; i++)
                {
                    var requestResults = transformBlock.Receive();

                    aggregatedResults.Add(requestResults);
                }

                hammerSwingResults.Add(new HammerSwingResult
                {
                    AverageResponseTime = aggregatedResults.Average(a => a.ResponseMilliseconds),
                    FailureCount = aggregatedResults.Count(c => !c.IsSuccessful),
                    TotalRequestCount = concurrentRequestCount,
                    RequestResults = aggregatedResults
                });

                Thread.Sleep(500);

                concurrentRequestCount += 1;
            }

            return new HammerStats
            {
                HammerSwingStats = hammerSwingResults
            };
        }

        private static RequestResult ExecuteConnection(HammerOptions hammerOptions)
        {
            var request = new RestRequest(Method.GET);

            if (hammerOptions.QueryStringParameters != null && hammerOptions.QueryStringParameters.Any())
            {
                foreach (var queryStringParameter in hammerOptions.QueryStringParameters)
                {
                    request.AddQueryParameter(queryStringParameter.Key, queryStringParameter.Value);
                }
            }

            if (hammerOptions.RequestHeaders != null && hammerOptions.RequestHeaders.Any())
            {
                foreach (var requestHeader in hammerOptions.RequestHeaders)
                {
                    request.AddHeader(requestHeader.Key, requestHeader.Value);
                }
            }

            return SendRequest(request, hammerOptions.Url);
        }

        private static RequestResult SendRequest(IRestRequest request, string url)
        {
            var restClient = new RestClient(new Uri(url));

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
