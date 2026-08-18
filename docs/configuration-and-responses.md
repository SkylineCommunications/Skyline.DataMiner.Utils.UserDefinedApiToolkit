# Configuration and responses

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
return NotFound();                // 404 without a body
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
