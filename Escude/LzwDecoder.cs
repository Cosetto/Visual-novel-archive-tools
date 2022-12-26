using System;
using System.IO;

namespace Escu
{
	internal sealed class LzwDecoder : IDisposable
	{
		public byte[] Output
		{
			get
			{
				return this.m_output;
			}
		}
		public LzwDecoder(Stream input, int unpacked_size)
		{
			this.m_input = new MsbBitStream(input, true);
			this.m_output = new byte[unpacked_size];
		}
		public void Unpack()
		{
			int i = 0;
			int[] array = new int[35072];
			int num = 9;
			int num2 = 0;
			while (i < this.m_output.Length)
			{
				int num3 = this.m_input.GetBits(num);
				if (-1 == num3)
				{
					throw new EndOfStreamException("Invalid compressed stream");
				}
				if (256 == num3)
				{
					break;
				}
				if (257 == num3)
				{
					num++;
					if (num > 24)
					{
						throw new Exception("Invalid comressed stream");
					}
				}
				else if (258 == num3)
				{
					num = 9;
					num2 = 0;
				}
				else
				{
					if (num2 >= array.Length)
					{
						throw new Exception("Invalid comressed stream");
					}
					array[num2++] = i;
					if (num3 < 256)
					{
						this.m_output[i++] = (byte)num3;
					}
					else
					{
						num3 -= 259;
						if (num3 >= num2)
						{
							throw new Exception("Invalid comressed stream");
						}
						int num4 = array[num3];
						int num5 = Math.Min(this.m_output.Length - i, array[num3 + 1] - num4 + 1);
						if (num5 < 0)
						{
							throw new Exception("Invalid comressed stream");
						}
						Binary.CopyOverlapped(this.m_output, num4, i, num5);
						i += num5;
					}
				}
			}
		}
		public void Dispose()
		{
			if (!this._disposed)
			{
				this.m_input.Dispose();
				this._disposed = true;
			}
			GC.SuppressFinalize(this);
		}
		private MsbBitStream m_input;
		private byte[] m_output;
		private bool _disposed;
	}
}
