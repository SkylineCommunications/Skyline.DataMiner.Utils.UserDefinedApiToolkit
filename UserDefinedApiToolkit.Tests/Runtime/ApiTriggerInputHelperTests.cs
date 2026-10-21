namespace UserDefinedApiToolkit.Tests.Runtime
{
	using System;

	using FluentAssertions;

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
		public void Deserialize_WithNullJson_ThrowsArgumentNullException()
		{
			// Act
			var act = () => ApiTriggerInputHelper.Deserialize(null!);

			// Assert
			act.Should().Throw<ArgumentNullException>();
		}
	}
}
