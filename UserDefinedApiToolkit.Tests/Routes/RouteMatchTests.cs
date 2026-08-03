namespace UserDefinedApiToolkit.Tests.Routes
{
	using System.Collections.Generic;

	using FluentAssertions;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Routes;

	[TestClass]
	public sealed class RouteMatchTests
	{
		[TestMethod]
		public void NoMatch_RouteValues_CannotBeMutatedThroughDictionaryCast()
		{
			// Arrange: RouteMatch.NoMatch is a static, shared instance reused across every
			// non-matching route comparison for every request. If its RouteValues can be cast
			// back to a mutable Dictionary and modified, that mutation leaks into every other
			// request that subsequently hits RouteMatch.NoMatch.
			var routeValues = RouteMatch.NoMatch.RouteValues;

			// Act
			var act = () => ((IDictionary<string, string>)routeValues)["leaked"] = "value";

			// Assert
			act.Should().Throw<System.NotSupportedException>();
		}
	}
}
