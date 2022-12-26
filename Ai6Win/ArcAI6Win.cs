using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Ai6Win
{
	internal class ArcAI6Win
	{
		public List<Entry> TryOpen(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				int num = binaryReader.ReadInt32();
				if (num > 16384)
				{
					throw new Exception("Unsupported file format!");
				}
				if ((ulong)(num * 272) > (ulong)binaryReader.BaseStream.Length)
				{
					throw new Exception("Unsupported file format!");
				}
				for (int i = 0; i < num; i++)
				{
					byte[] array = binaryReader.ReadBytes(260);
					int num2 = Array.IndexOf<byte>(array, 0);
					if (num2 == 0)
					{
						throw new Exception("Unsupported file format!");
					}
					if (-1 == num2)
					{
						num2 = array.Length;
					}
					byte b = (byte)(num2 + 1);
					for (int j = 0; j < num2; j++)
					{
						byte[] array2 = array;
						int num3 = j;
						byte b2 = array2[num3];
						byte b3 = b;
						b = (byte)(b3 - 1);
						array2[num3] = (byte)(b2 - b3);
					}
					string @string = Encoding.GetEncoding(932).GetString(array, 0, num2);
					Entry entry = new Entry();
					if (@string.Equals("sis4__scene000.mes"))
					{
						string.Format("", new object[0]);
					}
					entry.Name = @string;
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
			uint num = (uint)(list.Count * 260 + list.Count * 12 + 4);
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
			{
				int count = list.Count;
				binaryWriter.Write(count);
				binaryWriter.Flush();
				for (int j = 0; j < list.Count; j++)
				{
					byte[] bytes = Encoding.GetEncoding(932).GetBytes(list[j].Name);
					if (bytes.Length > 259)
					{
						throw new Exception("File's name is too long!");
					}
					byte b = (byte)(bytes.Length + 1);
					for (int k = 0; k < bytes.Length; k++)
					{
						byte[] array3 = bytes;
						int num2 = k;
						byte b2 = array3[num2];
						byte b3 = b;
						b = (byte)(b3 - 1);
						array3[num2] = (byte)(b2 + b3);
					}
					byte[] dst = new byte[260];
					Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
					binaryWriter.Write(dst);
					binaryWriter.Flush();
					uint value = Binary.ToBigEndian((uint)list[j].fileBuffer.Length);
					uint value2 = Binary.ToBigEndian((uint)list[j].UnpackedSize);
					uint value3 = Binary.ToBigEndian(num);
					num += (uint)list[j].fileBuffer.Length;
					binaryWriter.Write(value);
					binaryWriter.Flush();
					binaryWriter.Write(value2);
					binaryWriter.Flush();
					binaryWriter.Write(value3);
					binaryWriter.Flush();
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
