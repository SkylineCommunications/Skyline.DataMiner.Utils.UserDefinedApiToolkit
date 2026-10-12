namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Describes a raw value that could not be converted to the expected type.
	/// </summary>
	internal sealed class ConversionFailure
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConversionFailure"/> class.
		/// </summary>
		/// <param name="rawValue">The raw value that could not be converted.</param>
		/// <param name="targetType">The type the raw value was expected to convert to.</param>
		/// <param name="valueIndex">The zero-based index of the value in a collection, if applicable.</param>
		public ConversionFailure(string rawValue, Type targetType, int? valueIndex = null)
		{
			RawValue = rawValue ?? throw new ArgumentNullException(nameof(rawValue));
			TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
			ValueIndex = valueIndex;
		}

		/// <summary>
		/// Gets the raw value that could not be converted.
		/// </summary>
		public string RawValue { get; }

		/// <summary>
		/// Gets the type the raw value was expected to convert to.
		/// </summary>
		public Type TargetType { get; }

		/// <summary>
		/// Gets the zero-based index of the value in a collection, if applicable.
		/// </summary>
		public int? ValueIndex { get; }
	}
}
