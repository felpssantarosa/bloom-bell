using System.Threading.Tasks;
using BloomBell.src.Domain.Models;

namespace BloomBell.src.Domain.Ports;

/// <summary>
/// Contract for fetching the current platform connection status from the backend.
/// </summary>
public interface IPlatformClient
{
    Task<PlatformStatus> GetStatusAsync(ulong userId);
}
