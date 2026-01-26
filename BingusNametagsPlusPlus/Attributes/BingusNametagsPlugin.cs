using System;
using System.Collections.Generic;
using BingusNametagsPlusPlus.Classes;

namespace BingusNametagsPlusPlus.Attributes
{
    /// <summary>
    /// BingusNametagsPlugin holds the metadata of a plugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class BingusNametagsPlugin(string name, string author, string description, string[]? unsupported = null) : Attribute
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
        /// Any unsupported plugins. These will be turned off when your nametag is enabled.
        /// </summary>
        public string[] Unsupported = unsupported ?? [];

        /// <summary>
        /// Whether the nametag is enabled.
        /// </summary>
        public bool Enabled = false;

        internal Dictionary<BingusNametagsNametag, Action<PlayerNametag>> Nametags = [];
    }
}
