namespace UserDefinedApiToolkit.Tests.Runtime.PathVariables.TestFiles
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	// Invalid: the "{id}" placeholder is only "matched" by an unattributed framework-provided
	// parameter (IEngine) that happens to be named "id". Framework-provided parameters are never
	// route-bound (they're resolved by type, not by name), so Build() should throw.
	[ApiController]
	[Route("/v1/items")]
	public class Controller_PathVariables_FrameworkParamNotBound : ControllerBase
	{
		[HttpGet("{id}")]
		public IApiResult GetById(IEngine id)
		{
			return Ok(id is not null);
		}
	}
}
