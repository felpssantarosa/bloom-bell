namespace BloomBell.src.config;

public static class InternalConfiguration
{
    public static string baseServerHttpUri = "http://localhost:3333";
    public static string baseServerWsUri = "ws://localhost:3334";
    public static string OAuthLoginUrl = $"{baseServerHttpUri}/login";
    public static string NotificationUrl = $"{baseServerHttpUri}/notify";
    public static string CallbackUrl = $"{baseServerHttpUri}/callback";
}
