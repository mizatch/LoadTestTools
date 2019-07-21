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
        public static async Task<DrillStats> DrillUrl(
            string url, 
            int connectionCount, 
            int drillTime,
            Dictionary<string, string> queryStringParameters)
        {
            return await ExecuteConnections(url, connectionCount, drillTime, queryStringParameters);
        }
        
        public static async Task<DrillStats> DrillUrl(string url, int connectionCount, int drillTime)
        {
            return await ExecuteConnections(url, connectionCount, drillTime, new Dictionary<string, string>());
        }

        private static async Task<DrillStats> ExecuteConnections(string url, int connectionCount, int drillTime, Dictionary<string, string> queryStringParameters)
        {
            var tasks = new List<Task<List<RequestResult>>>();

            for (var i = 0; i < connectionCount; i++)
            {
                var drillUtlTask = Task.Factory.StartNew(() => ExecuteConnection(url, drillTime, queryStringParameters));

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
        
        private static List<RequestResult> ExecuteConnection(string url, int drillTime, Dictionary<string, string> queryStringParameters)
        {
            var restClient = new RestClient(url);

            var drillResults = new List<RequestResult>();

            var sessionStopWatch = new Stopwatch();
            sessionStopWatch.Start();

            while (sessionStopWatch.Elapsed < TimeSpan.FromMilliseconds(drillTime))
            {
                var request = new RestRequest(Method.GET);

                foreach (var queryStringParameter in queryStringParameters)
                {
                    request.AddQueryParameter(queryStringParameter.Key, queryStringParameter.Value);
                }

                var requestStopwatch = Stopwatch.StartNew();

                var restResponse = restClient.Execute(request);

                requestStopwatch.Stop();

                drillResults.Add(new RequestResult
                {
                    ResponseMilliseconds = requestStopwatch.ElapsedMilliseconds,
                    IsSuccessful = restResponse.IsSuccessful
                });
            }

            return drillResults;
        }
    }

}
