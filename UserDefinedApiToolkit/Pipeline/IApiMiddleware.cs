namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	/// <summary>
	/// Represents a component in the API request middleware pipeline.
	/// </summary>
	public interface IApiMiddleware
	{
		/// <summary>
		/// Invokes this middleware for the current request.
		/// </summary>
		/// <param name="context">The context for the current API request.</param>
		/// <param name="next">The next component in the middleware pipeline.</param>
		void Invoke(ApiContext context, ApiRequestDelegate next);
	}
}
