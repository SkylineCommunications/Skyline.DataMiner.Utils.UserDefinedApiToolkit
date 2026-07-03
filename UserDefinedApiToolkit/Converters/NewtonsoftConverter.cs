namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public class NewtonsoftConverter : IInputConverter, IOutputConverter
	{
		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			Converters = { new StringEnumConverter() },
		};

		public string InputMediaType => "application/json";

		public string OutputMediaType => "application/json";

		public bool CanConvertInput(Type type) => true;

		public bool CanConvertOutput(Type type) => true;

		public object? ConvertInput(string input, Type type)
		{
#pragma warning disable SLC_SC0004 // Avoid deserializing json strings by using Newtonsoft directly.
			return JsonConvert.DeserializeObject(input, type, Settings);
#pragma warning restore SLC_SC0004 // Avoid deserializing json strings by using Newtonsoft directly.
		}

		public string ConvertOutput(object? output, Type type)
		{
			return JsonConvert.SerializeObject(output, type, Settings);
		}
	}
}
