namespace UserDefinedApiToolkit.Tests.Runtime.Pipeline.TestFiles
{
	using System;

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
	}
}
