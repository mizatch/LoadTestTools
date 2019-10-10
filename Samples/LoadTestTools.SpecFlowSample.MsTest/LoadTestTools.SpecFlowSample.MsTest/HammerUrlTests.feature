Feature: HammerUrlTests

Scenario: Hammer URL
	When I hammer 'https://{Enter your URL here}' with up to '20' concurrent requests for a maximum of '30000' millseconds
	
	Then the average response time is less than '1000' milliseconds
	
	And there are fewer than '5' failed responses


Scenario: Hammer URL and record to Application Insights

	Given I want to record results to Application Insights

	When I hammer 'https://{Enter your URL here}' with up to '20' concurrent requests for a maximum of '30000' millseconds
	
	Then the average response time is less than '1000' milliseconds
	
	And there are fewer than '5' failed responses


Scenario: Hammer URL with POST requests and Json payload
	
	When I hammer 'https://{Enter your URL here}' with up to '20' concurrent 'POST' requests for a maximum of '30000' milliseconds and Json payload
		| Key   |
		| Value |

	Then the average response time is less than '1000' milliseconds
	
	And there are fewer than '5' failed responses


Scenario: Hammer URL with parameters
	When I hammer 'https://{Enter your URL here}' with up to '20' concurrent requests for a maximum of '30000' millseconds, with query parameters
	| Key  | Value        |
	| text | example_text |

	Then the average response time is less than '500' milliseconds
	
	And there are fewer than '5' failed responses


Scenario: Hammer URL with request headers
	Given the request headers
		| Key           | Value               |
		| Authorization | Bearer blahblahblah |

	When I hammer 'https://{Enter your URL here}' with up to '20' concurrent requests for a maximum of '30000' millseconds
	
	Then the average response time is less than '1000' milliseconds
	
	And there are fewer than '5' failed responses