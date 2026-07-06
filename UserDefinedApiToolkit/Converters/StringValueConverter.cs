namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.ComponentModel;
	using System.Globalization;

	/// <summary>
	/// Converts a raw HTTP string value (e.g. a query string or route segment value) into a CLR
	/// <see cref="Type"/>.
	/// </summary>
	internal static class StringValueConverter
	{
		/// <summary>
		/// Determines whether <paramref name="targetType"/> can be converted from a raw string value,
		/// without actually needing a value to convert. Useful for build-time/setup-time validation.
		/// </summary>
		/// <param name="targetType">The type to check.</param>
		/// <returns><c>true</c> if <paramref name="targetType"/> can be converted from a string; otherwise <c>false</c>.</returns>
		public static bool CanConvert(Type targetType)
		{
			if (targetType is null)
			{
				throw new ArgumentNullException(nameof(targetType));
			}

			if (targetType == typeof(string))
			{
				return true;
			}

			var converter = TypeDescriptor.GetConverter(targetType);
			return converter is not null && converter.CanConvertFrom(typeof(string));
		}

		/// <summary>
		/// Attempts to convert <paramref name="rawValue"/> to <paramref name="targetType"/>.
		/// </summary>
		/// <param name="rawValue">The raw string value to convert.</param>
		/// <param name="targetType">The type to convert <paramref name="rawValue"/> to.</param>
		/// <param name="value">The converted value when this method returns <c>true</c>; otherwise <c>null</c>.</param>
		/// <returns><c>true</c> if the conversion succeeded; otherwise <c>false</c>.</returns>
		public static bool TryConvert(string rawValue, Type targetType, out object? value)
		{
			if (targetType is null)
			{
				throw new ArgumentNullException(nameof(targetType));
			}

			if (targetType == typeof(string))
			{
				value = rawValue;
				return true;
			}

			try
			{
				// TypeDescriptor.GetConverter transparently handles Nullable<T> (unwrapping to the
				// underlying type and treating an empty string as null), enums, Guid, DateTime,
				// TimeSpan, and all the numeric/bool primitives - the same mechanism ASP.NET Core's
				// model binding relies on for simple-type query/route parameters.
				if (!CanConvert(targetType))
				{
					value = null;
					return false;
				}

				var converter = TypeDescriptor.GetConverter(targetType);
				value = converter.ConvertFromString(null, CultureInfo.InvariantCulture, rawValue);
				return true;
			}
			catch (Exception)
			{
				// TypeConverter implementations don't agree on a single exception type for invalid
				// input (e.g. Int32Converter wraps a FormatException in a plain Exception, while
				// others throw FormatException/ArgumentException/NotSupportedException directly),
				// so any failure here is treated as "could not convert".
				value = null;
				return false;
			}
		}
	}
}
