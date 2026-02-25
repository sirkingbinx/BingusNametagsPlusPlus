using System;
using BepInEx.Configuration;
using System.Collections.Generic;
using BingusNametagsPlusPlus.Utilities;

namespace BingusNametagsPlusPlus;

public static class Extensions
{
	public static bool Uninitialized<T>(this T obj)
	{
		return obj == null || obj.Equals(default(T));
	}

	public static string AsString(this ReleaseChannel obj)
	{
		switch (obj)
		{
			case ReleaseChannel.Stable:
				return "stable";
			default:
			case ReleaseChannel.Beta:
				return "beta";
		}
	}

    public static string Zip(this IEnumerable<object> enumerable, string seperator = ", ")
    {
        var str = "";
        enumerable.ForEach(e => str += $"{(str != "" ? seperator: "")}{e}");
        return str;
    }

    public static ConfigEntry<T> Get<T>(this ConfigFile file, string section, string name)
    {
        file.TryGetEntry(section, name, out ConfigEntry<T> thing);
        return thing;
    }

    public static void Report(this Exception ex) => LogManager.LogException(ex);
}