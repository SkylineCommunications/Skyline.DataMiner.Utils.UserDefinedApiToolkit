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

			var references = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => !a.IsDynamic && !String.IsNullOrEmpty(a.Location))
				.Select(a => a.Location)
				.Distinct()
				.Select(location => (ITaskItem)new TaskItem(location))
				.ToArray();

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

		[TestMethod]
		public void Execute_ValidControllerAssembly_YamlFormat_GeneratesOpenApiFileWithControllerRoute()
		{
			var task = CreateTask(_outputPath, "yaml");

			var result = task.Execute();

			result.Should().BeTrue();

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

			result.Should().BeTrue();

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
