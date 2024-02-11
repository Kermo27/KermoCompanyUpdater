namespace KermoCompanyUpdater;

public class VersionManager
{
    private static readonly string VersionFilePath = Path.Combine(Directory.GetCurrentDirectory(), "version.txt");

    public static string GetCurrentVersion()
    {
        if (File.Exists(VersionFilePath))
        {
            return File.ReadAllText(VersionFilePath);
        }

        return "0";
    }

    public static void UpdateVersion(string? newVersion)
    {
        File.WriteAllText(VersionFilePath, newVersion);
    }
}