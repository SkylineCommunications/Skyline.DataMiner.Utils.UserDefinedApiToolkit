namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text.Json.Nodes;

	using Microsoft.OpenApi;

	internal static class ComponentFactory
	{
		// Keyed by FullName so MetadataLoadContext types match correctly
		private static readonly Dictionary<string, OpenApiSchema> _primitives = new Dictionary<string, OpenApiSchema>
		{
			["System.String"] = new OpenApiSchema { Type = JsonSchemaType.String },
			["System.Char"] = new OpenApiSchema { Type = JsonSchemaType.String },
			["System.Boolean"] = new OpenApiSchema { Type = JsonSchemaType.Boolean },
			["System.Byte"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			["System.SByte"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			["System.Int16"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			["System.UInt16"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			["System.Int32"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			["System.UInt32"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			["System.Int64"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64" },
			["System.UInt64"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64" },
			["System.Double"] = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" },
			["System.Single"] = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "float" },
			["System.Decimal"] = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" },
			["System.DateTime"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "date-time" },
		};

		internal static OpenApiSchema? Create(Type? type)
		{
			if (type is null)
			{
				return null;
			}

			// Unwrap Nullable<T> — treat int? the same as int
			if (type.IsGenericType && type.GetGenericTypeDefinition().FullName == "System.Nullable`1")
			{
				type = type.GetGenericArguments()[0];
			}

			if (type.FullName != null && _primitives.TryGetValue(type.FullName, out var primitive))
			{
				return primitive;
			}

			if (TryCreateGuidSchema(type, out var guidSchema)) return guidSchema;
			if (TryCreateTimeSpanSchema(type, out var timeSpanSchema)) return timeSpanSchema;
			if (TryCreateEnumSchema(type, out var enumSchema)) return enumSchema;
			if (TryCreateSdmObjectReferenceSchema(type, out var sdmSchema)) return sdmSchema;
			if (TryCreateIEnumerableSchema(type, out var enumerableSchema)) return enumerableSchema;
			if (TryCreateComplexSchema(type, out var complexSchema)) return complexSchema;

			return null;
		}

		private static bool TryCreateGuidSchema(Type type, out OpenApiSchema? schema)
		{
			if (type.FullName != "System.Guid")
			{
				schema = null;
				return false;
			}

			schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" };
			return true;
		}

		private static bool TryCreateTimeSpanSchema(Type type, out OpenApiSchema? schema)
		{
			if (type.FullName != "System.TimeSpan")
			{
				schema = null;
				return false;
			}

			schema = new OpenApiSchema
			{
				Type = JsonSchemaType.String,
				Description = "TimeSpan formatted as hh:mm:ss.fffffff",
				Example = JsonValue.Create("00:00:00.0000000"),
			};
			return true;
		}

		private static bool TryCreateEnumSchema(Type type, out OpenApiSchema? schema)
		{
			if (!type.IsEnum)
			{
				schema = null;
				return false;
			}

			schema = new OpenApiSchema
			{
				Type = JsonSchemaType.String,
				Enum = type.GetFields(BindingFlags.Public | BindingFlags.Static)
						   .Select(f => (JsonNode)JsonValue.Create(f.Name)!)
						   .ToList(),
			};
			return true;
		}

		private static bool TryCreateSdmObjectReferenceSchema(Type type, out OpenApiSchema? schema)
		{
			if (!TypeHelper.IsSdmObjectReference(type))
			{
				schema = null;
				return false;
			}

			var referencedType = type.GetGenericArguments()[0];
			schema = TypeHelper.HasDomStorageAttribute(referencedType)
				? new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" }
				: new OpenApiSchema { Type = JsonSchemaType.String };

			return true;
		}

		private static bool TryCreateIEnumerableSchema(Type type, out OpenApiSchema? schema)
		{
			if (type.IsArray)
			{
				schema = new OpenApiSchema
				{
					Type = JsonSchemaType.Array,
					Items = Create(type.GetElementType()),
				};
				return true;
			}

			var ienumerableInterface = type.IsGenericType
				? type.GetInterfaces()
					  .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "IEnumerable`1")
				: null;

			if (ienumerableInterface != null)
			{
				schema = new OpenApiSchema
				{
					Type = JsonSchemaType.Array,
					Items = Create(ienumerableInterface.GetGenericArguments()[0]),
				};
				return true;
			}

			schema = null;
			return false;
		}

		private static bool TryCreateComplexSchema(Type type, out OpenApiSchema? schema)
		{
			if (!type.IsClass)
			{
				schema = null;
				return false;
			}

			schema = new OpenApiSchema
			{
				Type = JsonSchemaType.Object,
				Properties = new Dictionary<string, IOpenApiSchema>(),
			};

			foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var propSchema = Create(property.PropertyType);
				if (propSchema is null)
				{
					continue;
				}

				schema.Properties[property.Name] = propSchema;
			}

			return true;
		}
	}
}
