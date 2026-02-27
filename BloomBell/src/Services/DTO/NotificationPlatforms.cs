using System.Text.Json.Serialization;

public class ConnectedPlatformsResponse
{
    [JsonPropertyName("platforms")]
    public NotificationPlatforms Platforms { get; set; } = new NotificationPlatforms();
}

public class NotificationPlatforms
{
    [JsonPropertyName("discord")]
    public bool Discord { get; set; } = false;
}
