namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;

	/// <summary>
	/// Base class for controllers. Inherit from this class and decorate the derived type with a
	/// <see cref="RouteAttribute"/> to register a controller via
	/// <see cref="UserDefinedApiBuilder.AddController(System.Type)"/>.
	/// </summary>
	public abstract partial class ControllerBase
	{
		/// <summary>
		/// Gets the <see cref="UserDefinedApiToolkit.ApiContext"/> for the current request. Set by
		/// the framework before the action method is invoked.
		/// </summary>
		public ApiContext ApiContext { get; internal set; } = new ApiContext();

		/// <summary>
		/// Gets the raw request received from DataMiner for the current API call.
		/// </summary>
		public ApiTriggerInput Request => ApiContext.Request;

		/// <summary>
		/// Gets the response that will be sent back to DataMiner.
		/// </summary>
		public ApiTriggerOutput Response => ApiContext.Response;

		/// <summary>
		/// Gets the input converter used to deserialize <c>[FromBody]</c> parameters when no other
		/// registered converter can handle the parameter's type.
		/// </summary>
		public virtual IInputConverter DefaultInputConverter => ApiContext.DefaultInputConverter;

		/// <summary>
		/// Gets the output converter used to serialize action results when no other registered
		/// converter can handle the result's type.
		/// </summary>
		public virtual IOutputConverter DefaultOutputConverter => ApiContext.DefaultOutputConverter;
	}
}
