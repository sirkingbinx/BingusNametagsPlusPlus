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

    public static readonly Dictionary<string, string> SpecialBadgeIds = new()
    {
        ["596994ce81d973e1"] = "bingus,beta",
        ["defc9810769f1f55"] = "bingus,beta",
        ["e678d10eca536d58"] = "beta",
        ["846e7dd5aceac0d4"] = "beta",
        ["68ccddc115fdc9fb"] = "beta",
        ["706572060708c655"] = "beta",
        ["7adb8b7e8f60e767"] = "beta",
        ["21f6d8f675c9234"]  = "beta"
    };
}

public enum ReleaseChannel
{
    Stable = 0,
    Beta = 1
}