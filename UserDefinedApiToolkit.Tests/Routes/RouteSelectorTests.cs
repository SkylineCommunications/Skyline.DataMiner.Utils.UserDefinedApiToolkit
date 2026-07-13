namespace UserDefinedApiToolkit.Tests.Routes
{
	using System;

	using FluentAssertions;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Routes;

	[TestClass]
	public sealed class RouteSelectorTests
	{
		[TestMethod]
		public void SelectRoute_WithNullServices_ThrowsArgumentNullException()
		{
			// Arrange
			var selector = new RouteSelector(Array.Empty<RouteHandlerInfo>());
			var context = new ApiContext();

			// Act
			var act = () => selector.SelectRoute(context, null!);

			// Assert
			act.Should().Throw<ArgumentNullException>()
				.Which.ParamName.Should().Be("services");
		}
	}
}
