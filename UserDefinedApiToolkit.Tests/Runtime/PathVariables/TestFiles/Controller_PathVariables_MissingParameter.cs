namespace UserDefinedApiToolkit.Tests.Runtime.PathVariables.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	// Invalid: the route template has a "{id}" placeholder, but no method parameter (implicit or
	// via [FromRoute]) binds it. Build() should throw.
	[ApiController]
	[Route("/v1/items")]
	public class Controller_PathVariables_MissingParameter : ControllerBase
	{
		[HttpGet("{id}")]
		public IApiResult GetById()
		{
			return Ok(0);
		}
	}
}
