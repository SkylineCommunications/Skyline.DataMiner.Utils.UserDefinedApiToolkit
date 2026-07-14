namespace UserDefinedApiToolkit.Tests.Build
{
	using System.Collections.Generic;

	using FluentAssertions;

	using Microsoft.OpenApi;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi;

	[TestClass]
	public sealed class ComponentRegistryTests
	{
		private static OpenApiDocument CreateDocument()
		{
			return new OpenApiDocument
			{
				Components = new OpenApiComponents
				{
					Schemas = new Dictionary<string, IOpenApiSchema>(),
				},
			};
		}

		[TestMethod]
		public void GetOrRegisterSchema_Primitive_ReturnsInlineSchemaWithoutRegistering()
		{
			var doc = CreateDocument();
			var registry = new ComponentRegistry(doc);

			var schema = registry.GetOrRegisterSchema(typeof(string));

			schema.Should().BeOfType<OpenApiSchema>();
			doc.Components!.Schemas.Should().BeEmpty();
		}

		[TestMethod]
		public void GetOrRegisterSchema_ComplexType_RegistersSchemaAndReturnsReference()
		{
			var doc = CreateDocument();
			var registry = new ComponentRegistry(doc);

			var schema = registry.GetOrRegisterSchema(typeof(TestFiles.SampleDto));

			schema.Should().BeOfType<OpenApiSchemaReference>();
			doc.Components!.Schemas.Should().ContainKey(nameof(TestFiles.SampleDto));

			var registeredSchema = (OpenApiSchema)doc.Components!.Schemas[nameof(TestFiles.SampleDto)];
			registeredSchema.Type.Should().Be(JsonSchemaType.Object);
			registeredSchema.Properties.Should().ContainKey(nameof(TestFiles.SampleDto.Name));
			registeredSchema.Properties.Should().ContainKey(nameof(TestFiles.SampleDto.Count));
		}

		[TestMethod]
		public void GetOrRegisterSchema_Array_ReturnsArraySchemaWithItemSchema()
		{
			var doc = CreateDocument();
			var registry = new ComponentRegistry(doc);

			var schema = registry.GetOrRegisterSchema(typeof(int[]));

			schema.Should().BeOfType<OpenApiSchema>();
			((OpenApiSchema)schema!).Type.Should().Be(JsonSchemaType.Array);
			((OpenApiSchema)schema!).Items.Should().NotBeNull();
		}

		[TestMethod]
		public void GetOrRegisterSchema_ComplexTypeCalledTwice_RegistersOnlyOnce()
		{
			var doc = CreateDocument();
			var registry = new ComponentRegistry(doc);

			registry.GetOrRegisterSchema(typeof(TestFiles.SampleDto));
			registry.GetOrRegisterSchema(typeof(TestFiles.SampleDto));

			doc.Components!.Schemas.Should().HaveCount(1);
		}

		[TestMethod]
		public void GetOrRegisterSchema_CollectionOfComplexType_ReturnsArraySchema()
		{
			var doc = CreateDocument();
			var registry = new ComponentRegistry(doc);

			var schema = registry.GetOrRegisterSchema(typeof(List<TestFiles.SampleDto>));

			schema.Should().BeOfType<OpenApiSchema>();
			((OpenApiSchema)schema!).Type.Should().Be(JsonSchemaType.Array);
			doc.Components!.Schemas.Should().ContainKey(nameof(TestFiles.SampleDto));
		}

		[TestMethod]
		public void GetOrRegisterSchema_Null_ReturnsNull()
		{
			var doc = CreateDocument();
			var registry = new ComponentRegistry(doc);

			registry.GetOrRegisterSchema(null!).Should().BeNull();
		}

		[TestMethod]
		public void GetOrRegisterSchema_CircularlyReferencedComplexTypes_RegistersBothAndDoesNotOverflow()
		{
			var doc = CreateDocument();
			var registry = new ComponentRegistry(doc);

			var schema = registry.GetOrRegisterSchema(typeof(TestFiles.Profile));

			schema.Should().BeOfType<OpenApiSchemaReference>();
			doc.Components!.Schemas.Should().ContainKey(nameof(TestFiles.Profile));
			doc.Components!.Schemas.Should().ContainKey(nameof(TestFiles.Parameter));

			var profileSchema = (OpenApiSchema)doc.Components!.Schemas[nameof(TestFiles.Profile)];
			var parametersArraySchema = (OpenApiSchema)profileSchema.Properties![nameof(TestFiles.Profile.Parameters)];
			var parameterSchema = (OpenApiSchemaReference)parametersArraySchema.Items!;
			parameterSchema.Reference.Id.Should().Be(nameof(TestFiles.Parameter));

			var registeredParameterSchema = (OpenApiSchema)doc.Components!.Schemas[nameof(TestFiles.Parameter)];
			var parentSchema = (OpenApiSchemaReference)registeredParameterSchema.Properties![nameof(TestFiles.Parameter.Parent)];
			parentSchema.Reference.Id.Should().Be(nameof(TestFiles.Profile));
		}
	}
}
