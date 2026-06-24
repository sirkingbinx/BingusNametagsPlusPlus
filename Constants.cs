using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BingusNametagsPlusPlus;

public static class Constants
{
	public const string Name = "BingusNametags++";
	public const string Guid = "bingus.nametagsplusplus";
    public const string Version = "1.5.1";

    public const ReleaseChannel Channel = ReleaseChannel.Stable;

    public static string BingusNametagsData =>
        Path.Combine(Application.dataPath[..Application.dataPath.LastIndexOfAny(['/', '\\'])], "BingusNametags++");
}

public enum ReleaseChannel
{
    Stable = 0,
    Beta = 1
}