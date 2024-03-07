using Newtonsoft.Json;
using System.Security.Cryptography;

namespace KermoCompanyUpdater;

public class FileUpdater
{
    private readonly ApiClient _apiClient;
    private Logger logger = new Logger("log.txt");

    public FileUpdater(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task UpdateFileAsync()
    {
        var updatesJson = await _apiClient.GetUpdatesAsync();
        var filesToUpdate = JsonConvert.DeserializeObject<List<FileUpdateInfo>>(updatesJson);

        if (filesToUpdate != null)
        {
            CleanupLocalFiles(filesToUpdate);

            foreach (var fileInfo in filesToUpdate)
            {
                Console.WriteLine($"Checking: {fileInfo.Path}");
                logger.Log($"Checking: {fileInfo.Path}");

                await UpdateFileAsync(fileInfo);
            }

            CleanupLocalFiles(filesToUpdate);
        }
    }

    private async Task UpdateFileAsync(FileUpdateInfo fileInfo)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileInfo.Path.Replace("/", "\\"));

        if (File.Exists(filePath))
        {
            var localFileHash = CalculateFileHash(filePath);
            var serverFileHash = fileInfo.Hash;

            if (localFileHash.Equals(serverFileHash, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        await DownloadFileAsync(fileInfo, filePath);
    }

    private async Task DownloadFileAsync(FileUpdateInfo fileInfo, string filePath)
    {
        var fileUrl = $"http://51.38.131.66/api/files/{Uri.EscapeUriString(fileInfo.Path.Replace("\\", "/"))}";

        using (var httpClient = new HttpClient())
        {
            try
            {
                var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);

                if (File.Exists(filePath))
                {
                    Console.WriteLine($"Deleting existing file: {filePath}");
                    logger.Log($"Deleting existing file: {filePath}");
                    File.Delete(filePath);
                }

                var directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                }

                Console.WriteLine($"Updated: {fileInfo.Name}");
                logger.Log($"Updated: {fileInfo.Name}");
            }
            catch (Exception ex)
            {
                logger.Log($"Error downloading file '{fileUrl}': {ex.Message}");
            }
        }
    }

    private void CleanupLocalFiles(List<FileUpdateInfo> serverFilesInfo)
    {
        var coreAppFiles = new HashSet<string>(new string[] {
        "KermoCompanyUpdater.deps.json",
        "KermoCompanyUpdater.dll",
        "KermoCompanyUpdater.exe",
        "KermoCompanyUpdater.pdb",
        "KermoCompanyUpdater.runtimeconfig.json",
        "Newtonsoft.Json.dll",
        "version.txt",
        "log.txt"
    }, StringComparer.OrdinalIgnoreCase);

        var applicationDirectory = Directory.GetCurrentDirectory();
        var serverFilesPaths = new HashSet<string>(serverFilesInfo.Select(f => Path.Combine(applicationDirectory, f.Path.Replace("/", "\\"))), StringComparer.OrdinalIgnoreCase);

        var allFiles = Directory.EnumerateFiles(applicationDirectory, "*", SearchOption.AllDirectories);
        foreach (var filePath in allFiles)
        {
            if (!serverFilesPaths.Contains(filePath) && !coreAppFiles.Contains(Path.GetFileName(filePath)))
            {
                Console.WriteLine($"Deleting: {filePath}");
                logger.Log($"Deleting: {filePath}");
                File.Delete(filePath);
            }
        }

        var allDirectories = Directory.EnumerateDirectories(applicationDirectory, "*", SearchOption.AllDirectories)
                                      .OrderByDescending(d => d.Length);
        foreach (var dir in allDirectories)
        {
            if (!Directory.EnumerateFileSystemEntries(dir).Any())
            {
                Console.WriteLine($"Deleting directory: {dir}");
                logger.Log($"Deleting directory: {dir}");
                Directory.Delete(dir, false);
            }
        }
    }

    private string CalculateFileHash(string filePath)
    {
        using (var sha256 = SHA256.Create())
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(fileStream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}