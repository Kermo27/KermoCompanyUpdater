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
			foreach (var fileInfo in filesToUpdate)
			{
				Console.WriteLine($"Checking: {fileInfo.Path}");
				logger.Log($"Checking: {fileInfo.Path}");

				await UpdateFileAsync(fileInfo);
			}

			await CleanupLocalFilesAsync(filesToUpdate);
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
		var fileUrl = $"http://51.38.131.66/api/files/{Uri.EscapeDataString(fileInfo.Path.Replace("\\", "/"))}";

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
			logger.Log($"Updated: {fileInfo.Name}");
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
			"version.txt",
			"log.txt"
		};

		var applicationDirectory = Directory.GetCurrentDirectory();
		var serverFiles = serverFilesInfo.Select(f => f.Name).ToList();
		var localFiles = Directory.GetFiles(applicationDirectory, "*", SearchOption.AllDirectories).ToList();

		// Delete files not present on the server and not part of the core application files
		foreach (var filePath in localFiles)
		{
			var fileName = Path.GetFileName(filePath);
			if (fileName != null && !serverFiles.Contains(fileName) && !coreAppFiles.Contains(fileName))
			{
				Console.WriteLine($"{filePath} was deleted.");
				logger.Log($"{filePath} was deleted.");

				File.Delete(filePath);
			}
		}

		// Remove empty directories
		var allDirectories = Directory.GetDirectories(applicationDirectory, "*", SearchOption.AllDirectories);
		foreach (var dir in allDirectories)
		{
			if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
			{
				Console.WriteLine($"{dir} was deleted.");
				logger.Log($"{dir} was deleted.");

				Directory.Delete(dir);
			}
		}

		return Task.CompletedTask;
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