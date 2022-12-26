using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XFL
{
	internal class ArcXFL
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
		public List<Entry> TryOpen(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				if (binaryReader.ReadInt32() != 82508)
				{
					throw new Exception("Unsupported file format!");
				}
				uint num = binaryReader.ReadUInt32();
				int num2 = binaryReader.ReadInt32();
				if (num2 <= 0)
				{
					throw new Exception("Unsupported file format!");
				}
				long num3 = (long)this.buffer.Length;
				uint num4 = num + 12U;
				if ((ulong)num >= (ulong)num3 || (ulong)num4 >= (ulong)num3)
				{
					throw new Exception("Unsupported file format!");
				}
				long num5 = 12L;
				for (int i = 0; i < num2; i++)
				{
					if (num5 + 40L > (long)((ulong)num4))
					{
						throw new Exception("Unsupported file format!");
					}
					binaryReader.BaseStream.Position = num5;
					int count = this.strlen(binaryReader);
					byte[] bytes = binaryReader.ReadBytes(count);
					string @string = Encoding.GetEncoding(932).GetString(bytes);
					binaryReader.BaseStream.Position = num5 + 32L;
					long num6 = (long)((ulong)binaryReader.ReadUInt32());
					uint size = binaryReader.ReadUInt32();
					if (num6 > num3)
					{
						throw new Exception("Unsupported file format!");
					}
					Entry entry = new Entry();
					entry.Name = @string;
					entry.Offset = num6 + (long)((ulong)num4);
					entry.Size = size;
					this.dir.Add(entry);
					num5 += 40L;
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
				int value = 82508;
				binaryWriter.Write(value);
				binaryWriter.Flush();
				uint value2 = (uint)(list.Count * 40);
				binaryWriter.Write(value2);
				binaryWriter.Flush();
				binaryWriter.Write(list.Count);
				binaryWriter.Flush();
				uint num = 0U;
				for (int j = 0; j < list.Count; j++)
				{
					byte[] bytes = Encoding.GetEncoding(932).GetBytes(list[j].Name);
					if (bytes.Length > 32)
					{
						throw new Exception("Files' name is too long!");
					}
					byte[] dst = new byte[32];
					Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
					binaryWriter.Write(dst);
					binaryWriter.Flush();
					binaryWriter.Write(num);
					num += (uint)list[j].fileBuffer.Length;
					binaryWriter.Write(list[j].fileBuffer.Length);
					binaryWriter.Flush();
				}
				for (int k = 0; k < list.Count; k++)
				{
					binaryWriter.Write(list[k].fileBuffer);
				}
			}
		}
		private byte[] buffer;
		private List<Entry> dir = new List<Entry>();
	}
}
