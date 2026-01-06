using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;
using UnityEngine;

namespace BingusNametagsPlusPlus.Nametags;

public class AdvancedNametag : IBaseNametag
{
    public string Name => "Advanced";
    public string Description => "Includes some extra details, like FPS and m/s. This is often used for testing the latest features, beware!";
    public string Author => "Bingus";
    public float Offset => 0.35f;
    public List<string> Unsupported => [];
    public bool Enabled { get; set; } = false;

    public void UpdateNametag(PlayerNametag nametag)
    {
        nametag.Size = ConfigManager.NametagScale * 0.85f;
        nametag.AddStyle("color", "#d6d6d6");
        nametag.Text = $"{nametag.Owner.fps} fps / {nametag.Owner.LatestVelocity().magnitude:F1} m/s";
    }
}
