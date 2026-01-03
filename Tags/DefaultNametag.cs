using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;

namespace BingusNametagsPlusPlus.Tags
{
    public class DefaultNametag : IBaseNametag
    {
        public string Name => "Default";
        public string Description => "The default nametag provided by BingusNametags++.";
        public float Offset => 0f;
        public bool Enabled { get; set; } = true;

        private static string GetPlatformString(VRRig player)
        {
            var cosmetics = player.concatStringOfCosmeticsAllowed.ToLower();
            var properties = player.OwningNetPlayer.GetPlayerRef().CustomProperties.Count;

            return cosmetics.Contains("s. first login") ? "steam" : (cosmetics.Contains("first login") || cosmetics.Contains("game-purchase") || properties > 1) ? "oculus" : "meta";
        }

        public void UpdateNametag(PlayerNametag nametag)
        {
            var prefix = "";

            if (Config.GlobalIconsEnabled)
            {
                if (Config.UserCustomIcons &&
                    Constants.SpecialBadgeIds.TryGetValue(nametag.Owner.OwningNetPlayer.UserId.ToLower(), out var n))
                {
                    var adding = "";
                    n.Split(",").ForEach(sprite => adding += $"<sprite name=\"{sprite}\"> ");
                    prefix += adding;
                }

                if (Config.ShowingPlatform)
                    prefix += $"<sprite name=\"{GetPlatformString(nametag.Owner)}\">";
            }

            nametag.Text = ($"{prefix}{(Config.ShowingName ? nametag.Owner.OwningNetPlayer.NickName : "")}");
        }
    }
}
