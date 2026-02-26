using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using BloomBell.src.services;
using System;
using BloomBell.src.Library.External.Game.PartyList;
using BloomBell.src.GUI.GameWindows;
using BloomBell.src.Configuration;
using BloomBell.src.Library.External.Services;

namespace BloomBell;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/bb";

    internal readonly IDalamudPluginInterface pluginInterface;
    internal readonly PartyListProvider partyListProvider;
    internal readonly PartyNotifier partyNotifier;
    internal readonly WebSocketHandler webSocketHandler;
    internal readonly AuthRouter authRouter;

    internal readonly WindowSystem WindowSystem;
    internal readonly MainWindow MainWindow;
    internal readonly PluginConfiguration Configuration;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        try
        {
            this.pluginInterface = pluginInterface;

            GameServices.Initialize(pluginInterface);
            Configuration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();

            partyListProvider = new PartyListProvider();
            partyNotifier = new PartyNotifier();
            webSocketHandler = new WebSocketHandler();
            authRouter = new AuthRouter(Configuration, webSocketHandler);

            webSocketHandler.OnAuthCompleted += authRouter.HandleAuthCompleted;
            partyListProvider.OnEvent += OnPartyChanged;

            WindowSystem = new(pluginInterface.Manifest.InternalName);

            MainWindow = new MainWindow(this);
            WindowSystem.AddWindow(MainWindow);

            GameServices.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Use /bb to toggle the main window."
            });

            pluginInterface.UiBuilder.Draw += WindowSystem.Draw;
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
        pluginInterface?.UiBuilder.Draw -= WindowSystem.Draw;
        pluginInterface?.UiBuilder.OpenMainUi -= ToggleMainUi;
        partyListProvider.OnEvent -= OnPartyChanged;

        GameServices.CommandManager.RemoveHandler(CommandName);

        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();
        authRouter.Dispose();
        webSocketHandler?.Dispose();
        partyNotifier?.Dispose();
        partyListProvider?.Dispose();
    }

    public void ToggleMainUi() => MainWindow.Toggle();

    private void OnPartyChanged(bool status, PartyListMemberInfo member)
    {
        if (!GameServices.ClientState.IsLoggedIn ||
            GameServices.PlayerState.ContentId == 0
        ) return;

        var currentPartySize = partyListProvider.GetPartySize();

        Task.Run(async () => partyNotifier.UpdateAsync(currentPartySize, GameServices.PlayerState.ContentId));
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }
}
