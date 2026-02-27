using System.Numerics;
using BloomBell.src.Infrastructure.Game;
using BloomBell.src.Infrastructure.Game.PartyList;
using BloomBell.src.Presentation.Theme;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;

namespace BloomBell.src.Presentation.Components;

/// <summary>
/// Renders the plugin header with the icon and party member count.
/// </summary>
public sealed class HeaderComponent(PartyListProvider partyList, ISharedImmediateTexture iconTexture)
{
    public void Draw()
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

        var partySize = partyList.GetPartySize();
        var badgeColor = partySize > 0 ? Colors.Accent : Colors.MutedText;
        ImGui.TextColored(badgeColor, $"\u25CF {partySize}");
        ImGui.SameLine();
        ImGui.TextColored(Colors.MutedText, partySize == 1 ? "party member" : "party members");
        ImGui.EndGroup();

        ImGui.Separator();
    }
}
