using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;

namespace BingusNametagsPlusPlus.Nametags;

[BingusNametagsPlugin("Advanced", "Bingus", "Extra nametag fields with FPS and speed in m/s.", 0.5f)]
public class AdvancedNametag : IBaseNametag
{
    public void UpdateNametag(PlayerNametag nametag)
    {
        nametag.Size = ConfigManager.Scale * 0.85f;
        nametag.AddStyle("color", "#d6d6d6");
        nametag.Text = $"{nametag.Owner.fps} fps / {nametag.Owner.LatestVelocity().magnitude:F1} m/s";
    }
}
