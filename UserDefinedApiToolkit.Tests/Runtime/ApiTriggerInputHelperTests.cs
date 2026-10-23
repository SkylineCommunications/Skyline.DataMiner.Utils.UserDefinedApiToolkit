namespace UserDefinedApiToolkit.Tests.Runtime
{
	using System;

	using FluentAssertions;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[TestClass]
	public sealed class ApiTriggerInputHelperTests
	{
		[TestMethod]
		public void Deserialize_WithQueryParameters_CreatesUsableQueryParameters()
		{
			// Arrange
			var json = """
				{
					"requestMethod": "Get",
					"route": "/v1/items",
					"rawBody": "{\"name\":\"item\"}",
					"parameters": {
						"token": "abc"
					},
					"queryParameters": {
						"tag": ["one", "two"],
						"page": "2"
					},
					"routeParameters": {
						"id": "42"
					}
				}
				""";

			// Act
			var input = ApiTriggerInputHelper.Deserialize(json);

			// Assert
			input.RequestMethod.Should().Be(RequestMethod.Get);
			input.Route.Should().Be("/v1/items");
			input.RawBody.Should().Be("{\"name\":\"item\"}");
			input.Parameters.Should().ContainKey("token").WhoseValue.Should().Be("abc");
			input.RouteParameters.Should().ContainKey("id").WhoseValue.Should().Be("42");
			input.QueryParameters.Should().NotBeNull();
			input.QueryParameters!.TryGetValues("tag", out var tags).Should().BeTrue();
			tags.Should().Equal("one", "two");
			input.QueryParameters.TryGetValue("page", out var page).Should().BeTrue();
			page.Should().Be("2");
		}

		[TestMethod]
		public void Deserialize_WithoutQueryParameters_LeavesQueryParametersNull()
		{
			// Act
			var input = ApiTriggerInputHelper.Deserialize("{\"route\":\"/v1/items\"}");

			// Assert
			input.QueryParameters.Should().BeNull();
		}

		[TestMethod]
		public void Deserialize_WithIso8601QueryParameter_PreservesValue()
		{
			// Arrange
			const string expectedValue = "2024-01-02T10:00:00Z";
			var json = $"{{\"queryParameters\":{{\"since\":\"{expectedValue}\"}}}}";

			// Act
			var input = ApiTriggerInputHelper.Deserialize(json);

			// Assert
			input.QueryParameters!.TryGetValue("since", out var value).Should().BeTrue();
			value.Should().Be(expectedValue);
		}

		[TestMethod]
		public void Deserialize_WithTrailingContent_ThrowsJsonException()
		{
			// Act
			var act = () => ApiTriggerInputHelper.Deserialize("{\"route\":\"/v1/items\"} garbage");

			// Assert
			act.Should().Throw<JsonException>();
		}

		[TestMethod]
		public void Deserialize_WithNullJson_ThrowsArgumentNullException()
		{
			// Act
			var act = () => ApiTriggerInputHelper.Deserialize(null!);

			// Assert
			act.Should().Throw<ArgumentNullException>();
		}
	}
}
