using System.Numerics;
using BloomBell.src.Infrastructure.Configuration;
using BloomBell.src.Presentation.Theme;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin;

namespace BloomBell.src.Presentation.Components;

/// <summary>
/// Renders the Party tab content: notification trigger settings and behavior toggles.
/// </summary>
public sealed class PartyTab
{
    private readonly PluginConfiguration configuration;
    private readonly IDalamudPluginInterface pluginInterface;

    public PartyTab(PluginConfiguration configuration, IDalamudPluginInterface pluginInterface)
    {
        this.configuration = configuration;
        this.pluginInterface = pluginInterface;
    }

    public void Draw()
    {
        ImGui.Dummy(new Vector2(0, 4));

        DrawSectionHeader("Notification Trigger");

        int maxInt = configuration.maxPartySize;

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Max party size");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.SliderInt("##MaxPartySize", ref maxInt, 1, 8))
        {
            configuration.maxPartySize = (byte)maxInt;
            configuration.Save(pluginInterface);
        }

        ImGui.Dummy(new Vector2(0, 6));
        DrawSectionHeader("Behavior");

        var pause = configuration.pauseNotifications;
        if (ImGui.Checkbox("Pause notifications", ref pause))
        {
            configuration.pauseNotifications = pause;
            configuration.Save(pluginInterface);
        }

        var notifyFocused = configuration.notifyWhenFocused;
        if (ImGui.Checkbox("Notify when window is focused", ref notifyFocused))
        {
            configuration.notifyWhenFocused = notifyFocused;
            configuration.Save(pluginInterface);
        }
    }

    private static void DrawSectionHeader(string label)
    {
        ImGui.TextColored(Colors.Accent, label);
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 2));
    }
}
