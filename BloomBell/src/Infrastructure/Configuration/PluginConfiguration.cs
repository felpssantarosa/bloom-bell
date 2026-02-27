using Dalamud.Configuration;
using Dalamud.Plugin;

namespace BloomBell.src.Infrastructure.Configuration;

public class PluginConfiguration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string OAuthLoginUrl = InternalConfiguration.OAuthLoginUrl;

    public bool DiscordLinked = false;

    public byte maxPartySize = 8;
    public bool pauseNotifications = false;
    public bool notifyWhenFocused = false;

    public void Save(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.SavePluginConfig(this);
    }
}
