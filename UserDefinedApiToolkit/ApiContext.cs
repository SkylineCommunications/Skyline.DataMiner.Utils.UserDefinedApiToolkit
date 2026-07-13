namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;

	/// <summary>
	/// Holds the request/response and converter state for a single <see cref="UserDefinedApi.Run"/>
	/// call. An instance is created for each incoming request and is accessible from within a
	/// controller via <see cref="ControllerBase.ApiContext"/>.
	/// </summary>
	public class ApiContext
	{
		/// <summary>
		/// Gets the raw request received from DataMiner for the current API call.
		/// </summary>
		public ApiTriggerInput Request { get; internal set; } = new ApiTriggerInput();

		/// <summary>
		/// Gets the response that will be sent back to DataMiner. Action results write to this
		/// object via <see cref="IApiResult.ExecuteResult"/>.
		/// </summary>
		public ApiTriggerOutput Response { get; internal set; } = new ApiTriggerOutput();

		/// <summary>
		/// Gets the input converter used to deserialize <c>[FromBody]</c> parameters when no other
		/// registered converter (see <see cref="InputConverters"/>) can handle the parameter's type.
		/// </summary>
		public IInputConverter DefaultInputConverter { get => InputConverters[0]; }

		/// <summary>
		/// Gets the output converter used to serialize action results when no other registered
		/// converter (see <see cref="OutputConverters"/>) can handle the result's type.
		/// </summary>
		public IOutputConverter DefaultOutputConverter { get => OutputConverters[0]; }

		/// <summary>
		/// Gets the input converters registered on the <see cref="UserDefinedApiBuilder"/>, in the
		/// order they should be tried (index 0 is the default converter, tried last).
		/// </summary>
		public IReadOnlyList<IInputConverter> InputConverters { get; internal set; }
			= new ReadOnlyCollection<IInputConverter>(
				new List<IInputConverter>
				{
					// TODO: NewtonsoftConverter
				});

		/// <summary>
		/// Gets the output converters registered on the <see cref="UserDefinedApiBuilder"/>, in the
		/// order they should be tried (index 0 is the default converter, tried last).
		/// </summary>
		public IReadOnlyList<IOutputConverter> OutputConverters { get; internal set; }
			= new ReadOnlyCollection<IOutputConverter>(
				new List<IOutputConverter>
				{
					// TODO: NewtonsoftConverter
				});
	}
}
