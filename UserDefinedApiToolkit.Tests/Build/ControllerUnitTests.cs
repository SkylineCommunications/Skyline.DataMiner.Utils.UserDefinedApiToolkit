namespace UserDefinedApiToolkit.Tests.Build
{
	using System.Xml.Linq;

	using FluentAssertions;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build;

	[TestClass]
	public sealed class ControllerUnitTests
	{
		[TestMethod]
		public void GetRoute_WithRouteAttribute_ReturnsTemplate()
		{
			var unit = new ControllerUnit(typeof(TestFiles.SampleController), null);

			unit.GetRoute().Should().Be("v1/sample");
		}

		[TestMethod]
		public void GetRoute_WithoutRouteAttribute_ReturnsRootPath()
		{
			var unit = new ControllerUnit(typeof(object), null);

			unit.GetRoute().Should().Be("/");
		}

		[TestMethod]
		public void GetRoute_WithMethodTemplate_CombinesControllerAndMethodTemplate()
		{
			var unit = new ControllerUnit(typeof(TestFiles.PathVariableController), null);
			var method = typeof(TestFiles.PathVariableController).GetMethod(nameof(TestFiles.PathVariableController.GetById));

			unit.GetRoute(method!).Should().Be("v1/items/{id}");
		}

		[TestMethod]
		public void GetRoute_WithoutMethodTemplate_ReturnsControllerRouteOnly()
		{
			var unit = new ControllerUnit(typeof(TestFiles.PathVariableController), null);
			var method = typeof(TestFiles.PathVariableController).GetMethod(nameof(TestFiles.PathVariableController.GetAll));

			unit.GetRoute(method!).Should().Be("v1/items");
		}

		[TestMethod]
		public void GetClassDocs_WithSummary_ReturnsSummary()
		{
			var unit = new ControllerUnit(typeof(TestFiles.SampleController), CreateXmlDocsForType(typeof(TestFiles.SampleController), @"
				<summary>
				Represents a sample endpoint.
				</summary>"));

			var docs = unit.GetClassDocs();

			docs.Should().NotBeNull();
			docs!.Summary.Should().Be("Represents a sample endpoint.");
		}

		[TestMethod]
		public void GetMethodDocs_WithNullableParameter_ReturnsSummary()
		{
			var method = typeof(TestFiles.DocumentationParameterFixture).GetMethod(
				nameof(TestFiles.DocumentationParameterFixture.GetOptionalValue));
			var xmlDocs = CreateXmlDocs(
				$"M:{typeof(TestFiles.DocumentationParameterFixture).FullName}.GetOptionalValue(System.Nullable{{System.Int32}})",
				"Gets an optional value.");
			var unit = new ControllerUnit(typeof(TestFiles.DocumentationParameterFixture), xmlDocs);

			var docs = unit.GetMethodDocs(method!);

			docs.Should().NotBeNull();
			docs!.Summary.Should().Be("Gets an optional value.");
		}

		[TestMethod]
		public void GetClassDocs_WithGenericType_PreservesGenericArity()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var unit = new ControllerUnit(type, CreateXmlDocs($"T:{type.FullName}", "Represents a generic fixture."));

			var docs = unit.GetClassDocs();

			docs.Should().NotBeNull();
			docs!.Summary.Should().Be("Represents a generic fixture.");
		}

		[TestMethod]
		public void GetClassDocs_WithNestedGenericType_PreservesEachGenericArity()
		{
			var type = typeof(TestFiles.DocumentationOuter<>.Inner<>);
			var unit = new ControllerUnit(
				type,
				CreateXmlDocs($"T:{type.FullName!.Replace('+', '.')}", "Represents a nested generic fixture."));

			var docs = unit.GetClassDocs();

			docs.Should().NotBeNull();
			docs!.Summary.Should().Be("Represents a nested generic fixture.");
		}

		[TestMethod]
		public void GetMethodDocs_WithGenericDeclaringTypeAndNoParameters_ReturnsSummary()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var method = type.GetMethod(nameof(TestFiles.GenericDocumentationFixture<object>.Get));
			var xmlDocs = CreateXmlDocs($"M:{type.FullName}.Get", "Gets a value.");
			var unit = new ControllerUnit(type, xmlDocs);

			var docs = unit.GetMethodDocs(method!);

			docs.Should().NotBeNull();
			docs!.Summary.Should().Be("Gets a value.");
		}

		[TestMethod]
		public void GetMethodDocs_WithMultidimensionalArrayParameter_ReturnsSummary()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var method = type.GetMethod(nameof(TestFiles.GenericDocumentationFixture<object>.GetMatrix));
			var xmlDocs = CreateXmlDocs(
				$"M:{type.FullName}.GetMatrix(System.Int32[0:,0:])",
				"Gets a matrix.");
			var unit = new ControllerUnit(type, xmlDocs);

			var docs = unit.GetMethodDocs(method!);

			docs.Should().NotBeNull();
			docs!.Summary.Should().Be("Gets a matrix.");
		}

		[TestMethod]
		public void GetMethodDocs_WithVectorAndJaggedArrayParameters_ReturnsSummaries()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var vectorSummary = GetMethodSummary(type, "GetVector", "System.Int32[]", "Gets a vector.");
			var jaggedSummary = GetMethodSummary(type, "GetJaggedArray", "System.Int32[][]", "Gets a jagged array.");

			vectorSummary.Should().Be("Gets a vector.");
			jaggedSummary.Should().Be("Gets a jagged array.");
		}

		[TestMethod]
		public void GetMethodDocs_WithThreeDimensionalArrayParameter_ReturnsSummary()
		{
			var summary = GetMethodSummary(
				typeof(TestFiles.GenericDocumentationFixture<>),
				"GetCube",
				"System.Int32[0:,0:,0:]",
				"Gets a cube.");

			summary.Should().Be("Gets a cube.");
		}

		[TestMethod]
		public void GetMethodDocs_WithGenericTypeParameter_ReturnsSummary()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var genericParameterSummary = GetMethodSummary(type, "GetGenericValue", "`0", "Gets a generic value.");
			var genericArraySummary = GetMethodSummary(type, "GetGenericVector", "`0[]", "Gets a generic vector.");

			genericParameterSummary.Should().Be("Gets a generic value.");
			genericArraySummary.Should().Be("Gets a generic vector.");
		}

		[TestMethod]
		public void GetMethodDocs_WithDeclaringAndMethodGenericParameters_ReturnsSummary()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var method = type.GetMethod(nameof(TestFiles.GenericDocumentationFixture<object>.GetGenericValues));
			var xmlDocs = CreateXmlDocs(
				$"M:{type.FullName}.GetGenericValues``1(`0,``0)",
				"Gets values from both generic scopes.");
			var unit = new ControllerUnit(type, xmlDocs);

			var docs = unit.GetMethodDocs(method!);

			docs.Should().NotBeNull();
			docs!.Summary.Should().Be("Gets values from both generic scopes.");
		}

		[TestMethod]
		public void GetMethodDocs_WithNestedGenericParameter_ReturnsSummary()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var method = type.GetMethod(nameof(TestFiles.GenericDocumentationFixture<object>.GetNested));
			var xmlDocs = CreateXmlDocs(
				$"M:{type.FullName}.GetNested(System.Collections.Generic.Dictionary{{System.String,System.Int32}}.KeyCollection)",
				"Gets nested values.");
			var unit = new ControllerUnit(type, xmlDocs);

			var docs = unit.GetMethodDocs(method!);

			docs.Should().NotBeNull();
			docs!.Summary.Should().Be("Gets nested values.");
		}

		[TestMethod]
		public void GetMethodDocs_WithNestedGenericArguments_ReturnsSummary()
		{
			var summary = GetMethodSummary(
				typeof(TestFiles.GenericDocumentationFixture<>),
				"GetNestedGeneric",
				"System.Collections.Generic.Dictionary{`0,System.Collections.Generic.List{System.Int32}}",
				"Gets nested generic values.");

			summary.Should().Be("Gets nested generic values.");
		}

		[TestMethod]
		public void GetMethodDocs_WithNestedGenericTypeParameter_ReturnsSummary()
		{
			var summary = GetMethodSummary(
				typeof(TestFiles.GenericDocumentationFixture<>),
				"GetNestedType",
				$"{typeof(TestFiles.DocumentationOuter<>).FullName!.Split('`')[0]}{{`0}}.Inner{{System.Int32}}",
				"Gets a nested type.");

			summary.Should().Be("Gets a nested type.");
		}

		[TestMethod]
		public void GetMethodDocs_WithByReferenceAndOutParameters_ReturnsSummaries()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var refSummary = GetMethodSummary(type, "GetByReference", "System.Int32@", "Gets a by-reference value.");
			var outSummary = GetMethodSummary(type, "GetOutValue", "System.Int32@", "Gets an out value.");

			refSummary.Should().Be("Gets a by-reference value.");
			outSummary.Should().Be("Gets an out value.");
		}

		[TestMethod]
		public void GetMethodDocs_WithOverloads_ResolvesEachParameterSignature()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var intSummary = GetMethodSummary(
				type,
				"GetOverloaded",
				"System.Int32",
				"Gets the integer overload.",
				typeof(int));
			var stringSummary = GetMethodSummary(
				type,
				"GetOverloaded",
				"System.String",
				"Gets the string overload.",
				typeof(string));

			intSummary.Should().Be("Gets the integer overload.");
			stringSummary.Should().Be("Gets the string overload.");
		}

		[TestMethod]
		public void GetMethodDocs_WithGenericMethod_ReturnsSummary()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var method = type.GetMethod(nameof(TestFiles.GenericDocumentationFixture<object>.GetGeneric));
			var xmlDocs = CreateXmlDocs(
				$"M:{type.FullName}.GetGeneric``1(``0)",
				"Gets a generic value.");
			var unit = new ControllerUnit(type, xmlDocs);

			var docs = unit.GetMethodDocs(method!);

			docs.Should().NotBeNull();
			docs!.Summary.Should().Be("Gets a generic value.");
		}

		[TestMethod]
		public void GetMethodDocs_WithMultipleGenericMethodParameters_ReturnsSummary()
		{
			var type = typeof(TestFiles.GenericDocumentationFixture<>);
			var method = type.GetMethod(nameof(TestFiles.GenericDocumentationFixture<object>.GetGenericPair));
			var xmlDocs = CreateXmlDocs(
				$"M:{type.FullName}.GetGenericPair``2(``0,``1)",
				"Gets a pair of generic values.");
			var unit = new ControllerUnit(type, xmlDocs);

			var docs = unit.GetMethodDocs(method!);

			docs.Should().NotBeNull();
			docs!.Summary.Should().Be("Gets a pair of generic values.");
		}

		[TestMethod]
		public void GetTagName_ControllerType_ReturnsControllerNameWithoutSuffix()
		{
			var unit = new ControllerUnit(typeof(TestFiles.SampleController), null);

			unit.GetTagName().Should().Be("Sample");
		}

		private static XDocument CreateXmlDocsForType(System.Type type, string memberContent)
		{
			return XDocument.Parse($@"<doc><members><member name=""T:{type.FullName}"">{memberContent}</member></members></doc>");
		}

		private static XDocument CreateXmlDocs(string memberId, string summary)
		{
			return XDocument.Parse($@"<doc><members><member name=""{memberId}""><summary>{summary}</summary></member></members></doc>");
		}

		private static string? GetMethodSummary(
			Type type,
			string methodName,
			string parameterType,
			string expectedSummary,
			Type? reflectedParameterType = null)
		{
			var methods = type.GetMethods().Where(m => m.Name == methodName);
			var method = reflectedParameterType is null
				? methods.Single()
				: methods.Single(m => m.GetParameters().Length == 1 &&
					m.GetParameters()[0].ParameterType == reflectedParameterType);
			var xmlDocs = CreateXmlDocs(
				$"M:{type.FullName}.{methodName}({parameterType})",
				expectedSummary);
			var unit = new ControllerUnit(type, xmlDocs);

			return unit.GetMethodDocs(method!)?.Summary;
		}
	}
}
