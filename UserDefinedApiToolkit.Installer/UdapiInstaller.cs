namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using Skyline.AppInstaller;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.SecureCoding.SecureIO;
	using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer.Shared;

	internal class UdapiInstaller
	{
		private readonly UserDefinableApiHelper _apiHelper;
		private readonly AppInstaller _installer;
		private readonly string _setupContentDir;

		public UdapiInstaller(AppInstaller installer, IConnection connection)
		{
			_apiHelper = new UserDefinableApiHelper(connection.HandleMessages);
			_installer = installer;
			_setupContentDir = _installer.GetSetupContentDirectory();
		}

		public void InstallUserDefinedApiDefinitions()
		{
			var udapiDir = SecurePath.ConstructSecurePath(_setupContentDir, "UDAPI");
			if (!Directory.Exists(udapiDir))
			{
				_installer.Log($"No user-defined API definitions found at {GetRelativePath(_setupContentDir, udapiDir)}.");
				return;
			}

			var udapiPaths = Directory.GetFiles(udapiDir, "*.udapi.json", SearchOption.AllDirectories);
			foreach (var udapiPath in udapiPaths)
			{
				_installer.Log($"Installing user-defined API definitions from {GetRelativePath(_setupContentDir, udapiPath)}.");
				InstallUserDefinedApiDefinitions(udapiPath);
			}
		}

		private void InstallUserDefinedApiDefinitions(string path)
		{
			var filePath = SecurePath.CreateSecurePath(path);
			if (!File.Exists(filePath))
			{
				_installer.Log($"No user-defined API definitions found at {path}.");
				return;
			}

			var installJsonContent = File.ReadAllText(filePath);
			var installJson = SecureNewtonsoftDeserialization.DeserializeObject<UdapiInfo>(installJsonContent);
			Import(installJson);
		}

		private void Import(UdapiInfo udapi)
		{
			var routes = new List<ApiDefinition>();
			foreach (var route in udapi.Routes)
			{
				var apiDefinition = new ApiDefinition
				{
					Description = route.Description,
					Route = route.Route,
					ActionType = Enum.TryParse<ActionType>(route.ActionType, out var actionType) ? actionType : ActionType.AutomationScript,
					ActionMeta = new AutomationScriptActionMeta
					{
						ScriptName = udapi.ScriptName,
						InputType = Enum.TryParse<InputType>(route.InputType, out var inputType) ? inputType : InputType.RawBody,
					},
				};

				routes.Add(apiDefinition);
			}

			Import(routes);
		}

		private void Import(List<ApiDefinition> routes)
		{
			// Read existing routes in batches to avoid exceeding the maximum number of filter elements
			var filters = routes.Select(r => ApiDefinitionExposers.Route.Equal(r.Route));
			var existingRoutes = new Dictionary<string, ApiDefinition>();
			foreach (var batch in filters.Batch(100))
			{
				var filter = new ORFilterElement<ApiDefinition>(batch.ToArray());
				var existing = _apiHelper.ApiDefinitions.Read(filter);
				existing.ForEach(e => existingRoutes[e.Route] = e);
			}

			// Update existing routes and add new ones
			foreach (var route in routes)
			{
				try
				{
					if (existingRoutes.TryGetValue(route.Route, out var existingRoute))
					{
						// Update existing route
						existingRoute.ActionType = route.ActionType;
						existingRoute.ActionMeta = route.ActionMeta;
						existingRoute.Description = route.Description;
						_apiHelper.ApiDefinitions.Update(existingRoute);
						_installer.Log($"Updated existing route: {route.Route}");
					}
					else
					{
						// Add new route
						_apiHelper.ApiDefinitions.Create(route);
						_installer.Log($"Added new route: {route.Route}");
					}
				}
				catch (CrudFailedException ex)
				{
					_installer.Log($"Failed to import route {route.Route}: {ex.Message}");
					_installer.Log($"Trace Data: {ex.TraceData}");
				}
			}
		}

		private static string GetRelativePath(string basePath, string path)
		{
			return path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) ? path.Substring(basePath.Length) : path;
		}
	}
}
