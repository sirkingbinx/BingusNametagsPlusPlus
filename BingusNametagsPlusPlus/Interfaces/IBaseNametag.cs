using System;
using BingusNametagsPlusPlus.Classes;
using System.Linq;
using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Utilities;

namespace BingusNametagsPlusPlus.Interfaces
{
    /// <summary>
    /// The interface for defining custom nametags. This requires no activation from your end, most updates are done automatically by BingusNametags++.
    /// Add the attribute BingusNametagsPlugin to properly interface with BingusNametags++.
    /// </summary>
    public interface IBaseNametag
    {
        /// <summary>
        /// The metadata associated with your nametag.
        /// </summary>
        public BingusNametagsPlugin Metadata => PluginManager.PluginMetadata[this];

        /// <summary>
        /// <i>UpdateNametag()</i> is called for every user each frame to update their nametag.
        /// </summary>
        /// <param name="nametag">The nametag of the player. Use nametag.Owner to get it's owning VRRig. Use nametag.Text to set the text.</param>
        public void UpdateNametag(PlayerNametag nametag);

        internal void Update()
        {
            var AllowedToShowNametags = ConfigManager.Nametags && Main.PluginEnabled;

            Main.Nametags.TryAdd(this, []);
            var nametags = Main.Nametags[this];

            if (!AllowedToShowNametags && nametags.Count != 0)
            {
                nametags.ForEach(rig => rig.Value.Destroy());
                nametags.Clear();
            }

            if (!GorillaParent.hasInstance || !AllowedToShowNametags)
                return;

            foreach (var pair in nametags.Where(p => !GorillaParent.instance.vrrigs.Contains(p.Key)))
            {
                pair.Value.Destroy();
                nametags.Remove(pair.Key);
            }

            foreach (var rig in GorillaParent.instance.vrrigs.Where(rig =>
                         rig != GorillaTagger.Instance.offlineVRRig))
            {
                if (!nametags.ContainsKey(rig))
                    nametags.Add(rig, NametagCreator.CreateNametagObject(rig));
                try
                {
                    nametags[rig].UpdateSettings(Metadata.Offset);
                    UpdateNametag(nametags[rig]);
                } catch (Exception ex)
                {
                    // using raw TMP tags here so if the nametag error does not persist the nametag does not keep its nice red color
                    ex.Report();
                    nametags[rig].Text = $"<color=red>ERR: {ex.Message}</color>";
                }
            }
        }
    }
}
