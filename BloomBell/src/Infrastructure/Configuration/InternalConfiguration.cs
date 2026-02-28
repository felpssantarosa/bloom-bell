namespace BloomBell.src.Infrastructure.Configuration;

public static class InternalConfiguration
{
    public static string baseServerHttpUri = "https://bloombell.maot.dev";
    public static string baseServerWsUri = "wss://bloombell.maot.dev";
    public static string OAuthLoginUrl = $"{baseServerHttpUri}/login";
    public static string NotificationUrl = $"{baseServerHttpUri}/notify";
    public static string CallbackUrl = $"{baseServerHttpUri}/callback";
    public static string PlatformsUrl = $"{baseServerHttpUri}/platforms";
}
