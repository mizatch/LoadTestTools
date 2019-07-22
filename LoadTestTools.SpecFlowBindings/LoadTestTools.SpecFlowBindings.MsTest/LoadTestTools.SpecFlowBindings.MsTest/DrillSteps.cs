using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoadTestTools.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace LoadTestTools.SpecFlowBindings.MsTest
{
    [Binding]
    public sealed class DrillSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public DrillSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"the request headers")]
        public void GivenTheRequestHeaders(Table table)
        {
            var requestHeaderDictionary = new Dictionary<string, string>();

            foreach (var tableRow in table.Rows)
            {
                requestHeaderDictionary.Add(tableRow["Key"], tableRow["Value"]);
            }

            _scenarioContext.Add("RequestHeaders", requestHeaderDictionary);
        }

        [When(@"I drill '(.*)' with '(.*)' concurrent connections for '(.*)' milliseconds")]
        public async Task WhenIDrillWithConcurrentConnectionsForMilliseconds(string url, int connectionCount, int millisecondsToDrill)
        {
            var drillOptions = new DrillOptions
            {
                Url = url,
                ConnectionCount = connectionCount,
                MillisecondsToDrill = millisecondsToDrill,
                RequestHeaders = AddRequestHeaders()
            };

            var drillStats = await Drill.DrillUrl(drillOptions);

            _scenarioContext.Set(drillStats);
        }

        [When(@"I drill '(.*)' with '(.*)' concurrent connections for '(.*)' milliseconds, with query parameters")]
        public async Task WhenIDrillWithConcurrentConnectionsForMillisecondsWithQueryParameters(string url, int connectionCount, int millisecondsToDrill, Table table)
        {
            var queryStringParameters = new Dictionary<string, string>();

            foreach (var tableRow in table.Rows)
            {
                queryStringParameters.Add(tableRow["Key"], tableRow["Value"]);
            }

            var drillOptions = new DrillOptions
            {
                Url = url,
                ConnectionCount = connectionCount,
                MillisecondsToDrill = millisecondsToDrill,
                RequestHeaders = AddRequestHeaders(),
                QueryStringParameters = queryStringParameters
            };

            var drillStats = await Drill.DrillUrl(drillOptions);

            _scenarioContext.Set(drillStats);
        }

        [Then(@"the average response time is less than '(.*)' milliseconds")]
        public void ThenTheAverageResponseTimeIsLessThanMilliseconds(int averageResponseTime)
        {
            var drillStats = _scenarioContext.Get<DrillStats>();

            Assert.IsTrue(drillStats.AverageResponseTime < averageResponseTime,
                $"Average Response Time did not achieve expectation of {averageResponseTime} milliseconds.  Actual Average Respose Time was {drillStats.AverageResponseTime}");

            Console.WriteLine($"Actual Average Response Time: {drillStats.AverageResponseTime}");
        }

        [Then(@"there are fewer than '(.*)' failed responses")]
        public void ThenThereAreFewerThanFailedResponses(int failedResponseCount)
        {
            var drillStats = _scenarioContext.Get<DrillStats>();

            Assert.IsTrue(drillStats.FailureCount < failedResponseCount,
                $"Failed Response Count did not achieve expectation of {failedResponseCount}.  Actual Failed Response Count was {drillStats.FailureCount}");

            Console.WriteLine($"Actual Failed Response Count: {drillStats.FailureCount}");
        }

        private Dictionary<string, string> AddRequestHeaders()
        {
            if (!_scenarioContext.ContainsKey("RequestHeaders"))
            {
                return new Dictionary<string, string>();
            }

            var requestHeaders = _scenarioContext.Get<Dictionary<string, string>>("RequestHeaders");

            return requestHeaders ?? new Dictionary<string, string>();
        }
    }
}
