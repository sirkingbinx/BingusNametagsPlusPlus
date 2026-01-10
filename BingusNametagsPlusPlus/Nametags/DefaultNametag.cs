using System.Collections.Generic;
using System.Linq;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;

namespace BingusNametagsPlusPlus.Nametags;

public class DefaultNametag : IBaseNametag
{
    public string Name => "Default";
    public string Description => "The default nametag provided by BingusNametags++. Includes platform icons and a nametag.";
    public string Author => "Bingus";
    public float Offset => 0f;
    public List<string> Unsupported => [];
    public bool Enabled { get; set; } = false;

    public Dictionary<VRRig, string> CachedPlatforms = [ ];

    private (string platform, bool loaded) GetPlatformString(VRRig player)
    {
        var cosmetics = player.rawCosmeticString;

        if (cosmetics == "")
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

                if (n.Split(",").Contains("dev"))
                    nametag.AddStyle("color", "3dc5d4");

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