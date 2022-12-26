using System;
using System.Text;

namespace PAC
{
	public static class Binary
	{
		public static byte RotByteR(byte v, int count)
		{
			count &= 7;
			return (byte)(v >> count | (int)v << 8 - count);
		}
		public static byte RotByteL(byte v, int count)
		{
			count &= 7;
			return (byte)((int)v << count | v >> 8 - count);
		}
	}
}
