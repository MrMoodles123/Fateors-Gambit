namespace Projectiles
{
	using System.Runtime.CompilerServices;

	public static class StringExtensions
	{
		// if the string has a value, return true, else return false
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasValue(this string value)
		{
			return string.IsNullOrEmpty(value) == false;
		}
	}
}
