# Creating an API trigger input from a script parameter

> **Workaround**
>
> This page documents a workaround for triggering the toolkit from a normal Automation script
> run. It is intentionally not linked from the public documentation index.

When a normal Automation script needs to invoke a `UserDefinedApi` instance, store a JSON
representation of the request in a script parameter and retrieve it with the `IEngine` extension:

```csharp
// Build the API once and cache it; AddControllers() scans the calling assembly and
// registers every public ControllerBase implementation decorated with [Route].
private static Lazy<IUserDefinedApi> _api = new Lazy<IUserDefinedApi>(() => UserDefinedApi.CreateBuilder()
	.AddControllers()
	.Build());

/// <summary>
/// The script entry point.
/// </summary>
/// <param name="engine">Link with SLAutomation process.</param>
public void Run(IEngine engine)
{
	try
	{
		var input = engine.GetApiTriggerInput("ApiInput");
		var result = OnApiTrigger(engine, input);

		engine.AddOrUpdateScriptOutput("response_code", Convert.ToString(result.ResponseCode));
		engine.AddOrUpdateScriptOutput("response_body", result.ResponseBody);
		engine.AddOrUpdateScriptOutput("response_body_bytes", Convert.ToBase64String(result.ResponseBodyBytes));
		engine.AddOrUpdateScriptOutput("response_headers", JsonConvert.SerializeObject(result.ResponseHeaders.AsDictionary()));
	}
	catch (ScriptAbortException)
	{
		// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
		throw; // Comment if it should be treated as a normal exit of the script.
	}
	catch (ScriptForceAbortException)
	{
		// Catch forced abort exceptions, caused via external maintenance messages.
		throw;
	}
	catch (ScriptTimeoutException)
	{
		// Catch timeout exceptions for when a script has been running for too long.
		throw;
	}
	catch (InteractiveUserDetachedException)
	{
		// Catch a user detaching from the interactive script by closing the window.
		// Only applicable for interactive scripts, can be removed for non-interactive scripts.
		throw;
	}
	catch (Exception e)
	{
		engine.ExitFail("Run|Something went wrong: " + e);
	}
}

/// <summary>
///     The Script entry point.
/// </summary>
/// <param name="engine">Link with SLScripting process.</param>
/// <param name="requestData">The incoming API request.</param>
[AutomationEntryPoint(AutomationEntryPointType.Types.OnApiTrigger)]
public static ApiTriggerOutput OnApiTrigger(IEngine engine, ApiTriggerInput requestData)
{
	return _api.Value.Run(engine, requestData);
}
```

The overload that uses a numeric script parameter ID is also available:

```csharp
var request = engine.GetApiTriggerInput(1);
```

## JSON format

Use the `ApiTriggerInput` property names in the JSON object. The `RequestMethod` value is written
as a string:

```json
{
  "RequestMethod": "Post",
  "Route": "/v1/items",
  "RawBody": "{\"name\":\"test\"}",
  "QueryParameters": {
    "tag": ["one", "two"],
    "page": "2"
  }
}
```

`RawBody` is a JSON string, so quotes inside the body must be escaped. Query parameter values can
be supplied as either a string or an array of strings.

## Important limitation

The default `NewtonsoftConverter` does not preserve query parameter values when it serializes an
`ApiTriggerInput`. Because `IQueryParameters` exposes methods rather than serializable properties,
the default serializer produces `"QueryParameters": {}`. Provide the `QueryParameters` object
explicitly as shown above when creating the script parameter JSON.

The helper reconstructs `IQueryParameters` as a `QueryParameters` instance before the request is
passed to the toolkit.
