using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using RestSharp;
using System.Threading.Tasks.Dataflow;

namespace LoadTestTools.Core
{ 
    public class Drill
    {
        [Obsolete("Please use the DrillUrl method which accepts DrillOptions")]
        public DrillStats DrillUrl(string url, int connectionCount, int drillTime, 
            Dictionary<string, string> queryStringParameters)
        {
            return ExecuteConnections(new DrillOptions
            {
                Url = url,
                ConnectionCount = connectionCount,
                MillisecondsToDrill = drillTime,
                QueryStringParameters = queryStringParameters
            });
        }

        [Obsolete("Please use the DrillUrl method which accepts DrillOptions")]
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

        private static DrillStats ExecuteConnections(DrillOptions drillOptions)
        {
            if (drillOptions.Body != null && drillOptions.BodyType == null)
            {
                throw new ArgumentException("You supplied a Body object, so you must supply a BodyType");
            }

            ThreadPool.SetMinThreads(drillOptions.ConnectionCount, 
                drillOptions.ConnectionCount);

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

            var transformBlock = new TransformBlock<DrillOptions, List<RequestResult>>(
                RequestFunc, executionDataflowBlockOptions);

            for (var i = 0; i < drillOptions.ConnectionCount; i++)
            {
                transformBlock.Post(drillOptions);
            }

            for (var i = 0; i < drillOptions.ConnectionCount; i++)
            {
                var requestResults = transformBlock.Receive();

                aggregatedResults.AddRange(requestResults);
            }

            var drillStats = new DrillStats
            {
                AverageResponseTime = aggregatedResults.Average(a => a.ResponseMilliseconds),
                ConnectionCount = drillOptions.ConnectionCount,
                TotalRequestCount = aggregatedResults.Count,
                FailureCount = aggregatedResults.Count(c => !c.IsSuccessful),
                RequestResults = aggregatedResults
            };

            drillOptions.Recorder?.RecordDrill(drillOptions, drillStats);

            return drillStats;
        }

        private static List<RequestResult> ExecuteConnection(DrillOptions drillOptions)
        {
            var restClient = new RestClient(new Uri(drillOptions.Url));

            var drillResults = new List<RequestResult>();

            var request = CreateRequest(drillOptions);

            ExecutePreDrillProcesses(drillOptions, request);

            var sessionStopWatch = new Stopwatch();
            sessionStopWatch.Start();

            while (sessionStopWatch.Elapsed < TimeSpan.FromMilliseconds(drillOptions.MillisecondsToDrill))
            {
                drillResults.Add(SendRequest(request, restClient));

                if (drillOptions.MillisecondsToWaitAfterRequest.HasValue)
                {
                    Thread.Sleep(drillOptions.MillisecondsToWaitAfterRequest.Value);
                }
            }

            return drillResults;
        }

        private static RestRequest CreateRequest(DrillOptions drillOptions)
        {
            var request = new RestRequest(GetMethod(drillOptions.RequestMethod));

            if (drillOptions.Body != null)
            {
                if (drillOptions.BodyType == BodyType.Json)
                {
                    request.AddJsonBody(drillOptions.Body);
                }
                else
                {
                    request.AddXmlBody(drillOptions.Body);
                }
            }

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

            return request;
        }

        private static Method GetMethod(RequestMethod restMethod)
        {
            switch (restMethod)
            {
                case RequestMethod.Get:
                    return Method.GET;
                case RequestMethod.Post:
                    return Method.POST;
                case RequestMethod.Put:
                    return Method.PUT;
                default:
                    return Method.GET;
            }
        }

        private static void ExecutePreDrillProcesses(DrillOptions drillOptions, RestRequest request)
        {
            if (drillOptions.PreDrillProcesses == null || !drillOptions.PreDrillProcesses.Any())
            {
                return;
            }

            foreach (var drillOptionsDrillPreProcessor in drillOptions.PreDrillProcesses)
            {
                drillOptionsDrillPreProcessor.Execute(drillOptions, request);
            }
        }

        private static RequestResult SendRequest(IRestRequest request, IRestClient restClient)
        {
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
