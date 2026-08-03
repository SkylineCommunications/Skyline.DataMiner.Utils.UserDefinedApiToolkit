namespace UserDefinedApiToolkit.Tests.Attributes
{
	using System;
	using System.Reflection;

	using FluentAssertions;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[TestClass]
	public sealed class HttpMethodAttributeConstructorTests
	{
		// Consumers who compiled controllers against an earlier version of this library (before
		// the "template" constructor parameter was introduced) reference the parameterless
		// constructor of these attributes directly in their IL. An optional parameter
		// ("template = \"\"") is resolved at the caller's compile time and does not create a
		// second, parameterless overload, so removing the explicit parameterless constructor is a
		// binary breaking change for those already-compiled assemblies.
		[TestMethod]
		[DataRow(typeof(HttpGetAttribute))]
		[DataRow(typeof(HttpPostAttribute))]
		[DataRow(typeof(HttpPutAttribute))]
		[DataRow(typeof(HttpDeleteAttribute))]
		[DataRow(typeof(HttpPatchAttribute))]
		public void HttpVerbAttribute_HasPublicParameterlessConstructor(Type attributeType)
		{
			var ctor = attributeType.GetConstructor(Type.EmptyTypes);

			ctor.Should().NotBeNull($"'{attributeType.Name}' must keep a public parameterless constructor for binary compatibility with already-compiled consumers.");
		}

		[TestMethod]
		public void HttpMethodAttribute_HasProtectedParameterlessConstructor()
		{
			var ctor = typeof(HttpMethodAttribute).GetConstructor(
				BindingFlags.Instance | BindingFlags.NonPublic,
				null,
				Type.EmptyTypes,
				null);

			ctor.Should().NotBeNull();
			ctor!.IsFamily.Should().BeTrue("'HttpMethodAttribute' must keep a protected parameterless constructor for binary compatibility with existing derivatives.");
		}
	}
}
