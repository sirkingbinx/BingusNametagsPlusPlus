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
    [BingusNametagsNametag("Default", 0f)]
    public static void UpdateDefaultNametag(PlayerNametag nametag)
    {

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

        nametag.Text = API.GetDefaultNametagText(nametag.Owner);
    }
}
