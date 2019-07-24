using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace LoadTestTools.SpecFlowBindings.MsTest
{
    [Binding]
    public sealed class CommonSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public CommonSteps(ScenarioContext scenarioContext)
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

        [Then(@"the average response time is less than '(.*)' milliseconds")]
        public void ThenTheAverageResponseTimeIsLessThanMilliseconds(int averageResponseTime)
        {
            var actualAverageResponseTime = _scenarioContext.Get<decimal>("AverageResponseTime");

            Assert.IsTrue(actualAverageResponseTime < averageResponseTime,
                $"Average Response Time did not achieve expectation of {averageResponseTime} milliseconds.  Actual Average Respose Time was {actualAverageResponseTime}");

            Console.WriteLine($"Actual Average Response Time: { actualAverageResponseTime}");
        }

        [Then(@"there are fewer than '(.*)' failed responses")]
        public void ThenThereAreFewerThanFailedResponses(int failedResponseCount)
        {
            var actualFailureCount = _scenarioContext.Get<int>("FailureCount");

            Assert.IsTrue(actualFailureCount > failedResponseCount,
                $"Failed Response Count did not achieve expectation of {failedResponseCount}.  Actual Failed Response Count was {actualFailureCount}");

            Console.WriteLine($"Actual Failed Response Count: {actualFailureCount}");
        }
    }
}
