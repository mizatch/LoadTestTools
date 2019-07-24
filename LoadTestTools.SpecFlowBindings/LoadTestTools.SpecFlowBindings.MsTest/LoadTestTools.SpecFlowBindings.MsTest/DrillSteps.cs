using System.Collections.Generic;
using System.Threading.Tasks;
using LoadTestTools.Core;
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

            _scenarioContext.Set(drillStats, "DrillStats");
            _scenarioContext.Set(drillStats.AverageResponseTime, "AverageResponseTime");
            _scenarioContext.Set(drillStats.FailureCount, "FailureCount");
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

            _scenarioContext.Set(drillStats, "DrillStats");
            _scenarioContext.Set(drillStats.AverageResponseTime, "AverageResponseTime");
            _scenarioContext.Set(drillStats.FailureCount, "FailureCount");
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
