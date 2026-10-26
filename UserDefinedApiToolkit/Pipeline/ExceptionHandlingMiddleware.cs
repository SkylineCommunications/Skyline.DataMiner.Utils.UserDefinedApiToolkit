namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;

	using Microsoft.Extensions.Logging;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;

	internal sealed class ExceptionHandlingMiddleware : IApiMiddleware
	{
		private readonly IServiceProvider _services;
		private readonly IReadOnlyList<Func<IServiceProvider, IApiExceptionHandler>> _handlerFactories;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;

		public ExceptionHandlingMiddleware(
			IServiceProvider services,
			IReadOnlyList<Func<IServiceProvider, IApiExceptionHandler>> handlerFactories,
			ILogger<ExceptionHandlingMiddleware> logger)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_handlerFactories = handlerFactories ?? throw new ArgumentNullException(nameof(handlerFactories));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public void Invoke(ApiContext context, ApiRequestDelegate next)
		{
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (next is null)
			{
				throw new ArgumentNullException(nameof(next));
			}

			var headers = context.Response.ResponseHeaders.AsDictionary();
			try
			{
				next(context);
			}
			catch (Exception exception) when (IsControlFlowException(exception))
			{
				throw;
			}
			catch (Exception exception)
			{
				_logger.LogError("Unhandled API exception: " + exception);
				ResetOutput(context, headers);

				try
				{
					foreach (var handlerFactory in _handlerFactories)
					{
						var handler = handlerFactory(_services)
							?? throw new InvalidOperationException("The API exception handler factory returned null.");

						if (!handler.TryHandle(exception, context, out var result))
						{
							continue;
						}

						if (result is null)
						{
							throw new InvalidOperationException("The API exception handler reported the exception as handled without returning a result.");
						}

						result.ExecuteResult(context);
						return;
					}

					new StatusCodeResult(500).ExecuteResult(context);
				}
				catch (Exception handlerException) when (IsControlFlowException(handlerException))
				{
					throw;
				}
				catch (Exception handlerException)
				{
					_logger.LogError("The API exception handler failed: " + handlerException);
					ResetOutput(context, headers);
					new StatusCodeResult(500).ExecuteResult(context);
				}
			}
		}

		private static void ResetOutput(ApiContext context, Dictionary<string, List<string>> headers)
		{
			var newHeaders = new Headers();
			foreach (var header in headers)
			{
				newHeaders.Set(header.Key, header.Value);
			}

			var response = new ApiTriggerOutput();
			context.Response.ResponseBody = response.ResponseBody;
			context.Response.ResponseBodyBytes = response.ResponseBodyBytes;
			context.Response.ResponseCode = response.ResponseCode;
			context.Response.ResponseHeaders = newHeaders;
		}

		private static bool IsControlFlowException(Exception exception)
		{
			return exception is ScriptAbortException
				|| exception is ScriptForceAbortException
				|| exception is ScriptTimeoutException;
		}
	}
}
