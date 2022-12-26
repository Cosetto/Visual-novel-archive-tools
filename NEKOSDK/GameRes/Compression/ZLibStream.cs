using System;
using System.IO;
using System.IO.Compression;
using GameRes.Utility;

namespace GameRes.Compression
{
	public class ZLibStream : Stream
	{
		public Stream BaseStream
		{
			get
			{
				return this.m_stream.BaseStream;
			}
		}
		public int TotalIn
		{
			get
			{
				return this.m_total_in;
			}
		}
		public ZLibStream(Stream stream, CompressionMode mode, bool leave_open = false) : this(stream, mode, CompressionLevel.Default, leave_open)
		{
		}
		public ZLibStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leave_open = false)
		{
			try
			{
				if (CompressionMode.Decompress == mode)
				{
					this.InitDecompress(stream);
				}
				else
				{
					this.InitCompress(stream, level);
				}
				this.m_should_dispose_base = !leave_open;
			}
			catch
			{
				if (!leave_open)
				{
					stream.Dispose();
				}
				throw;
			}
		}
		private void InitDecompress(Stream stream)
		{
			int num = stream.ReadByte();
			int num2 = stream.ReadByte();
			if ((120 != num && 88 != num) || (num << 8 | num2) % 31 != 0)
			{
				throw new InvalidDataException("Data not recoginzed as zlib-compressed stream");
			}
			this.m_stream = new DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress, true);
			this.m_writing = false;
		}
		private void InitCompress(Stream stream, GameRes.Compression.CompressionLevel level)
		{
			int num = (int)level;
			System.IO.Compression.CompressionLevel compressionLevel;
			if (num == 0)
			{
				compressionLevel = System.IO.Compression.CompressionLevel.NoCompression;
			}
			else if (num > 5)
			{
				compressionLevel = System.IO.Compression.CompressionLevel.Optimal;
				num = 3;
			}
			else
			{
				compressionLevel = System.IO.Compression.CompressionLevel.Fastest;
				num = 1;
			}
			int num2 = 30720 | num << 6;
			num2 = (num2 + 30) / 31 * 31;
			stream.WriteByte((byte)(num2 >> 8));
			stream.WriteByte((byte)num2);
			this.m_stream = new DeflateStream(stream, compressionLevel, true);
			this.m_adler = new CheckedStream(this.m_stream, new Adler32());
			this.m_writing = true;
		}
		private void WriteCheckSum(Stream output)
		{
			uint checkSumValue = this.m_adler.CheckSumValue;
			output.WriteByte((byte)(checkSumValue >> 24));
			output.WriteByte((byte)(checkSumValue >> 16));
			output.WriteByte((byte)(checkSumValue >> 8));
			output.WriteByte((byte)checkSumValue);
		}
		public override bool CanRead
		{
			get
			{
				return !this.m_writing;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return this.m_writing;
			}
		}
		public override long Length
		{
			get
			{
				return this.m_stream.Length;
			}
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
		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.m_stream.Read(buffer, offset, count);
		}
		public override int ReadByte()
		{
			return this.m_stream.ReadByte();
		}
		public override void Flush()
		{
			this.m_stream.Flush();
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("ZLibStream.Seek method not supported");
		}
		public override void SetLength(long length)
		{
			throw new NotSupportedException("ZLibStream.SetLength method not supported");
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count > 0)
			{
				this.m_adler.Write(buffer, offset, count);
				this.m_total_in += count;
			}
		}
		public override void WriteByte(byte value)
		{
			this.m_adler.WriteByte(value);
			this.m_total_in++;
		}
		protected override void Dispose(bool disposing)
		{
			if (!this.m_disposed)
			{
				try
				{
					if (disposing)
					{
						Stream baseStream = this.m_stream.BaseStream;
						this.m_stream.Dispose();
						if (this.m_writing)
						{
							this.WriteCheckSum(baseStream);
							this.m_adler.Dispose();
						}
						if (this.m_should_dispose_base)
						{
							baseStream.Dispose();
						}
					}
					this.m_disposed = true;
				}
				finally
				{
					base.Dispose(disposing);
				}
			}
		}
		private DeflateStream m_stream;
		private CheckedStream m_adler;
		private bool m_should_dispose_base;
		private bool m_writing;
		public int m_total_in;
		private bool m_disposed;
	}
}
