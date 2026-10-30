namespace UserDefinedApiToolkit.Tests.Build.TestFiles
{
	using System;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	/// <summary>
	/// Fixture controller dedicated to exercising the various ways an operation's responses
	/// can be described: explicit <see cref="ProducesResponseTypeAttribute"/> usages (with and
	/// without a response type, single and multiple), the generic <see cref="ApiResult{TSuccess, TError}"/>
	/// fallback, priority between the two, collection response types, and the "no information at all" case.
	/// </summary>
	[ApiController]
	[Route("v1/responses")]
	public class ResponseTypesController : ControllerBase
	{
		[HttpGet]
		[ProducesResponseType(typeof(SampleDto), 200, "Successful response")]
		public IApiResult GetWithExplicitTypedResponse()
		{
			return Ok(new SampleDto());
		}

		[HttpGet]
		[ProducesResponseType(204, " ")]
		public IApiResult GetWithStatusOnlyResponse()
		{
			return NoContent();
		}

		[HttpGet]
		[ProducesResponseType(typeof(SampleDto), 200)]
		[ProducesResponseType(404, "Resource not found")]
		public IApiResult GetWithMultipleResponses()
		{
			return Ok(new SampleDto());
		}

		[HttpGet]
		[ProducesResponseType(typeof(SampleDto), 201, "Created response")]
		public IApiResult GetWithTypedResponseDescription()
		{
			return Created(new SampleDto());
		}

		[HttpGet]
		[ProducesResponseType(409, "First description")]
		[ProducesResponseType(409, "Last description")]
		public IApiResult GetWithDuplicateResponseDescriptions()
		{
			return Conflict();
		}

		[HttpGet]
		[ProducesResponseType(typeof(SampleDto), 200)]
		public ApiResult<SampleDto, string> GetWithExplicitAttributeOverridingApiResult()
		{
			return Ok(new SampleDto());
		}

		[HttpGet]
		public ApiResult<SampleDto, string> GetWithoutExplicitAttribute()
		{
			return Ok(new SampleDto());
		}

		[HttpGet]
		[ProducesResponseType(typeof(SampleDto[]), 200)]
		public IApiResult GetCollectionResponse()
		{
			return Ok(Array.Empty<SampleDto>());
		}

		[HttpDelete]
		public IApiResult DeleteWithNoResponseInfo()
		{
			return Ok();
		}
	}
}
