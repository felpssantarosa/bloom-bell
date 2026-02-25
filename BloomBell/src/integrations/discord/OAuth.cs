using System;
using BloomBell.src.config;
using Dalamud.Utility;

namespace BloomBell.src.integrations.discord;

public static class DiscordIntegration
{
    private const string ClientId = "1476010311449448530";

    public static void OpenDiscordOAuth(string userId)
    {
        var oauthUrl =
            "https://discord.com/api/oauth2/authorize" +
            $"?client_id={ClientId}" +
            "&response_type=code" +
            $"&redirect_uri={Uri.EscapeDataString(InternalConfiguration.CallbackUrl)}" +
            "&scope=identify" +
            $"&state={userId}";

        Util.OpenLink(oauthUrl);
    }
}
