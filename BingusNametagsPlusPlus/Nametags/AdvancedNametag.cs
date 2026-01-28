using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;

namespace BingusNametagsPlusPlus.Nametags;

[BingusNametagsPlugin("Advanced", "Bingus", "Extra nametag fields with FPS and speed in m/s.")]
public class AdvancedNametag : IBaseNametag
{
    [BingusNametagsNametag("Advanced", -0.35f)]
    public static void UpdateNametag(PlayerNametag nametag)
    {
        nametag.Size = 0.85f;
        nametag.AddStyle("color", "#d6d6d6");
        nametag.Text = $"{nametag.Owner.fps} fps / {nametag.Owner.LatestVelocity().magnitude:F1} m/s";
    }
}
