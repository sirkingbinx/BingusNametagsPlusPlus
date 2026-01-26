using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BingusNametagsPlusPlus;

public static class Constants
{
	public const string Name = "BingusNametags++";
	public const string Guid = "bingus.nametagsplusplus";
    public const string Version = "1.3.4";

    public const ReleaseChannel Channel = ReleaseChannel.Beta;

    public static string BingusNametagsData =>
        Path.Combine(Application.dataPath[.. Application.dataPath.LastIndexOf("/")], "BingusNametags++");

    public static readonly Dictionary<string, string> SpecialBadgeIds = new()
	{
		["a0454e65cad418df"] = "beta,bingus", // this a me! [steamvr]
		["defc9810769f1f55"] = "beta,bingus", // this a me! [oculus rift]
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
	Beta = 1,
	Development = 2
}