namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Microsoft.OpenApi;

	internal class ComponentRegistry
	{
		private readonly OpenApiDocument _doc;
		private readonly HashSet<string> _registered = new HashSet<string>(StringComparer.Ordinal);

		internal ComponentRegistry(OpenApiDocument doc)
		{
			_doc = doc;
		}

		/// <summary>
		/// Returns a $ref for complex types (and registers them in components/schemas),
		/// or an inline schema for primitives.
		/// </summary>
		/// <param name="type">The type to get or register an OpenAPI schema for.</param>
		/// <returns>The OpenAPI schema for <paramref name="type"/>, or <c>null</c> if <paramref name="type"/> is <c>null</c>.</returns>
		public IOpenApiSchema? GetOrRegisterSchema(Type type)
		{
			if (type is null) return null;

			// Collections — return array schema, recurse for the element type
			if (type.IsArray || IsGenericCollection(type))
			{
				var elementType = TypeHelper.GetElementType(type);
				return new OpenApiSchema
				{
					Type = JsonSchemaType.Array,
					Items = GetOrRegisterSchema(elementType),
				};
			}

			// Leaf types (primitives, enums, guid, timespan, sdm object references) — inline, no registration needed.
			var leafSchema = ComponentFactory.Create(type);
			if (leafSchema != null) return leafSchema;

			if (!type.IsClass) return null;

			// Reserve the slot before recursing into properties, so self-referencing types resolve to a $ref instead of recursing infinitely.
			if (type.FullName != null && !_registered.Add(type.FullName))
			{
				return new OpenApiSchemaReference(type.Name);
			}

			var schema = BuildComplexSchema(type);
			if (type.FullName != null)
			{
				_doc.Components!.Schemas![type.Name] = schema;
			}

			return new OpenApiSchemaReference(type.Name);
		}

		private OpenApiSchema BuildComplexSchema(Type type)
		{
			var schema = new OpenApiSchema
			{
				Type = JsonSchemaType.Object,
				Properties = new Dictionary<string, IOpenApiSchema>(),
			};

			foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var propSchema = GetOrRegisterSchema(property.PropertyType);
				if (propSchema is null)
				{
					continue;
				}

				schema.Properties[property.Name] = propSchema;
			}

			return schema;
		}

		private bool IsGenericCollection(Type type)
		{
			return type.IsGenericType &&
				   type.GetInterfaces()
					   .Any(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "IEnumerable`1");
		}
	}
}
