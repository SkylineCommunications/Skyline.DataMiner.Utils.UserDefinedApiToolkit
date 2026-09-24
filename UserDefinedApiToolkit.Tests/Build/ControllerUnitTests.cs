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
		public void GetTagName_ControllerType_ReturnsControllerNameWithoutSuffix()
		{
			var unit = new ControllerUnit(typeof(TestFiles.SampleController), null);

			unit.GetTagName().Should().Be("Sample");
		}

		private static XDocument CreateXmlDocsForType(System.Type type, string memberContent)
		{
			return XDocument.Parse($@"<doc><members><member name=""T:{type.FullName}"">{memberContent}</member></members></doc>");
		}
	}
}
