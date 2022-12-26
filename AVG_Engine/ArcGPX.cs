using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GPX
{
	internal class ArcGPX
	{
		public byte[] CryptIndex(byte[] buffer, int bufferPos, int cryptSize, int keyIndex, string arcName)
		{
			int i = 0;
			if (!string.IsNullOrEmpty(arcName))
			{
				byte[] bytes = Encoding.ASCII.GetBytes(arcName);
				int num = bufferPos;
				while (i < cryptSize)
				{
					int num2 = num;
					buffer[num2] ^= (byte)(num ^ (int)(ArcGPX.key[keyIndex % 23] ^ bytes[keyIndex % bytes.Length]));
					keyIndex++;
					i++;
					num++;
				}
			}
			else
			{
				for (int j = bufferPos; j < cryptSize; j++)
				{
					int num3 = j;
					buffer[num3] ^= (byte)((int)ArcGPX.key[(j + keyIndex) % 23] ^ j);
					i++;
				}
			}
			return buffer;
		}
		private void DecryptData(byte[] buffer, int iPos, uint size, int keyIIndex, byte[] arcName, uint arcNameSize)
		{
			if (arcName != null)
			{
				uint num = 0U;
				if (size > 0U)
				{
					int num2 = iPos;
					int num3 = keyIIndex - iPos;
					do
					{
						uint num4 = (uint)(num3 + num2++);
						int num5 = num2 - 1;
						buffer[num5] ^= (byte)((byte)((ulong)num + (ulong)((long)keyIIndex)) ^ (ArcGPX.key[(int)(num4 % 23U)] ^ arcName[(int)(num4 % arcNameSize)]));
						num3 = keyIIndex - iPos;
						num += 1U;
					}
					while (num < size);
					return;
				}
			}
			else
			{
				uint num6 = 0U;
				if (size > 0U)
				{
					int num7 = iPos;
					do
					{
						uint num8 = (uint)((long)(num7++ + keyIIndex - iPos) % 23L);
						byte b = (byte)((ulong)num6++ + (ulong)((long)keyIIndex));
						int num9 = num7 - 1;
						buffer[num9] ^= (byte)(ArcGPX.key[(int)num8] ^ b);
					}
					while (num6 < size);
				}
			}
		}
		public byte[] Unpack(PackEntry entry)
		{
			byte[] array;
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				binaryReader.BaseStream.Position = entry.Offset + (long)this.entryPointer;
				array = binaryReader.ReadBytes((int)entry.Size);
				if (this.isCrypt > 0)
				{
					array = this.CryptIndex(array, 0, array.Length, 0, this.arcName);
				}
			}
			return array;
		}
		public List<PackEntry> TryOpen(string filePath)
		{
			this.buffer = File.ReadAllBytes(filePath);
			this.arcName = Path.GetFileName(filePath);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				this.fileHead = binaryReader.ReadBytes(48);
				if (LittleEndian.ToInt32<byte[]>(this.fileHead, 0) != 5265479)
				{
					throw new Exception("Unsupported file formats!");
				}
				int count = LittleEndian.ToInt32<byte[]>(this.fileHead, 28);
				this.isCrypt = LittleEndian.ToInt32<byte[]>(this.fileHead, 20);
				int num = LittleEndian.ToInt32<byte[]>(this.fileHead, 24);
				this.entryPointer = LittleEndian.ToInt32<byte[]>(this.fileHead, 40);
				byte[] value = binaryReader.ReadBytes(count);
				if (this.isCrypt > 0)
				{
					int num2 = 0;
					byte[] bytes = Encoding.ASCII.GetBytes(this.arcName);
					for (int i = 0; i < num; i++)
					{
						this.DecryptData(value, num2, 4U, 0, bytes, (uint)bytes.Length);
						uint num3 = LittleEndian.ToUInt32<byte[]>(value, num2);
						num2 += 4;
						this.DecryptData(value, num2, num3 - 4U, 4, bytes, (uint)bytes.Length);
						num2 += (int)(num3 - 4U);
					}
				}
				using (BinaryReader binaryReader2 = new BinaryReader(new MemoryStream(value)))
				{
					for (int j = 0; j < num; j++)
					{
						int num4 = binaryReader2.ReadInt32();
						byte[] array = binaryReader2.ReadBytes(num4 - 4);
						uint size = LittleEndian.ToUInt32<byte[]>(array, 0);
						int num5 = LittleEndian.ToInt32<byte[]>(array, 8);
						int c = LittleEndian.ToInt32<byte[]>(array, 12);
						int c2 = LittleEndian.ToInt32<byte[]>(array, 16);
						int num6 = LittleEndian.ToInt32<byte[]>(array, 20);
						byte[] array2 = new byte[num5 * 2];
						Buffer.BlockCopy(array, 28, array2, 0, array2.Length);
						string @string = Encoding.Unicode.GetString(array2);
						PackEntry packEntry = new PackEntry();
						packEntry.IsPacked = false;
						packEntry.Offset = (long)num6;
						packEntry.Name = @string;
						packEntry.Size = size;
						packEntry.c1 = c;
						packEntry.c2 = c2;
						this.dir.Add(packEntry);
					}
				}
			}
			return this.dir;
		}
		public void WriteInt(List<byte> arrary, int data)
		{
			arrary.Add((byte)data);
			arrary.Add((byte)((uint)data >> 8));
			arrary.Add((byte)((uint)data >> 16));
			arrary.Add((byte)((uint)data >> 24));
		}
		public void Pack(string packPath, string outputFile)
		{
			List<PackEntry> list = new List<PackEntry>();
			List<byte> list2 = new List<byte>();
			string path = Path.Combine(packPath, "FileList.txt");
			if (!File.Exists(path))
			{
				throw new Exception("Can't find FileList.txt, please use this tool to unpack GPX archive to generate FileList.txt!");
			}
			string path2 = Path.Combine(packPath, "block.tmp");
			if (!File.Exists(path2))
			{
				throw new Exception("Can't find block.tmp, please use this tool to unpack GPX archive to generate block.tmp!");
			}
			using (StreamReader streamReader = new StreamReader(new FileStream(path, FileMode.Open)))
			{
				while (!streamReader.EndOfStream)
				{
					list.Add(new PackEntry
					{
						Name = streamReader.ReadLine(),
						c1 = int.Parse(streamReader.ReadLine()),
						c2 = int.Parse(streamReader.ReadLine())
					});
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				string path3 = Path.Combine(packPath, list[i].Name);
				if (!File.Exists(path3))
				{
					throw new Exception("The file(s) in FileList.txt couldn't be found");
				}
				byte[] fileBuffer = File.ReadAllBytes(path3);
				list[i].fileBuffer = fileBuffer;
			}
			byte[] array = File.ReadAllBytes(path2);
			if (array.Length != 48)
			{
				throw new Exception("File header size mismatch!");
			}
			string directoryName = Path.GetDirectoryName(packPath);
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
			{
				List<byte> list3 = new List<byte>();
				int num = 0;
				for (int j = 0; j < list.Count; j++)
				{
					list3.Clear();
					byte[] bytes = Encoding.Unicode.GetBytes(list[j].Name);
					int data = bytes.Length + 36;
					this.WriteInt(list3, data);
					int data2 = 0;
					this.WriteInt(list3, list[j].fileBuffer.Length);
					this.WriteInt(list3, data2);
					this.WriteInt(list3, bytes.Length / 2);
					this.WriteInt(list3, list[j].c1);
					this.WriteInt(list3, list[j].c2);
					this.WriteInt(list3, num);
					num += list[j].fileBuffer.Length;
					int data3 = 0;
					this.WriteInt(list3, data3);
					list3.AddRange(bytes);
					int data4 = 0;
					this.WriteInt(list3, data4);
					list2.AddRange(list3.ToArray());
				}
				byte[] array2 = list2.ToArray();
				using (BinaryWriter binaryWriter2 = new BinaryWriter(new MemoryStream(array)))
				{
					binaryWriter2.BaseStream.Position = 20L;
					int value = 0;
					binaryWriter2.Write(value);
					binaryWriter2.Flush();
					binaryWriter2.BaseStream.Position = 24L;
					int count = list.Count;
					binaryWriter2.Write(count);
					binaryWriter2.Flush();
					binaryWriter2.BaseStream.Position = 28L;
					int value2 = array2.Length;
					binaryWriter2.Write(value2);
					binaryWriter2.Flush();
					binaryWriter2.BaseStream.Position = 40L;
					int value3 = array.Length + array2.Length;
					binaryWriter2.Write(value3);
					binaryWriter2.Flush();
				}
				binaryWriter.Write(array);
				binaryWriter.Flush();
				binaryWriter.Write(array2);
				binaryWriter.Flush();
				foreach (PackEntry packEntry in list)
				{
					binaryWriter.Write(packEntry.fileBuffer);
					binaryWriter.Flush();
				}
			}
		}
		private List<PackEntry> dir = new List<PackEntry>();
		private byte[] buffer;
		private int isCrypt;
		private int entryPointer;
		private string arcName = "";
		public byte[] fileHead;
		public static readonly byte[] key = new byte[]
		{
			64,
			33,
			40,
			56,
			166,
			110,
			67,
			165,
			64,
			33,
			40,
			56,
			166,
			67,
			165,
			100,
			62,
			101,
			36,
			32,
			70,
			110,
			116
		};
	}
}
