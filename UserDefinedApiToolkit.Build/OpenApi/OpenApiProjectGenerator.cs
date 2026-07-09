namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml.Linq;

	using Microsoft.OpenApi;

	/// <summary>
	/// Builds an <see cref="OpenApiDocument"/> for a compiled user-defined API assembly and
	/// formats it as JSON or YAML.
	/// </summary>
	/// <remarks>
	/// This class contains the actual OpenAPI generation logic and has no dependency on
	/// MSBuild (Microsoft.Build.Framework/Utilities). <see cref="OpenApiTask"/> is a thin
	/// adapter around it that wires up MSBuild task properties and logging; keeping the two
	/// separate allows the generation logic to be used and unit tested independently of the
	/// MSBuild task infrastructure.
	/// </remarks>
	internal static class OpenApiProjectGenerator
	{
		/// <summary>
		/// Loads the target assembly (and its references) through a <see cref="MetadataLoadContext"/>,
		/// finds its user-defined API controllers, and builds the resulting <see cref="OpenApiDocument"/>.
		/// </summary>
		/// <param name="targetPath">Path to the compiled assembly to analyze.</param>
		/// <param name="references">Paths to the assemblies referenced by <paramref name="targetPath"/>.</param>
		/// <param name="documentationFile">Optional path to the assembly's XML documentation file.</param>
		/// <param name="projectName">Used as the OpenAPI document title.</param>
		/// <param name="projectVersion">Used as the OpenAPI document version.</param>
		/// <param name="log">Optional callback used to report progress/diagnostics.</param>
		/// <returns>The generated <see cref="OpenApiDocument"/> for the assembly's user-defined API controllers.</returns>
		public static OpenApiDocument CreateDocument(
			string targetPath,
			IEnumerable<string> references,
			string? documentationFile,
			string projectName,
			string projectVersion,
			IBuildLogger? log = null)
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

			var doc = OpenApiGenerator.Create(controllers, xmlDocs, log);
			doc.Info.Title = projectName ?? "User Defined API";
			doc.Info.Version = projectVersion ?? "1.0.0";

			return doc;
		}

		/// <summary>
		/// Serializes an <see cref="OpenApiDocument"/> as either JSON or YAML.
		/// </summary>
		/// <param name="doc">The document to serialize.</param>
		/// <param name="format">"json" for JSON output; anything else defaults to YAML.</param>
		/// <returns>The output file name and its serialized content.</returns>
		public static (string FileName, string Content) FormatDocument(OpenApiDocument doc, string format)
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

		private static XDocument? LoadXmlDocs(string? documentationFile)
		{
			if (String.IsNullOrEmpty(documentationFile) || !File.Exists(documentationFile))
			{
				return null;
			}

			return XDocument.Load(documentationFile);
		}
	}
}
