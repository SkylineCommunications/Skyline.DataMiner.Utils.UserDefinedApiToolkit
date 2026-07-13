namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Xml.Linq;

	internal class ControllerUnit
	{
		internal ControllerUnit(Type controllerType, XDocument? xmlDocs)
		{
			ControllerType = controllerType;
			XmlDocs = xmlDocs;
		}

		public Type ControllerType { get; }

		public XDocument? XmlDocs { get; }

		public string GetRoute()
		{
			var attr = ControllerType.GetCustomAttributesData()
				.FirstOrDefault(a => TypeHelper.GetAttributeName(a) == "Route" ||
									 TypeHelper.GetAttributeName(a) == "RouteAttribute");

			return attr?.ConstructorArguments[0].Value as string ?? "/";
		}

		/// <summary>
		/// Gets the combined route for a specific action method: the controller's <c>[Route]</c>
		/// template joined with the method's <c>[Http*]</c> template (e.g. controller
		/// <c>"v1/items"</c> + method <c>"{id}"</c> → <c>"v1/items/{id}"</c>). If the method has no
		/// template, only the controller route is returned.
		/// </summary>
		/// <param name="method">The action method to get the combined route for.</param>
		/// <returns>The combined route template for the given <paramref name="method"/>.</returns>
		public string GetRoute(MethodInfo method)
		{
			var controllerRoute = GetRoute();
			var methodTemplate = GetMethodTemplate(method);
			return CombineRoutes(controllerRoute, methodTemplate);
		}

		private static string? GetMethodTemplate(MethodInfo method)
		{
			var attr = method.GetCustomAttributesData()
				.FirstOrDefault(a => IsHttpMethodAttributeName(TypeHelper.GetAttributeName(a)));

			if (attr is null || attr.ConstructorArguments.Count == 0)
			{
				return null;
			}

			return attr.ConstructorArguments[0].Value as string;
		}

		private static bool IsHttpMethodAttributeName(string? name)
		{
			switch (name)
			{
				case "HttpGetAttribute":
				case "HttpPostAttribute":
				case "HttpPutAttribute":
				case "HttpDeleteAttribute":
				case "HttpPatchAttribute":
				case "HttpHeadAttribute":
				case "HttpOptionsAttribute":
					return true;
				default:
					return false;
			}
		}

		private static string CombineRoutes(string? controllerTemplate, string? methodTemplate)
		{
			var left = controllerTemplate?.Trim('/') ?? String.Empty;
			var right = methodTemplate?.Trim('/') ?? String.Empty;

			if (String.IsNullOrEmpty(right))
			{
				return left;
			}

			if (String.IsNullOrEmpty(left))
			{
				return right;
			}

			return $"{left}/{right}";
		}
	}
}
