namespace UserDefinedApiToolkit.Tests.Runtime.PATCH.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[ApiController]
	[Route("/v1/patch")]
	public class Controller_PATCH : ControllerBase
	{
		[HttpPatch]
		public IApiResult PatchDummy([FromBody] string body)
		{
			return Ok(body);
		}

		[HttpPatch("{id}")]
		public IApiResult PatchWithId(int id, [FromBody] string body)
		{
			return Ok($"{id}:{body}");
		}
	}
}
