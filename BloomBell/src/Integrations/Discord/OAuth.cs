using System;
using BloomBell.src.lib.infra;
using BloomBell.src.services;
using Dalamud.Utility;
using BloomBell.src.Configuration;

namespace BloomBell.src.integrations.discord;

public class DiscordOAuth(PluginConfiguration configuration, WebSocketHandler webSocketHandler) : IOAuthProvider, IDisposable
{
    private const string ClientId = "1476010311449448530";
    private readonly string provider = "discord";

    private readonly PluginConfiguration configuration = configuration;
    private readonly WebSocketHandler webSocketHandler = webSocketHandler;


    public void Dispose() { }

    public async void Authenticate(string userId)
    {
        try
        {
            Services.PluginLog.Info($"Starting WS connection for {userId} ({provider})");

            await webSocketHandler.Connect(userId, provider);

            Services.PluginLog.Info("WS connected, opening OAuth browser");

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
            Services.PluginLog.Error(ex, "Discord authentication crashed");
        }
    }

    public void AuthCompletedHandler(string provider)
    {
        Services.PluginLog.Info($"Auth completed for: {provider}");

        configuration.DiscordLinked = true;
    }
}
