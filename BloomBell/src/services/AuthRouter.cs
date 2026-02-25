using System;
using System.Threading.Tasks;
using BloomBell.src.config;
using BloomBell.src.integrations.discord;
using BloomBell.src.lib.infra;
using BloomBell.src.services;

public class AuthRouter : IDisposable
{
    private readonly Configuration configuration;
    private readonly AuthNotifier authNotifier;
    private readonly DiscordOAuth discordOAuth;

    public AuthRouter(Configuration configuration, AuthNotifier authNotifier)
    {
        this.configuration = configuration;
        this.authNotifier = authNotifier;

        discordOAuth = new DiscordOAuth(this.configuration, this.authNotifier);
    }

    public void Dispose()
    {
        discordOAuth.Dispose();
    }

    public async Task Authenticate(string provider)
    {
        var contentId = Services.PlayerState.ContentId;

        if (contentId == 0)
        {
            Services.PluginLog.Warning("Cannot authenticate: player not fully logged in.");
            return;
        }

        Services.PluginLog.Info($"Starting authentication for: {provider}");

        switch (provider.ToLower())
        {
            case "discord":
                discordOAuth.Authenticate(contentId.ToString());
                break;

            default:
                Services.PluginLog.Warning($"Unknown provider: {provider}");
                break;
        }
    }

    public void HandleAuthCompleted(string provider)
    {
        Services.PluginLog.Info($"Auth completed for: {provider}");

        switch (provider.ToLower())
        {
            case "discord":
                configuration.DiscordLinked = true;
                break;

            default:
                Services.PluginLog.Warning($"Unknown provider: {provider}");
                break;
        }
    }
}
