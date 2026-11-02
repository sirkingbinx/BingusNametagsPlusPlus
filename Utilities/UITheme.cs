using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

internal static class UITheme
{
    internal static readonly Color Background = Hex("12161e");
    internal static readonly Color Panel = Hex("192131");
    internal static readonly Color Input = Hex("111a29");
    internal static readonly Color Border = Hex("334159");
    internal static readonly Color InputBorder = Hex("3a4b68");
    internal static readonly Color Text = Hex("dce4f1");
    internal static readonly Color Muted = Hex("a6adb5");
    internal static readonly Color Label = Hex("aab2bc");
    internal static readonly Color Accent = Hex("79aaff");
    internal static readonly Color Primary = Hex("2d63cd");
    internal static readonly Color Hover = Hex("3b75e6");
    internal static readonly Color Selected = Hex("304361");
    internal static readonly Color Switch = Hex("3d78df");
    internal static readonly Color Focus = Hex("82adff");
    private static Font? font;

    internal static Font Font
    {
        get
        {
            if (font != null)
                return font;
            font = UnityEngine.Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Helvetica", "Liberation Sans", "DejaVu Sans", "Noto Sans CJK SC" }, 14);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString("#" + value, out var color);
        return color;
    }
}
