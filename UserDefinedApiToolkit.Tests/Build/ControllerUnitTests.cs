namespace UserDefinedApiToolkit.Tests.Build
{
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
	}
}
