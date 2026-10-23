namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	/// <summary>
	/// Represents the next component in the API middleware pipeline.
	/// </summary>
	/// <param name="context">The context for the current API request.</param>
	public delegate void ApiRequestDelegate(ApiContext context);
}
