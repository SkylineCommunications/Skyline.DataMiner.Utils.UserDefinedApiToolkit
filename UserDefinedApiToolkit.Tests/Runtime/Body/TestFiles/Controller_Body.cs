namespace UserDefinedApiToolkit.Tests.Runtime.Body.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[ApiController]
	[Route("/v1/body")]
	public class Controller_Body : ControllerBase
	{
		[HttpPost]
		public IApiResult PostAmount([FromBody] int amount)
		{
			return Ok(amount);
		}
	}
}
