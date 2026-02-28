using System;
using System.Collections.Generic;
using BloomBell.src.Domain.Events;
using BloomBell.src.Domain.Events.Auth;
using BloomBell.src.Domain.Ports;
using BloomBell.src.Infrastructure.Configuration;
using BloomBell.src.Infrastructure.Game;

namespace BloomBell.src.Application.Services;

/// <summary>
/// Orchestrates the OAuth authentication lifecycle.
/// Owns the auth state machine: Started → Completed | Cancelled | Failed.
/// Publishes domain events so that the UI and other consumers can react
/// without tight coupling.
/// </summary>
public sealed class AuthService : IDisposable
{
    private readonly PluginConfiguration configuration;
    private readonly IWebSocketClient webSocketClient;
    private readonly EventBus eventBus;
    private readonly Dictionary<string, IOAuthProvider> providers;

    private string? activeProvider;

    public bool IsAuthenticating => activeProvider is not null;

    public AuthService(
        PluginConfiguration configuration,
        IWebSocketClient webSocketClient,
        EventBus eventBus,
        Dictionary<string, IOAuthProvider> providers)
    {
        this.configuration = configuration;
        this.webSocketClient = webSocketClient;
        this.eventBus = eventBus;
        this.providers = providers;

        webSocketClient.OnAuthCompleted += HandleAuthCompleted;
        webSocketClient.OnAuthFailed += HandleAuthFailed;
        webSocketClient.OnDisconnected += HandleDisconnected;
    }

    public void AuthenticateWith(string provider)
    {
        var contentId = GameServices.PlayerState.ContentId;

        if (contentId == 0)
        {
            GameServices.PluginLog.Warning("Cannot authenticate: player not fully logged in.");
            return;
        }

        if (activeProvider is not null)
        {
            GameServices.PluginLog.Warning($"Authentication already in progress for: {activeProvider}");
            return;
        }

        var key = provider.ToLower();

        if (!providers.TryGetValue(key, out var oauthProvider))
        {
            GameServices.PluginLog.Warning($"Unknown provider: {provider}");
            return;
        }

        GameServices.PluginLog.Info($"Starting authentication for: {provider}");

        activeProvider = key;
        eventBus.Publish(new AuthStateChangedEvent(key, AuthState.Started));

        oauthProvider.Authenticate(contentId.ToString());
    }

    public void CancelAuthentication()
    {
        if (activeProvider is null) return;

        var provider = activeProvider;
        activeProvider = null;

        GameServices.PluginLog.Info($"Authentication cancelled for: {provider}");
        eventBus.Publish(new AuthStateChangedEvent(provider, AuthState.Cancelled));
    }

    private void HandleAuthCompleted(string provider)
    {
        var key = provider.ToLower();

        GameServices.PluginLog.Info($"Auth completed for: {key}");

        activeProvider = null;

        switch (key)
        {
            case "discord":
                configuration.DiscordLinked = true;
                break;
        }

        eventBus.Publish(new AuthStateChangedEvent(key, AuthState.Completed));
    }

    private void HandleAuthFailed(string provider, string error)
    {
        var key = provider.ToLower();

        GameServices.PluginLog.Warning($"Auth failed for {key}: {error}");

        activeProvider = null;
        eventBus.Publish(new AuthStateChangedEvent(key, AuthState.Failed));
    }

    private void HandleDisconnected()
    {
        if (activeProvider is null) return;

        var provider = activeProvider;
        activeProvider = null;

        GameServices.PluginLog.Warning($"WebSocket disconnected during auth — treating as cancellation for: {provider}");
        eventBus.Publish(new AuthStateChangedEvent(provider, AuthState.Cancelled));
    }

    public void Dispose()
    {
        webSocketClient.OnAuthCompleted -= HandleAuthCompleted;
        webSocketClient.OnAuthFailed -= HandleAuthFailed;
        webSocketClient.OnDisconnected -= HandleDisconnected;
    }
}
