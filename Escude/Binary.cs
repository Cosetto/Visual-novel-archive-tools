using System;
using System.Text;

namespace Escu
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
		public static short ToBigEndianUint16(short u)
		{
			return (short)(u >> 8 | (int)u << 8);
		}
		public static int BigEndian(int i)
		{
			return (int)Binary.BigEndian((uint)i);
		}
		public static ushort BigEndian(ushort u)
		{
			return (ushort)((int)u << 8 | u >> 8);
		}
		public static short BigEndian(short i)
		{
			return (short)Binary.BigEndian((ushort)i);
		}
		public static ulong BigEndian(ulong u)
		{
			return (ulong)Binary.BigEndian((uint)(u & (ulong)uint.MaxValue)) << 32 | (ulong)Binary.BigEndian((uint)(u >> 32));
		}
		public static long BigEndian(long i)
		{
			return (long)Binary.BigEndian((ulong)i);
		}
		public static void CopyOverlapped(byte[] data, int src, int dst, int count)
		{
			if (dst > src)
			{
				while (count > 0)
				{
					int num = Math.Min(dst - src, count);
					Buffer.BlockCopy(data, src, data, dst, num);
					dst += num;
					count -= num;
				}
				return;
			}
			Buffer.BlockCopy(data, src, data, dst, count);
		}
		public static string GetCString(byte[] data, int index, int length_limit, Encoding enc)
		{
			int num = 0;
			while (num < length_limit && data[index + num] != 0)
			{
				num++;
			}
			return enc.GetString(data, index, num);
		}
	}
}
