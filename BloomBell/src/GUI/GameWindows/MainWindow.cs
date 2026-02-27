using System;
using System.Numerics;
using System.Threading.Tasks;
using BloomBell.src.Library.External.Game.PartyList;
using BloomBell.src.Library.External.Services;
using Dalamud.Bindings.ImGui;
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

    public MainWindow(Plugin plugin) : base($"{plugin.pluginInterface.Manifest.Name}##Main")
    {
        this.plugin = plugin;
        partyList = plugin.partyListProvider;

        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        SizeConstraints = new() { MinimumSize = new(375, 330) };
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Discord Integration");

        if (!hasFetchedPlatforms && !isFetchingPlatforms)
        {
            isFetchingPlatforms = true;
            _ = FetchPlatformStatusAsync();
        }

        if (isFetchingPlatforms)
        {
            ImGui.Text("Checking Discord connection...");
        }
        else if (connectedPlatforms?.Discord == true || (plugin.Configuration.DiscordLinked && canTrustConfiguration))
        {
            plugin.Configuration.DiscordLinked = true;
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

        ImGui.Spacing();

        using var child = ImRaii.Child("Scrollbar", Vector2.Zero, true);
        if (child.Success)
        {
            var totalPartyMembers = partyList.GetPartySize();
            ImGui.Text($"Party members: {totalPartyMembers}");
        }
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
}
