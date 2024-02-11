using Newtonsoft.Json;
using System.Security.Cryptography;

namespace KermoCompanyUpdater;

public class FileUpdater
{
    private readonly ApiClient _apiClient;

    public FileUpdater(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [Obsolete("Obsolete")]
    public async Task UpdateFileAsync()
    {
        var updatesJson = await _apiClient.GetUpdatesAsync();
        var filesToUpdate = JsonConvert.DeserializeObject<List<FileUpdateInfo>>(updatesJson);

        if (filesToUpdate != null)
        {
            foreach (var fileInfo in filesToUpdate)
            {
                await UpdateFileAsync(fileInfo);
            }

            await CleanupLocalFilesAsync(filesToUpdate);
        }
    }

    private Task CleanupLocalFilesAsync(List<FileUpdateInfo> serverFilesInfo)
    {
        List<string> coreAppFiles = new List<string> {
            "KermoCompanyUpdater.deps.json",
            "KermoCompanyUpdater.dll",
            "KermoCompanyUpdater.exe",
            "KermoCompanyUpdater.pdb",
            "KermoCompanyUpdater.runtimeconfig.json",
            "Newtonsoft.Json.dll",
            "version.txt"
        };

        var applicationDirectory = Directory.GetCurrentDirectory();
        var serverFiles = serverFilesInfo.Select(f => f.Name).ToList();
        var localFiles = Directory.GetFiles(applicationDirectory, "*", SearchOption.AllDirectories).Select(Path.GetFileName).ToList();

        // Delete files not present on the server and not part of the core application files
        foreach (var file in localFiles)
        {
            if (file != null && !serverFiles.Contains(file) && !coreAppFiles.Contains(file))
            {
                var fullPath = Path.Combine(applicationDirectory, file);
                Console.WriteLine($"{file} was deleted.");
                File.Delete(fullPath);
            }
        }

        // Remove empty directories
        var allDirectories = Directory.GetDirectories(applicationDirectory, "*", SearchOption.AllDirectories);
        foreach (var dir in allDirectories)
        {
            if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
            {
                Directory.Delete(dir);
                Console.WriteLine($"{dir} was deleted.");
            }
        }

        return Task.CompletedTask;
    }

    [Obsolete("Obsolete")]
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
    
    [Obsolete("Obsolete")]
    private async Task DownloadFileAsync(FileUpdateInfo fileInfo, string filePath)
    {
        var fileUrl = $"http://51.38.131.66/api/files/{Uri.EscapeUriString(fileInfo.Path.Replace("\\", "/"))}";
        using (var httpClient = new HttpClient())
        {
            var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                if (directoryPath != null) Directory.CreateDirectory(directoryPath);
            }

            await File.WriteAllBytesAsync(filePath, fileBytes);
            Console.WriteLine($"Updated: {fileInfo.Name}");
        }
    }
}