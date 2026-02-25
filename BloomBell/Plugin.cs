using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using BloomBell.src.gui.windows;
using BloomBell.src.lib.infra;
using BloomBell.src.config;
using FFXIVClientStructs.FFXIV.Client.UI.Arrays;
using BloomBell.src.lib.game.partylist;

namespace BloomBell;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPartyList PartyList { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    private readonly PartyListProvider partyListProvider;
    private readonly PartyNotifier partyNotifier;

    private const string CommandName = "/warny";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("BloomBell");
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Services.Initialize(PluginInterface);
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        partyListProvider = new PartyListProvider();
        partyNotifier = new PartyNotifier();

        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        Services.Framework.Update += OnFrameworkUpdate;
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!ClientState.IsLoggedIn || PlayerState.ContentId == 0)
            return;

        var currentPartySize = this.partyListProvider.GetPartySize();

        _ = partyNotifier.UpdateAsync(currentPartySize, PlayerState.ContentId);
    }

    public void Dispose()
    {
        partyNotifier?.Dispose();
        partyListProvider?.Dispose();

        Services.Framework.Update -= OnFrameworkUpdate;

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }
    public void ToggleMainUi() => MainWindow.Toggle();
}
