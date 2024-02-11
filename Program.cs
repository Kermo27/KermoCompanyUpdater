using KermoCompanyUpdater;
using System;
using System.Diagnostics;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        var apiClient = new ApiClient();
        var fileUpdater = new FileUpdater(apiClient);

        Console.WriteLine("Checking for updates...");
        
        var currentVersion = VersionManager.GetCurrentVersion();
        var latestVersion = await apiClient.GetLatestVersionAsync();

        if (latestVersion != currentVersion)
        {
            Console.WriteLine($"Update available: Version {latestVersion}");
            var result = MessageBox.Show($"A new version {latestVersion} is available. Would you like to see the changes?",
                "KermoCompany", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start http://51.38.131.66/api/changelog{latestVersion}.html") { CreateNoWindow = true });
            }
            
            VersionManager.UpdateVersion(latestVersion);
            await fileUpdater.UpdateFileAsync();
            MessageBox.Show("The update process has completed successfully!", "KermoCompany", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("Modpack is up to date!", "KermoCompany", MessageBoxButtons.OK);
        }
    }
}