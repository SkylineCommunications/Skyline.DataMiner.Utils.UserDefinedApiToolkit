namespace UserDefinedApiToolkit.Tests.Runtime.Pipeline.TestFiles
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
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

	internal sealed class ConstructorThrowingMiddleware : IApiMiddleware
	{
		public ConstructorThrowingMiddleware()
		{
			throw new InvalidOperationException("middleware activation failure");
		}

		public void Invoke(ApiContext context, ApiRequestDelegate next)
		{
			throw new NotSupportedException();
		}
	}

	internal sealed class ConstructorThrowingExceptionHandler : IApiExceptionHandler
	{
		public ConstructorThrowingExceptionHandler()
		{
			throw new InvalidOperationException("exception handler activation failure");
		}

		public bool TryHandle(Exception exception, ApiContext context, out IApiResult? result)
		{
			result = null;
			return false;
		}
	}

	internal sealed class NonMatchingExceptionHandler : IApiExceptionHandler
	{
		private readonly ExceptionHandlerRecorder _recorder;

		public NonMatchingExceptionHandler(ExceptionHandlerRecorder recorder)
		{
			_recorder = recorder;
		}

		public bool TryHandle(Exception exception, ApiContext context, out IApiResult? result)
		{
			_recorder.Events.Add("non-matching");
			result = null;
			return false;
		}
	}

	internal sealed class WritesResponseAndThrowsMiddleware : IApiMiddleware
	{
		public void Invoke(ApiContext context, ApiRequestDelegate next)
		{
			context.Response.ResponseCode = 200;
			context.Response.ResponseBody = "stale response";
			context.Response.ResponseBodyBytes = new byte[] { 1, 2, 3 };
			throw new InvalidOperationException("middleware failure after response");
		}
	}

	internal sealed class SetsHeaderMiddleware : IApiMiddleware
	{
		public void Invoke(ApiContext context, ApiRequestDelegate next)
		{
			context.Response.ResponseHeaders.Set("X-Outer", new List<string> { "outer" });
			next(context);
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

	internal sealed class StatusOnlyExceptionHandler : IApiExceptionHandler
	{
		public bool TryHandle(Exception exception, ApiContext context, out IApiResult? result)
		{
			result = new StatusCodeResult(500);
			return true;
		}
	}

	internal sealed class NullResultExceptionHandler : IApiExceptionHandler
	{
		public bool TryHandle(Exception exception, ApiContext context, out IApiResult? result)
		{
			result = null;
			return true;
		}
	}

	internal sealed class ExceptionHandlerRecorder
	{
		public List<string> Events { get; } = new List<string>();
	}

	internal sealed class PartialResponseExceptionRecorder
	{
		public ApiTriggerOutput? PartialResponse { get; set; }
	}

	internal sealed class PartialResponseExceptionHandler : IApiExceptionHandler
	{
		private readonly PartialResponseExceptionRecorder _recorder;

		public PartialResponseExceptionHandler(PartialResponseExceptionRecorder recorder)
		{
			_recorder = recorder;
		}

		public bool TryHandle(Exception exception, ApiContext context, out IApiResult? result)
		{
			context.Response.ResponseCode = 299;
			context.Response.ResponseBody = "partial response";
			context.Response.ResponseBodyBytes = new byte[] { 4, 5, 6 };
			_recorder.PartialResponse = context.Response;
			result = null;
			throw new InvalidOperationException("exception handler failure");
		}
	}

	internal sealed class RecordingExceptionHandler : IApiExceptionHandler
	{
		private readonly ExceptionRecorder _recorder;

		public RecordingExceptionHandler(ExceptionRecorder recorder)
		{
			_recorder = recorder;
		}

		public bool TryHandle(Exception exception, ApiContext context, out IApiResult? result)
		{
			_recorder.Exception = exception;
			_recorder.Context = context;
			result = new ObjectResult<string>(418, "handled");
			return true;
		}
	}
}
