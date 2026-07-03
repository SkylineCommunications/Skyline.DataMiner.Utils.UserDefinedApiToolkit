namespace UserDefinedApiToolkit.Tests.Build
{
	using System.Collections;
	using System.Collections.Generic;

	using Microsoft.Build.Framework;

	/// <summary>
	/// Minimal <see cref="IBuildEngine"/> stub so <see cref="Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApiTask"/>
	/// can be executed outside of an actual MSBuild invocation. Captures logged errors/warnings for assertions.
	/// </summary>
	internal sealed class BuildEngineStub : IBuildEngine
	{
		public List<BuildErrorEventArgs> Errors { get; } = new List<BuildErrorEventArgs>();

		public List<BuildWarningEventArgs> Warnings { get; } = new List<BuildWarningEventArgs>();

		public List<BuildMessageEventArgs> Messages { get; } = new List<BuildMessageEventArgs>();

		public bool ContinueOnError => false;

		public int LineNumberOfTaskNode => 0;

		public int ColumnNumberOfTaskNode => 0;

		public string ProjectFileOfTaskNode => string.Empty;

		public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
		{
			return true;
		}

		public void LogCustomEvent(CustomBuildEventArgs e)
		{
		}

		public void LogErrorEvent(BuildErrorEventArgs e)
		{
			Errors.Add(e);
		}

		public void LogMessageEvent(BuildMessageEventArgs e)
		{
			Messages.Add(e);
		}

		public void LogWarningEvent(BuildWarningEventArgs e)
		{
			Warnings.Add(e);
		}
	}
}
