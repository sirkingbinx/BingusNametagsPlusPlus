namespace BingusNametagsPlusPlus;

/*
 * A minimized version of Cardboard
 */
public static class Extensions
{
	public static bool Uninitialized<T>(this T obj) =>
		obj == null || obj.Equals(default(T));
}