using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace BloomBell.src.Presentation.Windows;

/// <summary>
/// Main plugin window. Acts purely as a layout shell that delegates
/// rendering to individual UI components. Contains no business logic.
/// </summary>
public sealed class MainWindow : Window, IDisposable
{
    private readonly Components.HeaderComponent headerComponent;
    private readonly Components.IntegrationsTab integrationsTab;
    private readonly Components.PartyTab partyTab;

    public MainWindow(
        string windowName,
        Components.HeaderComponent headerComponent,
        Components.IntegrationsTab integrationsTab,
        Components.PartyTab partyTab
    ) : base($"{windowName}##Main")
    {
        this.headerComponent = headerComponent;
        this.integrationsTab = integrationsTab;
        this.partyTab = partyTab;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 280),
            MaximumSize = new Vector2(500, 600),
        };

        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    }

    public void Dispose()
    {
        integrationsTab.Dispose();
    }

    public override void Draw()
    {
        headerComponent.Draw();

        ImGui.Spacing();

        if (ImGui.BeginTabBar("##MainTabs", ImGuiTabBarFlags.None))
        {
            if (ImGui.BeginTabItem("Integrations"))
            {
                integrationsTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Party"))
            {
                partyTab.Draw();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        integrationsTab.Reset();
    }
}
