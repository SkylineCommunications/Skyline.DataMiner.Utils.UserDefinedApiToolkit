namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	public class StringConverter : IInputConverter, IOutputConverter
	{
		public string InputMediaType => "text/plain";

		public string OutputMediaType => "text/plain";

		public bool CanConvertInput(Type type) => true;

		public bool CanConvertOutput(Type type) => true;

		public object? ConvertInput(string input, Type type)
		{
			return input;
		}

		public string ConvertOutput(object? output, Type type)
		{
			return Convert.ToString(output);
		}
	}
}
