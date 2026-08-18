# Controllers and routing

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
