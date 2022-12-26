using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PAC
{
	internal class ArcPAC
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
		public List<Entry> TryOpen(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				int num = (int)binaryReader.ReadInt16();
				if (num > 4000)
				{
					throw new Exception("Unsupported file format!");
				}
				uint num2 = (uint)binaryReader.ReadByte();
				if (num2 == 0U)
				{
					throw new Exception("Unsupported file format!");
				}
				uint num3 = binaryReader.ReadUInt32();
				if ((ulong)num3 >= (ulong)binaryReader.BaseStream.Length)
				{
					throw new Exception("Unsupported file format!");
				}
				uint num4 = 7U + (num2 + 8U) * (uint)num;
				int num5;
				if (num3 == num4)
				{
					num5 = 1;
				}
				else
				{
					if (num3 != num4 + (uint)(4 * num))
					{
						throw new Exception("Unsupported file format!");
					}
					num5 = 2;
				}
				binaryReader.BaseStream.Position = 7L;
				for (int i = 0; i < num; i++)
				{
					byte[] array = binaryReader.ReadBytes((int)num2);
					string cstring = this.GetCstring(array, 0, array.Length);
					Entry entry = new Entry();
					if (1 == num5)
					{
						entry.Offset = (long)((ulong)(binaryReader.ReadUInt32() + num3));
						entry.Size = binaryReader.ReadUInt32();
					}
					else
					{
						entry.Offset = binaryReader.ReadInt64() + (long)((ulong)num3);
						entry.Size = binaryReader.ReadUInt32();
					}
					entry.Name = cstring;
					if (entry.Offset + (long)((ulong)entry.Size) > binaryReader.BaseStream.Length)
					{
						throw new Exception("Unsupported file format!");
					}
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
				binaryReader.BaseStream.Position = entry.Offset + 4L;
				int num = (int)binaryReader.ReadInt16();
				uint num2 = binaryReader.ReadUInt32();
				if (num == 6 && num2 == 1310800U)
				{
					binaryReader.BaseStream.Position = entry.Offset;
					int num3 = binaryReader.ReadInt32();
					binaryReader.BaseStream.Position = entry.Offset;
					byte[] array = binaryReader.ReadBytes((int)entry.Size);
					result = array;
					int num4 = 4;
					for (int i = 0; i < num3; i++)
					{
						if (num4 + 2 > array.Length)
						{
							break;
						}
						int num5 = (int)(LittleEndian.ToUInt16<byte[]>(array, num4) - 4);
						num4 += 6;
						if (num4 + num5 > array.Length)
						{
							return array;
						}
						for (int j = 0; j < num5; j++)
						{
							array[num4] = Binary.RotByteR(array[num4], 4);
							num4++;
						}
					}
				}
				else
				{
					binaryReader.BaseStream.Position = entry.Offset;
					result = binaryReader.ReadBytes((int)entry.Size);
				}
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
				byte[] fileBuffer = File.ReadAllBytes(files[i]);
				entry.fileBuffer = fileBuffer;
				entry.Name = Path.GetFileName(files[i]);
				list.Add(entry);
			}
			string directoryName = Path.GetDirectoryName(packPath);
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
			{
				short num = (short)list.Count;
				binaryWriter.Write(num);
				binaryWriter.Flush();
				byte b = 48;
				binaryWriter.Write(b);
				binaryWriter.Flush();
				uint value = (uint)(7L + (long)(b + 12) * (long)((ulong)num));
				binaryWriter.Write(value);
				binaryWriter.Flush();
				long num2 = 0L;
				for (int j = 0; j < list.Count; j++)
				{
					byte[] bytes = Encoding.GetEncoding(932).GetBytes(list[j].Name);
					if (bytes.Length > 48)
					{
						throw new Exception("File's name are too long!");
					}
					byte[] dst = new byte[48];
					Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
					binaryWriter.Write(dst);
					binaryWriter.Flush();
					binaryWriter.Write(num2);
					binaryWriter.Flush();
					num2 += (long)list[j].fileBuffer.Length;
					binaryWriter.Write(list[j].fileBuffer.Length);
					binaryWriter.Flush();
				}
				for (int k = 0; k < list.Count; k++)
				{
					byte[] array2;
					using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(list[k].fileBuffer)))
					{
						binaryReader.BaseStream.Position += 4L;
						int num3 = (int)binaryReader.ReadInt16();
						uint num4 = binaryReader.ReadUInt32();
						if (num3 == 6 && num4 == 1310800U)
						{
							binaryReader.BaseStream.Position = 0L;
							int num5 = binaryReader.ReadInt32();
							binaryReader.BaseStream.Position = 0L;
							byte[] array = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
							array2 = array;
							int num6 = 4;
							for (int l = 0; l < num5; l++)
							{
								if (num6 + 2 > array.Length)
								{
									break;
								}
								int num7 = (int)(LittleEndian.ToUInt16<byte[]>(array, num6) - 4);
								num6 += 6;
								if (num6 + num7 > array.Length)
								{
									break;
								}
								for (int m = 0; m < num7; m++)
								{
									array[num6] = Binary.RotByteL(array[num6], 4);
									num6++;
								}
							}
						}
						else
						{
							array2 = list[k].fileBuffer;
						}
					}
					binaryWriter.Write(array2);
					binaryWriter.Flush();
				}
			}
		}
		private byte[] buffer;
		private List<Entry> dir = new List<Entry>();
	}
}
