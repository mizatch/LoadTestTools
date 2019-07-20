Feature: DrillUrlTests

Scenario: Drill URL
	When I drill 'https://{Enter your URL here}' with '2' concurrent connections for '2000' milliseconds
	Then the average response time is less than '500' milliseconds
	And there are fewer than '5' failed responses