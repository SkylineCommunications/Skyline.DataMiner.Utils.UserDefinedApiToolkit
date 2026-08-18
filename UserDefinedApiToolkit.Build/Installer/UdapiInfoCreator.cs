namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Installer
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Xml;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Analysis;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Logging;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer.Shared;

	internal class UdapiInfoCreator
	{
		public static UdapiInfo Create(
			XmlDocument script,
			string? toolkitVersion,
			IList<ControllerUnit> controllers,
			MsBuildLogger logger)
		{
			var info = new UdapiInfo
			{
				ToolkitVersion = toolkitVersion ?? "0.0.0",
				ScriptName = script["DMSScript"]["Name"].InnerText,
			};

			var routes = new List<RouteInfo>();
			foreach (var controller in controllers)
			{
				routes.AddRange(GetRoutes(controller));
			}

			info.Routes = routes.ToArray();
			return info;
		}

		private static IEnumerable<RouteInfo> GetRoutes(ControllerUnit controller)
		{
			foreach (var method in controller.ControllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if (!method.TryGetHttpMethod(out _))
				{
					// This method is not an HTTP method, so we skip it.
					continue;
				}

				if (!TryGetRoute(controller, method, out var route))
				{
					continue;
				}

				yield return route;
			}
		}

		private static bool TryGetRoute(ControllerUnit controller, MethodInfo method, out RouteInfo route)
		{
			route = new RouteInfo
			{
				Route = controller.GetRoute(method).Trim('/'),
				Description = controller.GetMethodDocs(method)?.Summary?.Trim('\r').Replace(Environment.NewLine, " ") ?? string.Empty,
				ActionType = "AutomationScript",
				InputType = "RawBody",
			};

			return true;
		}
	}
}
