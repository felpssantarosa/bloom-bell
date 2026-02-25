using System;
using Dalamud.Utility;

namespace SamplePlugin.src.integrations.discord;

public class DiscordIntegration
{
    private const string ClientId = "1476010311449448530";
    private const string RedirectUri = "http://localhost:3333/callback";

    public void OpenDiscordOAuth(string userId)
    {
        var oauthUrl =
            "https://discord.com/api/oauth2/authorize" +
            $"?client_id={ClientId}" +
            "&response_type=code" +
            $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
            "&scope=identify" +
            $"&state={userId}";

        Util.OpenLink(oauthUrl);
    }
}
