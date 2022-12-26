using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Unity
{
	internal class OutputCryptoStream : CryptoStream
	{
		public OutputCryptoStream(Stream input, ICryptoTransform transform) : base(input, transform, CryptoStreamMode.Write)
		{
			this.m_transform = transform;
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && this.m_transform != null)
			{
				this.m_transform.Dispose();
				this.m_transform = null;
			}
		}
		public void Write(int data)
		{
			byte[] array = new List<byte>
			{
				(byte)data,
				(byte)(data >> 8),
				(byte)(data >> 16),
				(byte)(data >> 24)
			}.ToArray();
			this.Write(array, 0, array.Length);
			this.Flush();
			this.Total_byte += 4;
		}
		public void Write(short data)
		{
			byte[] array = new List<byte>
			{
				(byte)data,
				(byte)(data >> 8)
			}.ToArray();
			this.Write(array, 0, array.Length);
			this.Flush();
			this.Total_byte += 2;
		}
		public void Write(byte data)
		{
			byte[] array = new List<byte>
			{
				data
			}.ToArray();
			this.Write(array, 0, array.Length);
			this.Flush();
			this.Total_byte++;
		}
		public void Write(byte[] buffer)
		{
			this.Write(buffer, 0, buffer.Length);
			this.Flush();
			this.Total_byte += buffer.Length;
		}
		private ICryptoTransform m_transform;
		public int Total_byte;
	}
}
