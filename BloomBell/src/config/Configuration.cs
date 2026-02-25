using Dalamud.Configuration;
using Dalamud.Plugin;

namespace BloomBell.src.config;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string OAuthLoginUrl = InternalConfiguration.OAuthLoginUrl;

    public bool DiscordLinked = false;

    public void Save(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.SavePluginConfig(this);
    }
}
