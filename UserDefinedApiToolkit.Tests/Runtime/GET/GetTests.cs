namespace UserDefinedApiToolkit.Tests.Runtime.GET
{
	using FluentAssertions;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions;

	[TestClass]
	public sealed class GetTests
	{
		[TestMethod]
		[DataRow("UserDefinedApiToolkit.Tests.Runtime.GET.TestFiles.Controller_GET", "/v1/get")]
		public void GetTest_NoArgs(string controller_fullname, string route)
		{
			// Arrange
			var engine = new EngineMock();
			var controllerType = Type.GetType(controller_fullname);
			var api = UserDefinedApi.CreateBuilder()
				.AddController(controllerType)
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
			result.ResponseBody.Should().Be("[]");
		}

		[TestMethod]
		[DataRow("UserDefinedApiToolkit.Tests.Runtime.GET.TestFiles.Controller_GET", "/v1/get?query=hello")]
		[DataRow("UserDefinedApiToolkit.Tests.Runtime.GET.TestFiles.Controller_GET", "/v1/get?query=hello&limit=5")]
		public void GetTest_With_QueryParam(string controller_fullname, string route)
		{
			// Arrange
			var engine = new EngineMock();
			var controllerType = Type.GetType(controller_fullname);
			var api = UserDefinedApi.CreateBuilder()
				.AddController(controllerType)
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
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(url.QueryParameters));
		}

		[TestMethod]
		[DataRow("/v1/get?id=5", "My Awesome Body")]
		public void GetTest_With_Body(string route, string body)
		{
			// Arrange
			var engine = new EngineMock();
			var converter = new PlainTextConverter();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_GET>()
				.WithDefaultInputConverter(converter)
				.WithDefaultOutputConverter(converter)
				.Build();

			var url = Utility.ParseUrl(route);

			// Act
			var result = new ApiTriggerOutput();
			var act = () => result = api.Run(engine, new ApiTriggerInput
			{
				Route = url.Path,
				RequestMethod = RequestMethod.Get,
				QueryParameters = new QueryParameters(url.QueryParameters),
				RawBody = body,
			});

			// Assert
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be(body);
		}

		[TestMethod]
		[DataRow("/v1/get?limit=5", 5)]
		[DataRow("/v1/get?limit=-3", -3)]
		public void GetTest_With_IntQueryParam_ConvertsToInt(string route, int expected)
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_GET>()
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
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(expected));
		}

		[TestMethod]
		[DataRow("/v1/get?limit=not-a-number")]
		public void GetTest_With_InvalidIntQueryParam_ThrowsInvalidParameterException(string route)
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_GET>()
				.Build();

			var url = Utility.ParseUrl(route);

			// Act
			var act = () => api.Run(engine, new ApiTriggerInput
			{
				Route = url.Path,
				RequestMethod = RequestMethod.Get,
				QueryParameters = new QueryParameters(url.QueryParameters),
			});

			// Assert
			act.Should().Throw<InvalidParameterException>()
				.Which.ParameterName.Should().Be("limit");
		}
	}
}