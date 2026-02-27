using System.Numerics;

namespace BloomBell.src.Presentation.Theme;

/// <summary>
/// Centralized color palette for the plugin UI.
/// All components reference these constants instead of defining their own.
/// </summary>
public static class Colors
{
    public static readonly Vector4 Accent = new(0.42f, 0.60f, 0.90f, 1.00f);
    public static readonly Vector4 Success = new(0.30f, 0.85f, 0.45f, 1.00f);
    public static readonly Vector4 Warning = new(0.95f, 0.75f, 0.20f, 1.00f);
    public static readonly Vector4 MutedText = new(0.70f, 0.70f, 0.70f, 1.00f);
    public static readonly Vector4 SectionBackground = new(0.14f, 0.14f, 0.17f, 1.00f);
}
