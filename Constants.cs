using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BingusNametagsPlusPlus;

public static class Constants
{
	public const string Name = "BingusNametags++";
	public const string Guid = "bingus.nametagsplusplus";
    public const string Version = "1.3.7";

    public const ReleaseChannel Channel = ReleaseChannel.Beta;

    public static string BingusNametagsData =>
        Path.Combine(Application.dataPath[.. Application.dataPath.LastIndexOfAny(['/', '\\'])], "BingusNametags++");

    public static readonly Dictionary<string, string> SpecialBadgeIds = new()
	{
		// note: badges are applied in specified order, so put the icon you like more at the FRONT
		["3df1e7a71f3b9ef1"] = "bingus,beta",
		["defc9810769f1f55"] = "bingus,beta",
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