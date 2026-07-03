namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	public interface IInputConverter
	{
		string InputMediaType { get; }

		bool CanConvertInput(Type type);

		object? ConvertInput(string input, Type type);
	}
}
