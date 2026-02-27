namespace BloomBell.src.Domain.Ports;

/// <summary>
/// Contract for platform-specific OAuth implementations.
/// Each provider is responsible only for initiating the browser-based OAuth flow.
/// </summary>
public interface IOAuthProvider
{
    void Authenticate(string userId);
}
