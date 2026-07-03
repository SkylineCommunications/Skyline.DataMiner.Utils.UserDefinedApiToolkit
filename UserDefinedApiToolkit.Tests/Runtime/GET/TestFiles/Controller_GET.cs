namespace UserDefinedApiToolkit.Tests.Runtime.GET.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[ApiController]
	[Route("/v1/get")]
	public class Controller_GET : ControllerBase
	{
		[HttpGet]
		public IApiResult GetDummy()
		{
			return Ok(Array.Empty<string>());
		}

		[HttpGet]
		public IApiResult GetWithQuery(string query)
		{
			return Ok(new Dictionary<string, List<string>>
			{
				[nameof(query)] = new List<string> { query },
			});
		}

		[HttpGet]
		public IApiResult GetWithMultiQuery(string query, string limit)
		{
			return Ok(new Dictionary<string, List<string>>
			{
				[nameof(query)] = new List<string> { query },
				[nameof(limit)] = new List<string> { limit },
			});
		}

		[HttpGet]
		public IApiResult GetWithBody([FromBody] string body)
		{
			return Ok(body);
		}
	}
}
