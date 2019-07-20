using System;
using System.Collections.Generic;
using System.Linq;
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

        [When(@"I drill '(.*)' with '(.*)' concurrent connections for '(.*)' milliseconds")]
        public async Task WhenIDrillWithConcurrentConnectionsForMilliseconds(string url, int connectionCount, int drillTime)
        {
            var tasks = new List<Task<List<DrillResult>>>();

            for (var i = 0; i < connectionCount; i++)
            {
                var drillUtlTask = Task.Factory.StartNew(() => Drill.DrillUrl(url, drillTime));

                tasks.Add(drillUtlTask);
            }

            var taskResults = await Task.WhenAll(tasks);

            var sessionResults = taskResults.ToList();

            var aggregatedResults = sessionResults.SelectMany(s => s).ToList();

            _scenarioContext.Set(new DrillStats
            {
                AverageResponseTime = aggregatedResults.Average(a => a.ResponseMilliseconds),
                ConnectionCount = connectionCount,
                TotalRequestCount = aggregatedResults.Count,
                FailureCount = aggregatedResults.Count(c => !c.IsSuccessful)
            });

            Console.WriteLine($"Total Request Count: {aggregatedResults.Count}");
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
    }
}
