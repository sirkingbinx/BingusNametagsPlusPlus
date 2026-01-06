using BepInEx.Configuration;
using System.Collections.Generic;

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
			case ReleaseChannel.Beta:
				return "beta";
			default:
			case ReleaseChannel.Development:
				return "dev";
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
}