using System;
using System.IO;

namespace Lilim
{
	internal class Huffman
	{
		public void Initialize(Stream input)
		{
			this.m_input = new MsbBitStream(input, true);
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
			if (this.cache_bits >= 8)
			{
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
		}
		public byte[] Decoder(byte[] input, int unpakedSize)
		{
			this.Initialize(new MemoryStream(input));
			byte[] array = new byte[unpakedSize];
			this.m_token = 256;
			ushort num = this.CreateTree();
			int num2 = unpakedSize;
			do
			{
				ushort num3 = num;
				int num4 = 0;
				while (num3 >= 256)
				{
					num4++;
					int bits = this.m_input.GetBits(1);
					if (-1 == bits)
					{
						break;
					}
					if (bits != 0)
					{
						num3 = this.rhs[(int)num3];
					}
					else
					{
						num3 = this.lhs[(int)num3];
					}
				}
				byte[] array2 = array;
				int pos = this.m_pos;
				this.m_pos = pos + 1;
				array2[pos] = (byte)num3;
			}
			while (--num2 != 0);
			return array;
		}
		public byte[] Encoder(byte[] input)
		{
			byte[] array = new byte[input.Length * 255];
			byte[] array2 = new byte[1];
			int num = 0;
			for (ushort num2 = 256; num2 < 512; num2 += 1)
			{
				this.lhs[(int)num2] = num2;
			}
			ushort num3 = 256;
			ushort num4 = 0;
			while (num3 < 512)
			{
				this.rhs[(int)num3] = num4;
				num3 += 1;
				num4 += 1;
			}
			this.rhs[257] = 0;
			using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(array)))
			{
				int num5 = 9;
				this.WriteTree(binaryWriter);
				for (int i = 0; i < input.Length; i++)
				{
					if (input[i] == 0)
					{
						this.EncodeBit(1, 1, binaryWriter);
					}
					else if (input[i] == 1)
					{
						for (int j = 0; j < 255; j++)
						{
							this.EncodeBit(0, 1, binaryWriter);
						}
					}
					else
					{
						for (int k = 0; k < (int)(input[i] - 1); k++)
						{
							if (num5 == 0)
							{
								this.EncodeBit(0, 1, binaryWriter);
							}
							else
							{
								num5--;
							}
						}
						this.EncodeBit(1, 1, binaryWriter);
					}
				}
				this.EncodeBit(255, 8, binaryWriter);
				this.EncodeBit(255, 8, binaryWriter);
				num = (int)binaryWriter.BaseStream.Position;
			}
			if (num != 0)
			{
				array2 = new byte[num];
				Buffer.BlockCopy(array, 0, array2, 0, num);
			}
			return array2;
		}
		private void WriteTree(BinaryWriter writer)
		{
			for (int i = 256; i < 512; i++)
			{
				if (i == 511)
				{
					this.EncodeBit(0, 1, writer);
					this.EncodeBit(1, 8, writer);
				}
				else
				{
					this.EncodeBit(1, 1, writer);
				}
			}
			for (int j = 511; j > 255; j--)
			{
				this.EncodeBit(0, 1, writer);
				this.EncodeBit((int)this.rhs[j], 8, writer);
			}
		}
		private ushort CreateTree()
		{
			int bits = this.m_input.GetBits(1);
			if (-1 == bits)
			{
				throw new Exception("Unexpected end of the Huffman-compressed stream.");
			}
			if (bits == 0)
			{
				return (ushort)this.m_input.GetBits(8);
			}
			ushort token = this.m_token;
			this.m_token = (ushort)(token + 1);
			ushort num = token;
			if (num >= 512)
			{
				throw new Exception("Invalid Huffman-compressed stream.");
			}
			this.lhs[(int)num] = this.CreateTree();
			this.rhs[(int)num] = this.CreateTree();
			return num;
		}
		private MsbBitStream m_input;
		private const int TreeSize = 512;
		private ushort[] lhs = new ushort[512];
		private ushort[] rhs = new ushort[512];
		private int m_pos;
		private ushort m_token = 256;
		public int cache_bits;
		public int mm_bits;
	}
}
