namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Reflection;

	using Microsoft.OpenApi;

	internal class OperationProvider
	{
		private readonly ComponentRegistry _components;

		public OperationProvider(ComponentRegistry components)
		{
			_components = components;
		}

		public bool TryGetOperations(ControllerUnit unit, MethodInfo method, out HttpMethod httpMethod, out OpenApiOperation operation)
		{
			httpMethod = HttpMethod.Get;
			operation = new OpenApiOperation
			{
				Tags = new HashSet<OpenApiTagReference>(),
			};

			if (!TryGetHttpMethod(method, out httpMethod))
			{
				return false;
			}

			var controllerName = unit.ControllerType.Name;
			if (controllerName.EndsWith("Controller"))
			{
				controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);
			}

			operation.Tags.Add(new OpenApiTagReference(controllerName));

			var producesTypes = GetContentTypes(unit, method, "Produces");
			var consumesTypes = GetContentTypes(unit, method, "Consumes");

			var docs = GetMethodDocs(unit, method);
			operation.Responses = GetResponses(method, docs, producesTypes);
			operation.Parameters = GetParameters(method, docs);
			operation.RequestBody = GetRequestBody(method, consumesTypes);

			if (!String.IsNullOrEmpty(docs?.Summary))
			{
				operation.Summary = docs!.Summary;
			}

			if (!String.IsNullOrEmpty(docs?.Example))
			{
				operation.Description = docs!.Example;
			}

			return true;
		}

		private bool TryGetHttpMethod(MethodInfo method, out HttpMethod httpMethod)
		{
			httpMethod = HttpMethod.Get;

			foreach (var attr in method.GetCustomAttributesData())
			{
				switch (attr.AttributeType.Name)
				{
					case "HttpGetAttribute": httpMethod = HttpMethod.Get; return true;
					case "HttpPostAttribute": httpMethod = HttpMethod.Post; return true;
					case "HttpPutAttribute": httpMethod = HttpMethod.Put; return true;
					case "HttpDeleteAttribute": httpMethod = HttpMethod.Delete; return true;
					case "HttpPatchAttribute": httpMethod = new HttpMethod("PATCH"); return true;
					case "HttpHeadAttribute": httpMethod = HttpMethod.Head; return true;
					case "HttpOptionsAttribute": httpMethod = HttpMethod.Options; return true;
				}
			}

			return false;
		}

		private OpenApiResponses GetResponses(MethodInfo method, MethodDocs? docs, IReadOnlyList<string> contentTypes)
		{
			var responses = new OpenApiResponses();

			// Priority 1: explicit [ProducesResponseType] attributes
			var producesAttrs = method.GetCustomAttributesData()
				.Where(a => a.AttributeType.Name == "ProducesResponseType" ||
							a.AttributeType.Name == "ProducesResponseTypeAttribute")
				.ToList();

			foreach (var attr in producesAttrs)
			{
				if (attr.ConstructorArguments.Count == 2)
				{
					// [ProducesResponseType(typeof(MyResponse), 200)]
					var responseType = attr.ConstructorArguments[0].Value as Type;
					var statusCode = (int)attr.ConstructorArguments[1].Value!;
					AddResponse(responses, statusCode, responseType, contentTypes);
				}
				else
				{
					// [ProducesResponseType(200)]
					var statusCode = (int)attr.ConstructorArguments[0].Value!;
					AddResponse(responses, statusCode, null, contentTypes);
				}
			}

			if (responses.Count > 0)
			{
				return responses;
			}

			// Priority 2: read generic type args from ApiResult<TSuccess> / ApiResult<TSuccess, TError>
			var (successType, errorType) = TypeHelper.GetResultTypes(method.ReturnType);

			if (successType != null) AddResponse(responses, 200, successType, contentTypes);
			if (errorType != null) AddResponse(responses, 400, errorType, contentTypes);

			return responses;
		}

		private void AddResponse(OpenApiResponses responses, int statusCode, Type? responseType, IReadOnlyList<string> contentTypes)
		{
			var response = new OpenApiResponse { Description = String.Empty };

			if (responseType != null)
			{
				var schema = _components.GetOrRegisterSchema(responseType);
				if (schema != null)
				{
					response.Content = contentTypes.ToDictionary(ct => ct, ct => (IOpenApiMediaType)new OpenApiMediaType { Schema = schema });
				}
			}

			responses[statusCode.ToString()] = response;
		}

		private IList<IOpenApiParameter>? GetParameters(MethodInfo method, MethodDocs? docs)
		{
			var parameters = new List<IOpenApiParameter>();

			foreach (var param in method.GetParameters())
			{
				// [FromBody] is handled as request body, not a parameter
				if (TypeHelper.HasAttribute(param, "FromBody"))
				{
					continue;
				}

				// Only include parameters with an explicit From* attribute
				// (skips framework-injected params like IEngine, ApiContext)
				var location = GetParameterLocation(param);
				if (location is null)
				{
					continue;
				}

				var openApiParam = new OpenApiParameter
				{
					Name = param.Name,
					In = location,
					Required = !param.HasDefaultValue,
					Schema = _components.GetOrRegisterSchema(param.ParameterType),
				};

				if (docs?.Parameters?.TryGetValue(param.Name!, out var description) ?? false)
				{
					openApiParam.Description = description;
				}

				parameters.Add(openApiParam);
			}

			return parameters.Count > 0 ? parameters : null;
		}

		private ParameterLocation? GetParameterLocation(ParameterInfo param)
		{
			foreach (var attr in param.GetCustomAttributesData())
			{
				switch (attr.AttributeType.Name)
				{
					case "FromQueryAttribute": return ParameterLocation.Query;
					case "FromHeaderAttribute": return ParameterLocation.Header;
					case "FromRouteAttribute": return ParameterLocation.Path;
				}
			}

			return null;
		}

		private IOpenApiRequestBody? GetRequestBody(MethodInfo method, IReadOnlyList<string> contentTypes)
		{
			var bodyParam = method.GetParameters()
				.SingleOrDefault(p => TypeHelper.HasAttribute(p, "FromBody"));

			if (bodyParam is null)
			{
				return null;
			}

			var schema = _components.GetOrRegisterSchema(bodyParam.ParameterType);
			if (schema is null)
			{
				return null;
			}

			return new OpenApiRequestBody
			{
				Required = !bodyParam.HasDefaultValue,
				Content = contentTypes.ToDictionary(ct => ct, ct => (IOpenApiMediaType)new OpenApiMediaType { Schema = schema }),
			};
		}

		private IReadOnlyList<string> GetContentTypes(ControllerUnit unit, MethodInfo method, string attributeName)
		{
			// Method-level takes priority over controller-level
			var attr = method.GetCustomAttributesData()
							 .FirstOrDefault(a => a.AttributeType.Name == attributeName ||
												  a.AttributeType.Name == $"{attributeName}Attribute")
					  ?? unit.ControllerType.GetCustomAttributesData()
							 .FirstOrDefault(a => a.AttributeType.Name == attributeName ||
												  a.AttributeType.Name == $"{attributeName}Attribute");

			if (attr is null)
			{
				return new[] { "application/json" };
			}

			var types = new List<string> { (string)attr.ConstructorArguments[0].Value! };

			if (attr.ConstructorArguments.Count > 1 &&
				attr.ConstructorArguments[1].Value is IReadOnlyCollection<CustomAttributeTypedArgument> extras)
			{
				types.AddRange(extras.Select(e => (string)e.Value!));
			}

			return types;
		}

		private MethodDocs? GetMethodDocs(ControllerUnit unit, MethodInfo method)
		{
			if (unit.XmlDocs is null)
			{
				return null;
			}

			var memberId = BuildMemberId(method);
			var memberElement = unit.XmlDocs
				.Descendants("member")
				.FirstOrDefault(m => m.Attribute("name")?.Value == memberId);

			if (memberElement is null)
			{
				return null;
			}

			return new MethodDocs
			{
				Summary = memberElement.Element("summary")?.Value?.Trim(),
				Example = memberElement.Element("example")?.Value?.Trim(),
				Parameters = memberElement.Elements("param")
					.Where(p => p.Attribute("name") != null)
					.ToDictionary(
						p => p.Attribute("name")!.Value,
						p => p.Value.Trim()),
			};
		}

		private string BuildMemberId(MethodInfo method)
		{
			var typeName = method.DeclaringType!.FullName;
			var paramTypes = method.GetParameters()
				.Select(p => p.ParameterType.FullName ?? p.ParameterType.Name);

			var paramStr = String.Join(",", paramTypes);
			return String.IsNullOrEmpty(paramStr)
				? $"M:{typeName}.{method.Name}"
				: $"M:{typeName}.{method.Name}({paramStr})";
		}

		private class MethodDocs
		{
			public string? Summary { get; set; }
			public string? Example { get; set; }
			public Dictionary<string, string>? Parameters { get; set; }
		}
	}
}
