namespace KermoCompanyUpdater;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://51.38.131.66/api/";

    public ApiClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> GetUpdatesAsync()
    {
        var responseString = await _httpClient.GetStringAsync($"{_baseUrl}updates.php");
        return responseString;
    }

    public async Task<string> GetFilesListAsync()
    {
        return await _httpClient.GetStringAsync($"{_baseUrl}files.php");
    }

    public async Task<string?> GetLatestVersionAsync()
    {
        try
        {
            var versionUrl = $"{_baseUrl}version.txt";
            var version = await _httpClient.GetStringAsync(versionUrl);
            return version.Trim();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error while downloading the latest version: {ex.Message}");
            return null;
        }
    }
}