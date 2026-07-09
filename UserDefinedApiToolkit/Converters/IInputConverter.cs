namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Converts the raw string body of a request into a strongly-typed <c>[FromBody]</c> parameter
	/// value. Register custom implementations via
	/// <see cref="UserDefinedApiBuilder.AddInputConverter"/> or
	/// <see cref="UserDefinedApiBuilder.WithDefaultInputConverter"/> to support additional media
	/// types or custom (de)serialization logic.
	/// </summary>
	public interface IInputConverter
	{
		/// <summary>
		/// Gets the media type (e.g. <c>"application/json"</c>) this converter expects as input.
		/// </summary>
		string InputMediaType { get; }

		/// <summary>
		/// Determines whether this converter can deserialize input into the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The target type.</param>
		/// <returns><c>true</c> if this converter can handle <paramref name="type"/>; otherwise, <c>false</c>.</returns>
		bool CanConvertInput(Type type);

		/// <summary>
		/// Deserializes the raw <paramref name="input"/> string into an instance of <paramref name="type"/>.
		/// </summary>
		/// <param name="input">The raw request body.</param>
		/// <param name="type">The target type.</param>
		/// <returns>The deserialized value, or <c>null</c>.</returns>
		object? ConvertInput(string input, Type type);
	}
}
