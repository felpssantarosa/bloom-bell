using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BloomBell.src.config;
using BloomBell.src.lib.infra;
using Dalamud.Utility;

namespace BloomBell.src.services;

public class AuthNotifier() : IDisposable
{
    private ClientWebSocket? ws;

    public event Action<string>? OnAuthCompleted;

    public void Dispose()
    {
        ws?.Dispose();
    }

    public async Task Connect(string userId, string provider)
    {
        Services.PluginLog.Info($"Connecting WS to backend for user {userId}...");

        ws = new ClientWebSocket();

        try
        {
            await ws.ConnectAsync(
                new Uri(InternalConfiguration.baseServerWsUri),
                CancellationToken.None
            );

            Services.PluginLog.Info("WebSocket connected successfully!");

            var registerObj = new
            {
                type = "register",
                userId,
                provider
            };

            var json = JsonSerializer.Serialize(registerObj);

            await ws.SendAsync(
                Encoding.UTF8.GetBytes(json),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            Services.PluginLog.Info($"Sent register message for {userId}");

            _ = ReceiveLoop();
        }
        catch (Exception ex)
        {
            Services.PluginLog.Error(ex, "WebSocket connection failed!");
        }
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[1024];

        try
        {
            while (ws != null && ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;

                try
                {
                    result = await ws.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None
                    );
                }
                catch (WebSocketException ex)
                {
                    Services.PluginLog.Warning($"WebSocket error: {ex.Message}");
                    break;
                }
                catch (OperationCanceledException)
                {
                    Services.PluginLog.Info("WebSocket receive cancelled.");
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Services.PluginLog.Info("WebSocket closed by server.");
                    break;
                }

                if (result.MessageType != WebSocketMessageType.Text)
                {
                    continue;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                AuthMessage? authMessage = null;

                try
                {
                    authMessage = JsonSerializer.Deserialize<AuthMessage>(message);
                }
                catch (Exception ex)
                {
                    Services.PluginLog.Warning($"Failed to parse WS message: {ex.Message}");
                    continue;
                }

                if (authMessage == null)
                    continue;

                HandleAuthMessage(authMessage);
            }
        }
        catch (Exception ex)
        {
            Services.PluginLog.Error(ex, "ReceiveLoop crashed unexpectedly.");
        }
        finally
        {
            Services.PluginLog.Info("WebSocket receive loop ended.");
        }
    }

    private async void HandleAuthMessage(AuthMessage message)
    {
        if (message.Type != "authComplete")
            return;

        Services.PluginLog.Info($"{message.Provider.FirstCharToUpper()} auth complete for user {message.UserId}");
        OnAuthCompleted?.Invoke(message.Provider);

        if (ws?.State == WebSocketState.Open)
        {
            Services.PluginLog.Info("Closing WebSocket connection...");

            await ws.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Auth complete",
                CancellationToken.None
            );
        }
    }
}
