namespace UserDefinedApiToolkit.Tests.Runtime.Pipeline.TestFiles
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[ApiController]
	[Route("v1/pipeline")]
	internal sealed class Controller_Pipeline : ControllerBase
	{
		private readonly PipelineRecorder _recorder;

		public Controller_Pipeline(PipelineRecorder recorder)
		{
			_recorder = recorder;
		}

		[HttpGet("success")]
		public IApiResult Success()
		{
			_recorder.Events.Add("action");
			return Ok("success");
		}

		[HttpGet("throw")]
		public IApiResult Throw()
		{
			throw new InvalidOperationException("controller failure");
		}

		[HttpGet("header-throw")]
		public IApiResult HeaderThrow()
		{
			Response.ResponseHeaders.Set("X-Endpoint", new List<string> { "endpoint" });
			throw new InvalidOperationException("controller failure after setting a header");
		}
	}
}
