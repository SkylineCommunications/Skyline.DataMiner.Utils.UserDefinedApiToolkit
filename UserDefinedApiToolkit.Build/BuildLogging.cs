namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build
{
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Logging;

	/// <summary>
	/// Indicates how significant a message logged during OpenAPI generation is, so the consumer
	/// (e.g. <see cref="OpenApiTask"/>) can map it to an appropriate build log verbosity/importance.
	/// </summary>
	/// <remarks>
	/// This type intentionally has no dependency on MSBuild (e.g. Microsoft.Build.Framework's
	/// MessageImportance), so the OpenAPI generation logic stays decoupled from - and testable
	/// without - the MSBuild task infrastructure. The MSBuild task adapters map these levels
	/// to MSBuild-specific concepts.
	/// </remarks>
	internal enum BuildLogLevel
	{
		/// <summary>
		/// Fine-grained, per-item detail (e.g. one line per registered route) that is only
		/// useful when troubleshooting a build, not for a developer's default build output.
		/// </summary>
		Detail,

		/// <summary>
		/// A concise, user-facing summary that is useful to see on every build by default.
		/// </summary>
		Summary,

		/// <summary>
		/// A message that should stand out on every build by default, e.g. a warning-like
		/// condition that isn't severe enough to fail the build.
		/// </summary>
		Important,
	}

	/// <summary>
	/// Used by the OpenAPI generation logic to report progress/diagnostics, tagged with a
	/// <see cref="BuildLogLevel"/> so the caller can decide how prominently to surface it.
	/// </summary>
	/// <remarks>
	/// This interface intentionally has no dependency on MSBuild, so the OpenAPI generation
	/// logic stays decoupled from - and testable without - the MSBuild task infrastructure.
	/// The MSBuild task adapters provide implementations that map these levels to MSBuild's
	/// logging APIs (see <see cref="MsBuildLogger"/>).
	/// </remarks>
	internal interface IBuildLogger
	{
		/// <summary>
		/// Logs <paramref name="message"/> at the given <paramref name="level"/>.
		/// </summary>
		/// <param name="level">The significance of the message.</param>
		/// <param name="message">The message text.</param>
		void Log(BuildLogLevel level, string message);
	}
}
