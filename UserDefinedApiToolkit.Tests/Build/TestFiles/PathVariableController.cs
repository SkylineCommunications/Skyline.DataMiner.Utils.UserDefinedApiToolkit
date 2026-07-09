namespace UserDefinedApiToolkit.Tests.Build.TestFiles
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[ApiController]
	[Route("v1/items")]
	public class PathVariableController : ControllerBase
	{
		/// <summary>
		/// Gets an item by its identifier, bound implicitly from the "{id}" route placeholder.
		/// </summary>
		/// <param name="id">The identifier of the item.</param>
		[HttpGet("{id}")]
		public IApiResult GetById(int id)
		{
			return Ok(id);
		}

		/// <summary>
		/// Gets item details, binding the route placeholder via an explicit [FromRoute] override.
		/// </summary>
		/// <param name="itemId">The identifier of the item.</param>
		[HttpGet("{id}/details")]
		public IApiResult GetDetails([FromRoute(Name = "id")] int itemId)
		{
			return Ok(itemId);
		}

		/// <summary>
		/// Lists all items, no route template on the method (uses only the controller route).
		/// </summary>
		[HttpGet]
		public IApiResult GetAll()
		{
			return Ok(System.Array.Empty<int>());
		}
	}
}
