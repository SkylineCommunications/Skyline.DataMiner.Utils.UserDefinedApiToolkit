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
		[DataRow("/v1/get?limit=5&limit=10", 5)]
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

		[TestMethod]
		[DataRow("/v1/get/ArrayInt?list=5&list=not-a-number")]
		public void GetTest_With_InvalidListIntQueryParam_ReportsInvalidValue(string route)
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
				.Which.RawValue.Should().Be("not-a-number");
		}

		[TestMethod]
		[DataRow("/v1/get/ListString?list=Andi&list=Tamer", "Andi", "Tamer")]
		[DataRow("/v1/get/ArrayString?list=Andi&list=Tamer", "Andi", "Tamer")]
		[DataRow("/v1/get/IEnumerableString?list=Andi&list=Tamer", "Andi", "Tamer")]
		[DataRow("/v1/get/ICollectionString?list=Andi&list=Tamer", "Andi", "Tamer")]
		[DataRow("/v1/get/CollectionString?list=Andi&list=Tamer", "Andi", "Tamer")]
		public void GetTest_With_ListStringQueryParam(string route, params string[] names)
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
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(names));
		}

		[TestMethod]
		[DataRow("/v1/get/ListInt?list=5&list=10", 5, 10)]
		[DataRow("/v1/get/ArrayInt?list=5&list=10", 5, 10)]
		[DataRow("/v1/get/IEnumerableInt?list=5&list=10", 5, 10)]
		[DataRow("/v1/get/ICollectionInt?list=5&list=10", 5, 10)]
		[DataRow("/v1/get/CollectionInt?list=5&list=10", 5, 10)]
		public void GetTest_With_ListIntQueryParam(string route, params int[] names)
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
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(names));
		}

		[TestMethod]
		[DataRow("/v1/get/ISetString?list=Andi&list=Tamer", "Andi", "Tamer")]
		[DataRow("/v1/get/HashSetString?list=Andi&list=Tamer", "Andi", "Tamer")]
		public void GetTest_With_SetStringQueryParam(string route, params string[] names)
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
			JsonConvert.DeserializeObject<string[]>(result.ResponseBody)
				.Should()
				.BeEquivalentTo(names);
		}

		[TestMethod]
		[DataRow("/v1/get/ISetInt?list=5&list=10", 5, 10)]
		[DataRow("/v1/get/HashSetInt?list=5&list=10", 5, 10)]
		public void GetTest_With_SetIntQueryParam(string route, params int[] names)
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
			JsonConvert.DeserializeObject<int[]>(result.ResponseBody)
				.Should()
				.BeEquivalentTo(names);
		}
	}
}