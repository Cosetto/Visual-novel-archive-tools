using System;

namespace EAGLS
{
	public class CgEncryption
	{
		public CgEncryption(IRandomGenerator rng)
		{
			this.m_rng = rng;
		}
		public byte[] Decrypt(byte[] data)
		{
			this.m_rng.SRand((int)data[data.Length - 1]);
			int num = Math.Min(data.Length - 1, 5963);
			for (int i = 0; i < num; i++)
			{
				int num2 = i;
				data[num2] ^= this.Key[this.m_rng.Rand() % this.Key.Length];
			}
			return data;
		}
		private readonly byte[] Key = ArcPak.EaglsKey;
		private readonly IRandomGenerator m_rng;
	}
}
