using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Escu
{
	internal class ArcBIN
	{
		private uint NextKey()
		{
			this.m_seed ^= 1705808741U;
			this.m_seed ^= ((this.m_seed >> 1 ^ this.m_seed) >> 3 ^ (this.m_seed << 1 ^ this.m_seed) << 3);
			return this.m_seed;
		}
		
		private void Decrypt(byte[] data)
		{
			uint[] intData = new uint[data.Length / 4];
			Buffer.BlockCopy(data, 0, intData, 0, data.Length);
	
			for (int i = 0; i < intData.Length; i++)
			{
				intData[i] ^= this.NextKey();
			}

			Buffer.BlockCopy(intData, 0, data, 0, data.Length);
		}

		private void Encrypt(byte[] data)
		{
			uint[] intData = new uint[data.Length / 4];
			Buffer.BlockCopy(data, 0, intData, 0, data.Length);

			for (int i = 0; i < intData.Length; i++)
			{
				intData[i] ^= this.NextKey();
			}

			Buffer.BlockCopy(intData, 0, data, 0, data.Length);
		}
		
		public List<Entry> ReadIndexV2(BinaryReader reader)
		{
			if (this.m_count > 262144U)
			{
				return this.dir;
			}
			uint num = reader.ReadUInt32() ^ this.NextKey();
			uint num2 = this.m_count * 12U;
			byte[] array = reader.ReadBytes((int)num2);
			if ((ulong)num2 != (ulong)((long)array.Length))
			{
				return this.dir;
			}
			uint num3 = 20U + num2;
			reader.BaseStream.Position = (long)((ulong)num3);
			byte[] array2 = reader.ReadBytes((int)num);
			if ((long)array2.Length != (long)((ulong)num))
			{
				throw new Exception("Unsupported file format!");
			}
			this.Decrypt(array);
			int num4 = 0;
			for (uint num5 = 0U; num5 < this.m_count; num5 += 1U)
			{
				int num6 = LittleEndian.ToInt32<byte[]>(array, num4);
				if (num6 < 0 || num6 >= array2.Length)
				{
					return null;
				}
				string cstring = Binary.GetCString(array2, num6, array2.Length - num6, Encoding.GetEncoding(932));
				if (cstring.Length == 0)
				{
					return null;
				}
				Entry entry = new Entry();
				entry.Name = cstring;
				entry.Offset = (int)LittleEndian.ToUInt32<byte[]>(array, num4 + 4);
				entry.Size = LittleEndian.ToUInt32<byte[]>(array, num4 + 8);
				if ((long)entry.Offset + (long)((ulong)entry.Size) > reader.BaseStream.Length)
				{
					throw new Exception("Unsupported file format!");
				}
				num4 += 12;
				this.dir.Add(entry);
			}
			return this.dir;
		}
		public List<Entry> TryOpen(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				byte[] bytes = binaryReader.ReadBytes(8);
				if (!Encoding.ASCII.GetString(bytes).Equals("ESC-ARC2"))
				{
					throw new Exception("Unsupported file format!");
				}
				this.m_seed = binaryReader.ReadUInt32();
				this.m_count = (binaryReader.ReadUInt32() ^ this.NextKey());
				this.ReadIndexV2(binaryReader);
			}
			return this.dir;
		}
		public byte[] Unpack(Entry entry)
		{
			byte[] result;
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				binaryReader.BaseStream.Position = (long)entry.Offset;
				if (entry.Size < 8U)
				{
					result = binaryReader.ReadBytes((int)entry.Size);
				}
				else
				{
					if (binaryReader.ReadInt32() != 7365473)
					{
						binaryReader.BaseStream.Position = (long)entry.Offset;
						byte[] result2;
						result = (result2 = binaryReader.ReadBytes((int)entry.Size));
						return result2;
					}
					int num = Binary.BigEndian(binaryReader.ReadInt32());
					result = new byte[num];
					using (LzwDecoder lzwDecoder = new LzwDecoder(new MemoryStream(binaryReader.ReadBytes((int)(entry.Size - 8U))), num))
					{
						lzwDecoder.Unpack();
						result = lzwDecoder.Output;
					}
				}
			}
			return result;
		}
		public static int GetFileNameLength(string fileName)
		{
			return Encoding.GetEncoding(932).GetBytes(fileName).Length;
		}
		public void Pack(string packPath, string outputFile)
		{
			List<Entry> list = new List<Entry>();
			string path = Path.Combine(packPath, "FileList.txt");
			if (!File.Exists(path))
			{
				throw new Exception("Can't find FileList.txt, please use this tool to unpack BIN archive to generate FileList.txt!");
			}
			using (StreamReader streamReader = new StreamReader(new FileStream(path, FileMode.Open)))
			{
				while (!streamReader.EndOfStream)
				{
					list.Add(new Entry
					{
						Name = streamReader.ReadLine()
					});
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				string path2 = Path.Combine(packPath, list[i].Name);
				if (!File.Exists(path2))
				{
					throw new Exception("The file(s) in FileList.txt was not found");
				}
				byte[] array = File.ReadAllBytes(path2);
				array = new LzwEncoder(new byte[array.Length * 2]).Pack(array);
				list[i].fileBuffer = array;
			}
			string directoryName = Path.GetDirectoryName(packPath);
			string path3 = outputFile;
			int num = 0;
			for (int j = 0; j < list.Count; j++)
			{
				num += Encoding.GetEncoding(932).GetBytes(list[j].Name).Length + 1;
			}
			byte[] array2 = new byte[num];
			using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(array2)))
			{
				for (int k = 0; k < list.Count; k++)
				{
					list[k].filename_offset = (int)binaryWriter.BaseStream.Position;
					binaryWriter.Write(Encoding.GetEncoding(932).GetBytes(list[k].Name));
					binaryWriter.Flush();
					byte value = 0;
					binaryWriter.Write(value);
					binaryWriter.Flush();
				}
			}
			int num2 = list.Count * 12;
			int num3 = 20 + num + num2;
			for (int l = 0; l < list.Count; l++)
			{
				list[l].Offset = num3;
				num3 += list[l].fileBuffer.Length;
			}
			byte[] data = new byte[num2];
			using (BinaryWriter binaryWriter2 = new BinaryWriter(new MemoryStream(data)))
			{
				for (int m = 0; m < list.Count; m++)
				{
					binaryWriter2.Write(list[m].filename_offset);
					binaryWriter2.Flush();
					binaryWriter2.Write(list[m].Offset);
					binaryWriter2.Flush();
					binaryWriter2.Write(list[m].fileBuffer.Length);
					binaryWriter2.Flush();
				}
			}
			using (BinaryWriter binaryWriter3 = new BinaryWriter(new FileStream(path3, FileMode.Create)))
			{
				int value2 = 759386949;
				binaryWriter3.Write(value2);
				binaryWriter3.Flush();
				int value3 = 843272769;
				binaryWriter3.Write(value3);
				binaryWriter3.Flush();
				this.m_seed = 1747358894U;
				binaryWriter3.Write(this.m_seed);
				binaryWriter3.Flush();
				int value4 = (int)((long)list.Count ^ (long)((ulong)this.NextKey()));
				binaryWriter3.Write(value4);
				binaryWriter3.Flush();
				int value5 = (int)((long)num ^ (long)((ulong)this.NextKey()));
				binaryWriter3.Write(value5);
				binaryWriter3.Flush();
				this.Encrypt(data);
				binaryWriter3.Write(data);
				binaryWriter3.Flush();
				binaryWriter3.Write(array2);
				binaryWriter3.Flush();
				for (int n = 0; n < list.Count; n++)
				{
					binaryWriter3.Write(list[n].fileBuffer);
					binaryWriter3.Flush();
				}
			}
		}
		private byte[] buffer;
		private List<Entry> dir = new List<Entry>();
		private uint m_seed;
		private uint m_count;
	}
}
