# Getting started

## Installation

Add the NuGet package to the project that contains your Automation script and controllers:

```bash
dotnet add package Skyline.DataMiner.Utils.UserDefinedApiToolkit
```

The toolkit targets .NET Framework 4.8 and is intended to run from a DataMiner Automation script
that is triggered by a User-Defined API request.

## Quick start

Build the API once, cache it, and call `Run` for each incoming request:

```csharp
using Microsoft.Extensions.DependencyInjection;

using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

public static class Script
{
	private static IUserDefinedApi _api;

	[AutomationEntryPoint(AutomationEntryPointType.Types.OnApiTrigger)]
	public static ApiTriggerOutput OnApiTrigger(IEngine engine, ApiTriggerInput request)
	{
		if (_api is null)
		{
			_api = UserDefinedApi.CreateBuilder()
				.AddControllers()
				.Build();
		}

		return _api.Run(engine, request);
	}
}

[ApiController]
[Route("v1/health")]
public class HealthController : ControllerBase
{
	[HttpGet]
	public IApiResult Get()
	{
		return Ok(new { Status = "OK" });
	}
}
```

`AddControllers()` scans the calling assembly for public, non-abstract `ControllerBase`
implementations that have a non-empty `[Route]` attribute. The route in the example is relative to
the DataMiner User-Defined API endpoint. DataMiner exposes User-Defined APIs at:

```text
https://<dataminer-host>/api/custom/<route>
```

Therefore, the example can be called at
`https://<dataminer-host>/api/custom/v1/health`. See the official documentation on
[triggering a User-Defined API](https://docs.dataminer.services/dataminer/Functions/User-Defined_APIs/UD_APIs_Triggering_an_API.html)
for authentication and request details.

Next: [controllers and routing](controllers-and-routing.md).
