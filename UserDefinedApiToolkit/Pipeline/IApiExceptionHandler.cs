namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Converts an exception raised while processing an API request into an API result.
	/// </summary>
	public interface IApiExceptionHandler
	{
		/// <summary>
		/// Creates the response for the specified exception.
		/// </summary>
		/// <param name="exception">The exception raised while processing the request.</param>
		/// <param name="context">The context for the current API request.</param>
		/// <returns>
		/// A non-null result that will be executed to create the API response.
		/// </returns>
		IApiResult Handle(Exception exception, ApiContext context);
	}
}
