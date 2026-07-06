namespace UserDefinedApiToolkit.Tests.Build
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	using FluentAssertions;

	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build;

	[TestClass]
	public sealed class OpenApiTaskTests
	{
		private string _outputPath = string.Empty;

		[TestInitialize]
		public void TestInitialize()
		{
			_outputPath = Path.Combine(Path.GetTempPath(), "UserDefinedApiToolkit.Tests.OpenApi", Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(_outputPath);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (Directory.Exists(_outputPath))
			{
				Directory.Delete(_outputPath, true);
			}
		}

		private static OpenApiTask CreateTask(string outputPath, string format = "yaml")
		{
			// The test assembly itself contains [ApiController] fixtures (e.g. SampleController,
			// Controller_GET), so it can be used directly as the target assembly to analyze.
			var targetPath = Assembly.GetExecutingAssembly().Location;
			var references = GetReferencePaths();

			return new OpenApiTask
			{
				BuildEngine = new BuildEngineStub(),
				TargetPath = targetPath,
				OutputPath = outputPath,
				ProjectName = "UserDefinedApiToolkit.Tests",
				ProjectVersion = "1.0.0",
				References = references,
				Format = format,
			};
		}

		private static ITaskItem[] GetReferencePaths()
		{
			// MetadataLoadContext/PathAssemblyResolver needs every assembly that could be
			// referenced (directly or via a custom attribute) by any type in the target
			// assembly to be present on disk in the resolver's path list - otherwise
			// CustomAttributeData.AttributeType throws a NullReferenceException while trying
			// to resolve an attribute's declaring type. Relying on
			// AppDomain.CurrentDomain.GetAssemblies() is fragile because different test hosts
			// (notably the Linux/Mono runner used in CI) load assemblies lazily and in a
			// different order than on Windows, so the exact set of "already loaded" assemblies
			// varies by platform.
			//
			// Instead, deterministically collect every DLL from:
			//  - the test project's own output directory (contains the compiled test/build
			//    assemblies plus every NuGet dependency, since net48 copies them all locally),
			//  - the .NET Framework installation directory (contains mscorlib, System, etc.,
			//    which PathAssemblyResolver also requires but which are never copied locally).
			var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
			var frameworkDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

			var paths = Directory.GetFiles(binDirectory, "*.dll")
				.Concat(Directory.GetFiles(frameworkDirectory, "*.dll"));

			return paths
				// PathAssemblyResolver keys assemblies by simple name, so it throws if two
				// paths resolve to the same simple name. Prefer the copy from the test
				// project's own output directory over the framework directory.
				.GroupBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase)
				.Select(g => g.First())
				.Select(location => (ITaskItem)new TaskItem(location))
				.ToArray();
		}

		private static string GetErrorMessage(OpenApiTask task)
		{
			var buildEngine = (BuildEngineStub)task.BuildEngine;
			var message = String.Join(Environment.NewLine, buildEngine.Errors.Select(e => e.Message));

			// FluentAssertions treats the "because" string as a composite format string,
			// so curly braces in the underlying exception message must be escaped.
			return message.Replace("{", "{{").Replace("}", "}}");
		}

		[TestMethod]
		public void SampleControllerAttributes_ResolveThroughConstructorDeclaringType()
		{
			// Regression guard: on some .NET Framework/Mono test hosts, the
			// System.Reflection.MetadataLoadContext package's CustomAttributeData.AttributeType
			// getter throws NullReferenceException for every custom attribute (confirmed via a
			// prior diagnostic run - a runtime-specific bug, not a missing-assembly issue).
			// TypeHelper.HasAttribute therefore resolves the declaring type via
			// CustomAttributeData.Constructor.DeclaringType instead, which works reliably in
			// that same environment. This verifies that resolution path directly.
			var targetPath = Assembly.GetExecutingAssembly().Location;
			var allPaths = GetReferencePaths().Select(r => r.ItemSpec).Append(targetPath);
			var resolver = new PathAssemblyResolver(allPaths);

			using var mlc = new MetadataLoadContext(resolver);
			var assembly = mlc.LoadFromAssemblyPath(targetPath);
			var controllerType = assembly.GetType(typeof(TestFiles.SampleController).FullName!)!;

			var declaringTypeNames = controllerType.GetCustomAttributesData()
				.Select(a => a.Constructor.DeclaringType?.Name)
				.ToList();

			declaringTypeNames.Should().Contain("ApiControllerAttribute");
			declaringTypeNames.Should().Contain("RouteAttribute");
		}

		[TestMethod]
		public void Execute_ValidControllerAssembly_YamlFormat_GeneratesOpenApiFileWithControllerRoute()
		{
			var task = CreateTask(_outputPath, "yaml");

			var result = task.Execute();

			result.Should().BeTrue(GetErrorMessage(task));

			var filePath = Path.Combine(_outputPath, "openapi", "openapi.yaml");
			File.Exists(filePath).Should().BeTrue();

			var content = File.ReadAllText(filePath);
			content.Should().Contain("/v1/sample");
			content.Should().Contain("/v1/get");
		}

		[TestMethod]
		public void Execute_ValidControllerAssembly_JsonFormat_GeneratesOpenApiFile()
		{
			var task = CreateTask(_outputPath, "json");

			var result = task.Execute();

			result.Should().BeTrue(GetErrorMessage(task));

			var filePath = Path.Combine(_outputPath, "openapi", "openapi.json");
			File.Exists(filePath).Should().BeTrue();

			var content = File.ReadAllText(filePath);
			content.Should().Contain("\"/v1/sample\"");
		}

		[TestMethod]
		public void Execute_SetsProjectNameAndVersionAsDocumentInfo()
		{
			var task = CreateTask(_outputPath, "json");

			task.Execute();

			var filePath = Path.Combine(_outputPath, "openapi", "openapi.json");
			var content = File.ReadAllText(filePath);

			content.Should().Contain("UserDefinedApiToolkit.Tests");
			content.Should().Contain("1.0.0");
		}

		[TestMethod]
		public void Execute_NonExistingTargetAssembly_ReturnsFalseAndLogsError()
		{
			var buildEngine = new BuildEngineStub();
			var task = new OpenApiTask
			{
				BuildEngine = buildEngine,
				TargetPath = Path.Combine(_outputPath, "DoesNotExist.dll"),
				OutputPath = _outputPath,
				ProjectName = "UserDefinedApiToolkit.Tests",
				ProjectVersion = "1.0.0",
				References = Array.Empty<ITaskItem>(),
			};

			var result = task.Execute();

			result.Should().BeFalse();
			buildEngine.Errors.Should().NotBeEmpty();
		}
	}
}
