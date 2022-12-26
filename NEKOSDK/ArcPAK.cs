using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ComponentAce.Compression.Libs.zlib;
using GameRes.Compression;

namespace NEKOSDK
{
	internal class ArcPAK
	{
		public string Tag
		{
			get
			{
				return "NEKOPACK/4";
			}
		}
		public string Description
		{
			get
			{
				return "NekoSDK engine resource archive";
			}
		}
		public uint Signature
		{
			get
			{
				return 1330333006U;
			}
		}
		public bool IsHierarchic
		{
			get
			{
				return true;
			}
		}
		public bool CanWrite
		{
			get
			{
				return false;
			}
		}
		public List<Entry> TryOpen(string filePath)
		{
			this.dir = new List<Entry>();
			this.buffer = File.ReadAllBytes(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				byte[] bytes = binaryReader.ReadBytes(9);
				if (!Encoding.ASCII.GetString(bytes).Equals("NEKOPACK4"))
				{
					throw new Exception("Unsupported file format!");
				}
				int num = (int)binaryReader.ReadByte();
				uint num2;
				uint num3;
				if (65 == num)
				{
					num2 = 14U;
					binaryReader.BaseStream.Position = 10L;
					num3 = num2 + binaryReader.ReadUInt32();
				}
				else
				{
					if (83 != num)
					{
						throw new Exception("Unsupported file format!");
					}
					num2 = 10U;
					binaryReader.BaseStream.Position = 10L;
					num3 = num2 + binaryReader.ReadUInt32() + 12U;
				}
				byte[] array = new byte[256];
				while (num2 < num3)
				{
					binaryReader.BaseStream.Position = (long)((ulong)num2);
					uint num4 = binaryReader.ReadUInt32();
					num2 += 4U;
					if (num4 == 0U)
					{
						break;
					}
					if ((ulong)num4 > (ulong)((long)array.Length))
					{
						return null;
					}
					binaryReader.BaseStream.Position = (long)((ulong)num2);
					byte[] array2 = binaryReader.ReadBytes((int)num4);
					num2 += num4;
					int num5 = 0;
					uint num6 = 0U;
					while ((ulong)num6 < (ulong)((long)array2.Length))
					{
						num5 += (int)((sbyte)array2[(int)num6]);
						num6 += 1U;
					}
					string @string = Encoding.GetEncoding(932).GetString(array2, 0, array2.Length - 1);
					Entry entry = new Entry();
					entry.Name = @string;
					binaryReader.BaseStream.Position = (long)((ulong)num2);
					entry.Offset = (long)((ulong)(binaryReader.ReadUInt32() ^ (uint)num5));
					binaryReader.BaseStream.Position = (long)((ulong)(num2 + 4U));
					entry.Size = (binaryReader.ReadUInt32() ^ (uint)num5);
					this.dir.Add(entry);
					num2 += 8U;
				}
				if (this.dir.Count == 0)
				{
					throw new Exception("Unknown error");
				}
			}
			return this.dir;
		}
		public byte[] Unpack(Entry entry)
		{
			uint num = entry.Size / 8U + 34U;
			byte[] array = new byte[Math.Min(4U, entry.Size)];
			byte[] result;
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				binaryReader.BaseStream.Position = entry.Offset;
				array = binaryReader.ReadBytes(array.Length);
				for (int i = 0; i < array.Length; i++)
				{
					byte[] array2 = array;
					int num2 = i;
					array2[num2] ^= (byte)num;
					num <<= 3;
				}
				entry.IsPacked = true;
				binaryReader.BaseStream.Position = entry.Offset + (long)((ulong)entry.Size) - 4L;
				entry.UnpackedSize = binaryReader.ReadInt32();
				byte[] array3 = new byte[entry.Size - 4U];
				Buffer.BlockCopy(this.buffer, (int)(entry.Offset + 4L), array3, 4, (int)(entry.Size - 8U));
				Buffer.BlockCopy(array, 0, array3, 0, array.Length);
				byte[] array4 = new byte[entry.UnpackedSize];
				using (GameRes.Compression.ZLibStream zlibStream = new GameRes.Compression.ZLibStream(new MemoryStream(array3, 0, array3.Length), GameRes.Compression.CompressionMode.Decompress, true))
				{
					if (zlibStream.Read(array4, 0, array4.Length) != entry.UnpackedSize)
					{
						throw new Exception("Failed to decompress!");
					}
				}
				result = array4;
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
				binaryWriter.Write(Encoding.ASCII.GetBytes("NEKOPACK4"));
				binaryWriter.Flush();
				binaryWriter.Write(Encoding.ASCII.GetBytes("A"));
				binaryWriter.Flush();
				int value = 8956;
				binaryWriter.Write(value);
				binaryWriter.Flush();
				byte value2 = 0;
				for (int j = 0; j < list.Count; j++)
				{
					byte[] bytes = Encoding.GetEncoding(932).GetBytes(list[j].Name);
					binaryWriter.Write(bytes.Length + 1);
					binaryWriter.Flush();
					binaryWriter.Write(bytes);
					binaryWriter.Flush();
					binaryWriter.Write(value2);
					binaryWriter.Flush();
					list[j].StructPosition = binaryWriter.BaseStream.Position;
					uint value3 = 0U;
					uint value4 = 0U;
					binaryWriter.Write(value3);
					binaryWriter.Flush();
					binaryWriter.Write(value4);
					binaryWriter.Flush();
				}
				long position = binaryWriter.BaseStream.Position;
				for (int k = 0; k < list.Count; k++)
				{
					byte[] array = MyZlib.compressBytes(list[k].fileBuffer);
					list[k].Size = (uint)(array.Length + 4);
					list[k].Offset = binaryWriter.BaseStream.Position;
					uint num = list[k].Size / 8U + 34U;
					byte[] array2 = new byte[Math.Min(4U, list[k].Size)];
					Buffer.BlockCopy(array, 0, array2, 0, 4);
					for (int l = 0; l < array2.Length; l++)
					{
						byte[] array3 = array2;
						int num2 = l;
						array3[num2] ^= (byte)num;
						num <<= 3;
					}
					Buffer.BlockCopy(array2, 0, array, 0, 4);
					binaryWriter.Write(array);
					binaryWriter.Flush();
					binaryWriter.Write(list[k].fileBuffer.Length);
					binaryWriter.Flush();
				}
				for (int m = 0; m < list.Count; m++)
				{
					binaryWriter.BaseStream.Position = list[m].StructPosition;
					byte[] bytes2 = Encoding.GetEncoding(932).GetBytes(list[m].Name);
					int num3 = 0;
					uint num4 = 0U;
					while ((ulong)num4 < (ulong)((long)bytes2.Length))
					{
						num3 += (int)((sbyte)bytes2[(int)num4]);
						num4 += 1U;
					}
					uint value5 = (uint)list[m].Offset ^ (uint)num3;
					uint value6 = list[m].Size ^ (uint)num3;
					binaryWriter.Write(value5);
					binaryWriter.Flush();
					binaryWriter.Write(value6);
					binaryWriter.Flush();
				}
				binaryWriter.BaseStream.Position = 10L;
				value = (int)(position - 14L);
				binaryWriter.Write(value);
				binaryWriter.Flush();
			}
		}
		private byte[] buffer;
		private List<Entry> dir;
	}
}
