using System;
using System.Collections.Generic;

namespace Unity
{
	public static class BigEndian
	{
		public static ushort ToUInt16<TArray>(TArray value, int index) where TArray : IList<byte>
		{
			return (ushort)((int)value[index] << 8 | (int)value[index + 1]);
		}
		public static short ToInt16<TArray>(TArray value, int index) where TArray : IList<byte>
		{
			return (short)((int)value[index] << 8 | (int)value[index + 1]);
		}
		public static uint ToUInt32<TArray>(TArray value, int index) where TArray : IList<byte>
		{
			return (uint)((int)value[index] << 24 | (int)value[index + 1] << 16 | (int)value[index + 2] << 8 | (int)value[index + 3]);
		}
		public static int ToInt32<TArray>(TArray value, int index) where TArray : IList<byte>
		{
			return (int)BigEndian.ToUInt32<TArray>(value, index);
		}
		public static void Pack(ushort value, byte[] buf, int index)
		{
			buf[index] = (byte)(value >> 8);
			buf[index + 1] = (byte)value;
		}
		public static void Pack(uint value, byte[] buf, int index)
		{
			buf[index] = (byte)(value >> 24);
			buf[index + 1] = (byte)(value >> 16);
			buf[index + 2] = (byte)(value >> 8);
			buf[index + 3] = (byte)value;
		}
		public static void Pack(ulong value, byte[] buf, int index)
		{
			BigEndian.Pack((uint)(value >> 32), buf, index);
			BigEndian.Pack((uint)value, buf, index + 4);
		}
		public static void Pack(short value, byte[] buf, int index)
		{
			BigEndian.Pack((ushort)value, buf, index);
		}
		public static void Pack(int value, byte[] buf, int index)
		{
			BigEndian.Pack((uint)value, buf, index);
		}
		public static void Pack(long value, byte[] buf, int index)
		{
			BigEndian.Pack((ulong)value, buf, index);
		}
	}
}
