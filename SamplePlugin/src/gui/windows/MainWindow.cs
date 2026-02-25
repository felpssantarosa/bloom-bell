using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using SamplePlugin.src.integrations.discord;
using SamplePlugin.src.lib.game.partylist;

namespace SamplePlugin.src.gui.windows;

public class MainWindow : Window, IDisposable
{
    private readonly PartyListProvider partyList;
    private readonly Plugin plugin;
    private readonly DiscordIntegration discordIntegration;

    public MainWindow(Plugin plugin)
        : base("Warny##Main", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.discordIntegration = new DiscordIntegration();
        this.partyList = new PartyListProvider();
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Discord Integration");

        if (!plugin.Configuration.DiscordLinked)
        {
            ImGui.TextWrapped("Connect your Discord account to receive DM notifications.");
            var pluginUserId = Plugin.PlayerState.ContentId.ToString();

            if (ImGui.Button("Connect Discord"))
            {
                this.discordIntegration.OpenDiscordOAuth(pluginUserId);
            }
        }
        else
        {
            ImGui.TextColored(new Vector4(0, 1, 0, 1), "Discord account linked!");
        }

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
