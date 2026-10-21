namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	using Microsoft.Extensions.Logging;

	using Skyline.DataMiner.Automation;

	internal sealed class ExceptionHandlingMiddleware : IApiMiddleware
	{
		private readonly IApiExceptionHandler _exceptionHandler;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;

		public ExceptionHandlingMiddleware(
			IApiExceptionHandler exceptionHandler,
			ILogger<ExceptionHandlingMiddleware> logger)
		{
			_exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
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

				try
				{
					var result = _exceptionHandler.Handle(exception, context)
						?? throw new InvalidOperationException("The API exception handler returned a null result.");

					result.ExecuteResult(context);
				}
				catch (Exception handlerException) when (IsControlFlowException(handlerException))
				{
					throw;
				}
				catch (Exception handlerException)
				{
					_logger.LogError("The API exception handler failed: " + handlerException);
					context.Response.ResponseBody = null;
					new StatusCodeResult(500).ExecuteResult(context);
				}
			}
		}

		private static bool IsControlFlowException(Exception exception)
		{
			return exception is ScriptAbortException
				|| exception is ScriptForceAbortException
				|| exception is ScriptTimeoutException;
		}
	}
}
