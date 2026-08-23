using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using System;
using UnityEngine;

namespace BingusNametagsPlusPlus.Nametags;

[BingusNametagsPlugin("Advanced", "Bingus", "An advanced nametag that displays color, speed, and FPS.")]
public class AdvancedNametag : IBaseNametag
{
    public const string colorSquare = "██";

    [BingusNametagsNametag("Advanced", -0.15f)]
    public static void UpdateNametag(PlayerNametag nametag)
    {
        int fps = nametag.Owner.fps;
        float speed = nametag.Owner.LatestVelocity().magnitude;

        Color playerColor = nametag.Owner.playerColor;

        int r = (int)Math.Round((double)(playerColor.r * 9), MidpointRounding.AwayFromZero);
        int g = (int)Math.Round((double)(playerColor.g * 9), MidpointRounding.AwayFromZero);
        int b = (int)Math.Round((double)(playerColor.b * 9), MidpointRounding.AwayFromZero);

        string colorHex = ColorUtility.ToHtmlStringRGB(playerColor);
        string colorStr = $"""<color="red">{r}</color><color="green">{g}</color><color="blue">{b}</color>""";

        nametag.Text = $"<color=#{colorHex}>{colorSquare}</color> {colorStr} {fps} fps {speed:F1} m/s";
    }
}
