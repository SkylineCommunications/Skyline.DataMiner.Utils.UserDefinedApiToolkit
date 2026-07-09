namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Converts an action result value into the raw string body written to the response. Register
	/// custom implementations via <see cref="UserDefinedApiBuilder.AddOutputConverter"/> or
	/// <see cref="UserDefinedApiBuilder.WithDefaultOutputConverter"/> to support additional media
	/// types or custom (de)serialization logic.
	/// </summary>
	public interface IOutputConverter
	{
		/// <summary>
		/// Gets the media type (e.g. <c>"application/json"</c>) this converter produces as output.
		/// </summary>
		string OutputMediaType { get; }

		/// <summary>
		/// Determines whether this converter can serialize a value of the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The value's type.</param>
		/// <returns><c>true</c> if this converter can handle <paramref name="type"/>; otherwise, <c>false</c>.</returns>
		bool CanConvertOutput(Type type);

		/// <summary>
		/// Serializes <paramref name="output"/> into a string to be written as the response body.
		/// </summary>
		/// <param name="output">The value to serialize.</param>
		/// <param name="type">The value's type.</param>
		/// <returns>The serialized string representation of <paramref name="output"/>.</returns>
		string ConvertOutput(object? output, Type type);
	}
}
