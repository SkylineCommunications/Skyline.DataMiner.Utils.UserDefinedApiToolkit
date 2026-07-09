namespace UserDefinedApiToolkit.Tests.Build
{
	using System.Collections.Generic;
	using System.Net.Http;

	using FluentAssertions;

	using Microsoft.OpenApi;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi;

	[TestClass]
	public sealed class PathBuilderTests
	{
		private static (OpenApiDocument Doc, PathBuilder Builder) Create()
		{
			var doc = new OpenApiDocument
			{
				Paths = new OpenApiPaths(),
				Components = new OpenApiComponents
				{
					Schemas = new Dictionary<string, IOpenApiSchema>(),
				},
			};

			var provider = new OperationProvider(new ComponentRegistry(doc));
			return (doc, new PathBuilder(provider));
		}

		[TestMethod]
		public void HandleController_MethodsWithDifferentTemplates_ProduceSeparatePathEntries()
		{
			var (doc, builder) = Create();
			var unit = new ControllerUnit(typeof(TestFiles.PathVariableController), null);

			builder.HandleController(doc, unit);

			doc.Paths.Should().ContainKey("/v1/items/{id}");
			doc.Paths.Should().ContainKey("/v1/items/{id}/details");
			doc.Paths.Should().ContainKey("/v1/items");
		}

		[TestMethod]
		public void HandleController_MethodWithRouteTemplate_RegistersOperationUnderCombinedPath()
		{
			var (doc, builder) = Create();
			var unit = new ControllerUnit(typeof(TestFiles.PathVariableController), null);

			builder.HandleController(doc, unit);

			doc.Paths["/v1/items/{id}"].Operations.Should().ContainKey(HttpMethod.Get);
		}
	}
}
