namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// Converts a raw HTTP string value (e.g. a query string or route segment value) into a CLR
	/// <see cref="Type"/>.
	/// </summary>
	internal static class StringValueConverter
	{
		/// <summary>
		/// Determines whether a single raw string can be converted to <paramref name="targetType"/>,
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
		/// Attempts to determine the element type of a collection type (array or generic IEnumerable&lt;T&gt;).
		/// </summary>
		/// <param name="targetType">The type to inspect.</param>
		/// <param name="elementType">The element type if the target type is a collection; otherwise <c>null</c>.</param>
		/// <returns><c>true</c> if the target type is a collection and the element type was determined; otherwise <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType"/> is <c>null</c>.</exception>
		public static bool TryGetCollectionElementType(Type targetType, out Type elementType)
		{
			if (targetType is null)
			{
				throw new ArgumentNullException(nameof(targetType));
			}

			if (targetType == typeof(string))
			{
				elementType = null!;
				return false;
			}

			if (targetType.IsArray)
			{
				elementType = targetType.GetElementType()!;
				return true;
			}

			var enumerableType =
				targetType.IsGenericType &&
				targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
					? targetType
					: targetType
						.GetInterfaces()
						.FirstOrDefault(type =>
							type.IsGenericType &&
							type.GetGenericTypeDefinition() == typeof(IEnumerable<>));

			if (enumerableType is null)
			{
				elementType = null!;
				return false;
			}

			elementType = enumerableType.GetGenericArguments()[0];
			return true;
		}

		/// <summary>
		/// Attempts to convert <paramref name="rawValues"/> to <paramref name="targetType"/>.
		/// </summary>
		/// <param name="rawValues">The raw string values to convert.</param>
		/// <param name="targetType">The type to convert <paramref name="rawValues"/> to.</param>
		/// <param name="value">The converted value when this method returns <c>true</c>; otherwise <c>null</c>.</param>
		/// <returns><c>true</c> if the conversion succeeded; otherwise <c>false</c>.</returns>
		public static bool TryConvert(IReadOnlyList<string> rawValues, Type targetType, out object? value)
		{
			if (targetType is null)
			{
				throw new ArgumentNullException(nameof(targetType));
			}

			if (TryGetCollectionElementType(targetType, out var elementType))
			{
				return TryConvertCollection(rawValues, targetType, out value);
			}

			return TryConvert(rawValues.FirstOrDefault(), targetType, out value);
		}

		/// <summary>
		/// Attempts to convert a single raw string value to <paramref name="targetType"/>.
		/// </summary>
		/// <param name="rawValue">The raw string value to convert.</param>
		/// <param name="targetType">The type to convert <paramref name="rawValue"/> to.</param>
		/// <param name="value">The converted value when this method returns <c>true</c>; otherwise <c>null</c>.</param>
		/// <returns><c>true</c> if the conversion succeeded; otherwise <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType"/> is <c>null</c>.</exception>
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

		/// <summary>
		/// Attempts to convert a collection of raw string values to <paramref name="targetType"/>.
		/// </summary>
		/// <param name="rawValues">The raw string values to convert.</param>
		/// <param name="targetType">The type to convert the raw values to.</param>
		/// <param name="value">The converted collection when this method returns <c>true</c>; otherwise <c>null</c>.</param>
		/// <returns><c>true</c> if the conversion succeeded; otherwise <c>false</c>.</returns>
		private static bool TryConvertCollection(
			IReadOnlyList<string> rawValues,
			Type targetType,
			out object? value)
		{
			// Determine the element type of the target collection type.
			if (!TryGetCollectionElementType(
				targetType,
				out var elementType))
			{
				value = null;
				return false;
			}

			// Supports arrays, such as string[] or int[].
			if (targetType.IsArray)
			{
				var array = Array.CreateInstance(
					elementType,
					rawValues.Count);

				for (var i = 0; i < rawValues.Count; i++)
				{
					if (!TryConvert(
						rawValues[i],
						elementType,
						out var convertedValue))
					{
						value = null;
						return false;
					}

					array.SetValue(convertedValue, i);
				}

				value = array;
				return true;
			}

			// Supports List<T> and all of it's sub interfaces (e.g. IList<T>, ICollection<T>, IEnumerable<T>, IReadOnlyList<T>).
			var listType = typeof(List<>).MakeGenericType(elementType);
			if (targetType.IsAssignableFrom(listType))
			{
				return TryCreateAndPopulateCollection(
					listType,
					elementType,
					rawValues,
					out value);
			}

			// Supports ISet<T> and HashSet<T>.
			var setType = typeof(HashSet<>).MakeGenericType(elementType);
			if (targetType.IsAssignableFrom(setType))
			{
				return TryCreateAndPopulateCollection(
					setType,
					elementType,
					rawValues,
					out value);
			}

			// Supports concrete ICollection<T> implementations, including types with an explicitly implemented Add method.
			if (!targetType.IsAbstract &&
				!targetType.IsInterface)
			{
				var collectionInterface = typeof(ICollection<>)
					.MakeGenericType(elementType);

				if (collectionInterface.IsAssignableFrom(targetType))
				{
					return TryCreateAndPopulateCollection(
						targetType,
						elementType,
						rawValues,
						out value);
				}
			}

			value = null;
			return false;
		}

		/// <summary>
		/// Attempts to create an instance of a collection type and populate it with converted values.
		/// </summary>
		/// <param name="collectionType">The type of the collection to create.</param>
		/// <param name="elementType">The type of the elements in the collection.</param>
		/// <param name="rawValues">The raw string values to convert and add to the collection.</param>
		/// <param name="value">The created and populated collection when this method returns <c>true</c>; otherwise <c>null</c>.</param>
		/// <returns><c>true</c> if the collection was successfully created and populated; otherwise <c>false</c>.</returns>
		private static bool TryCreateAndPopulateCollection(
			Type collectionType,
			Type elementType,
			IReadOnlyList<string> rawValues,
			out object? value)
		{
			try
			{
				var collection = Activator.CreateInstance(collectionType);

				if (collection is null)
				{
					value = null;
					return false;
				}

				var collectionInterface = typeof(ICollection<>)
					.MakeGenericType(elementType);

				var addMethod = collectionInterface.GetMethod("Add");

				if (addMethod is null)
				{
					value = null;
					return false;
				}

				foreach (var rawValue in rawValues)
				{
					if (!TryConvert(
						rawValue,
						elementType,
						out var convertedValue))
					{
						value = null;
						return false;
					}

					addMethod.Invoke(
						collection,
						new[] { convertedValue });
				}

				value = collection;
				return true;
			}
			catch (ArgumentException)
			{
				value = null;
				return false;
			}
			catch (MissingMethodException)
			{
				value = null;
				return false;
			}
			catch (TargetInvocationException)
			{
				value = null;
				return false;
			}
		}
	}
}
