using System;
using System.Text;

namespace Unity
{
	public static class Binary
	{
		public static short ToBigEndianUint16(short u)
		{
			return (short)(u >> 8 | (int)u << 8);
		}
		
		public static uint BigEndian(uint u)
		{
			return u << 24 | (u & 65280U) << 8 | (u & 16711680U) >> 8 | u >> 24;
		}
		
		public static uint ToBigEndian(uint u)
		{
			return u >> 24 | (u & 16711680U) >> 8 | (u & 65280U) << 8 | u << 24;
		}
	}
}
