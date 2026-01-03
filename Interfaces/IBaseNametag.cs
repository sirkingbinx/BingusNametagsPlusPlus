using BingusNametagsPlusPlus.Classes;
using System.Collections.Generic;
using System.Linq;
using BingusNametagsPlusPlus.Utilities;

namespace BingusNametagsPlusPlus.Interfaces
{
    /// <summary>
    /// The interface for defining custom nametags. This requires no activation from your end, most updates are done automatically by BingusNametags++.
    /// Please also define the following fields for proper interfacing:
    /// <code>
    /// public string Name => "name";
    /// public string Description => "description";
    /// public float Offset => 0f;
    /// public bool Enabled { get; set; } = true;
    /// </code>
    /// </summary>
    public interface IBaseNametag
    {
        /// <summary>
        /// The name used for displaying your nametag.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A short description of your nametag. Please make it short, this will be displayed on the plugins menu.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The author of the nametag. That's you!
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// The offset from the user's selected offset of your nametag. The default nametag is at offset 0f;
        /// </summary>
        public float Offset { get; }

        /// <summary>
        /// Declares if the nametag is visible and receives updates.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Only true if both 1). The nametag is enabled and 2). The nametags are globally enabled.
        /// Rather than using this, please use Enabled.
        /// </summary>
        private bool NametagEnabled => (Config.ShowingNametags && Enabled);

        /// <summary>
        /// <i>UpdateNametag()</i> is called for every user each frame to update their nametag.
        /// </summary>
        /// <param name="nametag">The nametag of the player. Use nametag.Owner to get it's owning VRRig. Use nametag.Text to set the text.</param>
        public void UpdateNametag(PlayerNametag nametag) { }

        internal void Update(Dictionary<VRRig, PlayerNametag> nametags, float offset)
        {
            if (!NametagEnabled && nametags.Count != 0)
            {
                nametags.ForEach(rig => rig.Value.Destroy());
                nametags.Clear();
            }

            if (!GorillaParent.hasInstance || !NametagEnabled)
                return;

            foreach (var pair in nametags.Where(p => !GorillaParent.instance.vrrigs.Contains(p.Key)))
            {
                pair.Value.Destroy();
                nametags.Remove(pair.Key);
            }

            foreach (var rig in GorillaParent.instance.vrrigs.Where(rig => rig != GorillaTagger.Instance.offlineVRRig))
            {
                if (!nametags.ContainsKey(rig))
                    nametags.Add(rig, Nametags.CreateNametagObject(rig));

                nametags[rig].UpdateSettings(offset);
                UpdateNametag(nametags[rig]);
            }
        }
    }
}
