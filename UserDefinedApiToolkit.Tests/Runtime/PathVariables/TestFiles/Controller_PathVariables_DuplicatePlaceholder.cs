namespace UserDefinedApiToolkit.Tests.Runtime.PathVariables.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	// Invalid: the route template repeats the "{id}" placeholder name. RouteTemplate.Match
	// captures placeholders into a single dictionary keyed by name, so the second occurrence
	// would silently overwrite the first at runtime. Build() should reject this as ambiguous.
	[ApiController]
	[Route("/v1/items")]
	public class Controller_PathVariables_DuplicatePlaceholder : ControllerBase
	{
		[HttpGet("{id}/{id}")]
		public IApiResult GetById(string id)
		{
			return Ok(id);
		}
	}
}
