namespace UserDefinedApiToolkit.Tests.Runtime.PathVariables.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[ApiController]
	[Route("/v1/items")]
	public class Controller_PathVariables : ControllerBase
	{
		// Implicit binding: unattributed parameter name matches the "{id}" placeholder.
		[HttpGet("{id}")]
		public IApiResult GetById(int id)
		{
			return Ok(id);
		}

		// Explicit [FromRoute(Name = ...)] override: C# parameter name ("itemId") differs from
		// the placeholder name ("id") in the combined route template.
		[HttpGet("{id}/details")]
		public IApiResult GetDetails([FromRoute(Name = "id")] int itemId)
		{
			return Ok(itemId);
		}

		// A fully literal route that should outrank "{id}" when the incoming path is "items/count".
		[HttpGet("count")]
		public IApiResult GetCount()
		{
			return Ok(-1);
		}

		// Combines a route parameter with a [FromQuery(Name = ...)] override.
		[HttpGet("{id}/search")]
		public IApiResult Search(int id, [FromQuery(Name = "q")] string searchTerm)
		{
			return Ok($"{id}:{searchTerm}");
		}
	}
}
