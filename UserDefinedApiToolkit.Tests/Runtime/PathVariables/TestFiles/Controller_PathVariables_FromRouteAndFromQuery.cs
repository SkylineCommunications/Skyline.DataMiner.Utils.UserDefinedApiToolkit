namespace UserDefinedApiToolkit.Tests.Runtime.PathVariables.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	// Valid but unusual: "id" is decorated with both [FromRoute] and [FromQuery]. At runtime,
	// ParameterBinder.Classify checks [FromRoute] before [FromQuery], so this parameter is
	// route-bound. Build() validation must agree with that precedence instead of treating the
	// [FromQuery] attribute as disqualifying the parameter from satisfying the "{id}" placeholder.
	[ApiController]
	[Route("/v1/items")]
	public class Controller_PathVariables_FromRouteAndFromQuery : ControllerBase
	{
		[HttpGet("{id}")]
		public IApiResult GetById([FromRoute] [FromQuery] int id)
		{
			return Ok(id);
		}
	}
}
