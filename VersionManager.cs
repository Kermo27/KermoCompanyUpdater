namespace KermoCompanyUpdater;

public class VersionManager
{
    private static readonly string versionFilePath = Path.Combine(Directory.GetCurrentDirectory(), "version.txt");

    public static string GetCurrentVersion()
    {
        if (File.Exists(versionFilePath))
        {
            return File.ReadAllText(versionFilePath);
        }

        return "0";
    }

    public static void UpdateVersion(string newVersion)
    {
        File.WriteAllText(versionFilePath, newVersion);
    }
}