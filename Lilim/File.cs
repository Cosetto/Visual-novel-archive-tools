using System;
using System.Collections.Generic;
using System.IO;

namespace Lilim
{
	public static class Files
	{
		public static List<string> GetFileList(string path)
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
					list.AddRange(Files.GetFileList(path2));
				}
			}
			return list;
		}
		public static string GetSubFileName(string rootPath, string filePath)
		{
			return filePath.Substring(rootPath.Length + 1, filePath.Length - (rootPath.Length + 1));
		}
	}
}
