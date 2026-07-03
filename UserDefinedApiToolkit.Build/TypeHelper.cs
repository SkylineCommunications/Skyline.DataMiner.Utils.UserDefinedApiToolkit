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
		public static (Type? SuccessType, Type? ErrorType) GetResultTypes(Type returnType)
		{
			if (!returnType.IsGenericType)
			{
				return (null, null);
			}

			var genericName = returnType.GetGenericTypeDefinition().Name;
			var args = returnType.GetGenericArguments();

			if (genericName == ApiResult1)
			{
				return (args[0], null);
			}

			if (genericName == ApiResult2)
			{
				return (args[0], args[1]);
			}

			return (null, null);
		}

		/// <summary>
		/// Returns true if the type has the SdmDomStorageAttribute applied.
		/// </summary>
		public static bool HasDomStorageAttribute(Type type)
		{
			return type.GetCustomAttributesData()
				.Any(a => a.AttributeType.Name == SdmDomStorageAttribute);
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
				.Any(a => a.AttributeType.Name == attributeName
					   || a.AttributeType.Name == attributeName + "Attribute");
		}

		/// <summary>
		/// Returns true if the parameter has an attribute matching the given name
		/// (with or without the "Attribute" suffix).
		/// </summary>
		public static bool HasAttribute(ParameterInfo parameter, string attributeName)
		{
			return parameter.GetCustomAttributesData()
				.Any(a => a.AttributeType.Name == attributeName
					   || a.AttributeType.Name == attributeName + "Attribute");
		}
	}
}
