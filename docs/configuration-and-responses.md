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

### Default registrations

The toolkit adds the following services and components automatically:

| Registration | Lifetime | Purpose |
| --- | --- | --- |
| Each registered controller type | Scoped | Creates one controller instance per request. |
| `IAccessor<IEngine>` | Scoped | Provides the current request's `IEngine` to controllers and services. |
| `IAccessor<IConnection>` | Scoped | Provides the current request's user `IConnection` to controllers and services. |
| `ILogger<T>` | Scoped | Writes categorized logs through the current `IEngine`. |
| `ILogger` | Scoped | Writes uncategorized logs through the current `IEngine`. |
| `IApiMiddleware` registered with `UseMiddleware<T>()` | Scoped | Adds the middleware to the fluent pipeline in registration order. |
| `IApiExceptionHandler` and exception middleware | Scoped, opt-in | Handles exceptions from components registered after `UseExceptionHandler<T>()`. |

`IEngine` and `IConnection` themselves are not registered as regular DI services. Inject
`IAccessor<IEngine>` or `IAccessor<IConnection>` when constructor-injected access is needed.
Action parameters of those framework types are supplied directly by the toolkit's parameter
binding.

The toolkit also creates a default `NewtonsoftConverter` for request-body input and result
output conversion. It is held by the request context rather than registered in the DI container.
Use `WithDefaultInputConverter`, `WithDefaultOutputConverter`, `AddInputConverter`, or
`AddOutputConverter` to replace or extend the converter behavior.

For test or integration work that starts with a serialized request, the
`ApiTriggerInputHelper.Deserialize` workaround creates an `ApiTriggerInput` and reconstructs its
`IQueryParameters` property. Query parameters can be represented by a string or an array of
strings:

```json
{
  "requestMethod": "Get",
  "route": "/v1/items",
  "queryParameters": {
    "tag": ["one", "two"],
    "page": "2"
  }
}
```

When the JSON is stored in an Automation script parameter, use the `IEngine` extension directly:

```csharp
var request = engine.GetApiTriggerInput("ApiInput");
// Or: var request = engine.GetApiTriggerInput(1);
```

## Middleware

The toolkit supports a synchronous middleware pipeline for cross-cutting request concerns such as
logging, authorization, validation, and exception handling. Middleware is resolved from the
request DI scope. Middleware runs in registration order, with the first registered middleware
being the outermost component. Middleware can be registered with `UseMiddleware<T>()`,
`UseExceptionHandler<T>()`, or directly through
`ConfigureServices` as an `IApiMiddleware` service.

```csharp
_api = UserDefinedApi.CreateBuilder()
	.AddControllers()
	.UseMiddleware<RequestLoggingMiddleware>()
	.UseExceptionHandler<ApiExceptionHandler>()
	.Build();
```

Fluent middleware registrations define the explicit pipeline order. Middleware registered through
`ConfigureServices` as an `IApiMiddleware` is supported, but is appended when the deferred
configuration callbacks run during `Build()`; it is not interleaved with fluent registrations
based on the position of the `ConfigureServices` call. Use `UseMiddleware<T>()` or
`UseExceptionHandler<T>()` when a middleware component needs a precise position.

Middleware can short-circuit a request by not calling `next`:

```csharp
public sealed class RequestLoggingMiddleware : IApiMiddleware
{
	public void Invoke(ApiContext context, ApiRequestDelegate next)
	{
		// Log request metadata here.
		next(context);
		// Log response metadata here.
	}
}
```

In this example, the logging middleware wraps the exception handler. It can log the original
request before the handler creates an error response, and inspect the handled response after
`next` returns. Register `UseExceptionHandler<T>()` before another middleware when that
middleware's exceptions should be handled. Register a logging middleware after the exception
handler when it should observe the original exception, catch it, log it, and rethrow it before
the exception handler converts it into a response.

The pipeline is synchronous because it executes inside a DataMiner Automation API trigger. Do not
use asynchronous continuations with DataMiner `IEngine` or other request-scoped DataMiner objects.

### Global exception handling

`UseExceptionHandler<T>()` adds exception handling at its registration position. It catches
exceptions thrown by middleware and endpoint execution registered after it. The handler receives
the original exception and the current `ApiContext`, then returns the `IApiResult` that should be
sent to the caller:

```csharp
public sealed class ApiExceptionHandler : IApiExceptionHandler
{
	public IApiResult Handle(Exception exception, ApiContext context)
	{
		if (exception is InvalidParameterException)
		{
			return new StatusCodeResult(400);
		}

		return new StatusCodeResult(500);
	}
}
```

The handler should log diagnostic details internally and return stable, non-sensitive responses.
Do not expose exception messages, stack traces, raw request bodies, credentials, or tokens. Abort
and timeout exceptions raised by DataMiner are not converted into API responses.

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
| `UseMiddleware<TMiddleware>()` | Add a middleware component to the request pipeline. |
| `UseExceptionHandler<TExceptionHandler>()` | Add exception middleware at the current registration position and configure its handler. |
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
