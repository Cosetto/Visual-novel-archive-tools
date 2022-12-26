using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EAGLS
{
	internal class ArcPak
	{
		public byte[] CrypteIndex(byte[] index, int indexSize)
		{
			CRuntimeRandomGenerator cruntimeRandomGenerator = new CRuntimeRandomGenerator();
			cruntimeRandomGenerator.SRand(indexSize);
			byte[] array = new byte[indexSize];
			for (int i = 0; i < indexSize; i++)
			{
				int num = (int)ArcPak.IndexKey[cruntimeRandomGenerator.Rand() % ArcPak.IndexKey.Length];
				array[i] = (byte)((int)index[i] ^ num);
			}
			return array;
		}
		public void EnCrypteIndex(byte[] index, int indexSize, int seed)
		{
			CRuntimeRandomGenerator cruntimeRandomGenerator = new CRuntimeRandomGenerator();
			cruntimeRandomGenerator.SRand(seed);
			for (int i = 0; i < indexSize; i++)
			{
				if (i + 1 == indexSize)
				{
					string.Format("sd", new object[0]);
				}
				int num = (int)ArcPak.IndexKey[cruntimeRandomGenerator.Rand() % ArcPak.IndexKey.Length];
				index[i] = (byte)((int)index[i] ^ num);
			}
		}
		public void Pack(string packPath)
		{
			List<PackEntry> list = new List<PackEntry>();
			string[] files = Directory.GetFiles(packPath);
			for (int i = 0; i < files.Length; i++)
			{
				PackEntry packEntry = new PackEntry();
				byte[] fileBuffer = File.ReadAllBytes(files[i]);
				packEntry.fileBuffer = fileBuffer;
				packEntry.Name = Path.GetFileName(files[i]);
				list.Add(packEntry);
			}
			string directoryName = Path.GetDirectoryName(packPath);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(packPath);
			string path = Path.Combine(directoryName, fileNameWithoutExtension + ".pak");
			string path2 = Path.Combine(directoryName, fileNameWithoutExtension + ".idx");
			long num = 0L;
			MemoryStream memoryStream = new MemoryStream(new byte[400004]);
			byte[] array;
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				for (int j = 0; j < list.Count; j++)
				{
					byte[] bytes = Encoding.GetEncoding(932).GetBytes(list[j].Name);
					if (bytes.Length > 24)
					{
						throw new Exception("Files' name are too long!");
					}
					byte[] dst = new byte[24];
					Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
					binaryWriter.Write(dst);
					binaryWriter.Flush();
					binaryWriter.Write(num);
					num += (long)list[j].fileBuffer.Length;
					binaryWriter.Write(list[j].fileBuffer.Length);
					binaryWriter.Flush();
					int value = 0;
					binaryWriter.Write(value);
					binaryWriter.Flush();
				}
				int num2 = (int)binaryWriter.BaseStream.Length;
				binaryWriter.BaseStream.Position = binaryWriter.BaseStream.Length - 4L;
				binaryWriter.Write(num2);
				binaryWriter.Flush();
				array = memoryStream.ToArray();
				this.EnCrypteIndex(array, 400000, num2);
			}
			File.WriteAllBytes(path2, array);
			EaglsEncryption eaglsEncryption = new EaglsEncryption();
			LehmerRandomGenerator lehmerRandomGenerator = new LehmerRandomGenerator();
			lehmerRandomGenerator.setSeed(2039843147);
			CgEncryption cgEncryption = new CgEncryption(lehmerRandomGenerator);
			using (BinaryWriter binaryWriter2 = new BinaryWriter(new FileStream(path, FileMode.Create)))
			{
				for (int k = 0; k < list.Count; k++)
				{
					byte[] bytes2 = new byte[1];
					string extension = Path.GetExtension(list[k].Name);
					if (extension.Equals(".dat"))
					{
						bytes2 = eaglsEncryption.Decrypt(list[k].fileBuffer);
						File.WriteAllBytes(Path.Combine(directoryName, list[k].Name), bytes2);
					}
					else if (extension.Equals("gr"))
					{
						bytes2 = cgEncryption.Decrypt(list[k].fileBuffer);
					}
					else
					{
						bytes2 = list[k].fileBuffer;
					}
					binaryWriter2.Write(bytes2);
				}		
			}
		}
		internal static readonly string IndexKey = "1qaz2wsx3edc4rfv5tgb6yhn7ujm8ik,9ol.0p;/-@:^[]";
		internal static readonly byte[] EaglsKey = Encoding.ASCII.GetBytes("EAGLS_SYSTEM");
		internal static readonly byte[] AdvSysKey = Encoding.ASCII.GetBytes("ADVSYS");
		private byte[] buffer;
		private List<PackEntry> dir = new List<PackEntry>();
	}
}
