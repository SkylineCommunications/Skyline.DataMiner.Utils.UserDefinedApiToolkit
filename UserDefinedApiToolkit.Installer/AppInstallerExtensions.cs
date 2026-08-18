namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer
{
	using Skyline.AppInstaller;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;

	public static class AppInstallerExtensions
	{
		public static void InstallUserDefinedApiDefinitions(this AppInstaller installer, IEngine engine)
		{
			var udapiInstaller = new UdapiInstaller(installer, engine.GetUserConnection());
			udapiInstaller.InstallUserDefinedApiDefinitions();
		}

		public static void InstallUserDefinedApiDefinitions(this AppInstaller installer, IConnection connection)
		{
			var udapiInstaller = new UdapiInstaller(installer, connection);
			udapiInstaller.InstallUserDefinedApiDefinitions();
		}
	}
}