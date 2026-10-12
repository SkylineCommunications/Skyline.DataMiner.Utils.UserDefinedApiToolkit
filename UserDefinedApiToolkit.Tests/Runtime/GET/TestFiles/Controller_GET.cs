namespace UserDefinedApiToolkit.Tests.Runtime.GET.TestFiles
{
	using System.Collections.ObjectModel;

	using Skyline.DataMiner.Automation;
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
		public IApiResult GetWithIntQuery(int limit)
		{
			return Ok(limit);
		}

		[HttpGet]
		public IApiResult GetWithBody([FromBody] string body)
		{
			return Ok(body);
		}

		[HttpGet]
		public IApiResult GetWithFrameworkDependency(string dummy, IEngine engine)
		{
			return Ok(dummy);
		}

		[HttpGet("ListString")]
		public IApiResult GetWithListQuery_ListString(List<string> list)
		{
			return Ok(list);
		}

		[HttpGet("ListInt")]
		public IApiResult GetWithListQuery_ListInt(List<int> list)
		{
			return Ok(list);
		}

		[HttpGet("ArrayString")]
		public IApiResult GetWithListQuery_ArrayString(string[] list)
		{
			return Ok(list);
		}

		[HttpGet("ArrayInt")]
		public IApiResult GetWithListQuery_ArrayInt(int[] list)
		{
			return Ok(list);
		}

		[HttpGet("IEnumerableString")]
		public IApiResult GetWithListQuery_IEnumerableString(IEnumerable<string> list)
		{
			return Ok(list);
		}

		[HttpGet("IEnumerableInt")]
		public IApiResult GetWithListQuery_IEnumerableInt(IEnumerable<int> list)
		{
			return Ok(list);
		}

		[HttpGet("ICollectionString")]
		public IApiResult GetWithListQuery_ICollectionString(ICollection<string> list)
		{
			return Ok(list);
		}

		[HttpGet("ICollectionInt")]
		public IApiResult GetWithListQuery_ICollectionInt(ICollection<int> list)
		{
			return Ok(list);
		}

		[HttpGet("ISetString")]
		public IApiResult GetWithListQuery_ISetString(ISet<string> list)
		{
			return Ok(list);
		}

		[HttpGet("ISetInt")]
		public IApiResult GetWithListQuery_ISetInt(ISet<int> list)
		{
			return Ok(list);
		}

		[HttpGet("HashSetString")]
		public IApiResult GetWithListQuery_HashSetString(HashSet<string> list)
		{
			return Ok(list);
		}

		[HttpGet("HashSetInt")]
		public IApiResult GetWithListQuery_HashSetInt(HashSet<int> list)
		{
			return Ok(list);
		}

		[HttpGet("CollectionString")]
		public IApiResult GetWithListQuery_CollectionString(Collection<string> list)
		{
			return Ok(list);
		}

		[HttpGet("CollectionInt")]
		public IApiResult GetWithListQuery_CollectionInt(Collection<int> list)
		{
			return Ok(list);
		}
	}
}
