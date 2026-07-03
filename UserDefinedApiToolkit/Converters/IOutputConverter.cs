namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	public interface IOutputConverter
	{
		string OutputMediaType { get; }

		bool CanConvertOutput(Type type);

		string ConvertOutput(object? output, Type type);
	}
}
