namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi
{
	using System;
	using System.Reflection;

	using Microsoft.OpenApi;

	internal class PathBuilder
	{
		private readonly OperationProvider _operationProvider;

		internal PathBuilder(OperationProvider operationProvider)
		{
			_operationProvider = operationProvider;
		}

		public void HandleController(OpenApiDocument doc, ControllerUnit unit, IBuildLogger? log = null)
		{
			foreach (var method in unit.ControllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if (!_operationProvider.TryGetOperations(unit, method, out var httpMethod, out var operation))
				{
					continue;
				}

				var path = $"/{unit.GetRoute(method).Trim('/')}";

				log?.Log(BuildLogLevel.Detail, $"Registering {httpMethod.Method} {path} → {unit.ControllerType.Name}.{method.Name}");

				if (!doc.Paths.TryGetValue(path, out var existingPathItem) || existingPathItem is not OpenApiPathItem pathItem)
				{
					pathItem = new OpenApiPathItem();
					doc.Paths[path] = pathItem;
				}

				pathItem.AddOperation(httpMethod, operation);
			}
		}
	}
}
