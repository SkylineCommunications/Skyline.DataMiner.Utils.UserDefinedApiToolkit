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
			task.References = task.References
				.Where(reference => !String.Equals(
					reference.ItemSpec,
					typeof(object).Assembly.Location,
					StringComparison.OrdinalIgnoreCase))
				.ToArray();

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
		public void InstallerTargets_BuildsUdapiProjectsAndCopiesGeneratedFiles()
		{
			var packageDirectory = Path.Combine(_outputPath, "Package");
			var packageBuildDirectory = Path.Combine(packageDirectory, "build");
			var taskDirectory = Path.Combine(packageDirectory, "tasks", "netstandard2.0");
			var childDirectory = Path.Combine(_outputPath, "ChildApi");
			var childProjectPath = Path.Combine(childDirectory, "ChildApi.proj");
			var childScriptPath = Path.ChangeExtension(childProjectPath, ".xml");
			var centralProjectPath = Path.Combine(packageDirectory, "CentralPackage.proj");
			var toolkitTargetSource = Path.Combine(GetTestAssemblyDirectory(), "Skyline.DataMiner.Utils.UserDefinedApiToolkit.targets");
			var installerTargetSource = Path.Combine(GetTestAssemblyDirectory(), "Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer.targets");

			Directory.CreateDirectory(packageBuildDirectory);
			Directory.CreateDirectory(taskDirectory);
			Directory.CreateDirectory(childDirectory);
			File.WriteAllText(childScriptPath, "<DMSScript><Name>Central Test Script</Name></DMSScript>");
			File.Copy(toolkitTargetSource, Path.Combine(packageBuildDirectory, Path.GetFileName(toolkitTargetSource)));
			File.Copy(installerTargetSource, Path.Combine(packageBuildDirectory, Path.GetFileName(installerTargetSource)));

			foreach (var file in Directory.GetFiles(GetBuildTaskOutputDirectory()))
			{
				File.Copy(file, Path.Combine(taskDirectory, Path.GetFileName(file)));
			}

			var references = GetReferencePaths()
				.Select(path => $"<ReferencePath Include=\"{Escape(path)}\" />");
			File.WriteAllText(childProjectPath, $@"<Project ToolsVersion=""Current"">
	<PropertyGroup>
		<OutDir>bin\</OutDir>
		<TargetPath>{Escape(Assembly.GetExecutingAssembly().Location)}</TargetPath>
		<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include=""Skyline.DataMiner.Utils.UserDefinedApiToolkit"" />
		<PackageVersion Include=""Skyline.DataMiner.Utils.UserDefinedApiToolkit"" Version=""9.8.7"" />
		{String.Join(Environment.NewLine, references)}
	</ItemGroup>
	<Target Name=""ResolveReferences"" />
	<Target Name=""Build"">
		<WriteLinesToFile
			File=""{Escape(childScriptPath)}""
			Lines=""&lt;DMSScript&gt;&lt;Name&gt;Central Test Script&lt;/Name&gt;&lt;/DMSScript&gt;""
			Overwrite=""true"" />
	</Target>
	<Import Project=""{Escape(Path.Combine(packageBuildDirectory, Path.GetFileName(toolkitTargetSource)))}"" />
</Project>");

			File.WriteAllText(centralProjectPath, $@"<Project ToolsVersion=""Current"">
	<PropertyGroup>
		<DataMinerType>Package</DataMinerType>
		<OutDir>{Escape(Path.Combine(packageDirectory, "bin") + Path.DirectorySeparatorChar)}</OutDir>
	</PropertyGroup>
	<ItemGroup>
		<UdapiProject Include=""..\ChildApi\*.proj"" />
	</ItemGroup>
	<Target Name=""Build"">
		<Error
			Condition=""!Exists('{Escape(Path.Combine(packageDirectory, "SetupContent", "UDAPI", "ChildApi.udapi.json"))}')""
			Text=""UDAPI installer metadata was not copied before the package build."" />
	</Target>
	<Import Project=""{Escape(Path.Combine(packageBuildDirectory, Path.GetFileName(installerTargetSource)))}"" />
</Project>");

			var process = Process.Start(new ProcessStartInfo
			{
				FileName = "dotnet",
				Arguments = $"msbuild \"{centralProjectPath}\" /t:Build /nologo /v:minimal",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
			});
			process.Should().NotBeNull();

			var output = process!.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd();
			process.WaitForExit();

			process.ExitCode.Should().Be(0, output);

			var generatedFile = Path.Combine(packageDirectory, "SetupContent", "UDAPI", "ChildApi.udapi.json");
			File.Exists(generatedFile).Should().BeTrue();
			var generatedContent = File.ReadAllText(generatedFile);
			generatedContent.Should().Contain("\"ToolkitVersion\":\"9.8.7\"");
			generatedContent.Should().Contain("\"ScriptName\":\"Central Test Script\"");
		}

		[TestMethod]
		public void InstallerTargets_RejectsProjectWithoutToolkit()
		{
			var packageDirectory = Path.Combine(_outputPath, "Package");
			var packageBuildDirectory = Path.Combine(packageDirectory, "build");
			var childDirectory = Path.Combine(_outputPath, "ChildApi");
			var childProjectPath = Path.Combine(childDirectory, "ChildApi.proj");
			var centralProjectPath = Path.Combine(packageDirectory, "CentralPackage.proj");
			var installerTargetSource = Path.Combine(GetTestAssemblyDirectory(), "Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer.targets");

			Directory.CreateDirectory(packageBuildDirectory);
			Directory.CreateDirectory(childDirectory);
			File.Copy(installerTargetSource, Path.Combine(packageBuildDirectory, Path.GetFileName(installerTargetSource)));

			File.WriteAllText(childProjectPath, @"<Project ToolsVersion=""Current"">
	<Target Name=""Build"" />
</Project>");
			File.WriteAllText(centralProjectPath, $@"<Project ToolsVersion=""Current"">
	<PropertyGroup>
		<DataMinerType>Package</DataMinerType>
	</PropertyGroup>
	<ItemGroup>
		<UdapiProject Include=""..\ChildApi\ChildApi.proj"" />
	</ItemGroup>
	<Target Name=""Build"" />
	<Import Project=""{Escape(Path.Combine(packageBuildDirectory, Path.GetFileName(installerTargetSource)))}"" />
</Project>");

			var output = RunMsBuild(centralProjectPath, out var exitCode);

			exitCode.Should().NotBe(0, output);
			output.Should().Contain("Every UdapiProject must reference the Skyline.DataMiner.Utils.UserDefinedApiToolkit package.");
		}

		[TestMethod]
		public void InstallerTargets_RejectsProjectWithoutScriptXml()
		{
			var packageDirectory = Path.Combine(_outputPath, "Package");
			var packageBuildDirectory = Path.Combine(packageDirectory, "build");
			var childDirectory = Path.Combine(_outputPath, "ChildApi");
			var childProjectPath = Path.Combine(childDirectory, "ChildApi.proj");
			var centralProjectPath = Path.Combine(packageDirectory, "CentralPackage.proj");
			var toolkitTargetSource = Path.Combine(GetTestAssemblyDirectory(), "Skyline.DataMiner.Utils.UserDefinedApiToolkit.targets");
			var installerTargetSource = Path.Combine(GetTestAssemblyDirectory(), "Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer.targets");

			Directory.CreateDirectory(packageBuildDirectory);
			Directory.CreateDirectory(childDirectory);
			File.Copy(toolkitTargetSource, Path.Combine(packageBuildDirectory, Path.GetFileName(toolkitTargetSource)));
			File.Copy(installerTargetSource, Path.Combine(packageBuildDirectory, Path.GetFileName(installerTargetSource)));

			File.WriteAllText(childProjectPath, $@"<Project ToolsVersion=""Current"">
	<Target Name=""Build"" />
	<Import Project=""{Escape(Path.Combine(packageBuildDirectory, Path.GetFileName(toolkitTargetSource)))}"" />
</Project>");
			File.WriteAllText(centralProjectPath, $@"<Project ToolsVersion=""Current"">
	<PropertyGroup>
		<DataMinerType>Package</DataMinerType>
	</PropertyGroup>
	<ItemGroup>
		<UdapiProject Include=""..\ChildApi\ChildApi.proj"" />
	</ItemGroup>
	<Target Name=""Build"" />
	<Import Project=""{Escape(Path.Combine(packageBuildDirectory, Path.GetFileName(installerTargetSource)))}"" />
</Project>");

			var output = RunMsBuild(centralProjectPath, out var exitCode);

			exitCode.Should().NotBe(0, output);
			output.Should().Contain("must have a matching Automation script XML file next to the project file.");
		}

		[TestMethod]
		public void InstallerTargets_RejectsNonPackageProject()
		{
			var packageDirectory = Path.Combine(_outputPath, "Package");
			var packageBuildDirectory = Path.Combine(packageDirectory, "build");
			var centralProjectPath = Path.Combine(packageDirectory, "NonPackage.proj");
			var installerTargetSource = Path.Combine(GetTestAssemblyDirectory(), "Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer.targets");

			Directory.CreateDirectory(packageBuildDirectory);
			File.Copy(installerTargetSource, Path.Combine(packageBuildDirectory, Path.GetFileName(installerTargetSource)));
			File.WriteAllText(centralProjectPath, $@"<Project ToolsVersion=""Current"">
	<PropertyGroup>
		<DataMinerType>Automation</DataMinerType>
	</PropertyGroup>
	<Target Name=""Build"" />
	<Import Project=""{Escape(Path.Combine(packageBuildDirectory, Path.GetFileName(installerTargetSource)))}"" />
</Project>");

			var output = RunMsBuild(centralProjectPath, out var exitCode);

			exitCode.Should().NotBe(0, output);
			output.Should().Contain("can only be used in a DataMiner package project");
		}

		private static string RunMsBuild(string projectPath, out int exitCode)
		{
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
			exitCode = process.ExitCode;
			return output;
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
