using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace KermoCompanyUpdater;

internal static class Program
{
	[STAThread]
	private static async Task Main()
	{
        var logFilePath = "log.txt";

		Logger logger = new Logger(logFilePath);

		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		var apiClient = new ApiClient();
		var fileUpdater = new FileUpdater(apiClient);

		var currentVersion = VersionManager.GetCurrentVersion();
		var latestVersion = await apiClient.GetLatestVersionAsync();

		if (latestVersion != currentVersion && IsAppRunningAsAdmin())
		{
			logger.ClearLog();

			Console.WriteLine($"Update available: Version {latestVersion}");
			logger.Log($"Update available: Version {latestVersion}");

			var newVersionResult = MessageBox.Show($"A new version {latestVersion} is available. Would you like to see the changes?",
				"KermoCompany", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (newVersionResult == DialogResult.Yes)
			{
				Process.Start(new ProcessStartInfo("cmd", $"/c start http://51.38.131.66/api/changelog{latestVersion}.html") { CreateNoWindow = true });
			}

			Console.WriteLine("Checking files...");
			logger.Log("Checking files...");

			VersionManager.UpdateVersion(latestVersion);

			await fileUpdater.UpdateFileAsync();

			var updatedVersionResult = MessageBox.Show("Update successfully completed! Do you want to run Lethal Company?",
				"KermoCompany", MessageBoxButtons.YesNo);
			logger.Log("The update process has completed successfully!");

			if (updatedVersionResult == DialogResult.Yes)
			{
				Process.Start($"{Path.Combine(Directory.GetCurrentDirectory())}\\Lethal Company.exe");
			}
		}
		else if (latestVersion == currentVersion && IsAppRunningAsAdmin())
		{
			var upToDateResult = MessageBox.Show("Modpack is up to date! Would you like to lauch Lethal Company now?", "KermoCompany", MessageBoxButtons.YesNo);
			logger.Log("Modpack is up to date!");

			if (upToDateResult == DialogResult.Yes)
			{
				Process.Start($"{Path.Combine(Directory.GetCurrentDirectory())}\\Lethal Company.exe");
			}
		}
		else
		{
            MessageBox.Show("Run this app as administrator!",
                "KermoCompany", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
	}

    private static bool IsAppRunningAsAdmin()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (Environment.OSVersion.Version >= new Version(6, 2))
            {
                var identity = WindowsIdentity.GetCurrent();
                return identity.Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
        }

        return false;
    }
}