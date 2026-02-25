namespace BloomBell.src.config;

public static class InternalConfiguration
{
    public static string baseUrl = "http://localhost:3333";
    public static string OAuthLoginUrl = $"{baseUrl}/login";
    public static string NotificationUrl = $"{baseUrl}/notify";
    public static string CallbackUrl = $"{baseUrl}/callback";
}
