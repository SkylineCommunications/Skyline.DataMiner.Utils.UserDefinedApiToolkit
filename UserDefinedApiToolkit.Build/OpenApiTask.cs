namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices.ComTypes;
	using System.Xml.Linq;

	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;
	using Microsoft.OpenApi;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi;

	public class OpenApiTask : Task
	{
		[Required]
		public string OutputPath { get; set; }

		/// <summary>
		/// Gets or sets the path to the compiled assembly that will be analyzed for OpenAPI generation.
		/// </summary>
		[Required]
		public string TargetPath { get; set; }

		/// <summary>
		/// Gets or sets the name of the project, used as the OpenAPI document title.
		/// </summary>
		[Required]
		public string ProjectName { get; set; }

		/// <summary>
		/// Gets or sets the version of the project, used as the OpenAPI document version.
		/// </summary>
		[Required]
		public string ProjectVersion { get; set; }

		/// <summary>
		/// Gets or sets the assembly references required for loading types during OpenAPI generation.
		/// </summary>
		[Required]
		public ITaskItem[] References { get; set; }

		/// <summary>
		/// Gets or sets the format of the OpenAPI output file. Default is "yaml".<br/>
		/// Options:<br/>
		/// json - Outputs the OpenAPI specification in JSON format.<br/>
		/// yaml - Outputs the OpenAPI specification in YAML format.
		/// </summary>
		public string Format { get; set; } = "yaml";

		/// <summary>
		/// Gets or sets the path to the XML documentation file containing the assembly's documentation comments.
		/// Used to populate descriptions in the OpenAPI specification.
		/// </summary>
		public string DocumentationFile { get; set; }

		/// <summary>
		/// Executes the OpenAPI generation task.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the OpenAPI file was generated successfully; otherwise, <c>false</c>.
		/// </returns>
		public override bool Execute()
		{
			try
			{
				Log.LogMessage(MessageImportance.Normal, $"Generating OpenAPI file for project '{ProjectName}' version '{ProjectVersion}'...");

				var doc = CreateDocument(
					 TargetPath,
					 References.Select(r => r.ItemSpec),
					 DocumentationFile,
					 ProjectName,
					 ProjectVersion,
					 message => Log.LogMessage(MessageImportance.High, message));

				var (fileName, content) = FormatDocument(doc, Format);

				var outputPath = Path.Combine(OutputPath, "openapi", fileName);
				Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
				File.WriteAllText(outputPath, content);

				Log.LogMessage(MessageImportance.Normal, $"OpenApi file generated at: {outputPath}");
				return true;
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
				Log.LogMessage(MessageImportance.High, $"Error generating OpenAPI file: {ex.Message}");
				Log.LogMessage(MessageImportance.High, ex.StackTrace);
				return false;
			}
		}

		private static OpenApiDocument CreateDocument(
			string targetPath,
			IEnumerable<string> references,
			string? documentationFile,
			string projectName,
			string projectVersion,
			Action<string>? logMethod = null)
		{
			var allPaths = references.Append(targetPath);
			var resolver = new PathAssemblyResolver(allPaths);

			using var mlc = new MetadataLoadContext(resolver);
			var assembly = mlc.LoadFromAssemblyPath(targetPath);
			var xmlDocs = LoadXmlDocs(documentationFile);

			var controllers = assembly.GetTypes()
				.Where(t => t.IsClass &&
							!t.IsAbstract &&
							TypeHelper.HasAttribute(t, "ApiController") &&
							TypeHelper.HasAttribute(t, "Route") &&
							t.BaseType?.Name == "ControllerBase")
				.Select(t => new ControllerUnit(t, xmlDocs))
				.ToList();

			var doc = OpenApiGenerator.Create(controllers, xmlDocs, logMethod);
			doc.Info.Title = projectName ?? "User Defined API";
			doc.Info.Version = projectVersion ?? "1.0.0";

			return doc;
		}

		private static XDocument? LoadXmlDocs(string? documentationFile)
		{
			if (String.IsNullOrEmpty(documentationFile) || !File.Exists(documentationFile))
			{
				return null;
			}

			return XDocument.Load(documentationFile);
		}

		private static (string, string) FormatDocument(OpenApiDocument doc, string format)
		{
			using var sw = new StringWriter();
			if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
			{
				var writer = new OpenApiJsonWriter(sw);
				doc.SerializeAsV3(writer);
				return ("openapi.json", sw.ToString());
			}
			else
			{
				var writer = new OpenApiYamlWriter(sw);
				doc.SerializeAsV3(writer);
				return ("openapi.yaml", sw.ToString());
			}
		}
	}
}