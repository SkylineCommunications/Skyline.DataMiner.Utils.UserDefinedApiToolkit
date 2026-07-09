namespace UserDefinedApiToolkit.Tests.Runtime.PathVariables
{
	using System;

	using FluentAssertions;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions;

	[TestClass]
	public sealed class PathVariableTests
	{
		[TestMethod]
		[DataRow("/v1/items/5", 5)]
		[DataRow("/v1/items/42", 42)]
		public void Get_WithImplicitRouteParameter_BindsAndConverts(string route, int expected)
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_PathVariables>()
				.Build();

			// Act
			var result = new ApiTriggerOutput();
			var act = () => result = api.Run(engine, new ApiTriggerInput
			{
				Route = route,
				RequestMethod = RequestMethod.Get,
			});

			// Assert
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(expected));
		}

		[TestMethod]
		[DataRow("/v1/items/5/details", 5)]
		public void Get_WithExplicitFromRouteNameOverride_BindsAndConverts(string route, int expected)
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_PathVariables>()
				.Build();

			// Act
			var result = new ApiTriggerOutput();
			var act = () => result = api.Run(engine, new ApiTriggerInput
			{
				Route = route,
				RequestMethod = RequestMethod.Get,
			});

			// Assert
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(expected));
		}

		[TestMethod]
		[DataRow("/v1/items/count", -1)]
		public void Get_LiteralSegmentTakesPrecedenceOverPlaceholder(string route, int expected)
		{
			// Arrange: "/v1/items/count" could match both "{id}" (with id="count", which fails int
			// conversion) and the literal "count" route. The literal route must win.
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_PathVariables>()
				.Build();

			// Act
			var result = new ApiTriggerOutput();
			var act = () => result = api.Run(engine, new ApiTriggerInput
			{
				Route = route,
				RequestMethod = RequestMethod.Get,
			});

			// Assert
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(expected));
		}

		[TestMethod]
		[DataRow("/v1/items/5/search?q=hello", 5, "hello")]
		public void Get_WithRouteParameterAndFromQueryNameOverride_BindsBoth(string route, int id, string searchTerm)
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_PathVariables>()
				.Build();

			var url = Utility.ParseUrl(route);

			// Act
			var result = new ApiTriggerOutput();
			var act = () => result = api.Run(engine, new ApiTriggerInput
			{
				Route = url.Path,
				RequestMethod = RequestMethod.Get,
				QueryParameters = new QueryParameters(url.QueryParameters),
			});

			// Assert
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject($"{id}:{searchTerm}"));
		}

		[TestMethod]
		[DataRow("/v1/items/not-a-number")]
		public void Get_WithInvalidRouteParameter_ThrowsInvalidParameterException(string route)
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_PathVariables>()
				.Build();

			// Act
			var act = () => api.Run(engine, new ApiTriggerInput
			{
				Route = route,
				RequestMethod = RequestMethod.Get,
			});

			// Assert
			act.Should().Throw<InvalidParameterException>()
				.Which.ParameterName.Should().Be("id");
		}

		[TestMethod]
		public void Build_WithPlaceholderMissingBoundParameter_ThrowsInvalidRouteException()
		{
			// Arrange
			var act = () => UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_PathVariables_MissingParameter>()
				.Build();

			// Assert
			act.Should().Throw<InvalidRouteException>();
		}

		[TestMethod]
		public void Build_WithFromRouteReferencingUnknownPlaceholder_ThrowsInvalidRouteException()
		{
			// Arrange
			var act = () => UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_PathVariables_UnmatchedFromRoute>()
				.Build();

			// Assert
			act.Should().Throw<InvalidRouteException>();
		}
	}
}
