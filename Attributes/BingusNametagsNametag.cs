using System;

namespace BingusNametagsPlusPlus.Attributes
{
    /// <summary>
    /// Defines a nametag that will be updated each frame.
    /// </summary>
    /// <param name="name">The name of this specific nametag. For example, "speed_counter" or "fps_counter".</param>
    /// <param name="offset">The offset from the default nametag's position. The default nametag's offset is 0f.</param>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class BingusNametagsNametag(string name, float offset) : Attribute
    {
        /// <summary>
        /// The name of this specific nametag.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// The offset from the default nametag's position. The default nametag's offset is 0f.
        /// </summary>
        public float Offset => offset;
    }
}
