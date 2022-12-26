using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Unity
{
	internal class BinDeserializer
	{
		public IDictionary DeserializeEntry(Stream input)
		{
			int num = input.ReadByte();
			if (num < 128 || num > 143)
			{
				throw new FormatException();
			}
			int num2 = num & 15;
			Hashtable hashtable = new Hashtable(num2);
			for (int i = 0; i < num2; i++)
			{
				num = input.ReadByte();
				if (num < 160 || num > 191)
				{
					throw new FormatException();
				}
				int num3 = num & 31;
				if (input.Read(this.m_buffer, 0, num3) < num3)
				{
					throw new FormatException();
				}
				string @string = Encoding.UTF8.GetString(this.m_buffer, 0, num3);
				object value = this.ReadField(input);
				hashtable[@string] = value;
			}
			return hashtable;
		}
		public void SerializeEntryBinary(BinaryWriter writer, Dictionary<string, object> map)
		{
			new Dictionary<string, object>();
			int count = map.Count;
			if (count > 15)
			{
				throw new Exception("Too many attributes!");
			}
			int num = count | 128;
			writer.Write((byte)num);
			Dictionary<string, object>.Enumerator enumerator = map.GetEnumerator();
			for (int i = 0; i < count; i++)
			{
				enumerator.MoveNext();
				KeyValuePair<string, object> keyValuePair = enumerator.Current;
				string key = keyValuePair.Key;
				byte[] bytes = Encoding.UTF8.GetBytes(key);
				if (key.Length > 31)
				{
					throw new Exception("The key is too long!");
				}
				num = (bytes.Length | 160);
				writer.Write((byte)num);
				writer.Write(bytes);
				object obj;
				if (!map.TryGetValue(key, out obj))
				{
					throw new Exception("Failed to get value!");
				}
				if (key.Equals("fileName"))
				{
					this.WriteFieldBinary(writer, 0, true, obj.ToString());
				}
				else
				{
					this.WriteFieldBinary(writer, Convert.ToInt32(obj), false, "");
				}
			}
		}
		private void WriteFieldBinary(BinaryWriter writer, int data, bool isString, string str)
		{
			if (!isString)
			{
				byte value = 210;
				writer.Write(value);
				uint value2 = Binary.ToBigEndian((uint)data);
				writer.Write((int)value2);
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			if (bytes.Length > 4095)
			{
				throw new Exception("The string is too long!");
			}
			short value3 = Binary.ToBigEndianUint16((short)bytes.Length);
			byte value4 = 218;
			writer.Write(value4);
			writer.Write(value3);
			writer.Write(bytes);
		}
		public void SerializeEntry(OutputCryptoStream writer, Dictionary<string, object> map)
		{
			new Dictionary<string, object>();
			int count = map.Count;
			if (count > 15)
			{
				throw new Exception("Too many attributes!");
			}
			int num = count | 128;
			writer.Write((byte)num);
			Dictionary<string, object>.Enumerator enumerator = map.GetEnumerator();
			for (int i = 0; i < count; i++)
			{
				enumerator.MoveNext();
				KeyValuePair<string, object> keyValuePair = enumerator.Current;
				string key = keyValuePair.Key;
				byte[] bytes = Encoding.UTF8.GetBytes(key);
				if (key.Length > 31)
				{
					throw new Exception("The key is too long!");
				}
				num = (bytes.Length | 160);
				writer.Write((byte)num);
				writer.Write(bytes);
				object obj;
				if (!map.TryGetValue(key, out obj))
				{
					throw new Exception("Failed to get value!");
				}
				if (key.Equals("fileName"))
				{
					this.WriteField(writer, 0, true, obj.ToString());
				}
				else
				{
					this.WriteField(writer, Convert.ToInt32(obj), false, "");
				}
			}
		}
		private void WriteField(OutputCryptoStream writer, int data, bool isString, string str)
		{
			if (!isString)
			{
				byte data2 = 210;
				writer.Write(data2);
				uint data3 = Binary.ToBigEndian((uint)data);
				writer.Write((int)data3);
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			if (bytes.Length > 4095)
			{
				throw new Exception("The string is too long!");
			}
			short data4 = Binary.ToBigEndianUint16((short)bytes.Length);
			byte data5 = 218;
			writer.Write(data5);
			writer.Write(data4);
			writer.Write(bytes);
		}
		private object ReadField(Stream input)
		{
			int num = input.ReadByte();
			if (num >= 0 && num < 128)
			{
				return num;
			}
			if (num >= 160 && num < 192)
			{
				int length = num & 31;
				return this.ReadString(input, length);
			}
			switch (num)
			{
			case 208:
			{
				int num2 = input.ReadByte();
				if (-1 == num2)
				{
					throw new FormatException();
				}
				return (sbyte)num2;
			}
			case 209:
				if (input.Read(this.m_buffer, 0, 2) < 2)
				{
					throw new FormatException();
				}
				return BigEndian.ToInt16<byte[]>(this.m_buffer, 0);
			case 210:
				if (input.Read(this.m_buffer, 0, 4) < 4)
				{
					throw new FormatException();
				}
				return BigEndian.ToInt32<byte[]>(this.m_buffer, 0);
			default:
			{
				if (num != 218)
				{
					throw new FormatException();
				}
				if (input.Read(this.m_buffer, 0, 2) < 2)
				{
					throw new FormatException();
				}
				int length2 = (int)BigEndian.ToUInt16<byte[]>(this.m_buffer, 0);
				return this.ReadString(input, length2);
			}
			}
		}
		private string ReadString(Stream input, int length)
		{
			if (length > this.m_buffer.Length)
			{
				this.m_buffer = new byte[length + 15 & -16];
			}
			input.Read(this.m_buffer, 0, length);
			return Encoding.UTF8.GetString(this.m_buffer, 0, length);
		}
		private byte[] m_buffer = new byte[32];
		private int Total_Byte;
	}
}
