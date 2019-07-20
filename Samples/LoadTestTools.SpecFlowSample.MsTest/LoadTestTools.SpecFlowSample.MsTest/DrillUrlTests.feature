Feature: DrillUrlTests

Scenario: Drill URL
	When I drill 'https://www.google.com/complete/search?q=des%20moines&cp=10&client=psy-ab&xssi=t&gs_ri=gws-wiz&hl=en&authuser=0&psi=_2YyXZ70JJrWtQa7gxo.1563584256429' with '2' concurrent connections for '2000' milliseconds
	Then the average response time is less than '500' milliseconds
	And there are fewer than '5' failed responses