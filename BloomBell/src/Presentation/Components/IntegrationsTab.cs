using System;
using System.Numerics;
using System.Threading.Tasks;
using BloomBell.src.Application.Services;
using BloomBell.src.Domain.Events;
using BloomBell.src.Domain.Events.Auth;
using BloomBell.src.Presentation.Theme;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace BloomBell.src.Presentation.Components;

/// <summary>
/// Renders the Integrations tab content.
/// Subscribes to auth events to reflect the correct connection state.
/// </summary>
public sealed class IntegrationsTab : IDisposable
{
    private readonly AuthService authService;
    private readonly PlatformService platformService;
    private readonly EventBus eventBus;

    private bool isFetchingPlatforms = false;
    private bool hasFetchedPlatforms = false;
    private bool isAuthenticating = false;

    public IntegrationsTab(AuthService authService, PlatformService platformService, EventBus eventBus)
    {
        this.authService = authService;
        this.platformService = platformService;
        this.eventBus = eventBus;

        eventBus.Subscribe<AuthStateChangedEvent>(OnAuthStateChanged);
    }

    public void Dispose()
    {
        eventBus.Unsubscribe<AuthStateChangedEvent>(OnAuthStateChanged);
    }

    public void Reset()
    {
        isFetchingPlatforms = false;
        hasFetchedPlatforms = false;
        isAuthenticating = false;
    }

    private void OnAuthStateChanged(AuthStateChangedEvent e)
    {
        switch (e.State)
        {
            case AuthState.Started:
                isAuthenticating = true;
                break;

            case AuthState.Completed:
                isAuthenticating = false;
                hasFetchedPlatforms = false;
                isFetchingPlatforms = false;
                break;

            case AuthState.Cancelled:
            case AuthState.Failed:
                isAuthenticating = false;
                break;
        }
    }

    public void Draw()
    {
        ImGui.Dummy(new Vector2(0, 4));

        DrawSectionHeader("Discord");

        if (!hasFetchedPlatforms && !isFetchingPlatforms && !isAuthenticating)
        {
            isFetchingPlatforms = true;
            _ = FetchPlatformStatusAsync();
        }

        if (isAuthenticating)
        {
            ImGui.TextColored(Colors.MutedText, "Waiting for authentication...");
            ImGui.SameLine();

            using (ImRaii.PushColor(ImGuiCol.Button, Colors.Warning))
            using (ImRaii.PushColor(ImGuiCol.ButtonHovered, Colors.Warning * new Vector4(1, 1, 1, 0.85f)))
            {
                if (ImGui.SmallButton("Cancel"))
                {
                    authService.CancelAuthentication();
                }
            }
        }
        else if (isFetchingPlatforms)
        {
            ImGui.TextColored(Colors.MutedText, "Checking connection...");
        }
        else if (platformService.CurrentStatus?.Discord == true)
        {
            ImGui.TextColored(Colors.Success, "\u2713 Discord account linked");
        }
        else
        {
            ImGui.TextColored(Colors.MutedText, "Not connected.");
            ImGui.SameLine();

            using (ImRaii.PushColor(ImGuiCol.Button, Colors.Accent))
            using (ImRaii.PushColor(ImGuiCol.ButtonHovered, Colors.Accent * new Vector4(1, 1, 1, 0.85f)))
            {
                if (ImGui.SmallButton("Connect"))
                {
                    authService.AuthenticateWith("discord");
                }
            }
        }
    }

    private async Task FetchPlatformStatusAsync()
    {
        try
        {
            await platformService.RefreshAsync();
        }
        finally
        {
            isFetchingPlatforms = false;
            hasFetchedPlatforms = true;
        }
    }

    private static void DrawSectionHeader(string label)
    {
        ImGui.TextColored(Colors.Accent, label);
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 2));
    }
}
