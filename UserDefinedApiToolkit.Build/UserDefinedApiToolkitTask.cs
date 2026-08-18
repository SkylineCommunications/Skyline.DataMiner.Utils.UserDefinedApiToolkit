namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml;

	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Installer;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Logging;

	public class UserDefinedApiToolkitTask : Task
	{
		[Required]
		public string OutputPath { get; set; }

		/// <summary>
		/// Gets or sets the generated installer metadata file name.
		/// </summary>
		[Required]
		public string OutputFileName { get; set; }

		/// <summary>
		/// Gets or sets the path to the compiled assembly that will be analyzed for installer metadata
		/// generation.
		/// </summary>
		[Required]
		public string TargetPath { get; set; }

		[Required]
		public string ScriptXmlPath { get; set; }

		[Required]
		public string ToolkitVersion { get; set; }

		/// <summary>
		/// Gets or sets the assembly references required for loading types during installer metadata
		/// generation.
		/// </summary>
		[Required]
		public ITaskItem[] References { get; set; }

		/// <summary>
		/// Gets or sets the path to the XML documentation file containing the assembly's documentation comments.
		/// Used to populate route descriptions in the installer metadata.
		/// </summary>
		public string DocumentationFile { get; set; }

		/// <summary>
		/// Executes the installer-file generation task.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the installer metadata file was generated successfully; otherwise,
		/// <c>false</c>.
		/// </returns>
		public override bool Execute()
		{
			try
			{
				Log.LogMessage(MessageImportance.Normal, $"Generating installer file...");

				var logger = new MsBuildLogger(Log);
				var scriptXml = new XmlDocument();
				scriptXml.Load(ScriptXmlPath);
				if (scriptXml is null)
				{
					Log.LogError($"Failed to load the script XML file '{ScriptXmlPath}'.");
					return false;
				}

				using var resolver = new ControllerResolver(TargetPath, References.Select(r => r.ItemSpec), DocumentationFile, logger);
				var controllers = resolver.Resolve();
				var udapiInfo = UdapiInfoCreator.Create(scriptXml, ToolkitVersion, controllers, logger);

				var outputPath = Path.Combine(OutputPath, OutputFileName);
				Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
				File.WriteAllText(outputPath, JsonConvert.SerializeObject(udapiInfo));

				Log.LogMessage(MessageImportance.Normal, $"Generated installer file with {udapiInfo.Routes.Length} route(s).");
				Log.LogMessage(MessageImportance.Normal, $"Installer file generated at: {outputPath}");
				return true;
			}
			catch (Exception ex)
			{
				// showStackTrace: true ensures the full exception (including stack trace and any
				// inner exceptions) ends up as a build ERROR, not just a low-visibility message —
				// otherwise consumers only see the bare ex.Message on build failure.
				Log.LogErrorFromException(ex, showStackTrace: true, showDetail: true, file: null);
				return false;
			}
		}
	}
}