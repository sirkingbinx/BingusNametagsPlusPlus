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

    private static string GetPlatformString(VRRig player)
    {
        var cosmetics = player.rawCosmeticString;
        var properties = player.OwningNetPlayer.GetPlayerRef().CustomProperties.Count;

        return cosmetics.Contains("s. first login") ? "steam" : (cosmetics.Contains("first login") || cosmetics.Contains("game-purchase") || properties > 1) ? "oculus" : "meta";
    }

    public void UpdateNametag(PlayerNametag nametag)
    {
        var prefix = "";

        if (ConfigManager.Icons)
        {
            if (ConfigManager.UserIcons &&
                Constants.SpecialBadgeIds.TryGetValue(nametag.Owner.OwningNetPlayer.UserId.ToLower(), out var n))
            {
                var adding = "";
                n.Split(",").ForEach(sprite => adding += $"<sprite name=\"{sprite}\"> ");

                if (n.Split(",").Contains("dev"))
                    nametag.AddStyle("color", "3dc5d4");

                prefix += adding;
            }

            if (ConfigManager.PlatformIcons)
                prefix += $"<sprite name=\"{GetPlatformString(nametag.Owner)}\">";
        }

        nametag.Text = ($"{prefix}{(ConfigManager.Default_ShowingName ? nametag.Owner.OwningNetPlayer.NickName : "")}");
    }
}