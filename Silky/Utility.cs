using System;
using System.Text;

namespace Silky
{
	public static class Binary
	{
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
