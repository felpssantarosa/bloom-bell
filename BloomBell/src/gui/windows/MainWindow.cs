using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using BloomBell.src.integrations.discord;
using BloomBell.src.lib.game.partylist;
using BloomBell.src.lib.infra;
using System.Threading.Tasks;

namespace BloomBell.src.gui.windows;

public class MainWindow : Window, IDisposable
{
    private readonly PartyListProvider partyList;
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin) : base($"{plugin.pluginInterface.Manifest.Name}##Main")
    {
        this.plugin = plugin;
        partyList = plugin.partyListProvider;

        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        SizeConstraints = new() { MinimumSize = new(375, 330) };
    }

    public void Dispose() { }

    public override async void Draw()
    {
        ImGui.Text("Discord Integration");

        if (!plugin.Configuration.DiscordLinked)
        {
            ImGui.TextWrapped("Connect your Discord account to receive DM notifications.");
            var pluginUserId = Services.PlayerState.ContentId.ToString();

            if (ImGui.Button("Connect Discord"))
            {
                await plugin.authRouter.Authenticate("discord");
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
            var totalPartyMembers = partyList.GetPartySize();

            ImGui.Text($"Party members: {totalPartyMembers}");
        }
    }
}
