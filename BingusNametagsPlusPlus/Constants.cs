using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BingusNametagsPlusPlus;

public static class Constants
{
	public const string Name = "BingusNametags++";
	public const string Guid = "bingus.nametagsplusplus";
	public const string Version = "1.3.0";

    public const ReleaseChannel Channel = ReleaseChannel.Stable;

    public static string BingusNametagsData =>
        Path.Combine(Application.dataPath[.. Application.dataPath.LastIndexOf("/")], "BingusNametags++");

    public static readonly Dictionary<string, string> SpecialBadgeIds = new()
	{
		["a0454e65cad418df"] = "dev,beta1,beta2", // this a me! [steamvr]
		["defc9810769f1f55"] = "dev,beta1,beta2", // this a me! [oculus rift]
		["846e7dd5aceac0d4"] = "beta1",
		["68ccddc115fdc9fb"] = "med,beta1",
		["706572060708c655"] = "golden,beta2",
		["7adb8b7e8f60e767"] = "beta2",
		["21f6d8f675c9234"]  = "beta2"
	};

	public static string AssemblyDirectory =>
		Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? @"C:\Users\Public";
}

public enum ReleaseChannel
{
	Stable = 0,
	Beta = 1,
	Development = 2
}