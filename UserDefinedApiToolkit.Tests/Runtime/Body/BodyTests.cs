namespace UserDefinedApiToolkit.Tests.Runtime.Body
{
	using FluentAssertions;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions;

	[TestClass]
	public sealed class BodyTests
	{
		[TestMethod]
		[DataRow("42", 42)]
		[DataRow("-3", -3)]
		public void PostTest_IntBody_NoConverterSupportsIt_FallsBackToStringValueConverter(string rawBody, int expected)
		{
			// Arrange: only register a converter that explicitly refuses to handle anything but
			// string, so the only way an `int` body can be bound is via the StringValueConverter
			// fallback in RouteHandlerInfo.HandleBodyParam.
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_Body>()
				.WithDefaultInputConverter(new TestFiles.StringOnlyInputConverter())
				.Build();

			// Act
			var result = new ApiTriggerOutput();
			var act = () => result = api.Run(engine, new ApiTriggerInput
			{
				Route = "/v1/body",
				RequestMethod = RequestMethod.Post,
				RawBody = rawBody,
			});

			// Assert
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(expected));
		}

		[TestMethod]
		[DataRow("not-a-number")]
		public void PostTest_InvalidIntBody_NoConverterSupportsIt_ThrowsInvalidParameterException(string rawBody)
		{
			// Arrange
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_Body>()
				.WithDefaultInputConverter(new TestFiles.StringOnlyInputConverter())
				.Build();

			// Act
			var act = () => api.Run(engine, new ApiTriggerInput
			{
				Route = "/v1/body",
				RequestMethod = RequestMethod.Post,
				RawBody = rawBody,
			});

			// Assert
			act.Should().Throw<InvalidParameterException>()
				.Which.ParameterName.Should().Be("amount");
		}

		[TestMethod]
		[DataRow("42", 42)]
		public void PostTest_IntBody_DefaultConverter_UsesConverterNotFallback(string rawBody, int expected)
		{
			// Arrange: default builder keeps the Newtonsoft converter, which already supports any
			// type - the StringValueConverter fallback should not be needed (and isn't) here.
			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<TestFiles.Controller_Body>()
				.Build();

			// Act
			var result = new ApiTriggerOutput();
			var act = () => result = api.Run(engine, new ApiTriggerInput
			{
				Route = "/v1/body",
				RequestMethod = RequestMethod.Post,
				RawBody = rawBody,
			});

			// Assert
			act.Should().NotThrow();
			result.Should().NotBeNull();
			result.ResponseBody.Should().Be(JsonConvert.SerializeObject(expected));
		}
	}
}
