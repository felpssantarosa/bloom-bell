using System;
using BloomBell.src.services;
using Dalamud.Utility;
using BloomBell.src.Configuration;
using BloomBell.src.Library.External.Services;

namespace BloomBell.src.integrations.discord;

public class DiscordOAuth(WebSocketHandler webSocketHandler) : IOAuthProvider, IDisposable
{
    private const string ClientId = "1476010311449448530";
    private readonly string provider = "discord";

    private readonly WebSocketHandler webSocketHandler = webSocketHandler;

    public void Dispose() { }

    public async void Authenticate(string userId)
    {
        try
        {
            GameServices.PluginLog.Info($"Starting WS connection for {userId} ({provider})");

            await webSocketHandler.StartAuthAsync(userId, provider);

            GameServices.PluginLog.Info("WS connected, opening OAuth browser");

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
            GameServices.PluginLog.Error(ex, "Discord authentication crashed");
        }
    }
}
