namespace UserDefinedApiToolkit.Tests.Runtime.FrameworkDependency.TestFiles
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[ApiController]
	[Route("/v1/framework-dependency")]
	public class Controller_FrameworkDependency : ControllerBase
	{
		[HttpGet]
		public IApiResult GetDummy()
		{
			return Ok(Array.Empty<string>());
		}

		[HttpGet("engine")]
		public IApiResult GetWithEngine(string dummy, IEngine engine)
		{
			return Ok(dummy);
		}

		[HttpGet("connection")]
		public IApiResult GetWithConnection(IConnection connection)
		{
			return Ok(connection is not null);
		}
	}
}
