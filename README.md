# LoadTestTools

This solution was created with the goal making it easy to create and manage API load tests.  To accomplish this goal, a package was created with a set of predefined SpecFlow bindings which allow a consumer to easily make API GET requests to any URL.

**_These libraries should only be used for good.  Do not load test any API without owner approval._**

# How do you load test?

This package aims to use two common tools for testing an API: a Hammer and a Drill.  In both cases, we primarily measure average response time and request failure count.

* When you use the _Drill_, you will be creating a sustained load against an API with a simulated number of concurrent users (connections) and for a specific period of time.  What we are trying to validate with the Drill is that the API can withstand a certain amount of requests and still perform as expected.
* When you use the _Hammer_, you will be gradually increasing the load against an API to determine the point at which the API begins to perform below standards.

# Instructions

* Create a Unit Test Project (.Net Framework).
* Using Nuget, install the package called "LoadTestTools.SpecFlowBindings.MsTest".
* Add a specflow.json file to your test project and add a stepAssembly reference to "LoadTestTools.SpecFlowBindings.MsTest".  This tells SpecFlow that you want to utilize the SpecFlow step bindings found in the installed module.  A sample of my specflow.json can be found below.
* Open the Properties of the specflow.json file and set the "Copy to Output Directory" property to "Copy always".
* Add a SpecFlow feature file to your project.
* Open the Properties of the feature file and remove "SpecFlowSingleFileGenerator" from the "Custom Tool" property.  This property should be left blank.
* Use your Gherkin skills to start writing tests using the step bindings supplied to you by this package.  See below for a couple of Gherkin usage examples.
* If the step bindings aren't immediately recognized, restarting Visual Studio should do the trick.


# specflow.json:

```
{
    "bindingCulture": {
        "language": "en-us"
    },
    "language": {
        "feature": "en-us"
    },
    "plugins": [],
    "stepAssemblies": [
        { "assembly": "LoadTestTools.SpecFlowBindings.MsTest" }
    ]
}
```


# Drill an API with GET requests with only a URL

```
Scenario: Drill URL
	When I drill 'https://{Enter your URL here}' with '2' concurrent connections for '2000' milliseconds
	
	Then the average response time is less than '500' milliseconds
	
	And there are fewer than '5' failed responses
```

# Add a wait period between requests within a connection

```
Scenario: Drill URL with wait period
	Given I wait '50' milliseconds after each request

	When I drill 'https://{Enter your URL here}' with '2' concurrent connections for '2000' milliseconds
	
	Then the average response time is less than '500' milliseconds
	
	And there are fewer than '5' failed responses

# Drill an API with GET requests with a URL and a collection of query string parameters

```
Scenario: Drill URL with parameters
	When I drill 'https://{Enter your URL here}' with '2' concurrent connections for '2000' milliseconds, with query parameters
	| Key   | Value         |
	| text  | example_text  |
	| text2 | example_text2 |
	| etc   | etc           |

	Then the average response time is less than '500' milliseconds
	
	And there are fewer than '5' failed responses
```

# Hammer an API with GET requests with only a URL

```
Scenario: Hammer URL
	When I hammer 'https://{{Enter your URL here}}' with up to '20' concurrent requests for a maximum of '30000' millseconds
	
	Then the average response time is less than '1000' milliseconds
	
	And there are fewer than '5' failed responses
```

# Hammer an API with GET requests with a URL and a collection of query string parameters

```
Scenario: Hammer URL with parameters
	When I hammer 'https://{{Enter your URL here}}' with up to '10' concurrent requests for a maximum of '30000' millseconds, with query parameters
	| Key   | Value         |
	| text  | example_text  |
	| text2 | example_text2 |
	| etc   | etc           |

	Then the average response time is less than '1000' milliseconds
	
	And there are fewer than '5' failed responses

```

# Add request headers to a Drill or Hammer operation

```
Given the request headers
	| Key           | Value               |
	| Authorization | Bearer blahblahblah |
```


# Getting to the data

Any data which is returned by a Drill/Hammer operation is stored in SpecFlow's ScenarioContext.


## Getting Drill results

```
var actualAverageResponseTime = _scenarioContext.Get<decimal>("AverageResponseTime");

var actualFailureCount = _scenarioContext.Get<int>("FailureCount");

var drillStats = _scenarioContext.Get<DrillStats>("DrillStats");
```

## Getting Hammer results

```
var actualAverageResponseTime = _scenarioContext.Get<decimal>("AverageResponseTime");

var actualFailureCount = _scenarioContext.Get<int>("FailureCount");
	
var hammerStats = _scenarioContext.Get<HammerStats>("HammerStats");

```

## HammerStats object definition

```
public class HammerStats
{
	public List<HammerSwingResult> HammerSwingStats { get; set; }
}
```

## HammerSwingResult definition

```
public class HammerSwingResult
{
	public decimal AverageResponseTime { get; set; }
	public int TotalRequestCount { get; set; }
	public int FailureCount { get; set; }
	public List<RequestResult> RequestResults { get; set; }
}
```
