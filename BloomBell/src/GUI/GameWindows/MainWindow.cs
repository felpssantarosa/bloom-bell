using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using BloomBell.src.Library.External.Game.PartyList;
using BloomBell.src.Library.External.Services;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace BloomBell.src.GUI.GameWindows;

public class MainWindow : Window, IDisposable
{
    private readonly PartyListProvider partyList;
    private readonly Plugin plugin;
    private readonly ISharedImmediateTexture iconTexture;

    private bool isFetchingPlatforms = false;
    private bool hasFetchedPlatforms = false;
    private bool canTrustConfiguration = false;
    private NotificationPlatforms? connectedPlatforms;

    // Style constants
    private static readonly Vector4 AccentColor = new(0.42f, 0.60f, 0.90f, 1.00f);
    private static readonly Vector4 SuccessColor = new(0.30f, 0.85f, 0.45f, 1.00f);
    private static readonly Vector4 MutedTextColor = new(0.70f, 0.70f, 0.70f, 1.00f);
    private static readonly Vector4 SectionBgColor = new(0.14f, 0.14f, 0.17f, 1.00f);

    public MainWindow(Plugin plugin) : base($"{plugin.pluginInterface.Manifest.Name}##Main")
    {
        this.plugin = plugin;
        partyList = plugin.partyListProvider;

        var iconPath = Path.Combine(plugin.pluginInterface.AssemblyLocation.DirectoryName!, "images", "icon.png");
        iconTexture = GameServices.TextureProvider.GetFromFile(iconPath);

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 280),
            MaximumSize = new Vector2(500, 600),
        };

        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    }

    public void Dispose() { }

    public override void Draw()
    {
        DrawHeader();

        ImGui.Spacing();

        if (ImGui.BeginTabBar("##MainTabs", ImGuiTabBarFlags.None))
        {
            if (ImGui.BeginTabItem("Integrations"))
            {
                DrawIntegrationsTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Party"))
            {
                DrawPartyTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawHeader()
    {
        var wrap = iconTexture.GetWrapOrDefault();
        if (wrap != null)
        {
            ImGui.Image(wrap.Handle, new Vector2(64, 64));
            ImGui.SameLine();
        }

        var iconSize = 64f;
        var lineHeight = ImGui.GetTextLineHeightWithSpacing();
        var groupHeight = lineHeight * 2;
        var offsetY = (iconSize - groupHeight) * 0.5f;
        if (offsetY > 0) ImGui.SetCursorPosY(ImGui.GetCursorPosY() + offsetY);

        ImGui.BeginGroup();
        ImGui.Text("Bloom Bell");

        // Highlighted party member count
        var partySize = partyList.GetPartySize();
        var badgeColor = partySize > 0 ? AccentColor : MutedTextColor;
        ImGui.TextColored(badgeColor, $"\u25CF {partySize}");
        ImGui.SameLine();
        ImGui.TextColored(MutedTextColor, partySize == 1 ? "party member" : "party members");
        ImGui.EndGroup();

        ImGui.Separator();
    }

    private void DrawIntegrationsTab()
    {
        ImGui.Dummy(new Vector2(0, 4));

        DrawSectionHeader("Discord");

        if (!hasFetchedPlatforms && !isFetchingPlatforms)
        {
            isFetchingPlatforms = true;
            _ = FetchPlatformStatusAsync();
        }

        if (isFetchingPlatforms)
        {
            ImGui.TextColored(MutedTextColor, "Checking connection...");
        }
        else if (connectedPlatforms?.Discord == true || (plugin.PluginConfiguration.DiscordLinked && canTrustConfiguration))
        {
            plugin.PluginConfiguration.DiscordLinked = true;
            ImGui.TextColored(SuccessColor, "\u2713 Discord account linked");
        }
        else
        {
            ImGui.TextColored(MutedTextColor, "Not connected.");
            ImGui.SameLine();

            using (ImRaii.PushColor(ImGuiCol.Button, AccentColor))
            using (ImRaii.PushColor(ImGuiCol.ButtonHovered, AccentColor * new Vector4(1, 1, 1, 0.85f)))
            {
                if (ImGui.SmallButton("Connect") && !isFetchingPlatforms)
                {
                    isFetchingPlatforms = true;
                    _ = AuthenticateDiscordAsync();
                }
            }
        }
    }

    private void DrawPartyTab()
    {
        ImGui.Dummy(new Vector2(0, 4));

        DrawSectionHeader("Notification Trigger");

        // Max party size — label left, slider right
        int maxInt = plugin.PluginConfiguration.maxPartySize;

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Max party size");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.SliderInt("##MaxPartySize", ref maxInt, 1, 8))
        {
            plugin.PluginConfiguration.maxPartySize = (byte)maxInt;
            plugin.PluginConfiguration.Save(plugin.pluginInterface);
        }

        ImGui.Dummy(new Vector2(0, 6));
        DrawSectionHeader("Behavior");

        var pause = plugin.PluginConfiguration.pauseNotifications;
        if (ImGui.Checkbox("Pause notifications", ref pause))
        {
            plugin.PluginConfiguration.pauseNotifications = pause;
            plugin.PluginConfiguration.Save(plugin.pluginInterface);
        }

        var notifyFocused = plugin.PluginConfiguration.notifyWhenFocused;
        if (ImGui.Checkbox("Notify when window is focused", ref notifyFocused))
        {
            plugin.PluginConfiguration.notifyWhenFocused = notifyFocused;
            plugin.PluginConfiguration.Save(plugin.pluginInterface);
        }
    }

    private static void DrawSectionHeader(string label)
    {
        ImGui.TextColored(AccentColor, label);
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 2));
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
