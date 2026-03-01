using System;
using System.Threading.Tasks;
using BloomBell.src.Domain.Models;
using BloomBell.src.Domain.Ports;
using BloomBell.src.Infrastructure.Configuration;
using BloomBell.src.Infrastructure.Game;
using BloomBell.src.Infrastructure.Network.DTO;
using Dalamud.Plugin;

namespace BloomBell.src.Application.Services;

/// <summary>
/// Fetches the authoritative platform connection status from the backend
/// and synchronizes it with the local plugin configuration.
/// </summary>
public sealed class PlatformService
{
    private readonly IPlatformClient platformClient;
    private readonly PluginConfiguration configuration;
    private readonly IDalamudPluginInterface pluginInterface;

    public PlatformStatus? CurrentStatus { get; private set; }

    public PlatformService(
        IPlatformClient platformClient,
        PluginConfiguration configuration,
        IDalamudPluginInterface pluginInterface)
    {
        this.platformClient = platformClient;
        this.configuration = configuration;
        this.pluginInterface = pluginInterface;
    }

    public async Task<DisconnectResponse?> DisconnectAsync(string? platform = null)
    {
        var userId = GameServices.PlayerState.ContentId;

        try
        {
            var response = await platformClient.DisconnectAsync(userId, platform);

            if (response is { IsSuccess: true })
            {
                if (response.Platform is "discord")
                {
                    configuration.DiscordLinked = false;
                }

                configuration.Save(pluginInterface);
                CurrentStatus = await platformClient.GetStatusAsync(userId);
            }

            return response;
        }
        catch (Exception ex)
        {
            GameServices.PluginLog.Error(ex, "Failed to disconnect platform");
            return null;
        }
    }

    public async Task<PlatformStatus> RefreshAsync()
    {
        var userId = GameServices.PlayerState.ContentId;

        try
        {
            CurrentStatus = await platformClient.GetStatusAsync(userId);

            configuration.DiscordLinked = CurrentStatus.Discord;
            configuration.Save(pluginInterface);
        }
        catch (Exception ex)
        {
            GameServices.PluginLog.Error(ex, "Failed to fetch connected platforms");
            CurrentStatus = new PlatformStatus(Discord: false);
        }

        return CurrentStatus;
    }
}
