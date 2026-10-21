namespace UserDefinedApiToolkit.Tests.Runtime.Pipeline.TestFiles
{
	using System;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	internal sealed class FirstMiddleware : IApiMiddleware
	{
		private readonly PipelineRecorder _recorder;

		public FirstMiddleware(PipelineRecorder recorder)
		{
			_recorder = recorder;
		}

		public void Invoke(ApiContext context, ApiRequestDelegate next)
		{
			_recorder.Events.Add("first-before");
			next(context);
			_recorder.Events.Add("first-after");
		}
	}

	internal sealed class SecondMiddleware : IApiMiddleware
	{
		private readonly PipelineRecorder _recorder;

		public SecondMiddleware(PipelineRecorder recorder)
		{
			_recorder = recorder;
		}

		public void Invoke(ApiContext context, ApiRequestDelegate next)
		{
			_recorder.Events.Add("second-before");
			next(context);
			_recorder.Events.Add("second-after");
		}
	}

	internal sealed class ShortCircuitMiddleware : IApiMiddleware
	{
		private readonly PipelineRecorder _recorder;

		public ShortCircuitMiddleware(PipelineRecorder recorder)
		{
			_recorder = recorder;
		}

		public void Invoke(ApiContext context, ApiRequestDelegate next)
		{
			_recorder.Events.Add("short-circuit");
			new StatusCodeResult(204).ExecuteResult(context);
		}
	}

	internal sealed class ThrowingMiddleware : IApiMiddleware
	{
		public void Invoke(ApiContext context, ApiRequestDelegate next)
		{
			throw new InvalidOperationException("middleware failure");
		}
	}

	internal sealed class RequestLoggingMiddleware : IApiMiddleware
	{
		private readonly PipelineRecorder _recorder;

		public RequestLoggingMiddleware(PipelineRecorder recorder)
		{
			_recorder = recorder;
		}

		public void Invoke(ApiContext context, ApiRequestDelegate next)
		{
			_recorder.Events.Add("request:" + context.Request.Route);
			next(context);
			_recorder.Events.Add("response:" + context.Response.ResponseCode);
		}
	}

	internal sealed class RecordingExceptionHandler : IApiExceptionHandler
	{
		private readonly ExceptionRecorder _recorder;

		public RecordingExceptionHandler(ExceptionRecorder recorder)
		{
			_recorder = recorder;
		}

		public IApiResult Handle(Exception exception, ApiContext context)
		{
			_recorder.Exception = exception;
			_recorder.Context = context;
			return new ObjectResult<string>(418, "handled");
		}
	}
}
