using System;

namespace GameRes.Utility
{
	public sealed class Adler32 : ICheckSum
	{
		public unsafe static uint Compute(byte[] buf, int pos, int len)
		{
			if (len == 0)
			{
				return 1U;
			}
			fixed (byte* ptr = &buf[pos])
			{
				byte* buf2 = ptr;
				return Adler32.Update(1U, buf2, len);
			}
		}
		public unsafe static uint Compute(byte* buf, int len)
		{
			return Adler32.Update(1U, buf, len);
		}
		private unsafe static uint Update(uint adler, byte* buf, int len)
		{
			uint num = adler >> 16 & 65535U;
			adler &= 65535U;
			if (1 == len)
			{
				adler += (uint)(*buf);
				if (adler >= 65521U)
				{
					adler -= 65521U;
				}
				num += adler;
				if (num >= 65521U)
				{
					num -= 65521U;
				}
				return adler | num << 16;
			}
			if (len < 16)
			{
				while (len-- != 0)
				{
					adler += (uint)(*(buf++));
					num += adler;
				}
				if (adler >= 65521U)
				{
					adler -= 65521U;
				}
				num %= 65521U;
				return adler | num << 16;
			}
			while (len >= 5552)
			{
				len -= 5552;
				int num2 = 347;
				do
				{
					adler += (uint)(*buf);
					num += adler;
					adler += (uint)buf[1];
					num += adler;
					adler += (uint)buf[2];
					num += adler;
					adler += (uint)buf[3];
					num += adler;
					adler += (uint)buf[4];
					num += adler;
					adler += (uint)buf[5];
					num += adler;
					adler += (uint)buf[6];
					num += adler;
					adler += (uint)buf[7];
					num += adler;
					adler += (uint)buf[8];
					num += adler;
					adler += (uint)buf[9];
					num += adler;
					adler += (uint)buf[10];
					num += adler;
					adler += (uint)buf[11];
					num += adler;
					adler += (uint)buf[12];
					num += adler;
					adler += (uint)buf[13];
					num += adler;
					adler += (uint)buf[14];
					num += adler;
					adler += (uint)buf[15];
					num += adler;
					buf += 16;
				}
				while (--num2 != 0);
				adler %= 65521U;
				num %= 65521U;
			}
			if (len != 0)
			{
				while (len >= 16)
				{
					len -= 16;
					adler += (uint)(*buf);
					num += adler;
					adler += (uint)buf[1];
					num += adler;
					adler += (uint)buf[2];
					num += adler;
					adler += (uint)buf[3];
					num += adler;
					adler += (uint)buf[4];
					num += adler;
					adler += (uint)buf[5];
					num += adler;
					adler += (uint)buf[6];
					num += adler;
					adler += (uint)buf[7];
					num += adler;
					adler += (uint)buf[8];
					num += adler;
					adler += (uint)buf[9];
					num += adler;
					adler += (uint)buf[10];
					num += adler;
					adler += (uint)buf[11];
					num += adler;
					adler += (uint)buf[12];
					num += adler;
					adler += (uint)buf[13];
					num += adler;
					adler += (uint)buf[14];
					num += adler;
					adler += (uint)buf[15];
					num += adler;
					buf += 16;
				}
				while (len-- != 0)
				{
					adler += (uint)(*(buf++));
					num += adler;
				}
				adler %= 65521U;
				num %= 65521U;
			}
			return adler | num << 16;
		}
		public uint Value
		{
			get
			{
				return this.m_adler;
			}
		}
		public unsafe void Update(byte[] buf, int pos, int len)
		{
			if (len == 0)
			{
				return;
			}
			fixed (byte* ptr = &buf[pos])
			{
				byte* buf2 = ptr;
				this.m_adler = Adler32.Update(this.m_adler, buf2, len);
			}
		}
		public unsafe uint Update(byte* buf, int len)
		{
			this.m_adler = Adler32.Update(this.m_adler, buf, len);
			return this.m_adler;
		}
		private const uint BASE = 65521U;
		private const int NMAX = 5552;
		private uint m_adler = 1U;
	}
}
