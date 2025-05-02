namespace Protocols
{
	using System;

	using Skyline.AppInstaller;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.AppPackages;

	public class ProtocolInstaller
	{
		private const string PROTOCOLS_FOLDERPATH = @"\Protocols";
		private readonly AppInstallContext context;
		private readonly Action<string> logMethod;
		private readonly string originalContentPath;

		private readonly AppInstaller installer;

		public ProtocolInstaller(IConnection connection, AppInstallContext context, string setupContentPath, Action<string> logMethod)
		{
			originalContentPath = context.AppContentPath;

			this.context = context;
			this.context.AppContentPath = setupContentPath + PROTOCOLS_FOLDERPATH;
			this.logMethod = logMethod;
			this.installer = new AppInstaller(connection, this.context);
		}

		public void InstallDefaultContent()
		{
			installer.InstallProtocols();
			this.context.AppContentPath = originalContentPath;
		}
	}
}
