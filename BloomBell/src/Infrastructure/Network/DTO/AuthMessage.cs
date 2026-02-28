using System.Text.Json.Serialization;

namespace BloomBell.src.Infrastructure.Network.DTO;

public sealed class AuthMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = "";

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = "";

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
