using System;
using BingusNametagsPlusPlus.Utilities;

namespace BingusNametagsPlusPlus.Attributes
{
    /// <summary>
    /// BingusNametagsPlugin holds the metadata of a plugin.
    /// </summary>
    public class BingusNametagsPlugin(string name, string author, string description, float offset = float.NaN, string[]? unsupported = null) : Attribute
    {
        /// <summary>
        /// The display name of the nametag. This is used on the Plugins tab.
        /// </summary>
        public string Name = name;

        /// <summary>
        /// The author of the nametag. Hi!
        /// </summary>
        public string Author = author;

        /// <summary>
        /// A description explaining what your nametag does.
        /// </summary>
        public string Description = description;

        /// <summary>
        /// The offset of the nametag from the user's selected nametag offset.
        /// </summary>
        public float Offset = float.IsNaN(offset) ? PluginManager.DetermineOffset() : offset;

        /// <summary>
        /// Any unsupported plugins. These will be turned off when your nametag is enabled.
        /// </summary>
        public string[] Unsupported = unsupported ?? [];

        /// <summary>
        /// Whether the nametag is enabled.
        /// </summary>
        public bool Enabled = false;
    }
}
