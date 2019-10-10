Feature: DrillUrlTests

Scenario: Drill URL

	When I drill 'https://{Enter your URL here}' with '2' concurrent connections for '2000' milliseconds
	
	Then the average response time is less than '500' milliseconds
	
	And there are fewer than '5' failed responses


Scenario: Drill URL and record to Application Insights

	Given I want to record results to Application Insights

	When I drill 'https://{Enter your URL here}' with '2' concurrent connections for '2000' milliseconds
	
	Then the average response time is less than '500' milliseconds
	
	And there are fewer than '5' failed responses


Scenario: Drill URL with POST requests and Json payload

	Given I wait '0' milliseconds after each request

	When I drill 'https://{Enter your URL here}' with '20' concurrent 'POST' connections for '30000' milliseconds and Json payload
		| Key   |
		| Value |
	
	Then the average response time is less than '5000' milliseconds
	
	And there are fewer than '5' failed responses


Scenario: Drill URL with parameters
	When I drill 'https://{Enter your URL here}' with '2' concurrent connections for '2000' milliseconds, with query parameters
	| Key  | Value        |
	| text | example_text |

	Then the average response time is less than '500' milliseconds
	
	And there are fewer than '5' failed responses


Scenario: Drill URL with request headers
	Given the request headers
		| Key           | Value               |
		| Authorization | Bearer blahblahblah |
	
	When I drill 'https://{Enter your URL here}' with '2' concurrent connections for '2000' milliseconds
	
	Then the average response time is less than '500' milliseconds
	
	And there are fewer than '5' failed responses