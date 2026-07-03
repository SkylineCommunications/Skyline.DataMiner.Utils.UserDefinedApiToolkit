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

		public void HandleController(OpenApiDocument doc, ControllerUnit unit, Action<string>? logMethod = null)
		{
			var pathItem = new OpenApiPathItem();
			foreach (var method in unit.ControllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if (!_operationProvider.TryGetOperations(unit, method, out var httpMethod, out var operation))
				{
					continue;
				}

				logMethod?.Invoke($"Registering {httpMethod.Method} /{unit.GetRoute().Trim('/')} → {unit.ControllerType.Name}.{method.Name}");

				pathItem.AddOperation(httpMethod, operation);
			}

			if (pathItem.Operations is null || pathItem.Operations.Count == 0)
			{
				return;
			}

			var path = $"/{unit.GetRoute().Trim('/')}";
			if (doc.Paths.ContainsKey(path))
			{
				foreach (var operation in pathItem.Operations)
				{
					doc.Paths[path].Operations![operation.Key] = operation.Value;
				}
			}
			else
			{
				doc.Paths.Add(path, pathItem);
			}
		}
	}
}
