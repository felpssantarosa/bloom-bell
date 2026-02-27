namespace BloomBell.src.Domain.Models;

/// <summary>
/// Value object representing the connection status of each supported notification platform.
/// </summary>
public sealed record PlatformStatus(bool Discord);
