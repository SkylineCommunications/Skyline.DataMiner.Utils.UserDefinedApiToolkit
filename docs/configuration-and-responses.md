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
| Middleware registered with `UseMiddleware<T>()` | Scoped | Adds the middleware to the fluent pipeline in registration order. |
| Exception handlers and middleware | Scoped, opt-in | Handles exceptions from components registered after the first `UseExceptionHandler<T>()`; handlers are tried in registration order. |

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
being the outermost component. Register pipeline middleware with `UseMiddleware<T>()` or
`UseExceptionHandler<T>()`. Use `ConfigureServices` to register dependencies consumed by
middleware, controllers, and handlers.

```csharp
_api = UserDefinedApi.CreateBuilder()
	.AddControllers()
	.UseMiddleware<RequestLoggingMiddleware>()
	.UseExceptionHandler<ValidationExceptionHandler>()
	.UseExceptionHandler<ApiExceptionHandler>()
	.Build();
```

The fluent middleware methods define the complete pipeline order. Middleware is resolved lazily
when its position is reached, so a short-circuiting middleware does not activate later
middleware. This also allows an exception handler to catch activation failures from middleware
registered after it.

Register middleware through the fluent `UseMiddleware<T>()` method. `ConfigureServices` is used
for dependencies; registering an `IApiMiddleware` implementation there does not add it to the
request pipeline.

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

The exception middleware is added at the position of the first `UseExceptionHandler<T>()` call.
Additional exception handlers are tried in registration order within that middleware. A handler
returns `true` when it handled the exception and provides the response result through the
`out` parameter. It returns `false` to let the next handler try. If no handler handles the
exception, the middleware returns a 500 response. When an exception is handled, headers that
were set by middleware outside the exception handler are preserved, while headers written by
the failed downstream processing are discarded.

The pipeline is synchronous because it executes inside a DataMiner Automation API trigger. Do not
use asynchronous continuations with DataMiner `IEngine` or other request-scoped DataMiner objects.

### Global exception handling

`UseExceptionHandler<T>()` registers an exception handler. The first call adds exception handling
at its registration position, so it catches exceptions thrown by middleware and endpoint execution
registered after it. Additional handlers are tried in registration order. Each handler receives
the original exception and the current `ApiContext`:

```csharp
public sealed class ValidationExceptionHandler : IApiExceptionHandler
{
	public bool TryHandle(Exception exception, ApiContext context, out IApiResult? result)
	{
		if (exception is InvalidParameterException)
		{
			result = new StatusCodeResult(400);
			return true;
		}

		result = null;
		return false;
	}
}
```

The handler should log diagnostic details internally and return stable, non-sensitive responses.
Do not expose exception messages, stack traces, raw request bodies, credentials, or tokens. Abort
and timeout exceptions raised by DataMiner are also passed to exception handlers.

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
| `UseExceptionHandler<TExceptionHandler>()` | Add a handler; the first call also adds exception middleware at the current registration position. |
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
