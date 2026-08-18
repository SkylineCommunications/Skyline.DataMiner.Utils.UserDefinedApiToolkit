namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer.Shared
{
	using System.Runtime.CompilerServices;

	internal class UdapiInfo
	{
		public string? ToolkitVersion { get; set; }

		public string? ScriptName { get; set; }

		public RouteInfo[] Routes { get; set; } = [];
	}
}
