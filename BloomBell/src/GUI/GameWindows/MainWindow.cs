using System;
using System.Numerics;
using System.Threading.Tasks;
using BloomBell.src.Library.External.Game.PartyList;
using BloomBell.src.Library.External.Services;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace BloomBell.src.GUI.GameWindows;

public class MainWindow : Window, IDisposable
{
    private readonly PartyListProvider partyList;
    private readonly Plugin plugin;

    private bool isFetchingPlatforms = false;
    private bool hasFetchedPlatforms = false;
    private bool canTrustConfiguration = false;
    private NotificationPlatforms? connectedPlatforms;

    private enum MainTab { Integrations, Party }

    public MainWindow(Plugin plugin) : base($"{plugin.pluginInterface.Manifest.Name}##Main")
    {
        this.plugin = plugin;
        partyList = plugin.partyListProvider;

        Size = new Vector2(400, 300);
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(1, 1),
        };

        Flags = ImGuiWindowFlags.None;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var font = ImRaii.PushFont(UiBuilder.DefaultFont);
        ImGui.TextWrapped("Welcome to Bloom Bell! Use the tabs below to configure your settings and integrations.");

        ImGui.Spacing();

        ImGui.Text($"Party members: {partyList.GetPartySize()}");

        ImGui.Spacing();

        if (ImGui.BeginTabBar("MainTabs", ImGuiTabBarFlags.None))
        {
            if (ImGui.BeginTabItem("Integrations"))
            {
                DrawDiscordSection();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Party Settings"))
            {
                DrawPartySettings();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawDiscordSection()
    {
        ImGui.Dummy(new Vector2(0, 5));

        ImGui.Spacing();

        ImGui.Indent(10);

        ImGui.Text("Discord Integration");
        ImGui.Separator();
        ImGui.Spacing();

        if (!hasFetchedPlatforms && !isFetchingPlatforms)
        {
            isFetchingPlatforms = true;
            _ = FetchPlatformStatusAsync();
        }

        if (isFetchingPlatforms)
        {
            ImGui.Text("Checking Discord connection...");
            ImGui.SameLine();
            ImGui.ProgressBar((float)(Math.Sin(ImGui.GetTime() * 3) * 0.5 + 0.5), new Vector2(120, 0));
        }
        else if (connectedPlatforms?.Discord == true || (plugin.PluginConfiguration.DiscordLinked && canTrustConfiguration))
        {
            plugin.PluginConfiguration.DiscordLinked = true;
            ImGui.TextColored(new Vector4(0, 1, 0, 1), "Discord account linked!");
        }
        else
        {
            ImGui.TextWrapped("Connect your Discord account to receive DM notifications.");
            if (ImGui.Button("Connect Discord") && !isFetchingPlatforms)
            {
                isFetchingPlatforms = true;
                _ = AuthenticateDiscordAsync();
            }
        }
    }

    private void DrawPartySettings()
    {
        ImGui.Dummy(new Vector2(0, 5));

        ImGui.Spacing();

        ImGui.Indent(10);

        if (ImGui.BeginTable("PartyTable", 2, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableNextColumn();
            ImGui.Text("Max party size");
            ImGui.TableNextColumn();
            int maxInt = plugin.PluginConfiguration.maxPartySize;
            ImGui.PushItemWidth(44);
            if (ImGui.InputInt("##MaxPartySize", ref maxInt))
            {
                maxInt = Math.Clamp(maxInt, 1, 9);
                plugin.PluginConfiguration.maxPartySize = (byte)maxInt;
                plugin.PluginConfiguration.Save(plugin.pluginInterface);
            }
            ImGui.PopItemWidth();

            ImGui.TableNextColumn();
            ImGui.Text("Pause notifications");
            ImGui.TableNextColumn();
            bool pause = plugin.PluginConfiguration.pauseNotifications;
            if (ImGui.Checkbox("##PauseNotifications", ref pause))
            {
                plugin.PluginConfiguration.pauseNotifications = pause;
                plugin.PluginConfiguration.Save(plugin.pluginInterface);
            }

            ImGui.TableNextColumn();
            ImGui.Text("Notify when focused");
            ImGui.TableNextColumn();
            bool notifyFocused = plugin.PluginConfiguration.notifyWhenFocused;
            if (ImGui.Checkbox("##NotifyFocused", ref notifyFocused))
            {
                plugin.PluginConfiguration.notifyWhenFocused = notifyFocused;
                plugin.PluginConfiguration.Save(plugin.pluginInterface);
            }

            ImGui.EndTable();
        }

        ImGui.Unindent(10);
        ImGui.Dummy(new Vector2(0, 5));
    }

    private async Task FetchPlatformStatusAsync()
    {
        try
        {
            connectedPlatforms = await ConnectedPlatforms.GetAllAsync();
        }
        catch (Exception ex)
        {
            GameServices.PluginLog.Error(ex, "Failed to fetch connected platforms");
        }
        finally
        {
            isFetchingPlatforms = false;
            hasFetchedPlatforms = true;
        }
    }

    private async Task AuthenticateDiscordAsync()
    {
        try
        {
            await plugin.authRouter.AuthenticateWith("discord");
            canTrustConfiguration = true;
        }
        catch (Exception ex)
        {
            GameServices.PluginLog.Error(ex, "Discord authentication failed");
        }
        finally
        {
            isFetchingPlatforms = false;
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        isFetchingPlatforms = false;
        hasFetchedPlatforms = false;
        canTrustConfiguration = false;
    }
}
