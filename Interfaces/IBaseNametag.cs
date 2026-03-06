using System;
using System.Collections.Generic;
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
         
        internal void Update()
        {
            var allowedToShowNametags = ConfigManager.Nametags && Plugin.PluginEnabled;

            foreach (var pair in Metadata.Nametags)
            {
                var nametags = pair.Key.Nametags;

                if (!allowedToShowNametags && nametags.Count != 0)
                {
                    nametags.ForEach(rig => rig.Value.Destroy());
                    nametags.Clear();
                }

                if (!GorillaParent.hasInstance || !allowedToShowNametags)
                    return;

                foreach (var pair2 in nametags.Where(p => !VRRigCache.ActiveRigs.Contains(p.Key)))
                {
                    pair2.Value.Destroy();
                    nametags.Remove(pair2.Key);
                }

                foreach (var rig in VRRigCache.ActiveRigs.Where(rig => rig != GorillaTagger.Instance.offlineVRRig))
                {
                    if (!nametags.ContainsKey(rig))
                        nametags.Add(rig, NametagCreator.CreateNametagObject(rig));

                    try
                    {
                        nametags[rig].UpdateSettings(pair.Key.Offset);
                        pair.Value?.Invoke(nametags[rig]);
                    }
                    catch (Exception ex)
                    {
                        // using raw TMP tags here so if the nametag error does not persist the nametag does not keep its nice red color
                        ex.Report();
                        nametags[rig].Text = $"<color=red>ERR: {ex.Message}</color>";
                    }
                }
            }

        }
    }
}
