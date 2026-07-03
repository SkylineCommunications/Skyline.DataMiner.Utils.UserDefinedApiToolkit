namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;

	public abstract partial class ControllerBase
	{
		public ApiContext ApiContext { get; internal set; } = new ApiContext();

		public ApiTriggerInput Request => ApiContext.Request;

		public ApiTriggerOutput Response => ApiContext.Response;

		public virtual IInputConverter DefaultInputConverter => ApiContext.DefaultInputConverter;

		public virtual IOutputConverter DefaultOutputConverter => ApiContext.DefaultOutputConverter;
	}
}
