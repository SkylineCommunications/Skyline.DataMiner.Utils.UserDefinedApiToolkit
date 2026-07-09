namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;

	internal static class TypeHelper
	{
		private const string ApiResult1 = "ApiResult`1";
		private const string ApiResult2 = "ApiResult`2";
		private const string IEnumerable1 = "IEnumerable`1";
		private const string SdmDomStorageAttribute = "SdmDomStorageAttribute";
		private const string SdmObjectReference1 = "SdmObjectReference`1";

		/// <summary>
		/// Gets the element type of an array or IEnumerable&lt;T&gt;.
		/// Returns the type itself if it is not a collection.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <returns>The element type, or <paramref name="type"/> itself if it is not a collection.</returns>
		public static Type GetElementType(Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType()!;
			}

			if (type.IsGenericType && type.GetGenericTypeDefinition().Name == IEnumerable1)
			{
				return type.GetGenericArguments()[0];
			}

			var ienumerable = type.GetInterfaces()
				.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == IEnumerable1);

			return ienumerable?.GetGenericArguments()[0] ?? type;
		}

		/// <summary>
		/// Reads the success and error types from ApiResult&lt;TSuccess&gt; or
		/// ApiResult&lt;TSuccess, TError&gt;. Returns (null, null) for any other return type.
		/// </summary>
		/// <remarks>
		/// Returns a dedicated <see cref="ResultTypes"/> struct rather than a
		/// <see cref="ValueTuple{T1, T2}"/>. Some .NET Framework/Mono test hosts resolve
		/// <c>System.ValueTuple</c> inconsistently between the built-in mscorlib type and the
		/// <c>System.ValueTuple</c> NuGet package pulled in transitively, which can cause a
		/// <see cref="MissingMethodException"/> at call time even though the method clearly
		/// exists. A locally defined type has no such ambiguity.
		/// </remarks>
		/// <param name="returnType">The action method's declared return type.</param>
		/// <returns>The extracted success/error types, or <c>(null, null)</c> if <paramref name="returnType"/> is not <c>ApiResult&lt;TSuccess&gt;</c>/<c>ApiResult&lt;TSuccess, TError&gt;</c>.</returns>
		public static ResultTypes GetResultTypes(Type returnType)
		{
			if (!returnType.IsGenericType)
			{
				return new ResultTypes(null, null);
			}

			var genericName = returnType.GetGenericTypeDefinition().Name;
			var args = returnType.GetGenericArguments();

			if (genericName == ApiResult1)
			{
				return new ResultTypes(args[0], null);
			}

			if (genericName == ApiResult2)
			{
				return new ResultTypes(args[0], args[1]);
			}

			return new ResultTypes(null, null);
		}

		/// <summary>
		/// Returns true if the type has the SdmDomStorageAttribute applied.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <returns><c>true</c> if the type has the <c>SdmDomStorageAttribute</c> applied; otherwise, <c>false</c>.</returns>
		public static bool HasDomStorageAttribute(Type type)
		{
			return type.GetCustomAttributesData()
				.Any(a => GetAttributeName(a) == SdmDomStorageAttribute);
		}

		/// <summary>
		/// Returns true if the type is SdmObjectReference&lt;T&gt;.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <returns><c>true</c> if the type is <c>SdmObjectReference&lt;T&gt;</c>; otherwise, <c>false</c>.</returns>
		public static bool IsSdmObjectReference(Type type)
		{
			return type.IsGenericType &&
				   type.GetGenericTypeDefinition().Name == SdmObjectReference1;
		}

		/// <summary>
		/// Returns true if the member has an attribute matching the given name
		/// (with or without the "Attribute" suffix).
		/// </summary>
		/// <param name="member">The member to inspect.</param>
		/// <param name="attributeName">The attribute name to match, with or without the "Attribute" suffix.</param>
		/// <returns><c>true</c> if a matching attribute is found; otherwise, <c>false</c>.</returns>
		public static bool HasAttribute(MemberInfo member, string attributeName)
		{
			return member.GetCustomAttributesData()
				.Any(a => Matches(GetAttributeName(a), attributeName));
		}

		/// <summary>
		/// Returns true if the parameter has an attribute matching the given name
		/// (with or without the "Attribute" suffix).
		/// </summary>
		/// <param name="parameter">The parameter to inspect.</param>
		/// <param name="attributeName">The attribute name to match, with or without the "Attribute" suffix.</param>
		/// <returns><c>true</c> if a matching attribute is found; otherwise, <c>false</c>.</returns>
		public static bool HasAttribute(ParameterInfo parameter, string attributeName)
		{
			return parameter.GetCustomAttributesData()
				.Any(a => Matches(GetAttributeName(a), attributeName));
		}

		private static bool Matches(string? actualAttributeName, string expectedAttributeName)
		{
			return actualAttributeName == expectedAttributeName
				|| actualAttributeName == expectedAttributeName + "Attribute";
		}

		/// <summary>
		/// Reads the custom attribute's declaring type name defensively.
		/// </summary>
		/// <remarks>
		/// This intentionally uses <see cref="CustomAttributeData.Constructor"/>.DeclaringType
		/// instead of <see cref="CustomAttributeData.AttributeType"/>. On some .NET
		/// Framework/Mono test hosts, the <c>AttributeType</c> getter of the
		/// <c>System.Reflection.MetadataLoadContext</c> package throws a
		/// <see cref="NullReferenceException"/> for every custom attribute,
		/// while <c>Constructor.DeclaringType</c> resolves correctly in the same environment.
		/// The try/catch is kept as a last-resort safety net in case that path also fails for a
		/// given attribute - a lookup miss then simply means the attribute isn't a match.
		/// </remarks>
		/// <param name="attributeData">The parsed attribute data to inspect.</param>
		/// <returns>The attribute's declaring type name, or <c>null</c> if it could not be determined.</returns>
		public static string? GetAttributeName(CustomAttributeData attributeData)
		{
			try
			{
				return attributeData.Constructor.DeclaringType?.Name;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Reads the value of a named argument (e.g. <c>Name</c> on <c>[FromRoute(Name = "id")]</c>)
		/// from a parsed attribute, or <c>null</c> if not present.
		/// </summary>
		/// <param name="attributeData">The parsed attribute data to inspect.</param>
		/// <param name="argumentName">The name of the named argument to read.</param>
		/// <returns>The string value of the named argument, or <c>null</c> if not present.</returns>
		public static string? GetNamedArgumentValue(CustomAttributeData attributeData, string argumentName)
		{
			var namedArguments = attributeData.NamedArguments;
			if (namedArguments is null)
			{
				return null;
			}

			foreach (var namedArgument in namedArguments)
			{
				if (namedArgument.MemberName == argumentName)
				{
					return namedArgument.TypedValue.Value as string;
				}
			}

			return null;
		}

		/// <summary>
		/// Extracts the <c>{placeholder}</c> names from a combined route template string
		/// (e.g. <c>"v1/items/{id}"</c> → <c>["id"]</c>).
		/// </summary>
		/// <param name="routeTemplate">The combined route template to extract placeholders from.</param>
		/// <returns>The placeholder names found in <paramref name="routeTemplate"/>, in order.</returns>
		public static IReadOnlyCollection<string> GetRoutePlaceholders(string? routeTemplate)
		{
			if (String.IsNullOrEmpty(routeTemplate))
			{
				return Array.Empty<string>();
			}

			return routeTemplate!
				.Trim('/')
				.Split('/')
				.Where(segment => segment.Length > 2 && segment[0] == '{' && segment[segment.Length - 1] == '}')
				.Select(segment => segment.Substring(1, segment.Length - 2))
				.ToList();
		}
	}

	/// <summary>
	/// Holds the success and error types read from an ApiResult&lt;TSuccess&gt; or
	/// ApiResult&lt;TSuccess, TError&gt; return type. See <see cref="TypeHelper.GetResultTypes"/>
	/// for why this is a dedicated type instead of a <see cref="ValueTuple{T1, T2}"/>.
	/// </summary>
	internal readonly struct ResultTypes
	{
		public ResultTypes(Type? successType, Type? errorType)
		{
			SuccessType = successType;
			ErrorType = errorType;
		}

		public Type? SuccessType { get; }

		public Type? ErrorType { get; }

		public void Deconstruct(out Type? successType, out Type? errorType)
		{
			successType = SuccessType;
			errorType = ErrorType;
		}
	}
}
