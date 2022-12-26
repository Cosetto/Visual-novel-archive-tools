using System;
using System.IO;

namespace ComponentAce.Compression.Libs.zlib
{
	internal class MyZlib
	{
		public static void CopyStream(Stream input, Stream output)
		{
			byte[] buffer = new byte[2000];
			int count;
			while ((count = input.Read(buffer, 0, 2000)) > 0)
			{
				output.Write(buffer, 0, count);
				output.Flush();
			}
		}
		public static byte[] compressBytes(byte[] sourceByte)
		{
			MemoryStream memoryStream = new MemoryStream(sourceByte);
			Stream stream = MyZlib.compressStream(memoryStream);
			byte[] array = new byte[stream.Length];
			stream.Position = 0L;
			stream.Read(array, 0, array.Length);
			stream.Close();
			memoryStream.Close();
			return array;
		}
		public static byte[] deCompressBytes(byte[] sourceByte)
		{
			MemoryStream memoryStream = new MemoryStream(sourceByte);
			Stream stream = MyZlib.deCompressStream(memoryStream);
			byte[] array = new byte[stream.Length];
			stream.Position = 0L;
			stream.Read(array, 0, array.Length);
			stream.Close();
			memoryStream.Close();
			return array;
		}
		public static Stream compressStream(Stream sourceStream)
		{
			MemoryStream memoryStream = new MemoryStream();
			ZOutputStream zoutputStream = new ZOutputStream(memoryStream, -1);
			MyZlib.CopyStream(sourceStream, zoutputStream);
			zoutputStream.finish();
			return memoryStream;
		}
		public static Stream deCompressStream(Stream sourceStream)
		{
			MemoryStream memoryStream = new MemoryStream();
			ZOutputStream zoutputStream = new ZOutputStream(memoryStream);
			MyZlib.CopyStream(sourceStream, zoutputStream);
			zoutputStream.finish();
			return memoryStream;
		}
	}
}
