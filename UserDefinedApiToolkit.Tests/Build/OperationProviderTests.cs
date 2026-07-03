namespace UserDefinedApiToolkit.Tests.Build
{
	using System.Collections.Generic;
	using System.Net.Http;

	using FluentAssertions;

	using Microsoft.OpenApi;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi;

	[TestClass]
	public sealed class OperationProviderTests
	{
		private static OperationProvider CreateProvider()
		{
			var doc = new OpenApiDocument
			{
				Components = new OpenApiComponents
				{
					Schemas = new Dictionary<string, IOpenApiSchema>(),
				},
			};

			return new OperationProvider(new ComponentRegistry(doc));
		}

		[TestMethod]
		public void TryGetOperations_HttpGetMethod_DetectsGetHttpMethodAndQueryParameter()
		{
			var provider = CreateProvider();
			var unit = new ControllerUnit(typeof(TestFiles.SampleController), null);
			var method = typeof(TestFiles.SampleController).GetMethod(nameof(TestFiles.SampleController.GetById));

			var success = provider.TryGetOperations(unit, method!, out var httpMethod, out var operation);

			success.Should().BeTrue();
			httpMethod.Should().Be(HttpMethod.Get);
			operation.Parameters.Should().ContainSingle(p => p.Name == "id" && p.In == ParameterLocation.Query);
		}

		[TestMethod]
		public void TryGetOperations_ApiResultOfTwo_GeneratesSuccessAndErrorResponses()
		{
			var provider = CreateProvider();
			var unit = new ControllerUnit(typeof(TestFiles.SampleController), null);
			var method = typeof(TestFiles.SampleController).GetMethod(nameof(TestFiles.SampleController.GetById));

			provider.TryGetOperations(unit, method!, out _, out var operation);

			operation.Responses.Should().ContainKey("200");
			operation.Responses.Should().ContainKey("400");
		}

		[TestMethod]
		public void TryGetOperations_FromBodyParameter_GeneratesRequestBody()
		{
			var provider = CreateProvider();
			var unit = new ControllerUnit(typeof(TestFiles.SampleController), null);
			var method = typeof(TestFiles.SampleController).GetMethod(nameof(TestFiles.SampleController.Create));

			var success = provider.TryGetOperations(unit, method!, out var httpMethod, out var operation);

			success.Should().BeTrue();
			httpMethod.Should().Be(HttpMethod.Post);
			operation.RequestBody.Should().NotBeNull();
		}

		[TestMethod]
		public void TryGetOperations_MethodWithoutHttpAttribute_ReturnsFalse()
		{
			var provider = CreateProvider();
			var unit = new ControllerUnit(typeof(TestFiles.SampleController), null);
			var method = typeof(object).GetMethod(nameof(ToString));

			var success = provider.TryGetOperations(unit, method!, out _, out _);

			success.Should().BeFalse();
		}

		[TestMethod]
		public void TryGetOperations_ValidMethod_AddsControllerNameAsTag()
		{
			var provider = CreateProvider();
			var unit = new ControllerUnit(typeof(TestFiles.SampleController), null);
			var method = typeof(TestFiles.SampleController).GetMethod(nameof(TestFiles.SampleController.Delete));

			provider.TryGetOperations(unit, method!, out _, out var operation);

			operation.Tags.Should().ContainSingle();
		}
	}
}
