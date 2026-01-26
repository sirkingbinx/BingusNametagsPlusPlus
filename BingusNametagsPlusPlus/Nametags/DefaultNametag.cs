using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace BingusNametagsPlusPlus.Nametags;

[BingusNametagsPlugin("Default", "Bingus", "The default nametag provided by BingusNametags++. Includes platform icons and a nametag.", 0f)]
public class DefaultNametag : IBaseNametag
{
    public Dictionary<VRRig, string> CachedPlatforms = [ ];

    private (string platform, bool loaded) GetPlatformString(VRRig player)
    {
        var cosmetics = player.rawCosmeticString.ToLower();

        if (!player.InitializedCosmetics)
            return ("meta", false);
        if (cosmetics != "" && CachedPlatforms.TryGetValue(player, out var cachedPlatform))
            return (cachedPlatform,true);

        var properties = player.OwningNetPlayer.GetPlayerRef().CustomProperties.Count;

        // these funny ranked variables were discovered by golden, thx
        /*
            Copyright (c) 2025 GoldenIsAProtogen
           
            Permission is hereby granted, free of charge, to any person obtaining a copy
            of this software and associated documentation files (the "Software"), to use,
            copy, modify, and redistribute the Software.

            https://github.com/GoldenIsAProtogen/GoldensGorillaNametags/blob/main/LICENSE
         */

        if (player.currentRankedSubTierPC > 0)
            return ("steam", true);
        if (player.currentRankedSubTierQuest > 0)
            return ("meta", true);

        if (cosmetics.Contains("s. first login"))
            return ("steam", true);
        if (cosmetics.Contains("first login") || properties > 1)
            return ("oculus", true);

        return ("meta", true);
    }

    public void UpdateNametag(PlayerNametag nametag)
    {
        var prefix = "";

        if (ConfigManager.Icons)
        {
            if (ConfigManager.UserIcons && Constants.SpecialBadgeIds.TryGetValue(nametag.Owner.OwningNetPlayer.UserId.ToLower(), out var n))
            {
                var adding = "";
                n.Split(",").ForEach(sprite => adding += $"<sprite name=\"{sprite}\">");

                prefix += adding;
            }

            if (ConfigManager.PlatformIcons)
            {
                var platformData = GetPlatformString(nametag.Owner);
                prefix += $"<sprite name=\"{platformData.platform}\">{(!platformData.loaded ? "? " : "")}";
            }
        }

        var shownNickname = ConfigManager.SanitizeNicknames
            ? nametag.Owner.OwningNetPlayer.SanitizedNickName
            : nametag.Owner.OwningNetPlayer.NickName;

        if (ConfigManager.GFriendsIntegration && nametag.Owner.playerText1.color != Color.white)
        {
            // this is just smarter way to add GFriends integration
            // shaved off a reference to GorillaFriends by just using the nametag text color
            var nametagColor = ColorUtility.ToHtmlStringRGB(nametag.Owner.playerText1.color);
            nametag.AddStyle("color", $"#{nametagColor}");
        }

        if (ConfigManager.CustomNametags && nametag.Owner.OwningNetPlayer.GetPlayerRef().CustomProperties.TryGetValue("BingusNametags++", out var rawData))
        {
            var data = (Dictionary<string, object>)rawData;

            var color = (string)data["Color"];
            var bold = (bool)data["isBold"];
            var italic = (bool)data["isItalic"];
            var underlined = (bool)data["isUnderlined"];

            if (ConfigManager.ValidHexCode(color))
                nametag.AddStyle("color", $"\"#{color}\"");

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

        nametag.Text = $"{prefix}{(ConfigManager.Default_ShowingName ? shownNickname : "")}";
    }
}