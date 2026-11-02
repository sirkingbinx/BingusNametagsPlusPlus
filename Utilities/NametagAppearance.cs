using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

public static class NametagAppearance
{
    public static bool TryParseColor(string value, out Color color)
    {
        color = Color.white;
        if (!Config.ValidHexCode(value))
            return false;
        string hex = value.TrimStart('#');
        if (hex.Length == 3)
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        return ColorUtility.TryParseHtmlString("#" + hex, out color);
    }
}
