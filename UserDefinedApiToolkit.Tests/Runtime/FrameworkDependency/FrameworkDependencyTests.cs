namespace UserDefinedApiToolkit.Tests.Runtime.FrameworkDependency
{
	using FluentAssertions;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[TestClass]
	public sealed class FrameworkDependencyTests
	{
		[TestMethod]
		[DataRow("/v1/framework-dependency/engine?dummy=hello", "hello")]
		public void GetTest_With_UnattributedEngineParameter_IsRankedAndResolved(string route, string expected)
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_FrameworkDependency>()
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
			// Today this route can never be selected: GetRank has no notion of unattributed
			// framework-provided parameters (IEngine here), so it treats "engine" as a required
			// query parameter with no matching key -> rank -1 -> the parameterless GetDummy()
			// overload wins instead, returning "[]" rather than "hello".
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(expected));
		}

		[TestMethod]
		[DataRow("/v1/framework-dependency/connection")]
		public void GetTest_With_UnattributedConnectionParameter_IsResolved(string route)
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_FrameworkDependency>()
				.Build();

			// Act
			var result = new ApiTriggerOutput();
			var act = () => result = api.Run(engine, new ApiTriggerInput
			{
				Route = route,
				RequestMethod = RequestMethod.Get,
			});

			// Assert
			// Today, Invoke's framework-parameter block checks `typeof(IEngine)` twice (a
			// copy-paste bug) instead of checking `typeof(IConnection)` on the second check, so an
			// unattributed IConnection parameter is never framework-bound and falls through to a
			// failed query lookup, throwing InvalidOperationException instead of resolving.
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(true));
		}
	}
}
