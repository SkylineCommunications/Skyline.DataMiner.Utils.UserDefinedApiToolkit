namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Logging
{
	internal class NullBuildLogger : IBuildLogger
	{
		public void Log(BuildLogLevel level, string message)
		{
		}
	}
}
