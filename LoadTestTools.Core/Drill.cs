using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;

namespace LoadTestTools.Core
{ 
    public class Drill
    {
        public static async Task<DrillStats> DrillUrl(string url, int connectionCount, int drillTime, Dictionary<string, string> queryStringParameters)
        {
            return await ExecuteConnections(url, connectionCount, drillTime, queryStringParameters, 
                new Dictionary<string, string>());
        }
        
        public static async Task<DrillStats> DrillUrl(string url, int connectionCount, int drillTime)
        {
            return await ExecuteConnections(url, connectionCount, drillTime, 
                new Dictionary<string, string>(), new Dictionary<string, string>());
        }

        public static async Task<DrillStats> DrillUrl(DrillOptions drillOptions)
        {
            return await ExecuteConnections(drillOptions.Url, drillOptions.ConnectionCount,
                drillOptions.MillisecondsToDrill, drillOptions.QueryStringParameters, drillOptions.RequestHeaders);
        }

        private static async Task<DrillStats> ExecuteConnections(string url, int connectionCount, int drillTime, 
            Dictionary<string, string> queryStringParameters, Dictionary<string, string> requestHeaders)
        {
            var tasks = new List<Task<List<RequestResult>>>();

            for (var i = 0; i < connectionCount; i++)
            {
                var drillUtlTask = Task.Factory.StartNew(() => ExecuteConnection(url, drillTime, queryStringParameters, requestHeaders));

                tasks.Add(drillUtlTask);
            }

            var taskResults = await Task.WhenAll(tasks);

            var sessionResults = taskResults.ToList();

            var aggregatedResults = sessionResults.SelectMany(s => s).ToList();

            return new DrillStats
            {
                AverageResponseTime = aggregatedResults.Average(a => a.ResponseMilliseconds),
                ConnectionCount = connectionCount,
                TotalRequestCount = aggregatedResults.Count,
                FailureCount = aggregatedResults.Count(c => !c.IsSuccessful),
                RequestResults = aggregatedResults
            };
        }
        
        private static List<RequestResult> ExecuteConnection(string url, int drillTime, 
            Dictionary<string, string> queryStringParameters, Dictionary<string, string> requestHeaders)
        {
            var restClient = new RestClient(url);

            var drillResults = new List<RequestResult>();

            var sessionStopWatch = new Stopwatch();
            sessionStopWatch.Start();

            while (sessionStopWatch.Elapsed < TimeSpan.FromMilliseconds(drillTime))
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

                drillResults.Add(SendRequest(restClient, request));
            }

            return drillResults;
        }

        private static RequestResult SendRequest(IRestClient restClient, IRestRequest request)
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
