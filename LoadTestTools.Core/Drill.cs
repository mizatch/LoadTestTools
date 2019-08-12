using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using RestSharp;
using System.Threading.Tasks.Dataflow;

namespace LoadTestTools.Core
{ 
    public class Drill
    {
        private readonly IRestClient _restClient;

        public Drill(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public Drill()
        {
            _restClient = new RestClient();
        }

        public DrillStats DrillUrl(string url, int connectionCount, int drillTime, Dictionary<string, string> queryStringParameters)
        {
            return ExecuteConnections(new DrillOptions
            {
                Url = url,
                ConnectionCount = connectionCount,
                MillisecondsToDrill = drillTime,
                QueryStringParameters = queryStringParameters
            });
        }
        
        public DrillStats DrillUrl(string url, int connectionCount, int drillTime)
        {
            return ExecuteConnections(new DrillOptions
            {
                Url = url,
                ConnectionCount = connectionCount,
                MillisecondsToDrill = drillTime
            });
        }

        public DrillStats DrillUrl(DrillOptions drillOptions)
        {
            return ExecuteConnections(drillOptions);
        }

        private DrillStats ExecuteConnections(DrillOptions drillOptions)
        {
            ServicePointManager.DefaultConnectionLimit = drillOptions.ConnectionCount;

            var aggregatedResults = new List<RequestResult>();

            List<RequestResult> RequestFunc(DrillOptions options)
            {
                return ExecuteConnection(options);
            }

            var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = drillOptions.ConnectionCount
            };

            var transformBlock = new TransformBlock<DrillOptions, List<RequestResult>>((Func<DrillOptions, List<RequestResult>>) RequestFunc, executionDataflowBlockOptions);

            for (var i = 0; i < drillOptions.ConnectionCount; i++)
            {
                transformBlock.Post(drillOptions);
            }

            for (var i = 0; i < drillOptions.ConnectionCount; i++)
            {
                var requestResults = transformBlock.Receive();

                aggregatedResults.AddRange(requestResults);
            }

            return new DrillStats
            {
                AverageResponseTime = aggregatedResults.Average(a => a.ResponseMilliseconds),
                ConnectionCount = drillOptions.ConnectionCount,
                TotalRequestCount = aggregatedResults.Count,
                FailureCount = aggregatedResults.Count(c => !c.IsSuccessful),
                RequestResults = aggregatedResults
            };
        }

        private List<RequestResult> ExecuteConnection(DrillOptions drillOptions)
        {
            _restClient.BaseUrl = new Uri(drillOptions.Url);

            var drillResults = new List<RequestResult>();

            var sessionStopWatch = new Stopwatch();
            sessionStopWatch.Start();

            while (sessionStopWatch.Elapsed < TimeSpan.FromMilliseconds(drillOptions.MillisecondsToDrill))
            {
                var request = new RestRequest(Method.GET);

                if (drillOptions.QueryStringParameters != null && drillOptions.QueryStringParameters.Any())
                {
                    foreach (var queryStringParameter in drillOptions.QueryStringParameters)
                    {
                        request.AddQueryParameter(queryStringParameter.Key, queryStringParameter.Value);
                    }
                }

                if (drillOptions.RequestHeaders != null && drillOptions.RequestHeaders.Any())
                {
                    foreach (var requestHeader in drillOptions.RequestHeaders)
                    {
                        request.AddHeader(requestHeader.Key, requestHeader.Value);
                    }
                }

                drillResults.Add(SendRequest(request));
            }

            return drillResults;
        }

        private RequestResult SendRequest(IRestRequest request)
        {
            var requestStartDateTime = DateTime.Now;

            var requestStopwatch = Stopwatch.StartNew();

            var restResponse = _restClient.Execute(request);

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
