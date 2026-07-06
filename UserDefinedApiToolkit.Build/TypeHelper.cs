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
		public static bool HasDomStorageAttribute(Type type)
		{
			return type.GetCustomAttributesData()
				.Any(a => TryGetAttributeTypeName(a) == SdmDomStorageAttribute);
		}

		/// <summary>
		/// Returns true if the type is SdmObjectReference&lt;T&gt;.
		/// </summary>
		public static bool IsSdmObjectReference(Type type)
		{
			return type.IsGenericType &&
				   type.GetGenericTypeDefinition().Name == SdmObjectReference1;
		}

		/// <summary>
		/// Returns true if the member has an attribute matching the given name
		/// (with or without the "Attribute" suffix).
		/// </summary>
		public static bool HasAttribute(MemberInfo member, string attributeName)
		{
			return member.GetCustomAttributesData()
				.Any(a => Matches(TryGetAttributeTypeName(a), attributeName));
		}

		/// <summary>
		/// Returns true if the parameter has an attribute matching the given name
		/// (with or without the "Attribute" suffix).
		/// </summary>
		public static bool HasAttribute(ParameterInfo parameter, string attributeName)
		{
			return parameter.GetCustomAttributesData()
				.Any(a => Matches(TryGetAttributeTypeName(a), attributeName));
		}

		private static bool Matches(string? actualAttributeName, string expectedAttributeName)
		{
			return actualAttributeName == expectedAttributeName
				|| actualAttributeName == expectedAttributeName + "Attribute";
		}

		/// <summary>
		/// Reads <see cref="CustomAttributeData.AttributeType"/>.Name defensively.
		/// </summary>
		/// <remarks>
		/// On some .NET Framework/Mono test hosts, resolving the declaring type of a custom
		/// attribute can throw a <see cref="NullReferenceException"/> when the assembly
		/// defining that (unrelated) attribute couldn't be fully resolved by the reflection
		/// context - even though the attribute we're actually looking for is unaffected.
		/// Since callers only care about matching one specific attribute name, an attribute
		/// whose type can't be resolved simply can't be a match and is skipped instead of
		/// failing the whole lookup.
		/// </remarks>
		/// <summary>
		/// Reads the custom attribute's declaring type name defensively.
		/// </summary>
		/// <remarks>
		/// This intentionally uses <see cref="CustomAttributeData.Constructor"/>.DeclaringType
		/// instead of <see cref="CustomAttributeData.AttributeType"/>. On some .NET
		/// Framework/Mono test hosts, the <c>AttributeType</c> getter of the
		/// <c>System.Reflection.MetadataLoadContext</c> package throws a
		/// <see cref="NullReferenceException"/> for every custom attribute (a runtime-specific
		/// bug, confirmed to affect unrelated compiler-generated attributes as well as our own),
		/// while <c>Constructor.DeclaringType</c> resolves correctly in the same environment.
		/// The try/catch is kept as a last-resort safety net in case that path also fails for a
		/// given attribute - a lookup miss then simply means the attribute isn't a match.
		/// </remarks>
		private static string? TryGetAttributeTypeName(CustomAttributeData attributeData)
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
