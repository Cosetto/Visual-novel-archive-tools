using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AMUSE
{
	internal class ArcPAC
	{
		private string GetStringLiteral(BinaryReader reader, uint index_offset, uint name_length)
		{
			List<byte> list = new List<byte>();
			reader.BaseStream.Position = (long)((ulong)index_offset);
			byte[] array = reader.ReadBytes((int)name_length);
			int num = 0;
			while (num < array.Length && array[num] != 0)
			{
				list.Add(array[num]);
				num++;
			}
			return Encoding.GetEncoding(932).GetString(list.ToArray());
		}
		protected List<Entry> ReadIndex(BinaryReader reader, int count, uint index_offset, uint name_length)
		{
			for (int i = 0; i < count; i++)
			{
				string stringLiteral = this.GetStringLiteral(reader, index_offset, name_length);
				index_offset += name_length;
				Entry entry = new Entry();
				reader.BaseStream.Position = (long)((ulong)index_offset);
				entry.Size = reader.ReadInt32();
				entry.Offset = reader.ReadInt32();
				entry.Name = stringLiteral;
				if ((long)(entry.Offset + entry.Size) > reader.BaseStream.Length)
				{
					throw new Exception("Unsupported file format!");
				}
				this.dir.Add(entry);
				index_offset += 8U;
			}
			return this.dir;
		}
		public List<Entry> TryOpen(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				byte[] bytes = binaryReader.ReadBytes(4);
				if (!Encoding.ASCII.GetString(bytes).Equals("PAC "))
				{
					throw new Exception("Unsupported file format!");
				}
				binaryReader.ReadInt32();
				int num = binaryReader.ReadInt32();
				if (num > 4000)
				{
					throw new Exception("Unsupported file format!");
				}
				uint num2 = 2052U;
				uint num3 = 32U;
				binaryReader.BaseStream.Position = (long)((ulong)(num2 + num3 + 4U));
				if (binaryReader.ReadUInt32() != num2 + (uint)(num * (int)(num3 + 8U)))
				{
					throw new Exception("Unsupported file format!");
				}
				this.ReadIndex(binaryReader, num, num2, num3);
				binaryReader.BaseStream.Position = 12L;
				this.blockBuffer = binaryReader.ReadBytes(2040);
			}
			return this.dir;
		}
		public unsafe byte[] Unpack(Entry entry)
		{
			byte[] array;
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				binaryReader.BaseStream.Position = (long)entry.Offset;
				if (this.buffer[entry.Offset] != 36 || entry.Size <= 16)
				{
					array = binaryReader.ReadBytes(entry.Size);
				}
				else
				{
					array = binaryReader.ReadBytes(entry.Size);
					int num = (array.Length - 16) / 4;
					if (num > 0)
					{
						try
						{
							fixed (byte* ptr = &array[16])
							{
								uint* ptr2 = (uint*)ptr;
								int num2 = 4;
								uint* ptr3 = ptr2 + num;
								while (ptr2 != ptr3)
								{
									byte* ptr4 = (byte*)ptr2;
									*ptr4 = Binary.RotByteL(*ptr4, num2++);
									*ptr2 ^= 4157965725U;
									ptr2++;
								}
							}
						}
						finally
						{
							byte* ptr = null;
						}
					}
				}
			}
			return array;
		}
		public unsafe void Pack(string packPath, string outputFile)
		{
			bool flag = false;
			byte[] array = new byte[2040];
			List<Entry> list = new List<Entry>();
			string[] files = Directory.GetFiles(packPath);
			for (int i = 0; i < files.Length; i++)
			{
				Entry entry = new Entry();
				byte[] array2 = File.ReadAllBytes(files[i]);
				entry.fileBuffer = array2;
				entry.Name = Path.GetFileName(files[i]);
				if (entry.Name.Equals("block.tmp") && entry.fileBuffer.Length == 2040)
				{
					flag = true;
					array = array2;
				}
				else
				{
					list.Add(entry);
				}
			}
			if (!flag)
			{
				throw new Exception("Can't find block.tmp, please use this tool to unpack PAC to generate block.tmp!");
			}
			string directoryName = Path.GetDirectoryName(packPath);
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
			{
				int value = 541278544;
				binaryWriter.Write(value);
				binaryWriter.Flush();
				int value2 = 0;
				binaryWriter.Write(value2);
				binaryWriter.Flush();
				int count = list.Count;
				binaryWriter.Write(count);
				binaryWriter.Flush();
				binaryWriter.Write(array);
				binaryWriter.Flush();
				ulong num = 2052UL;
				uint num2 = 32U;
				uint num3 = (uint)(num + (ulong)((long)list.Count * (long)((ulong)(num2 + 8U))));
				for (int j = 0; j < list.Count; j++)
				{
					byte[] bytes = Encoding.GetEncoding(932).GetBytes(list[j].Name);
					if (bytes.Length > 32)
					{
						throw new Exception("File's name is too long!");
					}
					byte[] dst = new byte[32];
					Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
					binaryWriter.Write(dst);
					binaryWriter.Flush();
					binaryWriter.Write(list[j].fileBuffer.Length);
					binaryWriter.Flush();
					binaryWriter.Write(num3);
					num3 += (uint)list[j].fileBuffer.Length;
				}
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k].fileBuffer[0] != 36 || list[k].fileBuffer.Length <= 16)
					{
						binaryWriter.Write(list[k].fileBuffer);
					}
					else
					{
						int num4 = (list[k].fileBuffer.Length - 16) / 4;
						if (num4 > 0)
						{
							try
							{
								fixed (byte* ptr = &list[k].fileBuffer[16])
								{
									uint* ptr2 = (uint*)ptr;
									int num5 = 4;
									uint* ptr3 = ptr2 + num4;
									while (ptr2 != ptr3)
									{
										byte* ptr4 = (byte*)ptr2;
										*ptr2 ^= 4157965725U;
										*ptr4 = Binary.RotByteR(*ptr4, num5++);
										ptr2++;
									}
								}
							}
							finally
							{
								byte* ptr = null;
							}
						}
						binaryWriter.Write(list[k].fileBuffer);
					}
				}
			}
		}
		private byte[] buffer;
		public byte[] blockBuffer;
		private List<Entry> dir = new List<Entry>();
	}
}
