namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build
{
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// <see cref="IBuildLogger"/> implementation that forwards messages to an MSBuild <see cref="TaskLoggingHelper"/>,
	/// mapping each <see cref="BuildLogLevel"/> to the corresponding <see cref="MessageImportance"/>.
	/// </summary>
	/// <remarks>
	/// This is the only place that bridges the MSBuild-independent <see cref="IBuildLogger"/> abstraction
	/// used by the OpenAPI generation logic to MSBuild's own logging APIs.
	/// </remarks>
	internal sealed class MsBuildLogger : IBuildLogger
	{
		private readonly TaskLoggingHelper log;

		public MsBuildLogger(TaskLoggingHelper log)
		{
			this.log = log;
		}

		/// <inheritdoc/>
		public void Log(BuildLogLevel level, string message)
		{
			log.LogMessage(ToMessageImportance(level), message);
		}

		private static MessageImportance ToMessageImportance(BuildLogLevel level)
		{
			switch (level)
			{
				// Per-item detail (e.g. one line per registered route) is only useful for
				// troubleshooting, not for every developer's default build output, so it's
				// only visible with -verbosity:detailed/diagnostic.
				case BuildLogLevel.Detail:
					return MessageImportance.Low;

				case BuildLogLevel.Important:
					return MessageImportance.High;

				case BuildLogLevel.Summary:
				default:
					return MessageImportance.Normal;
			}
		}
	}
}
