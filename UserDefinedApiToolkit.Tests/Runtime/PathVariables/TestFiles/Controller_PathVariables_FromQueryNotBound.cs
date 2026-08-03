namespace UserDefinedApiToolkit.Tests.Runtime.PathVariables.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	// Invalid: the "{id}" placeholder is only "matched" by a [FromQuery]-attributed parameter
	// named "id". Explicit [FromQuery] means the parameter is always query-bound, never
	// route-bound, so Build() should throw.
	[ApiController]
	[Route("/v1/items")]
	public class Controller_PathVariables_FromQueryNotBound : ControllerBase
	{
		[HttpGet("{id}")]
		public IApiResult GetById([FromQuery] int id)
		{
			return Ok(id);
		}
	}
}
