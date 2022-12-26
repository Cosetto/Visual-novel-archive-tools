using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using GameRes.Utility;
using GameRes.Compression;
using ZstdNet;

namespace Nexas
{
	internal class ArcPAC
	{
		private string GetStringLiteral(BinaryReader reader, long index_offset, uint name_length)
		{
			List<byte> list = new List<byte>();
			reader.BaseStream.Position = index_offset;
			byte[] array = reader.ReadBytes((int)name_length);
			int num = 0;
			while (num < array.Length && array[num] != 0)
			{
				list.Add(array[num]);
				num++;
			}
			return Encoding.GetEncoding(932).GetString(list.ToArray());
		}
		public List<Entry> TryOpen(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			List<Entry> result;
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				byte[] array = binaryReader.ReadBytes(4);
				if (array[0] != 80 || array[1] != 65 || array[2] != 67)
				{
					throw new Exception("Unsupported file format!");
				}
				int num = binaryReader.ReadInt32();
				this.pack_type = binaryReader.ReadInt32();
				if (num > 40000)
				{
					throw new Exception("Unsupported file format!");
				}
				binaryReader.BaseStream.Position = binaryReader.BaseStream.Length - 4L;
				int num2 = binaryReader.ReadInt32();
				int num3 = num * 76;
				if ((long)num2 >= binaryReader.BaseStream.Length)
				{
					throw new Exception("Unsupported file format!");
				}
				binaryReader.BaseStream.Position = binaryReader.BaseStream.Length - 4L - (long)num2;
				byte[] array2 = binaryReader.ReadBytes(num2);
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = (byte)(~array2[i]);
				}
				byte[] array3 = new byte[num3];
				using (BinaryReader binaryReader2 = new BinaryReader(new MemoryStream(new Huffman().Decoder(array2, num3))))
				{
					uint name_length = 64U;
					for (int j = 0; j < num; j++)
					{
						string stringLiteral = this.GetStringLiteral(binaryReader2, binaryReader2.BaseStream.Position, name_length);
						if (string.IsNullOrEmpty(stringLiteral))
						{
							throw new Exception("Unsupported file format!");
						}
						Entry entry = new Entry();
						entry.Name = stringLiteral;
						entry.Offset = binaryReader2.ReadInt32();
						entry.UnpackedSize = binaryReader2.ReadInt32();
						entry.Size = binaryReader2.ReadInt32();
						if ((long)(entry.Offset + entry.Size) > binaryReader.BaseStream.Length)
						{
							throw new Exception("Unsupported file format!");
						}
						entry.IsPacked = (this.pack_type != 0 && (this.pack_type != 4 || entry.Size != entry.UnpackedSize));
						if (Path.GetExtension(entry.Name).Equals(".png"))
						{
							entry.IsPacked = false;
						}
						if (Path.GetExtension(entry.Name).Equals(".ogg"))
						{
							entry.IsPacked = false;
						}
						if (Path.GetExtension(entry.Name).Equals(".wav"))
						{
							entry.IsPacked = false;
						}
						this.dir.Add(entry);
					}
				}
				result = this.dir;
			}
			return result;
		}
		public byte[] Unpack(Entry entry)
		{
			byte[] array = new byte[1];
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				binaryReader.BaseStream.Position = (long)entry.Offset;
				byte[] array2 = binaryReader.ReadBytes(entry.Size);
				if (!entry.IsPacked)
				{
					array = array2;
				}
				else
				{
					Compression compression = (Compression)this.pack_type;
					if (compression != Compression.Lzss)
					{
						if (compression != Compression.Huffman)
						{
							if (compression == Compression.zstd)
							{
								return new Decompressor().Unwrap(array2, int.MaxValue);
							}
							array = new byte[entry.UnpackedSize];
							using (GameRes.Compression.ZLibStream zlibStream = new GameRes.Compression.ZLibStream(new MemoryStream(array2, 0, array2.Length), GameRes.Compression.CompressionMode.Decompress, true))
							{
								if (zlibStream.Read(array, 0, array.Length) != entry.UnpackedSize)
								{
									throw new Exception("Failed to decompress!");
								}
								return array;
							}
						}
					}
					else
					{
						using (LzssStream lzssStream = new LzssStream(new MemoryStream(array2), System.IO.Compression.CompressionMode.Decompress))
						{
							array = new byte[entry.UnpackedSize];
							lzssStream.Read(array, 0, entry.UnpackedSize);
							return array;
						}
					}
					array = new Huffman().Decoder(array2, entry.UnpackedSize);
				}
			}
			return array;
		}
		public void Pack(string packPath, string outputFile)
		{
			List<Entry> list = new List<Entry>();
			string[] files = Directory.GetFiles(packPath);
			for (int i = 0; i < files.Length; i++)
			{
				Entry entry = new Entry();
				byte[] array = File.ReadAllBytes(files[i]);
				entry.UnpackedSize = array.Length;
				entry.fileBuffer = array;
				entry.Name = Path.GetFileName(files[i]);
				list.Add(entry);
			}
			string directoryName = Path.GetDirectoryName(packPath);
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
			{
				uint value = 4407632U;
				binaryWriter.Write(value);
				binaryWriter.Flush();
				int count = list.Count;
				binaryWriter.Write(count);
				binaryWriter.Flush();
				int value2 = 0;
				binaryWriter.Write(value2);
				binaryWriter.Flush();
				for (int j = 0; j < list.Count; j++)
				{
					binaryWriter.Write(list[j].fileBuffer);
					binaryWriter.Flush();
				}
				byte[] input = new byte[count * 76];
				using (BinaryWriter binaryWriter2 = new BinaryWriter(new MemoryStream(input)))
				{
					int num = 12;
					for (int k = 0; k < list.Count; k++)
					{
						byte[] bytes = Encoding.GetEncoding(932).GetBytes(list[k].Name);
						if (bytes.Length > 64)
						{
							throw new Exception("File's name is too long!");
						}
						byte[] dst = new byte[64];
						Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
						binaryWriter2.Write(dst);
						binaryWriter2.Flush();
						binaryWriter2.Write(num);
						binaryWriter2.Flush();
						num += list[k].fileBuffer.Length;
						int unpackedSize = list[k].UnpackedSize;
						int value3 = list[k].fileBuffer.Length;
						binaryWriter2.Write(unpackedSize);
						binaryWriter2.Flush();
						binaryWriter2.Write(value3);
						binaryWriter2.Flush();
					}
				}
				byte[] array2 = new Huffman().Encoder(input);
				for (int l = 0; l < array2.Length; l++)
				{
					array2[l] = (byte)(~array2[l]);
				}
				binaryWriter.Write(array2);
				binaryWriter.Flush();
				int value4 = array2.Length;
				binaryWriter.Write(value4);
				binaryWriter.Flush();
			}
		}
		private byte[] buffer;
		public byte[] blockBuffer;
		private List<Entry> dir = new List<Entry>();
		private int pack_type;
	}
}
