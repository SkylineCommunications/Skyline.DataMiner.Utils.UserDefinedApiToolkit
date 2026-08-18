namespace UserDefinedApiToolkit.Tests.Build.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[ApiController]
	[Route("v1/installer")]
	public class InstallerController : ControllerBase
	{
		[HttpGet("{id}")]
		public IApiResult Get(int id)
		{
			return Ok();
		}

		[HttpPatch("{id}")]
		public IApiResult Patch(int id)
		{
			return Ok();
		}

		public IApiResult HelperMethod()
		{
			return Ok();
		}
	}
}
