using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Silky
{
	internal class ArcARC
	{
		public int strlen(BinaryReader reader)
		{
			bool flag = false;
			long position = reader.BaseStream.Position;
			while (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				if (reader.ReadByte() == 0)
				{
					flag = true;
					break;
				}
			}
			int result;
			if (flag)
			{
				result = (int)(reader.BaseStream.Position - position - 1L);
			}
			else
			{
				result = (int)(reader.BaseStream.Position - position);
			}
			reader.BaseStream.Position = position;
			return result;
		}
		private string GetCstring(byte[] buffer, int index, int length)
		{
			if (index + length > buffer.Length)
			{
				throw new Exception("Unsupported file format!");
			}
			byte[] array = new byte[length];
			Buffer.BlockCopy(buffer, index, array, 0, length);
			int count = 0;
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(array)))
			{
				count = this.strlen(binaryReader);
			}
			return Encoding.GetEncoding(932).GetString(array, 0, count);
		}
		public List<Entry> TryOpenOLD(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				int num = binaryReader.ReadInt32();
				if (num > 16384)
				{
					throw new Exception("Unsupported file format!");
				}
				ulong num2 = 4UL;
				int num3 = 32;
				uint num4 = (uint)(num * (num3 + 8));
				if (num2 + (ulong)num4 > (ulong)((long)this.buffer.Length))
				{
					throw new Exception("Unsupported file format!");
				}
				HashSet<uint> hashSet = new HashSet<uint>();
				for (int i = 0; i < num; i++)
				{
					byte[] array = binaryReader.ReadBytes(num3);
					string cstring = this.GetCstring(array, 0, array.Length);
					if (cstring.Length == 0)
					{
						throw new Exception("Unsupported file format!");
					}
					Entry entry = new Entry();
					entry.Name = cstring;
					entry.Offset = (long)((ulong)binaryReader.ReadUInt32());
					entry.Size = binaryReader.ReadUInt32();
					if (entry.Offset + (long)((ulong)entry.Size) > (long)this.buffer.Length)
					{
						throw new Exception("Unsupported file format!");
					}
					if (!hashSet.Add((uint)entry.Offset))
					{
						throw new Exception("Unsupported file format!");
					}
					this.dir.Add(entry);
				}
			}
			return this.dir;
		}
		private static void DecryptName(byte[] buffer, int length)
		{
			byte b = (byte)length;
			for (int i = 0; i < length; i++)
			{
				int num = i;
				byte b2 = buffer[num];
				byte b3 = b;
				b = (byte)(b3 - 1);
				buffer[num] = (byte)(b2 + b3);
			}
		}
		private static void EncryptName(byte[] buffer, int length)
		{
			byte b = (byte)length;
			for (int i = 0; i < length; i++)
			{
				int num = i;
				byte b2 = buffer[num];
				byte b3 = b;
				b = (byte)(b3 - 1);
				buffer[num] = (byte)(b2 - b3);
			}
		}
		public List<Entry> TryOpen(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				uint num = binaryReader.ReadUInt32();
				if (num < 10U || (ulong)num >= (ulong)binaryReader.BaseStream.Length)
				{
					throw new Exception("Unsupported file format!");
				}
				while (binaryReader.BaseStream.Position < (long)((ulong)(num + 4U)))
				{
					byte b = binaryReader.ReadByte();
					if (b == 0)
					{
						throw new Exception("Unsupported file format!");
					}
					byte[] array = binaryReader.ReadBytes((int)b);
					ArcARC.DecryptName(array, (int)b);
					string cstring = this.GetCstring(array, 0, (int)b);
					Entry entry = new Entry();
					entry.Name = cstring;
					entry.Size = Binary.BigEndian(binaryReader.ReadUInt32());
					entry.UnpackedSize = (int)Binary.BigEndian(binaryReader.ReadUInt32());
					entry.Offset = (long)((ulong)Binary.BigEndian(binaryReader.ReadUInt32()));
					if (entry.Offset + (long)((ulong)entry.Size) > binaryReader.BaseStream.Length)
					{
						throw new Exception("Unsupported file format!");
					}
					entry.IsPacked = ((ulong)entry.Size != (ulong)((long)entry.UnpackedSize));
					this.dir.Add(entry);
				}
			}
			return this.dir;
		}
		public byte[] Unpack(Entry entry)
		{
			byte[] result;
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				binaryReader.BaseStream.Position = entry.Offset;
				result = binaryReader.ReadBytes((int)entry.Size);
			}
			if (entry.IsPacked)
			{
				byte[] result2;
				using (LzssStream lzssStream = new LzssStream(new MemoryStream(result), CompressionMode.Decompress))
				{
					result2 = new byte[entry.UnpackedSize];
					lzssStream.Read(result2, 0, entry.UnpackedSize);
				}
				return result2;
			}
			return result;
		}
		public void Pack(string packPath, string outputFile)
		{
			List<Entry> list = new List<Entry>();
			bool flag = true;
			string[] files = Directory.GetFiles(packPath);
			if (Path.GetFileName(packPath).Equals("layer"))
			{
				flag = false;
			}
			else if (Path.GetFileName(packPath).Equals("update"))
			{
				flag = false;
			}
			for (int i = 0; i < files.Length; i++)
			{
				Entry entry = new Entry();
				byte[] array = File.ReadAllBytes(files[i]);
				if (flag)
				{
					byte[] src = new byte[array.Length * 2];
					byte[] array2;
					int lastCodeLength;
					using (LzssStream lzssStream = new LzssStream(new MemoryStream(src), CompressionMode.Compress))
					{
						lzssStream.Write(array, 0, array.Length);
						if (lzssStream.LastCodeLength == 0)
						{
							throw new Exception("Failed to compress!");
						}
						array2 = new byte[lzssStream.LastCodeLength];
						lastCodeLength = lzssStream.LastCodeLength;
					}
					Buffer.BlockCopy(src, 0, array2, 0, lastCodeLength);
					entry.fileBuffer = array2;
					entry.UnpackedSize = array.Length;
					entry.IsPacked = true;
					entry.Name = Path.GetFileName(files[i]);
					list.Add(entry);
				}
				else
				{
					entry.fileBuffer = array;
					entry.UnpackedSize = array.Length;
					entry.IsPacked = false;
					entry.Name = Path.GetFileName(files[i]);
					list.Add(entry);
				}
			}
			string directoryName = Path.GetDirectoryName(packPath);
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
			{
				int num = 0;
				for (int j = 0; j < list.Count; j++)
				{
					int num2 = Encoding.GetEncoding(932).GetBytes(list[j].Name).Length;
					if (num2 > 112)
					{
						throw new Exception("File's name is too long!");
					}
					num += num2;
				}
				uint num3 = (uint)(4 + list.Count * 13 + num);
				uint value = num3 - 4U;
				binaryWriter.Write(value);
				binaryWriter.Flush();
				for (int k = 0; k < list.Count; k++)
				{
					byte[] bytes = Encoding.GetEncoding(932).GetBytes(list[k].Name);
					ArcARC.EncryptName(bytes, bytes.Length);
					if (bytes.Length > 112)
					{
						throw new Exception("File's name is too long!");
					}
					byte value2 = (byte)bytes.Length;
					binaryWriter.Write(value2);
					binaryWriter.Flush();
					binaryWriter.Write(bytes);
					binaryWriter.Flush();
					uint num4 = (uint)list[k].fileBuffer.Length;
					num4 = Binary.ToBigEndian(num4);
					binaryWriter.Write(num4);
					binaryWriter.Flush();
					if (list[k].IsPacked)
					{
						uint num5 = (uint)list[k].UnpackedSize;
						num5 = Binary.ToBigEndian(num5);
						binaryWriter.Write(num5);
						binaryWriter.Flush();
					}
					else
					{
						binaryWriter.Write(num4);
						binaryWriter.Flush();
					}
					binaryWriter.Write(Binary.ToBigEndian(num3));
					binaryWriter.Flush();
					num3 += (uint)list[k].fileBuffer.Length;
				}
				for (int l = 0; l < list.Count; l++)
				{
					binaryWriter.Write(list[l].fileBuffer);
					binaryWriter.Flush();
				}			
			}
		}
		private byte[] buffer;
		private List<Entry> dir = new List<Entry>();
	}
}
