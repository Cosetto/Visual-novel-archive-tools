using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VALKYRIA
{
	internal class ArcDAT
	{
		public List<Entry> TryOpen(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				uint num = binaryReader.ReadUInt32();
				if (num == 0U || (ulong)num >= (ulong)binaryReader.BaseStream.Length)
				{
					return this.dir;
				}
				int num2 = (int)(num / 268U);
				if (num != (uint)(num2 * 268) || num2 > 4000)
				{
					return this.dir;
				}
				uint num3 = 4U;
				long num4 = (long)((ulong)(num3 + num));
				for (int i = 0; i < num2; i++)
				{
					byte[] array = new byte[260];
					Buffer.BlockCopy(this.buffer, (int)num3, array, 0, 260);
					int count = Array.IndexOf<byte>(array, 0);
					string @string = Encoding.GetEncoding(932).GetString(array, 0, count);
					num3 += 260U;
					Entry entry = new Entry();
					entry.Name = @string;
					binaryReader.BaseStream.Position = (long)((ulong)num3);
					entry.Offset = (long)((ulong)binaryReader.ReadUInt32() + (ulong)num4);
					entry.Size = binaryReader.ReadUInt32();
					if (entry.Offset + (long)((ulong)entry.Size) > binaryReader.BaseStream.Length)
					{
						throw new Exception("Unsupported file format!");
					}
					num3 += 8U;
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
				uint value = (uint)((long)list.Count * 268L);
				binaryWriter.Write(value);
				binaryWriter.Flush();
				uint num = 0U;
				for (int j = 0; j < list.Count; j++)
				{
					byte[] bytes = Encoding.GetEncoding(932).GetBytes(list[j].Name);
					if (bytes.Length > 102)
					{
						throw new Exception("File's name is too long!");
					}
					byte[] dst = new byte[260];
					Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
					binaryWriter.Write(dst);
					binaryWriter.Flush();
					binaryWriter.Write(num);
					binaryWriter.Flush();
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
