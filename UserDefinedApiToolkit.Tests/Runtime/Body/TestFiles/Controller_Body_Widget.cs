namespace UserDefinedApiToolkit.Tests.Runtime.Body.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[ApiController]
	[Route("/v1/body-widget")]
	public class Controller_Body_Widget : ControllerBase
	{
		[HttpPost]
		public IApiResult PostWidget([FromBody] Widget widget)
		{
			return Ok(widget is null);
		}
	}

	public class Widget
	{
		public string Name { get; set; } = string.Empty;
	}
}
