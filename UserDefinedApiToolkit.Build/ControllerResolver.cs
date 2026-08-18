namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml.Linq;

	using Skyline.DataMiner.Utils.SecureCoding.SecureIO;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Logging;

	internal class ControllerResolver : IDisposable
	{
		private readonly string _targetPath;
		private readonly string[] _references;
		private readonly string? _documentationFile;

		private readonly MetadataLoadContext _mlc;

		public ControllerResolver(
			string targetPath,
			IEnumerable<string> references,
			string? documentationFile = null,
			IBuildLogger? logger = null)
		{
			_targetPath = targetPath ?? throw new ArgumentNullException(nameof(targetPath));
			_references = references.ToArray() ?? throw new ArgumentNullException(nameof(references));

			var allPaths = references.Append(targetPath);
			var resolver = new PathAssemblyResolver(allPaths);
			_mlc = new MetadataLoadContext(resolver);

			_documentationFile = documentationFile;
			Logger = logger ?? new NullBuildLogger();
		}

		public IBuildLogger Logger { get; }

		public IList<ControllerUnit> Resolve()
		{
			var assembly = _mlc.LoadFromAssemblyPath(_targetPath);
			var xmlDocs = LoadXmlDocs(_documentationFile);

			var controllers = assembly.GetTypes()
				.Where(t => t.IsClass &&
							!t.IsAbstract &&
							TypeHelper.HasAttribute(t, "ApiController") &&
							TypeHelper.HasAttribute(t, "Route") &&
							t.BaseType?.Name == "ControllerBase")
				.Select(t => new ControllerUnit(t, xmlDocs))
				.ToList();

			return controllers;
		}

		private static XDocument? LoadXmlDocs(string? documentationFile)
		{
			if (String.IsNullOrEmpty(documentationFile) || !documentationFile.IsPathValid() || !File.Exists(documentationFile))
			{
				return null;
			}

			return XDocument.Load(documentationFile);
		}

		public void Dispose()
		{
			_mlc?.Dispose();
		}
	}
}
