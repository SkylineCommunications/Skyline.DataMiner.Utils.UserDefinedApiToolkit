namespace UserDefinedApiToolkit.Tests.Runtime.PATCH
{
	using FluentAssertions;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions;

	[TestClass]
	public sealed class PatchTests
	{
		[TestMethod]
		public void PatchTest_NoRouteParam_RoutesToPatchActionAndBindsBody()
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_PATCH>()
				.WithDefaultInputConverter(new PlainTextConverter())
				.WithDefaultOutputConverter(new PlainTextConverter())
				.Build();

			// Act
			var result = new ApiTriggerOutput();
			var act = () => result = api.Run(engine, new ApiTriggerInput
			{
				Route = "/v1/patch",
				RequestMethod = RequestMethod.Patch,
				RawBody = "\"hello\"",
			});

			// Assert
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be("\"hello\"");
		}

		[TestMethod]
		public void PatchTest_WithRouteParam_BindsRouteAndBody()
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_PATCH>()
				.WithDefaultInputConverter(new PlainTextConverter())
				.WithDefaultOutputConverter(new PlainTextConverter())
				.Build();

			// Act
			var result = new ApiTriggerOutput();
			var act = () => result = api.Run(engine, new ApiTriggerInput
			{
				Route = "/v1/patch/42",
				RequestMethod = RequestMethod.Patch,
				RawBody = "\"hello\"",
			});

			// Assert
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be("42:\"hello\"");
		}

		[TestMethod]
		public void PatchTest_SameRouteDifferentMethod_ThrowsNoRouteException()
		{
			// Arrange: proves verb discrimination - a GET to a PATCH-only route must not match.
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_PATCH>()
				.Build();

			// Act
			var act = () => api.Run(engine, new ApiTriggerInput
			{
				Route = "/v1/patch",
				RequestMethod = RequestMethod.Get,
			});

			// Assert
			act.Should().Throw<NoRouteException>();
		}
	}
}
