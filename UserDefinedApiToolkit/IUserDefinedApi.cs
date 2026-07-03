namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;

	/// <summary>
	/// Represents a user-defined API that can route requests to registered controllers.
	/// </summary>
	public interface IUserDefinedApi
	{
		/// <summary>
		/// Executes the API for the specified trigger input and returns the output.
		/// </summary>
		/// <param name="engine">The DataMiner automation engine instance.</param>
		/// <param name="apiTriggerInput">The API trigger input containing request data.</param>
		/// <returns>
		/// An <see cref="ApiTriggerOutput"/> containing the result of the API execution.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when no matching route handler is found or the API action returns a null result.
		/// </exception>
		ApiTriggerOutput Run(IEngine engine, ApiTriggerInput apiTriggerInput);
	}
}
