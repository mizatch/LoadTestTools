﻿using System.Collections.Generic;
using System.Linq;
using LoadTestTools.Core;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace LoadTestTools.SpecFlowBindings.MsTest
{
    [Binding]
    public sealed class HammerSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public HammerSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [When(@"I hammer '(.*)' with up to '(.*)' concurrent requests for a maximum of '(.*)' millseconds")]
        [When(@"I hammer '(.*)' with up to '(.*)' concurrent requests for a maximum of '(.*)' milliseconds")]
        public void WhenIHammerUpToConcurrentRequestsForAMaximumOfMilliseconds(string url, int maximumConcurrentRequests, int maximumMillisecondsToHammer)
        {
            var hammerOptions = new HammerOptions
            {
                Url = url,
                MaximumConcurrentRequests = maximumConcurrentRequests,
                MaximumMillisecondsToHammer = maximumMillisecondsToHammer,
                RequestHeaders = AddRequestHeaders(),
                Recorder = GetRecorder(),
                PreHammerProcesses = GetPreHammerProcesses()
            };

            HammerUrl(hammerOptions);
        }
        
        [When(@"I hammer '(.*)' with up to '(.*)' concurrent '(.*)' requests for a maximum of '(.*)' milliseconds and Json payload")]
        public void WhenIHammerWithUpToConcurrentRequestsForAMaximumOfMilliseconds(string url, int maximumConcurrentRequests, RequestMethod requestMethod, int maximumMillisecondsToHammer, Table table)
        {
            var hammerOptions = new HammerOptions
            {
                Url = url,
                RequestMethod = requestMethod,
                BodyType = BodyType.Json,
                Body = table.CreateDynamicInstance(),
                MaximumConcurrentRequests = maximumConcurrentRequests,
                MaximumMillisecondsToHammer = maximumMillisecondsToHammer,
                RequestHeaders = AddRequestHeaders(),
                Recorder = GetRecorder(),
                PreHammerProcesses = GetPreHammerProcesses()
            };

            HammerUrl(hammerOptions);
        }


        [When(@"I hammer '(.*)' with up to '(.*)' concurrent requests for a maximum of '(.*)' millseconds, with query parameters")]
        [When(@"I hammer '(.*)' with up to '(.*)' concurrent requests for a maximum of '(.*)' milliseconds, with query parameters")]
        public void WhenIHammerWithUpToConcurrentRequestsForAMaximumOfMillisecondsWithQueryParameters(string url, int concurrentRequestCount, int maximumMillisecondsToHammer, Table table)
        {
            var queryStringParameters = new Dictionary<string, string>();

            foreach (var tableRow in table.Rows)
            {
                queryStringParameters.Add(tableRow["Key"], tableRow["Value"]);
            }

            var hammerOptions = new HammerOptions
            {
                Url = url,
                MaximumConcurrentRequests = concurrentRequestCount,
                MaximumMillisecondsToHammer = maximumMillisecondsToHammer,
                RequestHeaders = AddRequestHeaders(),
                QueryStringParameters = queryStringParameters,
                Recorder = GetRecorder(),
                PreHammerProcesses = GetPreHammerProcesses()
            };

            HammerUrl(hammerOptions);
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

        private IRecorder GetRecorder()
        {
            return _scenarioContext.ContainsKey("Recorder") ?
                _scenarioContext.Get<IRecorder>("Recorder") :
                new DoNotRecord();
        }

        private IEnumerable<IPreHammerProcess> GetPreHammerProcesses()
        {
            return _scenarioContext.ContainsKey("PreHammerProcesses") ?
                _scenarioContext.Get<IEnumerable<IPreHammerProcess>>("PreHammerProcesses") :
                null;
        }

        private void HammerUrl(HammerOptions hammerOptions)
        {
            var hammer = new Hammer();
            var hammerStats = hammer.HammerUrl(hammerOptions);

            var aggregatedResults = hammerStats.HammerSwingStats.SelectMany(s => s.RequestResults).ToList();

            _scenarioContext.Set(hammerStats, "HammerStats");
            _scenarioContext.Set(aggregatedResults.Average(a => a.ResponseMilliseconds), "AverageResponseTime");
            _scenarioContext.Set(aggregatedResults.Count(c => !c.IsSuccessful), "FailureCount");
        }

    }
}
