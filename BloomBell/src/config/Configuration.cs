using Dalamud.Configuration;
using System;

namespace BloomBell.src.config;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string OAuthLoginUrl = "http://localhost:3000/login"; // example config

    public bool DiscordLinked = false;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
