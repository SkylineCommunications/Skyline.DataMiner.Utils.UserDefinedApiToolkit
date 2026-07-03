namespace UserDefinedApiToolkit.Tests.Build.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	public class SampleDto
	{
		public string Name { get; set; } = string.Empty;

		public int Count { get; set; }
	}

	[ApiController]
	[Route("v1/sample")]
	public class SampleController : ControllerBase
	{
		/// <summary>
		/// Gets a sample by its identifier.
		/// </summary>
		/// <param name="id">The identifier of the sample.</param>
		[HttpGet]
		[Produces("application/json")]
		public ApiResult<SampleDto, string> GetById([FromQuery] int id)
		{
			return Ok(new SampleDto());
		}

		[HttpPost]
		[Consumes("application/json")]
		public ApiResult<SampleDto, string> Create([FromBody] SampleDto dto)
		{
			return Ok(dto);
		}

		[HttpDelete]
		public IApiResult Delete()
		{
			return Ok();
		}
	}
}
