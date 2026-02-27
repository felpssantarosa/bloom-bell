using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BloomBell.src.Domain.Models;
using BloomBell.src.Domain.Ports;
using BloomBell.src.Infrastructure.Configuration;
using BloomBell.src.Infrastructure.Game;
using BloomBell.src.Infrastructure.Network.DTO;

namespace BloomBell.src.Infrastructure.Network;

/// <summary>
/// HTTP client that fetches the authoritative platform connection status from the backend.
/// Maps the infrastructure DTO to the domain <see cref="PlatformStatus"/> value object.
/// </summary>
public sealed class HttpPlatformClient : IPlatformClient
{
    private static readonly HttpClient Client = new();

    public async Task<PlatformStatus> GetStatusAsync(ulong userId)
    {
        try
        {
            var response = await Client.GetAsync($"{InternalConfiguration.PlatformsUrl}?userId={userId}");

            if (!response.IsSuccessStatusCode)
            {
                return new PlatformStatus(Discord: false);
            }

            var content = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<PlatformResponse>(content);

            return new PlatformStatus(
                Discord: dto?.Platforms.Discord ?? false
            );
        }
        catch (HttpRequestException ex)
        {
            GameServices.PluginLog.Error(ex, "Failed to fetch connected platforms");
            return new PlatformStatus(Discord: false);
        }
        catch (JsonException ex)
        {
            GameServices.PluginLog.Error(ex, "Failed to parse connected platforms JSON");
            return new PlatformStatus(Discord: false);
        }
    }
}
