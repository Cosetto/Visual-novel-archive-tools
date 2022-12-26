using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HuffmanCompressor;

namespace Lilim
{
	internal class ArcAOS2
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

		public List<Entry> TryOpen(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				if (binaryReader.ReadInt32() != 0)
				{
					throw new Exception("Unsupported file format!");
				}
				long num = (long)((ulong)binaryReader.ReadUInt32());
				uint num2 = 273U;
				int num3 = binaryReader.ReadInt32();
				if (num >= (long)this.buffer.Length || (ulong)num2 + (ulong)((long)num3) >= (ulong)((long)this.buffer.Length) || num < (long)((ulong)num2 + (ulong)((long)num3)))
				{
					throw new Exception("Unsupported file format!");
				}
				this.blockBuffer = binaryReader.ReadBytes(261);
				int num4 = num3 / 40;
				if (num4 > 16384)
				{
					throw new Exception("Unsupported file format!");
				}
				for (int i = 0; i < num4; i++)
				{
					string stringLiteral = this.GetStringLiteral(binaryReader, num2, 32U);
					if (stringLiteral.Length == 0)
					{
						throw new Exception("Unsupported file format!");
					}
					num2 += 32U;
					Entry entry = new Entry();
					binaryReader.BaseStream.Position = (long)((ulong)num2);
					entry.Offset = binaryReader.ReadInt32() + (int)num;
					entry.Size = binaryReader.ReadInt32();
					string extension = Path.GetExtension(stringLiteral);
					entry.IsPacked = (extension.Equals(".scr") || extension.Equals(".cmp"));
					entry.Name = stringLiteral;
					if ((long)(entry.Offset + entry.Size) > binaryReader.BaseStream.Length)
					{
						throw new Exception("Unsupported file format!");
					}
					this.dir.Add(entry);
					num2 += 8U;
				}
			}
			return this.dir;
		}
		
		public byte[] Unpack(Entry entry)
		{
			byte[] result;
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				if (entry.IsPacked)
				{
					binaryReader.BaseStream.Position = (long)entry.Offset;
					int unpakedSize = binaryReader.ReadInt32();
					byte[] input = binaryReader.ReadBytes(entry.Size);
					result = new Huffman().Decoder(input, unpakedSize);
				}
				else
				{
					binaryReader.BaseStream.Position = (long)entry.Offset;
					result = binaryReader.ReadBytes(entry.Size);
				}
			}
			return result;
		}
		
		public void Pack(string packPath, string outputFile)
		{
			bool flag = false;
			List<Entry> list = new List<Entry>();
			List<string> fileList = Files.GetFileList(packPath);
			for (int i = 0; i < fileList.Count; i++)
			{
				Entry entry = new Entry();
				byte[] array = File.ReadAllBytes(fileList[i]);
				entry.Name = Files.GetSubFileName(packPath, fileList[i]);
				if (entry.Name.Equals("block.tmp") && array.Length == 261)
				{
					flag = true;
					this.blockBuffer = array;
				}
				else
				{
					byte[] fileBuffer = HuffmanEncoder.HuffmanEncoding(array);
					entry.fileBuffer = fileBuffer;
					entry.UnpackedSize = array.Length;
					list.Add(entry);
				}
			}
			if (!flag)
			{
				throw new Exception("Can't find block.tmp, please use this tool to unpack AOS archive to generate block.tmp!");
			}
			string directoryName = Path.GetDirectoryName(packPath);
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
			{
				int num = list.Count * 40;
				int value = num + 273;
				int value2 = 0;
				binaryWriter.Write(value2);
				binaryWriter.Flush();
				binaryWriter.Write(value);
				binaryWriter.Flush();
				binaryWriter.Write(num);
				binaryWriter.Flush();
				binaryWriter.Write(this.blockBuffer);
				binaryWriter.Flush();
				int num2 = 0;
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
					binaryWriter.Write(num2);
					binaryWriter.Flush();
					num2 += list[j].fileBuffer.Length;
					num2 += 4;
					binaryWriter.Write(list[j].fileBuffer.Length);
					binaryWriter.Flush();
				}
				for (int k = 0; k < list.Count; k++)
				{
					binaryWriter.Write(list[k].UnpackedSize);
					binaryWriter.Flush();
					binaryWriter.Write(list[k].fileBuffer);
					binaryWriter.Flush();
				}
			}
		}
		private byte[] buffer;
		public byte[] blockBuffer;
		private List<Entry> dir = new List<Entry>();
	}
}
