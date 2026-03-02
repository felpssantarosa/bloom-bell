namespace BloomBell.src.Infrastructure.Configuration;

public enum PluginEnvironment
{
    Production,
    Development,
}

public static class InternalConfiguration
{
    private const string ProdHttpUri = "https://bloombell.maot.dev";
    private const string ProdWsUri = "wss://bloombell.maot.dev";

    private const string DevHttpUri = "http://localhost:3333";
    private const string DevWsUri = "ws://localhost:3334";

    public static PluginEnvironment Environment { get; private set; } = PluginEnvironment.Production;

    public static string BaseServerHttpUri => Environment == PluginEnvironment.Development ? DevHttpUri : ProdHttpUri;
    public static string BaseServerWsUri => Environment == PluginEnvironment.Development ? DevWsUri : ProdWsUri;

    public static string OAuthLoginUrl => $"{BaseServerHttpUri}/login";
    public static string NotificationUrl => $"{BaseServerHttpUri}/notify";
    public static string CallbackUrl => $"{BaseServerHttpUri}/callback";
    public static string PlatformsUrl => $"{BaseServerHttpUri}/platforms";
    public static string DisconnectUrl => $"{BaseServerHttpUri}/disconnect";

    public static void SetEnvironment(PluginEnvironment env)
    {
        Environment = env;
    }
}
