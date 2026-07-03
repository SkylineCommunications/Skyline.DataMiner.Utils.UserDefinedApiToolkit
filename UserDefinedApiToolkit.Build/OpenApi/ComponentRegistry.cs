namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

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

			var schema = ComponentFactory.Create(type);
			if (schema is null) return null;

			// Primitives — inline
			if (schema.Type != JsonSchemaType.Object) return schema;

			// Complex object — register once, always return a $ref
			if (type.FullName != null && _registered.Add(type.FullName))
			{
				_doc.Components!.Schemas![type.Name] = schema;
			}

			return new OpenApiSchemaReference(type.Name);
		}

		private bool IsGenericCollection(Type type)
		{
			return type.IsGenericType &&
				   type.GetInterfaces()
					   .Any(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "IEnumerable`1");
		}
	}
}
