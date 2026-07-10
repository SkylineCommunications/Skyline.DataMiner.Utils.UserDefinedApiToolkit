namespace UserDefinedApiToolkit.Tests.Runtime.DI.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[ApiController]
	[Route("/v1/di-test")]
	public class Controller_DI : ControllerBase
	{
		[HttpGet]
		public IApiResult Get(TrackedTransientService service)
		{
			return Ok(service is not null);
		}
	}
}
