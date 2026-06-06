using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace BingusNametagsPlusPlus.Nametags;

[BingusNametagsPlugin("Default", "Bingus", "The default nametag provided by BingusNametags++. Includes platform icons and a nametag.")]
public class DefaultNametag : IBaseNametag
{
    private static string GetPlatformString(VRRig rig)
    {
        var platform = API.GetPlatform(rig);

        return platform switch
        {
            API.Platform.Quest => "<sprite name=\"meta\">",
            API.Platform.OculusRift => "<sprite name=\"oculus\">",
            API.Platform.SteamVR => "<sprite name=\"steam\">",
            API.Platform.PCBasedPlatform => "<sprite name=\"oculus\">≈", // ≈ denotes "almost"
            _ => "?"
        };
    }

    [BingusNametagsNametag("Default", 0f)]
    public static void UpdateDefaultNametag(PlayerNametag nametag)
    {
        var prefix = "";

        if (Config.Current.Icons)
        {
            if (Config.Current.UserIcons && Constants.SpecialBadgeIds.TryGetValue(nametag.Owner.Creator.UserId.ToLower(), out var n))
            {
                var adding = "";
                n.Split(",").ForEach(sprite => adding += $"<sprite name=\"{sprite}\">");

                prefix += adding;
            }

            if (Config.Current.PlatformIcons)
            {
                prefix += GetPlatformString(nametag.Owner);
            }
        }

        var shownNickname = Config.Current.SanitizeNicknames
            ? nametag.Owner.playerText1.text
            : nametag.Owner.Creator.NickName;

        if (Config.Current.GFriendsIntegration && nametag.Owner.playerText1.color != Color.white)
        {
            // this is just smarter way to add GFriends integration
            // shaved off a reference to GorillaFriends by just using the nametag text color
            var nametagColor = ColorUtility.ToHtmlStringRGB(nametag.Owner.playerText1.color);
            nametag.AddStyle("color", $"#{nametagColor}");
        }

        if (Config.Current.CustomNametags && nametag.Owner.Creator.GetPlayerRef().CustomProperties.TryGetValue("BingusNametags++", out var rawData))
        {
            var data = (Dictionary<string, object>)rawData;

            var color = (string)data["Color"];
            var bold = (bool)data["isBold"];
            var italic = (bool)data["isItalic"];
            var underlined = (bool)data["isUnderlined"];

            if (Config.ValidHexCode(color))
                nametag.AddStyle("color", $"#{color}");

            if (bold)
                nametag.AddStyle("b");
            else
                nametag.RemoveStyle("b");

            if (italic)
                nametag.AddStyle("i");
            else
                nametag.RemoveStyle("i");

            if (underlined)
                nametag.AddStyle("u");
            else
                nametag.RemoveStyle("u");
        }

        nametag.Text = $"{prefix}{shownNickname}";
    }
}
