namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;

	/// <summary>
	/// Extension methods for retrieving toolkit values from an Automation engine.
	/// </summary>
	public static class EngineExtensions
	{
		/// <summary>
		/// Gets an <see cref="ApiTriggerInput"/> from a JSON script parameter identified by ID.
		/// </summary>
		/// <param name="engine">The Automation engine.</param>
		/// <param name="scriptParameterId">The ID of the script parameter containing the JSON.</param>
		/// <returns>The deserialized API trigger input.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is <c>null</c>.</exception>
		public static ApiTriggerInput GetApiTriggerInput(this IEngine engine, int scriptParameterId)
		{
			if (engine is null)
			{
				throw new ArgumentNullException(nameof(engine));
			}

			return ApiTriggerInputHelper.Deserialize(engine.GetScriptParam(scriptParameterId).Value);
		}

		/// <summary>
		/// Gets an <see cref="ApiTriggerInput"/> from a JSON script parameter identified by name.
		/// </summary>
		/// <param name="engine">The Automation engine.</param>
		/// <param name="scriptParameterName">The name of the script parameter containing the JSON.</param>
		/// <returns>The deserialized API trigger input.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="engine"/> or <paramref name="scriptParameterName"/> is <c>null</c>.
		/// </exception>
		public static ApiTriggerInput GetApiTriggerInput(this IEngine engine, string scriptParameterName)
		{
			if (engine is null)
			{
				throw new ArgumentNullException(nameof(engine));
			}

			if (scriptParameterName is null)
			{
				throw new ArgumentNullException(nameof(scriptParameterName));
			}

			return ApiTriggerInputHelper.Deserialize(engine.GetScriptParam(scriptParameterName).Value);
		}
	}
}
