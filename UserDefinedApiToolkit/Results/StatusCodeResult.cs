namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// An <see cref="IApiResult"/> that only sets the HTTP status code on the response, without a
	/// body. Typically created via helper methods on <see cref="ControllerBase"/> such
	/// as <c>NotFound()</c> or <c>NoContent()</c>.
	/// </summary>
	public class StatusCodeResult : IApiResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StatusCodeResult"/> class.
		/// </summary>
		/// <param name="statusCode">The HTTP status code to write to the response.</param>
		public StatusCodeResult(int statusCode)
		{
			StatusCode = statusCode;
		}

		/// <summary>
		/// Gets the HTTP status code that will be written to the response.
		/// </summary>
		public int StatusCode { get; }

		/// <inheritdoc/>
		public virtual void ExecuteResult(ApiContext context)
		{
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			context.Response.ResponseCode = StatusCode;
		}
	}
}
