namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Attempts to convert an exception raised while processing an API request into an API result.
	/// </summary>
	public interface IApiExceptionHandler
	{
		/// <summary>
		/// Attempts to create a response for the specified exception.
		/// </summary>
		/// <param name="exception">The exception raised while processing the request.</param>
		/// <param name="context">The context for the current API request.</param>
		/// <param name="result">
		/// The result to execute when the exception is handled; otherwise <c>null</c>.
		/// </param>
		/// <returns>
		/// <c>true</c> when the exception was handled; otherwise <c>false</c>.
		/// </returns>
		bool TryHandle(Exception exception, ApiContext context, out IApiResult? result);
	}
}
