using System.Text.Json.Serialization;

public class AuthMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = "";

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = "";
}
