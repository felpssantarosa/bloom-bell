using System.Text.Json.Serialization;

public class NotificationPlatforms
{
    [JsonPropertyName("discord")]
    public bool Discord { get; set; } = false;
}
