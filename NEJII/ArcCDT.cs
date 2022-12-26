using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace NEJII
{
	internal class ArcCDT
	{
		private int strlen(BinaryReader reader)
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
		public List<Entry> TryOpen(string filePath)
		{
			this.fileFullPath = filePath;
			this.buffer = File.ReadAllBytes(filePath);
			long num = (long)this.buffer.Length;
			if (num <= 12L)
			{
				throw new Exception("Unsupported file format!");
			}
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				binaryReader.BaseStream.Position = binaryReader.BaseStream.Length - 12L;
				if (binaryReader.ReadInt32() != 3230546)
				{
					throw new Exception("Unsupported file format!");
				}
				int num2 = binaryReader.ReadInt32();
				if (num2 > 16384)
				{
					throw new Exception("Unsupported file format!");
				}
				int num3 = binaryReader.ReadInt32();
				if ((long)num3 > num)
				{
					throw new Exception("Unsupported file format!");
				}
				binaryReader.BaseStream.Position = (long)num3;
				for (int i = 0; i < num2; i++)
				{
					int num4 = this.strlen(binaryReader);
					if (num4 > 16)
					{
						throw new Exception("File's name is too long");
					}
					byte[] bytes = binaryReader.ReadBytes(16);
					string @string = Encoding.GetEncoding(932).GetString(bytes, 0, num4);
					Entry entry = new Entry();
					entry.Name = @string;
					entry.Size = binaryReader.ReadUInt32();
					entry.UnpackedSize = binaryReader.ReadInt32();
					entry.IsPacked = (binaryReader.ReadInt32() != 0);
					entry.Offset = binaryReader.ReadInt32();
					if ((long)entry.Offset + (long)((ulong)entry.Size) > num)
					{
						throw new Exception("Unsupported file format!");
					}
					this.dir.Add(entry);
				}
			}
			return this.dir;
		}
		public static bool LzssCompress(byte[] m_input, byte[] m_output, int unpakedSize)
		{
			byte[] array = new byte[4112];
			short num = 4078;
			ushort num2 = 0;
			int num3 = 0;
			int i = 0;
			while (i < m_input.Length)
			{
				num2 = (ushort)(num2 >> 1);
				if (((num2 = (ushort)(num2 >> 1)) & 256) == 0)
				{
					num2 = (ushort)m_input[i++];
					num2 |= 65280;
				}
				if ((num2 & 1) == 0)
				{
					int num4 = (int)m_input[i++];
					byte b = m_input[i++];
					int num5 = (int)(b & 15);
					num5 += 2;
					int num6 = num4 | (int)(b & 240) << 4;
					for (int j = 0; j < num5; j++)
					{
						byte b2 = array[num6 + j & 4095];
						m_output[num3++] = b2;
						unpakedSize--;
						if (unpakedSize == 0)
						{
							return true;
						}
						byte[] array2 = array;
						short num7 = num;
						num = (byte)(num7 + 1);
						array2[(int)num7] = b2;
					}
				}
				else
				{
					byte b3 = m_input[i++];
					m_output[num3++] = b3;
					unpakedSize--;
					if (unpakedSize == 0)
					{
						return true;
					}
					byte[] array3 = array;
					short num8 = num;
					num = (byte)(num8 + 1);
					array3[(int)num8] = b3;
				}
			}
			return unpakedSize == 0;
		}
		public byte[] Unpack(Entry entry)
		{
			byte[] result;
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				binaryReader.BaseStream.Position = (long)entry.Offset;
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
			string[] files = Directory.GetFiles(packPath);
			for (int i = 0; i < files.Length; i++)
			{
				Entry entry = new Entry();
				byte[] array = File.ReadAllBytes(files[i]);
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
			string directoryName = Path.GetDirectoryName(packPath);
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
			{
				uint num = (uint)(list.Count * 32);
				for (int j = 0; j < list.Count; j++)
				{
					byte[] bytes = Encoding.GetEncoding(932).GetBytes(list[j].Name);
					if (bytes.Length > 16)
					{
						throw new Exception("Files' name are too long!");
					}
					byte[] dst = new byte[16];
					Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
					binaryWriter.Write(dst);
					binaryWriter.Flush();
					binaryWriter.Write(list[j].fileBuffer.Length);
					binaryWriter.Flush();
					binaryWriter.Write(list[j].UnpackedSize);
					binaryWriter.Flush();
					int value = 1;
					binaryWriter.Write(value);
					binaryWriter.Flush();
					binaryWriter.Write(num);
					binaryWriter.Flush();
					num += (uint)list[j].fileBuffer.Length;
				}
				for (int k = 0; k < list.Count; k++)
				{
					binaryWriter.Write(list[k].fileBuffer);
					binaryWriter.Flush();
				}
				int value2 = 3230546;
				binaryWriter.Write(value2);
				binaryWriter.Flush();
				binaryWriter.Write(list.Count);
				binaryWriter.Flush();
				int value3 = 0;
				binaryWriter.Write(value3);
				binaryWriter.Flush();
			}				
		}
		private byte[] buffer;
		private List<Entry> dir = new List<Entry>();
		private string fileFullPath = "";
		private bool isScript;
		private byte[] key;
	}
}
