using System.Text.Json.Serialization;

namespace BloomBell.src.Infrastructure.Network.DTO;

public sealed class DisconnectResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }

    public bool IsSuccess => Error is null;
}
