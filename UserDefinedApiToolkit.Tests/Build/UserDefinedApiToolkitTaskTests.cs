namespace UserDefinedApiToolkit.Tests.Build
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	using FluentAssertions;

	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build;

	[TestClass]
	public sealed class UserDefinedApiToolkitTaskTests
	{
		private string _outputPath = string.Empty;
		private string _scriptXmlPath = string.Empty;

		[TestInitialize]
		public void TestInitialize()
		{
			_outputPath = Path.Combine(Path.GetTempPath(), "UserDefinedApiToolkit.Tests.Installer", Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(_outputPath);
			_scriptXmlPath = Path.Combine(_outputPath, "Script.xml");
			File.WriteAllText(_scriptXmlPath, "<DMSScript><Name>Test Script</Name></DMSScript>");
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (Directory.Exists(_outputPath))
			{
				Directory.Delete(_outputPath, true);
			}
		}

		[TestMethod]
		public void Execute_GeneratesNamedInstallerFile()
		{
			var buildEngine = new BuildEngineStub();
			var task = CreateTask(_outputPath, "Example API.udapi.json", buildEngine);

			var result = task.Execute();

			result.Should().BeTrue(String.Join(Environment.NewLine, buildEngine.Errors.Select(error => error.Message)));

			var filePath = Path.Combine(_outputPath, "Example API.udapi.json");
			File.Exists(filePath).Should().BeTrue();
			var content = File.ReadAllText(filePath);
			content.Should().Contain("\"ToolkitVersion\":\"1.2.3\"");
			content.Should().Contain("\"ScriptName\":\"Test Script\"");
			content.Should().Contain("\"Route\":\"v1/installer/{id}\"");
			content.Should().NotContain("\"Route\":\"v1/installer\"");
		}

		[TestMethod]
		public void ImportedTargets_UsesCentralPackageVersionAndCopiesToEveryPackage()
		{
			var projectDirectory = Path.Combine(_outputPath, "Consumer");
			var relativeInstallDirectory = Path.Combine(_outputPath, "RelativeInstall");
			var absoluteInstallDirectory = Path.Combine(_outputPath, "AbsoluteInstall");
			var relativeInstallPath = @"..\RelativeInstall";
			Directory.CreateDirectory(projectDirectory);

			var projectPath = Path.Combine(projectDirectory, "InstallerTargetTest.proj");
			var scriptPath = Path.ChangeExtension(projectPath, ".xml");
			File.WriteAllText(scriptPath, "<DMSScript><Name>Test Script</Name></DMSScript>");

			var targetDirectory = Path.Combine(_outputPath, "package", "build");
			var taskDirectory = Path.Combine(_outputPath, "package", "tasks", "netstandard2.0");
			Directory.CreateDirectory(targetDirectory);
			Directory.CreateDirectory(taskDirectory);

			var targetSource = Path.Combine(GetTestAssemblyDirectory(), "Skyline.DataMiner.Utils.UserDefinedApiToolkit.targets");
			File.Copy(targetSource, Path.Combine(targetDirectory, Path.GetFileName(targetSource)));

			var taskAssemblyDirectory = GetBuildTaskOutputDirectory();
			foreach (var file in Directory.GetFiles(taskAssemblyDirectory!))
			{
				File.Copy(file, Path.Combine(taskDirectory, Path.GetFileName(file)));
			}

			var references = GetReferencePaths()
				.Select(path => $"<ReferencePath Include=\"{Escape(path)}\" />");
			var projectContent = $@"<Project ToolsVersion=""Current"">
	<PropertyGroup>
		<OutDir>{Escape(_outputPath + Path.DirectorySeparatorChar)}</OutDir>
		<TargetPath>{Escape(Assembly.GetExecutingAssembly().Location)}</TargetPath>
		<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include=""Skyline.DataMiner.Utils.UserDefinedApiToolkit"" />
		<PackageVersion Include=""Skyline.DataMiner.Utils.UserDefinedApiToolkit"" Version=""9.8.7"" />
		<UdapiPackage Include=""{Escape(relativeInstallPath)}"" />
		<UdapiPackage Include=""{Escape(absoluteInstallDirectory)}"" />
		{String.Join(Environment.NewLine, references)}
	</ItemGroup>
	<Target Name=""Build"" />
	<Import Project=""{Escape(Path.Combine(targetDirectory, Path.GetFileName(targetSource)))}"" />
</Project>";
			File.WriteAllText(projectPath, projectContent);

			var process = Process.Start(new ProcessStartInfo
			{
				FileName = "dotnet",
				Arguments = $"msbuild \"{projectPath}\" /t:Build /nologo /v:minimal",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
			});
			process.Should().NotBeNull();

			var output = process!.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd();
			process.WaitForExit();

			process.ExitCode.Should().Be(0, output);

			var outputFileName = "InstallerTargetTest.udapi.json";
			var generatedFile = Path.Combine(_outputPath, outputFileName);
			var relativeCopiedFile = Path.Combine(relativeInstallDirectory, "SetupContent", "UDAPI", outputFileName);
			var absoluteCopiedFile = Path.Combine(absoluteInstallDirectory, "SetupContent", "UDAPI", outputFileName);

			File.Exists(generatedFile).Should().BeTrue();
			File.Exists(relativeCopiedFile).Should().BeTrue();
			File.Exists(absoluteCopiedFile).Should().BeTrue();
			var generatedContent = File.ReadAllText(generatedFile);
			generatedContent.Should().Contain("\"ToolkitVersion\":\"9.8.7\"");
			generatedContent.Should().Contain("\"Route\":\"v1/installer/{id}\"");
			generatedContent.Should().NotContain("\"Route\":\"v1/installer\"");
		}

		private static UserDefinedApiToolkitTask CreateTask(
			string outputPath,
			string outputFileName,
			BuildEngineStub buildEngine)
		{
			return new UserDefinedApiToolkitTask
			{
				BuildEngine = buildEngine,
				OutputPath = outputPath,
				OutputFileName = outputFileName,
				TargetPath = Assembly.GetExecutingAssembly().Location,
				ScriptXmlPath = Path.Combine(outputPath, "Script.xml"),
				ToolkitVersion = "1.2.3",
				References = GetReferencePaths()
					.Select(path => (ITaskItem)new TaskItem(path))
					.ToArray(),
			};
		}

		private static IEnumerable<string> GetReferencePaths()
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.Where(assembly => !assembly.IsDynamic && !String.IsNullOrEmpty(assembly.Location))
				.Select(assembly => assembly.Location)
				.Distinct(StringComparer.OrdinalIgnoreCase);
		}

		private static string GetBuildTaskOutputDirectory()
		{
			var directory = new DirectoryInfo(GetTestAssemblyDirectory());
			var configuration = directory.Parent?.Name ?? "Debug";
			while (directory is not null &&
				   !File.Exists(Path.Combine(directory.FullName, "Skyline.DataMiner.Utils.UserDefinedApiToolkit.slnx")))
			{
				directory = directory.Parent;
			}

			directory.Should().NotBeNull();
			return Path.Combine(directory!.FullName, "UserDefinedApiToolkit.Build", "bin", configuration, "netstandard2.0");
		}

		private static string GetTestAssemblyDirectory()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
		}

		private static string Escape(string value)
		{
			return System.Security.SecurityElement.Escape(value);
		}
	}
}
