using System;
using System.IO;

namespace GameRes.Utility
{
	public class CheckedStream : Stream
	{
		public override bool CanRead
		{
			get
			{
				return this.m_stream.CanRead;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return this.m_stream.CanWrite;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return this.m_stream.CanSeek;
			}
		}
		public override long Length
		{
			get
			{
				return this.m_stream.Length;
			}
		}
		public Stream BaseStream
		{
			get
			{
				return this.m_stream;
			}
		}
		public uint CheckSumValue
		{
			get
			{
				return this.m_checksum.Value;
			}
		}
		public CheckedStream(Stream stream, ICheckSum algorithm)
		{
			this.m_stream = stream;
			this.m_checksum = algorithm;
		}
		public override int Read(byte[] buffer, int offset, int count)
		{
			int num = this.m_stream.Read(buffer, offset, count);
			if (num > 0)
			{
				this.m_checksum.Update(buffer, offset, num);
			}
			return num;
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			this.m_stream.Write(buffer, offset, count);
			this.m_checksum.Update(buffer, offset, count);
		}
		public override long Position
		{
			get
			{
				return this.m_stream.Position;
			}
			set
			{
				this.m_stream.Position = value;
			}
		}
		public override void SetLength(long value)
		{
			this.m_stream.SetLength(value);
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.m_stream.Seek(offset, origin);
		}
		public override void Flush()
		{
			this.m_stream.Flush();
		}
		private Stream m_stream;
		private ICheckSum m_checksum;
	}
}
