using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BloomBell.src.Configuration;
using BloomBell.src.Library.External.Services;

public class ConnectedPlatforms
{
    private static readonly HttpClient Client = new();

    public static async Task<NotificationPlatforms> GetAllAsync()
    {
        try
        {
            var response = await Client.GetAsync(InternalConfiguration.PlatformsUrl);

            if (!response.IsSuccessStatusCode)
            {
                return new NotificationPlatforms();
            }

            var content = await response.Content.ReadAsStringAsync();
            var platforms = JsonSerializer.Deserialize<NotificationPlatforms>(content);

            return platforms ?? new NotificationPlatforms();
        }
        catch (HttpRequestException ex)
        {
            GameServices.PluginLog.Error(ex, "Failed to fetch connected platforms");
            return new NotificationPlatforms();
        }
        catch (JsonException ex)
        {
            GameServices.PluginLog.Error(ex, "Failed to parse connected platforms JSON");
            return new NotificationPlatforms();
        }
    }
}
