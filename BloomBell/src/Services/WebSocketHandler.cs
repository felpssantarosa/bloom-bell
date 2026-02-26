using System;
using System.IO;
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

    public bool IsConnected => socket is { State: WebSocketState.Open };

    public void Dispose()
    {
        try
        {
            cancellationTokenSource?.Cancel();
        }
        catch
        {
            GameServices.PluginLog.Warning(
                "Failed to cancel WebSocket receive loop, it may still be running until the next message is received."
            );
        }

        socket?.Dispose();
        cancellationTokenSource?.Dispose();
    }

    public async Task StartAuthAsync(string userId, string provider)
    {
        if (socket is null || socket.State != WebSocketState.Open)
        {
            await ConnectAsync();
        }

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
    /// Continuously listens for incoming WebSocket messages until the connection closes
    /// or the plugin is disposed.
    /// </summary>
    private async Task RunReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            while (socket is { State: WebSocketState.Open } && !token.IsCancellationRequested)
            {
                var message = await ReceiveMessageAsync(token);

                if (message == null) break;

                await ProcessMessageAsync(message);
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

    private async Task<string?> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        if (socket == null) return null;

        var buffer = new byte[4096];
        using var memoryStream = new MemoryStream();

        while (true)
        {
            WebSocketReceiveResult result;

            try
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            }
            catch (WebSocketException ex)
            {
                GameServices.PluginLog.Warning($"WebSocket error: {ex.Message}");
                return null;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                GameServices.PluginLog.Info("WebSocket closed by server.");
                return null;
            }

            memoryStream.Write(buffer, 0, result.Count);

            if (result.EndOfMessage)
                break;
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
    private async Task ProcessMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
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

        if (authMessage == null) return;

        switch (authMessage.Type)
        {
            case "authComplete":
                await HandleAuthCompleteAsync(authMessage);
                break;

            default:
                GameServices.PluginLog.Debug($"Unhandled WS message type: {authMessage.Type}");
                break;
        }
    }

    private async Task HandleAuthCompleteAsync(AuthMessage message)
    {
        GameServices.PluginLog.Info(
            $"{message.Provider.FirstCharToUpper()} auth complete for user with ID {message.UserId}"
        );

        OnAuthCompleted?.Invoke(message.Provider);

        await CloseWebSocketAsync();
    }

    private async Task ConnectAsync()
    {
        if (socket is { State: WebSocketState.Open })
        {
            GameServices.PluginLog.Info("WebSocket already connected.");
            return;
        }

        GameServices.PluginLog.Info("Connecting WebSocket to backend...");

        socket = new ClientWebSocket();
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await socket.ConnectAsync(
                new Uri(InternalConfiguration.baseServerWsUri),
                cancellationTokenSource.Token
            );

            GameServices.PluginLog.Info("WebSocket connected successfully!");

            _ = RunReceiveLoopAsync(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            GameServices.PluginLog.Error(ex, "WebSocket connection failed!");
        }
    }

    private async Task CloseWebSocketAsync()
    {
        if (socket is { State: WebSocketState.Open })
        {
            GameServices.PluginLog.Info("Closing WebSocket connection...");

            try
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closing connection",
                    cancellationTokenSource?.Token ?? CancellationToken.None
                );
            }
            catch (Exception ex)
            {
                GameServices.PluginLog.Error(ex, "Error while closing WebSocket");
            }

            socket.Dispose();
            socket = null;
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }
}
