using System.Text.Json.Serialization;

namespace BloomBell.src.Infrastructure.Network.DTO;

public sealed class PlatformResponse
{
    [JsonPropertyName("platforms")]
    public PlatformData Platforms { get; set; } = new();
}

public sealed class PlatformData
{
    [JsonPropertyName("discord")]
    public bool Discord { get; set; } = false;
}
