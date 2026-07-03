namespace UserDefinedApiToolkit.Tests.Build
{
	using System.Collections.Generic;

	using FluentAssertions;

	using Microsoft.OpenApi;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi;

	[TestClass]
	public sealed class ProducesResponseTypeTests
	{
		private static bool TryGetResponses(string methodName, out OpenApiDocument doc, out OpenApiResponses responses)
		{
			doc = new OpenApiDocument
			{
				Components = new OpenApiComponents
				{
					Schemas = new Dictionary<string, IOpenApiSchema>(),
				},
			};

			var provider = new OperationProvider(new ComponentRegistry(doc));
			var unit = new ControllerUnit(typeof(TestFiles.ResponseTypesController), null);
			var method = typeof(TestFiles.ResponseTypesController).GetMethod(methodName);

			var success = provider.TryGetOperations(unit, method!, out _, out var operation);
			responses = operation.Responses;
			return success;
		}

		[TestMethod]
		public void ExplicitTypedResponse_RegistersSchemaAndReturnsReferenceInContent()
		{
			TryGetResponses(nameof(TestFiles.ResponseTypesController.GetWithExplicitTypedResponse), out var doc, out var responses);

			responses.Should().ContainSingle();
			responses.Should().ContainKey("200");

			var response = responses["200"];
			response.Content.Should().NotBeNull();
			response.Content!.Should().ContainKey("application/json");
			response.Content["application/json"].Schema.Should().BeOfType<OpenApiSchemaReference>();
			doc.Components!.Schemas.Should().ContainKey(nameof(TestFiles.SampleDto));
		}

		[TestMethod]
		public void StatusOnlyResponse_HasNoContent()
		{
			TryGetResponses(nameof(TestFiles.ResponseTypesController.GetWithStatusOnlyResponse), out _, out var responses);

			responses.Should().ContainKey("204");
			responses["204"].Content.Should().BeNull();
		}

		[TestMethod]
		public void MultipleExplicitResponses_AreAllRegistered()
		{
			TryGetResponses(nameof(TestFiles.ResponseTypesController.GetWithMultipleResponses), out _, out var responses);

			responses.Should().HaveCount(2);
			responses.Should().ContainKey("200");
			responses.Should().ContainKey("404");
		}

		[TestMethod]
		public void ExplicitAttribute_TakesPriorityOverApiResultGenericInference()
		{
			TryGetResponses(nameof(TestFiles.ResponseTypesController.GetWithExplicitAttributeOverridingApiResult), out _, out var responses);

			responses.Should().ContainSingle();
			responses.Should().ContainKey("200");
			responses.Should().NotContainKey("400");
		}

		[TestMethod]
		public void NoExplicitAttribute_FallsBackToApiResultGenericInference()
		{
			TryGetResponses(nameof(TestFiles.ResponseTypesController.GetWithoutExplicitAttribute), out _, out var responses);

			responses.Should().HaveCount(2);
			responses.Should().ContainKey("200");
			responses.Should().ContainKey("400");
		}

		[TestMethod]
		public void CollectionResponseType_ReturnsArraySchema()
		{
			TryGetResponses(nameof(TestFiles.ResponseTypesController.GetCollectionResponse), out _, out var responses);

			var schema = responses["200"].Content!["application/json"].Schema;

			schema.Should().BeOfType<OpenApiSchema>();
			((OpenApiSchema)schema!).Type.Should().Be(JsonSchemaType.Array);
		}

		[TestMethod]
		public void NoResponseInformation_ReturnsEmptyResponses()
		{
			TryGetResponses(nameof(TestFiles.ResponseTypesController.DeleteWithNoResponseInfo), out _, out var responses);

			responses.Should().BeEmpty();
		}
	}
}
