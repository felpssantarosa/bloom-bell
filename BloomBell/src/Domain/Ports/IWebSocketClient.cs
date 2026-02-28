using System;
using System.Threading.Tasks;

namespace BloomBell.src.Domain.Ports;

/// <summary>
/// Contract for the WebSocket client used during OAuth authentication.
/// Fires <see cref="OnAuthCompleted"/> when the server confirms auth success,
/// and <see cref="OnDisconnected"/> when the connection drops without completing auth.
/// </summary>
public interface IWebSocketClient : IDisposable
{
    bool IsConnected { get; }

    event Action<string>? OnAuthCompleted;
    event Action<string, string>? OnAuthFailed;
    event Action? OnDisconnected;

    Task StartAuthAsync(string userId, string provider);
}
