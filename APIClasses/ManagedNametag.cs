using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;

namespace BingusNametagsPlusPlus.APIClasses;

/// <summary>
/// A nametag whose features are fully managed by another mod.
/// </summary>
public class ManagedNametag
{
    /// <summary>
    /// The managed nametag's IBaseNametag. You must cast this back to your nametag's type before using any custom fields.
    /// </summary>
    public IBaseNametag? Nametag;

    /// <summary>
    /// Enable or disable the nametag.
    /// </summary>
    public bool Enabled
    {
        get => Nametag?.Metadata.Enabled ?? false;
        set
        {
            if (value && Nametag != null)
                PluginManager.EnablePlugin(Nametag);
            else if (Nametag != null)
                PluginManager.DisablePlugin(Nametag);
        }
    }
}
