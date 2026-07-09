namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// An <see cref="IInputConverter"/>/<see cref="IOutputConverter"/> that passes request bodies
	/// and action results through as plain <c>text/plain</c> strings, without any serialization.
	/// Register it (e.g. via <see cref="UserDefinedApiBuilder.AddInputConverter"/>) when an action
	/// needs to accept or return raw string content instead of JSON.
	/// </summary>
	public class PlainTextConverter : IInputConverter, IOutputConverter
	{
		/// <inheritdoc/>
		public string InputMediaType => "text/plain";

		/// <inheritdoc/>
		public string OutputMediaType => "text/plain";

		/// <inheritdoc/>
		public bool CanConvertInput(Type type) => true;

		/// <inheritdoc/>
		public bool CanConvertOutput(Type type) => true;

		/// <inheritdoc/>
		public object? ConvertInput(string input, Type type)
		{
			return input;
		}

		/// <inheritdoc/>
		public string ConvertOutput(object? output, Type type)
		{
			return Convert.ToString(output);
		}
	}
}
