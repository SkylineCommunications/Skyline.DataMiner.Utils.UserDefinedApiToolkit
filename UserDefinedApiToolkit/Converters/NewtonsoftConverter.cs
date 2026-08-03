namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	/// <summary>
	/// The default <see cref="IInputConverter"/>/<see cref="IOutputConverter"/>, converting request
	/// bodies and action results to/from <c>application/json</c> using Newtonsoft.Json. Registered
	/// automatically as the default converter unless overridden via
	/// <see cref="UserDefinedApiBuilder.WithDefaultInputConverter"/> /
	/// <see cref="UserDefinedApiBuilder.WithDefaultOutputConverter"/>.
	/// </summary>
	public class NewtonsoftConverter : IInputConverter, IOutputConverter
	{
		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			Converters = { new StringEnumConverter() },
		};

		/// <inheritdoc/>
		public string InputMediaType => "application/json";

		/// <inheritdoc/>
		public string OutputMediaType => "application/json";

		/// <inheritdoc/>
		public bool CanConvertInput(Type type) => true;

		/// <inheritdoc/>
		public bool CanConvertOutput(Type type) => true;

		/// <inheritdoc/>
		public object? ConvertInput(string input, Type type)
		{
#pragma warning disable SLC_SC0004 // Avoid deserializing json strings by using Newtonsoft directly.
			return JsonConvert.DeserializeObject(input, type, Settings);
#pragma warning restore SLC_SC0004 // Avoid deserializing json strings by using Newtonsoft directly.
		}

		/// <inheritdoc/>
		public string ConvertOutput(object? output, Type type)
		{
			return JsonConvert.SerializeObject(output, type, Settings);
		}
	}
}
