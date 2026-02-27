using System;
using BloomBell.src.Configuration;
using BloomBell.src.Events;
using BloomBell.src.integrations.discord;
using BloomBell.src.Library.External.Services;
using BloomBell.src.services;

public class AuthRouter : IDisposable
{
    private readonly PluginConfiguration configuration;
    private readonly WebSocketHandler webSocketHandler;
    private readonly EventBus eventBus;
    private readonly DiscordOAuth discordOAuth;

    public AuthRouter(PluginConfiguration configuration, WebSocketHandler webSocketHandler, EventBus eventBus)
    {
        this.configuration = configuration;
        this.webSocketHandler = webSocketHandler;
        this.eventBus = eventBus;

        discordOAuth = new DiscordOAuth(this.webSocketHandler);
    }

    public void Dispose()
    {
        discordOAuth.Dispose();
    }

    public void AuthenticateWith(string provider)
    {
        var contentId = GameServices.PlayerState.ContentId;

        if (contentId == 0)
        {
            GameServices.PluginLog.Warning("Cannot authenticate: player not fully logged in.");
            return;
        }

        GameServices.PluginLog.Info($"Starting authentication for: {provider}");
        eventBus.Publish(new AuthStateChangedEvent(provider, AuthState.Started));

        switch (provider.ToLower())
        {
            case "discord":
                discordOAuth.Authenticate(contentId.ToString());
                break;

            default:
                GameServices.PluginLog.Warning($"Unknown provider: {provider}");
                break;
        }
    }

    public void HandleAuthCompleted(string provider)
    {
        GameServices.PluginLog.Info($"Auth completed for: {provider}");

        switch (provider.ToLower())
        {
            case "discord":
                configuration.DiscordLinked = true;
                break;

            default:
                GameServices.PluginLog.Warning($"Unknown provider: {provider}");
                break;
        }

        eventBus.Publish(new AuthStateChangedEvent(provider, AuthState.Completed));
    }
}
