# User-Defined API Toolkit

The User-Defined API Toolkit provides an attribute-based way to implement REST-like APIs in
DataMiner Automation scripts. Controllers use familiar route and HTTP method attributes, while the
toolkit handles route selection, parameter binding, dependency injection, typed results, and
optional OpenAPI generation.

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
the DataMiner User-Defined API endpoint.

## Defining controllers and routes

A controller must inherit from `ControllerBase` and have a non-empty `[Route]`. Public instance
methods decorated with an HTTP method attribute become actions:

```csharp
[ApiController]
[Route("v1/items")]
public class ItemsController : ControllerBase
{
	[HttpGet]
	public IApiResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
	{
		return Ok(new { Page = page, PageSize = pageSize });
	}

	[HttpGet("{id}")]
	public ApiResult<ItemDto, string> GetById(int id)
	{
		var item = FindItem(id);
		return item is null ? NotFound("Item not found.") : Ok(item);
	}

	[HttpPost]
	[Consumes("application/json")]
	public ApiResult<ItemDto, string> Create([FromBody] ItemDto item)
	{
		return Created(item);
	}

	private ItemDto FindItem(int id)
	{
		// Read from a repository or DataMiner here.
		return null;
	}
}
```

The supported HTTP attributes are `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpPatch]`, and
`[HttpDelete]`. An optional template on the method attribute is appended to the controller route:

```csharp
[Route("v1/items")]
public class ItemsController : ControllerBase
{
	[HttpGet("count")]
	public IApiResult Count()
	{
		return Ok(42);
	}

	[HttpGet("{id}")]
	public IApiResult Get(int id)
	{
		return Ok(id);
	}
}
```

Only simple placeholder segments such as `{id}` are supported. Route constraints such as
`{id:int}` and catch-all segments such as `{*path}` are not supported. If a literal route and a
placeholder route could both match, the literal route is preferred.

## Parameter binding

Action parameters are resolved in this order:

| Parameter | Source |
| --- | --- |
| `ApiContext`, `IEngine`, `IConnection`, `IServiceProvider` | Provided by the toolkit |
| `[FromBody]` | The raw request body, converted by an input converter |
| `[FromRoute]` | A `{placeholder}` in the combined controller/action route |
| `[FromQuery]` | A query-string value |
| Unattributed parameter matching a route placeholder | The matching route value |
| Other registered service types | Dependency injection |
| Other parameters | A query-string value with the parameter name |

Route parameters can bind implicitly when the C# parameter name matches the placeholder, or
explicitly when the names differ:

```csharp
[HttpGet("{id}/details")]
public IApiResult GetDetails([FromRoute(Name = "id")] int itemId)
{
	return Ok(itemId);
}
```

The same `Name` override is available for query parameters:

```csharp
public IApiResult Search([FromQuery(Name = "q")] string searchTerm)
{
	return Ok(searchTerm);
}
```

Route and query values are converted from strings using the built-in type conversion support.
Common primitive types, enums, `Guid`, dates, and nullable values are supported. Invalid values
raise `InvalidParameterException`. A missing query parameter can use a C# default value, for
example `int page = 1`.

Every route placeholder must have a matching parameter. Conversely, a `[FromRoute]` parameter must
refer to a placeholder in the route. These configuration errors are reported as
`InvalidRouteException` when `Build()` is called.

## Dependency injection

Register application services with `ConfigureServices` and consume them through controller
constructors or action parameters:

```csharp
_api = UserDefinedApi.CreateBuilder()
	.AddControllers()
	.ConfigureServices(services =>
	{
		services.AddScoped<IItemRepository, ItemRepository>();
		services.AddSingleton<IClock, SystemClock>();
	})
	.Build();
```

The toolkit creates a new DI scope for each request. The controller and scoped services are
resolved from that scope. The `IEngine` and `IConnection` for the current request are available
through action parameters or through the `ApiContext`/`ControllerBase` request context.

## Builder configuration

`UserDefinedApi.CreateBuilder()` returns a fluent `UserDefinedApiBuilder`. The main options are:

| Method | Purpose |
| --- | --- |
| `AddController<T>()` | Register one controller type. |
| `AddController(Type)` | Register one controller type discovered at runtime. |
| `AddControllers()` | Scan the calling assembly for controllers. |
| `AddControllersFromAssembly(Assembly)` | Scan a specific assembly for controllers. |
| `ConfigureServices(Action<IServiceCollection>)` | Register application services and configure DI. |
| `WithDefaultInputConverter(IInputConverter)` | Replace the default body input converter. |
| `WithDefaultOutputConverter(IOutputConverter)` | Replace the default result output converter. |
| `AddInputConverter(IInputConverter)` | Add an input converter before the default converter. |
| `AddOutputConverter(IOutputConverter)` | Add an output converter before the default converter. |
| `Build()` | Validate the configuration and create the API instance. |

The default converter is `NewtonsoftConverter`, which reads and writes JSON using
`application/json`. `PlainTextConverter` is available for raw text:

```csharp
_api = UserDefinedApi.CreateBuilder()
	.AddControllers()
	.AddInputConverter(new PlainTextConverter())
	.AddOutputConverter(new PlainTextConverter())
	.Build();
```

Converters added with `AddInputConverter` or `AddOutputConverter` are checked before the default
converter. Implement `IInputConverter` and/or `IOutputConverter` to support custom media types or
serialization rules. A converter must report whether it can handle a CLR type and provide its
media type and conversion method.

## Returning responses

Actions return `IApiResult`, `ApiResult<TSuccess>`, or `ApiResult<TSuccess, TError>`. The
`ControllerBase` helper methods cover the common status codes:

```csharp
return Ok(value);                 // 200 with a body
return Created(value);            // 201 with a body
return BadRequest(error);          // 400 with a body
return NotFound();                 // 404 without a body
return Conflict(error);            // 409 with a body
return NoContent();                // 204 without a body
return StatusCode(202, value);     // custom status code with a body
```

Other helpers include `Unauthorized`, `Forbid`, `UnprocessableEntity`,
`InternalServerError`, and `ServiceUnavailable`. `ObjectResult<T>` values are serialized with
the selected output converter; `StatusCodeResult` values only set the response status code.

`ControllerBase` also exposes `Request`, `Response`, `ApiContext`, `DefaultInputConverter`, and
`DefaultOutputConverter` for actions that need direct access to the current request or response
context.

## OpenAPI generation

The NuGet package includes an MSBuild task that can generate an OpenAPI 3.0 document after the
consumer project builds. Add these properties to the consuming `.csproj`:

```xml
<PropertyGroup>
	<GenerateOpenApi>True</GenerateOpenApi>
	<OpenApiFormat>yaml</OpenApiFormat>
	<OpenApiInfoTitle>Items API</OpenApiInfoTitle>
	<OpenApiInfoVersion>1.0.0</OpenApiInfoVersion>
	<GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Configuration properties:

| Property | Values/default | Description |
| --- | --- | --- |
| `GenerateOpenApi` | `True` to enable | Runs the generator after the build. |
| `OpenApiFormat` | `yaml` | Output format; `yaml` or `json`. |
| `OpenApiInfoTitle` | Project name | API title in the generated document. |
| `OpenApiInfoVersion` | `$(Version)` | API version in the generated document. |
| `GenerateDocumentationFile` | Project setting | When enabled, XML comments enrich the generated document. |

The generated file is written to the build output directory as `openapi.yaml` or `openapi.json`.
`[Consumes]`, `[Produces]`, and `[ProducesResponseType]` describe request and response metadata in
OpenAPI; they do not change runtime serialization or request handling.

## Installer file generation

The installer package generates the JSON metadata files used to install User-Defined APIs. Add the
installer package to a central DataMiner package project
(`<DataMinerType>Package</DataMinerType>`) and list the UDAPI projects that should be built and
included:

```xml
<ItemGroup>
	<UdapiProject Include="..\Orders API\Orders API.csproj" />
	<UdapiProject Include="..\Users API\Users API.csproj" />
</ItemGroup>
```

The installer package builds each listed project before the package project, generates a metadata
file named after each project (for example, `OrdersApi.udapi.json`), and copies the files to the
package project's `SetupContent/UDAPI` directory. Independent projects are built in parallel.
`UdapiProject` uses standard MSBuild item syntax, so projects can also be selected with wildcards:

```xml
<ItemGroup>
	<UdapiProject Include="..\Apis\**\*.csproj"
				 Exclude="..\Apis\UdapiInstaller\**\*" />
</ItemGroup>
```

All matched projects are built and must be UDAPI projects. Exclude the central package project if
it is within the wildcard scope. The installer package validates toolkit availability and the
matching script XML file before building the selected projects.

## Troubleshooting

- **`InvalidControllerException` at registration:** the type does not inherit from
  `ControllerBase` or has no valid `[Route]`.
- **`InvalidRouteException` at `Build()`:** a route placeholder and action parameter do not match,
  or a `[FromRoute]` name is not present in the route.
- **`NoRouteException` at runtime:** the HTTP method or request path does not match any action, or
  a required query parameter is missing.
- **`InvalidParameterException` at runtime:** a route, query, or body value cannot be converted
  to the declared parameter type.
- **No converter found:** register an input/output converter for the relevant type, or replace
  the default converter.

For a complete API surface and additional examples, see the [root README](../README.md).
