using System;
using BloomBell.src.Domain.Ports;
using BloomBell.src.Infrastructure.Configuration;
using Dalamud.Utility;

namespace BloomBell.src.Infrastructure.Discord;

/// <summary>
/// Discord-specific OAuth implementation.
/// Responsible only for opening the WebSocket registration and launching the browser.
/// </summary>
public sealed class DiscordOAuthProvider(IWebSocketClient webSocketClient) : IOAuthProvider
{
    private const string ClientId = "1476010311449448530";
    private const string Provider = "discord";

    public async void Authenticate(string userId)
    {
        try
        {
            Game.GameServices.PluginLog.Info($"Starting WS connection for {userId} ({Provider})");

            await webSocketClient.StartAuthAsync(userId, Provider);

            Game.GameServices.PluginLog.Info("WS connected, opening OAuth browser");

            var oauthUrl =
                "https://discord.com/api/oauth2/authorize" +
                $"?client_id={ClientId}" +
                "&response_type=code" +
                $"&redirect_uri={Uri.EscapeDataString(InternalConfiguration.CallbackUrl)}" +
                "&scope=identify" +
                $"&state={userId}";

            Util.OpenLink(oauthUrl);
        }
        catch (Exception ex)
        {
            Game.GameServices.PluginLog.Error(ex, "Discord authentication crashed");
        }
    }
}
