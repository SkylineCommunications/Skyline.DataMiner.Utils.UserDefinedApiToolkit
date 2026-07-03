namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;

	public class ApiContext
	{
		public ApiTriggerInput Request { get; internal set; } = new ApiTriggerInput();

		public ApiTriggerOutput Response { get; internal set; } = new ApiTriggerOutput();

		public IInputConverter DefaultInputConverter { get => InputConverters[0]; }

		public IOutputConverter DefaultOutputConverter { get => OutputConverters[0]; }

		public IReadOnlyList<IInputConverter> InputConverters { get; internal set; }
			= new ReadOnlyCollection<IInputConverter>(
				new List<IInputConverter>
				{
					// TODO: NewtonsoftConverter
				});

		public IReadOnlyList<IOutputConverter> OutputConverters { get; internal set; }
			= new ReadOnlyCollection<IOutputConverter>(
				new List<IOutputConverter>
				{
					// TODO: NewtonsoftConverter
				});
	}
}
