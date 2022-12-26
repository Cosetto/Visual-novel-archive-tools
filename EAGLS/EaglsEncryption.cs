using System;

namespace EAGLS
{
	public class EaglsEncryption
	{
		public EaglsEncryption()
		{
			this.m_rng = new CRuntimeRandomGenerator();
		}
		public byte[] Decrypt(byte[] data)
		{
			int num = 3600;
			int num2 = data.Length - num - 2;
			this.m_rng.SRand((int)((sbyte)data[data.Length - 1]));
			for (int i = 0; i < num2; i += 2)
			{
				int num3 = num + i;
				data[num3] ^= this.Key[this.m_rng.Rand() % this.Key.Length];
			}
			return data;
		}
		private readonly byte[] Key = ArcPak.EaglsKey;
		private readonly CRuntimeRandomGenerator m_rng;
	}
}
