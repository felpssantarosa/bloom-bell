using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using SamplePlugin.src.lib.game.partylist;

namespace SamplePlugin.src.gui.windows;

public class MainWindow : Window, IDisposable
{
    private readonly PartyListProvider partyList;
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin, string goatImagePath)
        : base("Warny##Main", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.partyList = new PartyListProvider();
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings")) plugin.ToggleConfigUi();

        ImGui.Spacing();

        using var child = ImRaii.Child("Scrollbar", Vector2.Zero, true);

        if (child.Success)
        {
            if (this.partyList == null)
            {
                Plugin.Log.Warning("PartyListProvider returned null.");
                return;
            }

            var totalPartyMembers = this.partyList.GetPartySize();

            ImGui.Text($"Party members: {totalPartyMembers}");
        }
    }
}
