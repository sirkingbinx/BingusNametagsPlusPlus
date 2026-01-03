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
}