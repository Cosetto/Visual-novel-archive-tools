using System;
using System.IO;

namespace Escu
{
	internal sealed class LzwEncoder
	{
		public LzwEncoder(byte[] output)
		{
			this.m_output = output;
		}
		private byte MakeByte(int pos, int src)
		{
			byte b = 0;
			int num;
			if (pos < 8)
			{
				num = pos;
			}
			else
			{
				num = 8;
			}
			for (int i = 0; i < num; i--)
			{
				int num2 = src >> pos;
				pos--;
				if ((num2 & 1) == 1)
				{
					b |= 1;
				}
				else
				{
					b |= 0;
				}
				b = (byte)(b << 1);
			}
			return b;
		}
		public void EncodeBit(int token, int token_width, BinaryWriter wrter)
		{
			int num = (1 << token_width) - 1;
			if (this.cache_bits < 24)
			{
				this.mm_bits <<= token_width;
				this.mm_bits |= (token & num);
				this.cache_bits += token_width;
			}
			while (this.cache_bits >= token_width)
			{
				int num2 = this.cache_bits - 8;
				byte b = (byte)(this.mm_bits >> num2 & 255);
				this.cache_bits -= 8;
				int num3 = (1 << this.cache_bits) - 1;
				this.mm_bits &= num3;
				byte value = b;
				wrter.Write(value);
				wrter.Flush();
			}
		}
		public byte[] Pack(byte[] buffer)
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(this.m_output)))
			{
				int value = 7365473;
				binaryWriter.Write(value);
				binaryWriter.Flush();
				int value2 = (int)Binary.ToBigEndian((uint)buffer.Length);
				binaryWriter.Write(value2);
				binaryWriter.Flush();
				int num = 0;
				for (int i = 0; i < buffer.Length; i++)
				{
					num++;
					if (num > 32768)
					{
						this.EncodeBit(258, 9, binaryWriter);
						num = 0;
					}
					int token = (int)buffer[i];
					this.EncodeBit(token, 9, binaryWriter);
				}
				this.EncodeBit(256, 9, binaryWriter);
				this.EncodeBit(256, 9, binaryWriter);
				this.last_size = (int)binaryWriter.BaseStream.Position;
			}
			this.m_paked = new byte[this.last_size];
			Buffer.BlockCopy(this.m_output, 0, this.m_paked, 0, this.last_size);
			return this.m_paked;
		}
		private byte[] m_output;
		private byte[] m_inbuffer;
		public int cache_bits;
		public int mm_bits;
		public int src;
		public byte[] m_paked;
		public int last_size;
	}
}
