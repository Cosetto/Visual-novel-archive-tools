//https://github.com/morkt/GARbro/blob/master/ArcFormats/Unity/ArcBIN.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Unity
{
	internal class ArcBIN
	{
		private static List<string> GetFileList(string path)
		{
			List<string> list = new List<string>();
			if (Directory.Exists(path))
			{
				foreach (string item in Directory.GetFiles(path))
				{
					list.Add(item);
				}
				foreach (string path2 in Directory.GetDirectories(path))
				{
					list.AddRange(ArcBIN.GetFileList(path2));
				}
			}
			return list;
		}
		public List<Entry> TryOpen(string filePath)
		{
			string path = Path.ChangeExtension(filePath, "idx");
			if (!File.Exists(path))
			{
				throw new Exception("Can't find index file!");
			}
			this.buffer = File.ReadAllBytes(filePath);
			this.idxBuffer = File.ReadAllBytes(path);
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.idxBuffer)))
			{
				using (Aes aes = Aes.Create())
				{
					aes.Padding = PaddingMode.PKCS7;
					aes.Mode = CipherMode.CBC;
					aes.KeySize = 128;
					aes.IV = new byte[]
					{
						65,
						82,
						67,
						45,
						80,
						65,
						67,
						75,
						80,
						65,
						83,
						83,
						87,
						79,
						82,
						68
					};
					aes.Key = new byte[]
					{
						99,
						54,
						101,
						97,
						104,
						98,
						113,
						57,
						115,
						106,
						117,
						97,
						119,
						104,
						118,
						100,
						114,
						57,
						107,
						118,
						104,
						112,
						115,
						109,
						53,
						113,
						118,
						51,
						57,
						51,
						103,
						97
					};
					byte[] array = new byte[256];
					BinDeserializer binDeserializer = new BinDeserializer();
					while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
					{
						int num = binaryReader.ReadInt32();
						if (num <= 0)
						{
							return null;
						}
						if (num > array.Length)
						{
							array = new byte[num];
						}
						if (binaryReader.Read(array, 0, num) < num)
						{
							return null;
						}
						using (ICryptoTransform cryptoTransform = aes.CreateDecryptor())
						{
							using (MemoryStream memoryStream = new MemoryStream(array, 0, num))
							{
								using (InputCryptoStream inputCryptoStream = new InputCryptoStream(memoryStream, cryptoTransform))
								{
									IDictionary dictionary = binDeserializer.DeserializeEntry(inputCryptoStream);
									string text = dictionary["fileName"] as string;
									if (string.IsNullOrEmpty(text))
									{
										return null;
									}
									text = text.TrimStart(new char[]
									{
										'/',
										'\\'
									});
									Entry entry = new Entry();
									entry.Name = text.Replace('/', '\\');
									entry.Offset = (int)Convert.ToInt64(dictionary["index"]);
									entry.Size = (int)Convert.ToUInt32(dictionary["size"]);
									if (entry.Offset + entry.Size > this.buffer.Length)
									{
										throw new Exception("Unsupported file format!");
									}
									this.dir.Add(entry);
								}
							}
						}
					}
				}
			}
			return this.dir;
		}
		public byte[] Unpack(Entry entry)
		{
			byte[] array;
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.buffer)))
			{
				binaryReader.BaseStream.Position = (long)entry.Offset;
				array = binaryReader.ReadBytes(entry.Size);
				using (Aes aes = Aes.Create())
				{
					aes.Padding = PaddingMode.PKCS7;
					aes.Mode = CipherMode.CBC;
					aes.KeySize = 256;
					aes.IV = new byte[]
					{
						65,
						82,
						67,
						45,
						80,
						65,
						67,
						75,
						80,
						65,
						83,
						83,
						87,
						79,
						82,
						68
					};
					aes.Key = new byte[]
					{
						99,
						54,
						101,
						97,
						104,
						98,
						113,
						57,
						115,
						106,
						117,
						97,
						119,
						104,
						118,
						100,
						114,
						57,
						107,
						118,
						104,
						112,
						115,
						109,
						53,
						113,
						118,
						51,
						57,
						51,
						103,
						97
					};
					byte[] bytes = new byte[256];
					using (ICryptoTransform cryptoTransform = aes.CreateDecryptor())
					{
						array = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
					}
				}
			}
			return array;
		}
		public void Pack(string packPath)
		{
			List<Entry> list = new List<Entry>();
			List<string> fileList = ArcBIN.GetFileList(packPath);
			int num = new DirectoryInfo(packPath).GetDirectories().Length;
			int num2 = packPath.LastIndexOf('\\') + 1;
			string text = packPath.Substring(num2, packPath.Length - num2);
			for (int i = 0; i < fileList.Count; i++)
			{
				Entry entry = new Entry();
				byte[] array = File.ReadAllBytes(fileList[i]);
				entry.UnpackedSize = array.Length;
				entry.fileBuffer = array;
				DirectoryInfo parent = Directory.GetParent(fileList[i]);
				string text2;
				if (parent.Name.Equals(text))
				{
					text2 = "\\" + text + "\\" + Path.GetFileName(fileList[i]);
				}
				else
				{
					text2 = string.Concat(new string[]
					{
						"\\",
						text,
						"\\",
						parent.Name,
						"\\",
						Path.GetFileName(fileList[i])
					});
				}
				entry.Name = text2.Replace('\\', '/');
				list.Add(entry);
			}
			string directoryName = Path.GetDirectoryName(packPath);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(packPath);
			string path = Path.Combine(directoryName, fileNameWithoutExtension + ".bin");
			string path2 = Path.ChangeExtension(path, ".idx");
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(path, FileMode.Create)))
			{
				using (Aes aes = Aes.Create())
				{
					aes.Padding = PaddingMode.PKCS7;
					aes.Mode = CipherMode.CBC;
					aes.KeySize = 256;
					aes.IV = new byte[]
					{
						65,
						82,
						67,
						45,
						80,
						65,
						67,
						75,
						80,
						65,
						83,
						83,
						87,
						79,
						82,
						68
					};
					aes.Key = new byte[]
					{
						99,
						54,
						101,
						97,
						104,
						98,
						113,
						57,
						115,
						106,
						117,
						97,
						119,
						104,
						118,
						100,
						114,
						57,
						107,
						118,
						104,
						112,
						115,
						109,
						53,
						113,
						118,
						51,
						57,
						51,
						103,
						97
					};
					byte[] bytes = new byte[256];
					new BinDeserializer();
					int num3 = 0;
					for (int j = 0; j < list.Count; j++)
					{
						byte[] array2 = new byte[1];
						using (ICryptoTransform cryptoTransform = aes.CreateEncryptor())
						{
							array2 = cryptoTransform.TransformFinalBlock(list[j].fileBuffer, 0, list[j].fileBuffer.Length);
							list[j].Size = array2.Length;
						}
						num3 += 4;
						list[j].Offset = num3;
						num3 += list[j].Size;
						binaryWriter.Write(list[j].Size);
						binaryWriter.Write(array2);
						binaryWriter.Flush();
					}
				}
			}
			using (BinaryWriter binaryWriter2 = new BinaryWriter(new FileStream(path2, FileMode.Create)))
			{
				using (Aes aes2 = Aes.Create())
				{
					aes2.Padding = PaddingMode.PKCS7;
					aes2.Mode = CipherMode.CBC;
					aes2.KeySize = 128;
					aes2.IV = new byte[]
					{
						65,
						82,
						67,
						45,
						80,
						65,
						67,
						75,
						80,
						65,
						83,
						83,
						87,
						79,
						82,
						68
					};
					aes2.Key = new byte[]
					{
						99,
						54,
						101,
						97,
						104,
						98,
						113,
						57,
						115,
						106,
						117,
						97,
						119,
						104,
						118,
						100,
						114,
						57,
						107,
						118,
						104,
						112,
						115,
						109,
						53,
						113,
						118,
						51,
						57,
						51,
						103,
						97
					};
					byte[] bytes = new byte[256];
					BinDeserializer binDeserializer = new BinDeserializer();
					for (int k = 0; k < list.Count; k++)
					{
						byte[] array3 = new byte[4096];
						byte[] array4 = new byte[1];
						using (ICryptoTransform cryptoTransform2 = aes2.CreateEncryptor())
						{
							using (MemoryStream memoryStream = new MemoryStream(array3, 0, array3.Length))
							{
								using (BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream))
								{
									binDeserializer.SerializeEntryBinary(binaryWriter3, new Dictionary<string, object>
									{
										{
											"fileName",
											list[k].Name
										},
										{
											"index",
											list[k].Offset
										},
										{
											"size",
											list[k].Size
										}
									});
									binaryWriter3.Flush();
									int num4 = (int)binaryWriter3.BaseStream.Position;
									array4 = new byte[num4];
									Buffer.BlockCopy(array3, 0, array4, 0, num4);
									array4 = cryptoTransform2.TransformFinalBlock(array4, 0, array4.Length);
								}
							}
						}
						binaryWriter2.Write(array4.Length);
						binaryWriter2.Flush();
						binaryWriter2.Write(array4);
						binaryWriter2.Flush();
					}
				}
			}
		}
		private byte[] buffer;
		private byte[] idxBuffer;
		public byte[] blockBuffer;
		private List<Entry> dir = new List<Entry>();
	}
}
