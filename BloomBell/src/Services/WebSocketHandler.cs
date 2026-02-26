using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BloomBell.src.Configuration;
using BloomBell.src.Library.External.Services;
using Dalamud.Utility;

namespace BloomBell.src.services;

public sealed class WebSocketHandler : IDisposable
{
    private ClientWebSocket? socket;
    private CancellationTokenSource? cancellationTokenSource;

    public event Action<string>? OnAuthCompleted;

    public void Dispose()
    {
        cancellationTokenSource?.Cancel();
        socket?.Dispose();
        cancellationTokenSource?.Dispose();
    }

    public async Task ConnectAndRegisterAsync(string userId, string provider)
    {
        GameServices.PluginLog.Info($"Connecting WS to backend for user {userId}...");

        socket = new ClientWebSocket();
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await socket.ConnectAsync(
                new Uri(InternalConfiguration.baseServerWsUri),
                cancellationTokenSource.Token
            );

            GameServices.PluginLog.Info("WebSocket connected successfully!");

            await SendRegistrationAsync(userId, provider);

            _ = RunReceiveLoopAsync(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            GameServices.PluginLog.Error(ex, "WebSocket connection failed!");
        }
    }

    private async Task SendRegistrationAsync(string userId, string provider)
    {
        var payload = new
        {
            type = "register",
            userId,
            provider
        };

        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        await socket!.SendAsync(
            bytes,
            WebSocketMessageType.Text,
            true,
            cancellationTokenSource!.Token
        );

        GameServices.PluginLog.Info($"Sent register message for {userId}");
    }

    /// <summary>
    /// Continuously listens for incoming WebSocket messages until the connection closes.
    /// </summary>
    private async Task RunReceiveLoopAsync(CancellationToken token)
    {
        var buffer = new byte[1024];

        try
        {
            while (socket is { State: WebSocketState.Open } && !token.IsCancellationRequested)
            {
                var result = await ReceiveMessageAsync(buffer, token);
                if (result == null)
                    break;

                await ProcessMessageAsync(result);
            }
        }
        catch (OperationCanceledException)
        {
            GameServices.PluginLog.Info("WebSocket receive loop cancelled.");
        }
        catch (Exception ex)
        {
            GameServices.PluginLog.Error(ex, "Receive loop crashed unexpectedly.");
        }
        finally
        {
            GameServices.PluginLog.Info("WebSocket receive loop ended.");
        }
    }

    private async Task<string?> ReceiveMessageAsync(byte[] buffer, CancellationToken token)
    {
        try
        {
            var result = await socket!.ReceiveAsync(buffer, token);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                GameServices.PluginLog.Info("WebSocket closed by server.");
                return null;
            }

            if (result.MessageType != WebSocketMessageType.Text)
                return string.Empty;

            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }
        catch (WebSocketException ex)
        {
            GameServices.PluginLog.Warning($"WebSocket error: {ex.Message}");
            return null;
        }
    }

    private async Task ProcessMessageAsync(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        AuthMessage? authMessage;

        try
        {
            authMessage = JsonSerializer.Deserialize<AuthMessage>(message);
        }
        catch (Exception ex)
        {
            GameServices.PluginLog.Warning($"Failed to parse WS message: {ex.Message}");
            return;
        }

        if (authMessage?.Type == "authComplete")
        {
            await HandleAuthCompleteAsync(authMessage);
        }
    }

    private async Task HandleAuthCompleteAsync(AuthMessage message)
    {
        GameServices.PluginLog.Info(
            $"{message.Provider.FirstCharToUpper()} auth complete for user {message.UserId}"
        );

        OnAuthCompleted?.Invoke(message.Provider);

        if (socket?.State == WebSocketState.Open)
        {
            GameServices.PluginLog.Info("Closing WebSocket connection...");

            await socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Auth complete",
                CancellationToken.None
            );
        }
    }
}
