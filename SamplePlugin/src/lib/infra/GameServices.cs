#pragma warning disable Dalamud001

using System;
using System.Threading;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace SamplePlugin.src.lib.infra;

public class Services
{
    [PluginService] public static IUnlockState UnlockState { get; private set; } = null!;
    [PluginService] public static IToastGui ToastGui { get; private set; } = null!;
    [PluginService] public static ITitleScreenMenu TitleScreenMenu { get; private set; } = null!;
    [PluginService] public static ITextureSubstitutionProvider TextureSubstitutionProvider { get; private set; } = null!;
    [PluginService] public static ITextureReadbackProvider TextureReadbackProvider { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] public static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] public static ISeStringEvaluator SeStringEvaluator { get; private set; } = null!;
    [PluginService] public static ISelfTestRegistry SelfTestRegistry { get; private set; } = null!;
    [PluginService] public static IReliableFileStorage ReliableFileStorage { get; private set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] public static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] public static IPartyList PartyList { get; private set; } = null!;
    [PluginService] public static IPartyFinderGui PartyFinderGui { get; private set; } = null!;
    [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] public static INotificationManager NotificationManager { get; private set; } = null!;
    [PluginService] public static INamePlateGui NamePlateGui { get; private set; } = null!;
    [PluginService] public static IMarketBoard MarketBoard { get; private set; } = null!;
    [PluginService] public static IKeyState KeyState { get; private set; } = null!;
    [PluginService] public static IJobGauges JobGauges { get; private set; } = null!;
    [PluginService] public static IGamepadState GamepadState { get; private set; } = null!;
    [PluginService] public static IGameLifecycle GameLifecycle { get; private set; } = null!;
    [PluginService] public static IGameInventory GameInventory { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IFlyTextGui FlyTextGui { get; private set; } = null!;
    [PluginService] public static IFateTable FateTable { get; private set; } = null!;
    [PluginService] public static IDutyState DutyState { get; private set; } = null!;
    [PluginService] public static IDtrBar DtrBar { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IContextMenu ContextMenu { get; private set; } = null!;
    [PluginService] public static IConsole Console { get; private set; } = null!;
    [PluginService] public static ICondition Condition { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static IBuddyList BuddyList { get; private set; } = null!;
    [PluginService] public static IAetheryteList AetheryteList { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] public static IAddonEventManager AddonEventManager { get; private set; } = null!;

    private static int Initialized = 0;
    internal static Services Instance { get; private set; } = null!;

    public static void Initialize(IDalamudPluginInterface pluginInterface)
    {
        if (Interlocked.CompareExchange(ref Initialized, 1, 0) == 1) throw new InvalidOperationException("Already initialized");

        Instance = new();

        // if (pluginInterface.Inject(Instance)) throw new Exception("Dalamud Services Dependency Injection Failed");
        pluginInterface.Inject(Instance);
    }
}
