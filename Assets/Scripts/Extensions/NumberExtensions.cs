namespace Projectiles
{
	public static class NumberExtensions
	{
		// checks if a bit is set in a byte
		public static bool IsBitSet(this byte flags, int bit)
		{
			return (flags & (1 << bit)) == (1 << bit);
		}

		// sets or clears a bit in a byte
		public static byte SetBit(ref this byte flags, int bit, bool value)
		{
			if (value == true)
				return flags |= (byte)(1 << bit);

			return flags &= unchecked((byte)~(1 << bit));
		}

		// checks if a bit is set in a int
		public static bool IsBitSet(this int flags, int bit)
		{
			return (flags & (1 << bit)) == (1 << bit);
		}

		// sets or clears a bit in an int
		public static int SetBit(ref this int flags, int bit, bool value)
		{
			if (value == true)
				return flags |= 1 << bit;

			return flags &= ~(1 << bit);
		}
	}
}
