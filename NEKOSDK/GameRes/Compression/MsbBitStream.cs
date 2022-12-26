using System;
using System.IO;

namespace GameRes.Compression
{
	public class MsbBitStream : BitStream
	{
		public MsbBitStream(Stream file, bool leave_open = false) : base(file, leave_open)
		{
		}
		public int GetBits(int count)
		{
			while (this.m_cached_bits < count)
			{
				int num = this.m_input.ReadByte();
				if (-1 == num)
				{
					return -1;
				}
				this.m_bits = (this.m_bits << 8 | num);
				this.m_cached_bits += 8;
			}
			int num2 = (1 << count) - 1;
			this.m_cached_bits -= count;
			return this.m_bits >> this.m_cached_bits & num2;
		}
		public int GetNextBit()
		{
			return this.GetBits(1);
		}
	}
}
