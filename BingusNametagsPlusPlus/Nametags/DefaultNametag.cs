using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;
using System.Collections.Generic;

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

        if (cosmetics.Contains("s. first login"))
            return ("steam", true);
        if (cosmetics.Contains("first login") || cosmetics.Contains("game-purchase") || properties > 1)
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
                n.Split(",").ForEach(sprite => adding += $"<sprite name=\"{sprite}\"> ");

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

        nametag.Text = $"{prefix}{(ConfigManager.Default_ShowingName ? shownNickname : "")}";
    }
}