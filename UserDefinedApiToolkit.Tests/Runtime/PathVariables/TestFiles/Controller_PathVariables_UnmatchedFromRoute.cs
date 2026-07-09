namespace UserDefinedApiToolkit.Tests.Runtime.PathVariables.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	// Invalid: [FromRoute(Name = "id")] references a placeholder that does not exist anywhere in
	// the combined route template (there is no "{id}" segment at all). Build() should throw.
	[ApiController]
	[Route("/v1/items")]
	public class Controller_PathVariables_UnmatchedFromRoute : ControllerBase
	{
		[HttpGet]
		public IApiResult GetById([FromRoute(Name = "id")] int id)
		{
			return Ok(id);
		}
	}
}
