namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	/// <summary>
	/// Represents the result of an action method, responsible for writing the final response to
	/// the <see cref="ApiContext"/>. Returned by controller actions, typically constructed via the
	/// helper methods on <see cref="ControllerBase"/> (e.g. <c>Ok()</c>, <c>NotFound()</c>).
	/// </summary>
	public interface IApiResult
	{
		/// <summary>
		/// Writes this result's response (status code, and body if applicable) to the given
		/// <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The context for the current request.</param>
		void ExecuteResult(ApiContext context);
	}
}
