using System;
using System.Collections.Generic;

namespace Escu
{
	public static class LittleEndian
	{
		public static ushort ToUInt16<TArray>(TArray value, int index) where TArray : IList<byte>
		{
			return (ushort)((int)value[index] | (int)value[index + 1] << 8);
		}
		public static short ToInt16<TArray>(TArray value, int index) where TArray : IList<byte>
		{
			return (short)((int)value[index] | (int)value[index + 1] << 8);
		}
		public static uint ToUInt32<TArray>(TArray value, int index) where TArray : IList<byte>
		{
			return (uint)((int)value[index] | (int)value[index + 1] << 8 | (int)value[index + 2] << 16 | (int)value[index + 3] << 24);
		}
		public static int ToInt32<TArray>(TArray value, int index) where TArray : IList<byte>
		{
			return (int)LittleEndian.ToUInt32<TArray>(value, index);
		}
		public static ulong ToUInt64<TArray>(TArray value, int index) where TArray : IList<byte>
		{
			return (ulong)LittleEndian.ToUInt32<TArray>(value, index) | (ulong)LittleEndian.ToUInt32<TArray>(value, index + 4) << 32;
		}
		public static long ToInt64<TArray>(TArray value, int index) where TArray : IList<byte>
		{
			return (long)LittleEndian.ToUInt64<TArray>(value, index);
		}
		public static void Pack(ushort value, byte[] buf, int index)
		{
			buf[index] = (byte)value;
			buf[index + 1] = (byte)(value >> 8);
		}
		public static void Pack(uint value, byte[] buf, int index)
		{
			buf[index] = (byte)value;
			buf[index + 1] = (byte)(value >> 8);
			buf[index + 2] = (byte)(value >> 16);
			buf[index + 3] = (byte)(value >> 24);
		}
		public static void Pack(ulong value, byte[] buf, int index)
		{
			LittleEndian.Pack((uint)value, buf, index);
			LittleEndian.Pack((uint)(value >> 32), buf, index + 4);
		}
		public static void Pack(short value, byte[] buf, int index)
		{
			LittleEndian.Pack((ushort)value, buf, index);
		}
		public static void Pack(int value, byte[] buf, int index)
		{
			LittleEndian.Pack((uint)value, buf, index);
		}
		public static void Pack(long value, byte[] buf, int index)
		{
			LittleEndian.Pack((ulong)value, buf, index);
		}
	}
}
