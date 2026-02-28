using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BloomBell.src.Application.Services;
using BloomBell.src.Domain.Events;
using BloomBell.src.Domain.Ports;
using BloomBell.src.Infrastructure.Configuration;
using BloomBell.src.Infrastructure.Discord;
using BloomBell.src.Infrastructure.Game;
using BloomBell.src.Infrastructure.Game.PartyList;
using BloomBell.src.Infrastructure.Network;
using BloomBell.src.Presentation.Components;
using BloomBell.src.Presentation.Windows;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;

namespace BloomBell;

/// <summary>
/// Composition root. Wires all layers together — Domain, Application,
/// Infrastructure, and Presentation — then hands control to Dalamud.
/// Contains no business logic.
/// </summary>
public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/bb";

    private readonly IDalamudPluginInterface pluginInterface;

    // Domain
    private readonly EventBus eventBus;

    // Infrastructure
    private readonly WebSocketClient webSocketClient;
    private readonly HttpPlatformClient httpPlatformClient;
    private readonly PartyListProvider partyListProvider;

    // Application
    private readonly AuthService authService;
    private readonly PlatformService platformService;
    private readonly PartyNotifier partyNotifier;

    // Presentation
    private readonly WindowSystem windowSystem;
    private readonly MainWindow mainWindow;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        try
        {
            this.pluginInterface = pluginInterface;

            GameServices.Initialize(pluginInterface);
            var config = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();

            // --- Domain ---
            eventBus = new EventBus();

            // --- Infrastructure ---
            webSocketClient = new WebSocketClient();
            httpPlatformClient = new HttpPlatformClient();
            partyListProvider = new PartyListProvider();

            var discordOAuth = new DiscordOAuthProvider(webSocketClient);
            var providers = new Dictionary<string, IOAuthProvider>
            {
                ["discord"] = discordOAuth,
            };

            // --- Application ---
            authService = new AuthService(config, webSocketClient, eventBus, providers);
            platformService = new PlatformService(httpPlatformClient, config, pluginInterface);
            partyNotifier = new PartyNotifier(config);

            // --- Presentation ---
            var iconPath = Path.Combine(pluginInterface.AssemblyLocation.Directory!.FullName, "resources", "icon.png");
            var iconTexture = GameServices.TextureProvider.GetFromFile(iconPath);

            var headerComponent = new HeaderComponent(partyListProvider, iconTexture);
            var integrationsTab = new IntegrationsTab(authService, platformService, eventBus);
            var partyTab = new PartyTab(config, pluginInterface);

            mainWindow = new MainWindow(
                pluginInterface.Manifest.Name,
                headerComponent,
                integrationsTab,
                partyTab
            );

            windowSystem = new WindowSystem(pluginInterface.Manifest.InternalName);
            windowSystem.AddWindow(mainWindow);

            // --- Hooks ---
            partyListProvider.OnEvent += OnPartyChanged;

            GameServices.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Use /bb to toggle the main window."
            });

            pluginInterface.UiBuilder.Draw += windowSystem.Draw;
            pluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
        }
        catch (Exception exception)
        {
            GameServices.PluginLog.Error(exception, "Failed to initialize plugin.");
            Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        pluginInterface?.UiBuilder.Draw -= windowSystem.Draw;
        pluginInterface?.UiBuilder.OpenMainUi -= ToggleMainUi;
        partyListProvider.OnEvent -= OnPartyChanged;

        GameServices.CommandManager.RemoveHandler(CommandName);

        windowSystem?.RemoveAllWindows();

        mainWindow?.Dispose();
        authService?.Dispose();
        webSocketClient?.Dispose();
        partyNotifier?.Dispose();
        partyListProvider?.Dispose();
        eventBus?.Dispose();
    }

    private void ToggleMainUi() => mainWindow.Toggle();

    private void OnPartyChanged(bool status, PartyListMemberInfo member)
    {
        if (!GameServices.ClientState.IsLoggedIn ||
            GameServices.PlayerState.ContentId == 0
        ) return;

        var currentPartySize = partyListProvider.GetPartySize();

        Task.Run(async () => partyNotifier.UpdateAsync(
                currentPartySize,
                GameServices.PlayerState.ContentId,
                partyListProvider.IsCrossWorld
            )
        );
    }

    private void OnCommand(string command, string args)
    {
        mainWindow.Toggle();
    }
}
