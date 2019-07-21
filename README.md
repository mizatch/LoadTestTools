# LoadTestTools

This solution was created with the goal making it easy to create and manage API load tests.  To accomplish this goal, a package was created with a set of predefined SpecFlow bindings which allow a consumer to easily make API GET requests to any URL.

**_These libraries should only be used for good.  Do not load test any API without owner approval._**

# Instructions

* Create a Unit Test Project (.Net Framework).
* Using Nuget, install the package called "LoadTestTools.SpecFlowBindings.MsTest".
* Add a specflow.json file to your test project and add a stepAssembly reference to "LoadTestTools.SpecFlowBindings.MsTest".  A sample of mine can be found below.


specflow.json:

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
