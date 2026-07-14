namespace UserDefinedApiToolkit.Tests.Build
{
	using System;

	using FluentAssertions;

	using Microsoft.OpenApi;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi;

	[TestClass]
	public sealed class ComponentFactoryTests
	{
		[TestMethod]
		public void Create_String_ReturnsStringSchema()
		{
			ComponentFactory.Create(typeof(string))!.Type.Should().Be(JsonSchemaType.String);
		}

		[TestMethod]
		public void Create_Bool_ReturnsBooleanSchema()
		{
			ComponentFactory.Create(typeof(bool))!.Type.Should().Be(JsonSchemaType.Boolean);
		}

		[TestMethod]
		public void Create_Int_ReturnsIntegerSchema()
		{
			ComponentFactory.Create(typeof(int))!.Type.Should().Be(JsonSchemaType.Integer);
		}

		[TestMethod]
		public void Create_Double_ReturnsNumberSchema()
		{
			ComponentFactory.Create(typeof(double))!.Type.Should().Be(JsonSchemaType.Number);
		}

		[TestMethod]
		public void Create_NullableInt_UnwrapsToUnderlyingType()
		{
			var schema = ComponentFactory.Create(typeof(int?));

			schema.Should().NotBeNull();
			schema!.Type.Should().Be(JsonSchemaType.Integer);
		}

		[TestMethod]
		public void Create_Guid_ReturnsUuidFormattedStringSchema()
		{
			var schema = ComponentFactory.Create(typeof(Guid));

			schema.Should().NotBeNull();
			schema!.Type.Should().Be(JsonSchemaType.String);
			schema.Format.Should().Be("uuid");
		}

		[TestMethod]
		public void Create_Enum_ReturnsStringSchemaWithEnumValues()
		{
			var schema = ComponentFactory.Create(typeof(DayOfWeek));

			schema.Should().NotBeNull();
			schema!.Type.Should().Be(JsonSchemaType.String);
			schema.Enum.Should().HaveCount(Enum.GetNames(typeof(DayOfWeek)).Length);
		}

		[TestMethod]
		public void Create_NullType_ReturnsNull()
		{
			ComponentFactory.Create(null).Should().BeNull();
		}

		[TestMethod]
		public void Create_Array_ReturnsNull()
		{
			ComponentFactory.Create(typeof(int[])).Should().BeNull();
		}

		[TestMethod]
		public void Create_ComplexType_ReturnsNull()
		{
			ComponentFactory.Create(typeof(TestFiles.SampleDto)).Should().BeNull();
		}
	}
}
