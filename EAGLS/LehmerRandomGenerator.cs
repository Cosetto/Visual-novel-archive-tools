using System;

namespace EAGLS
{
	public class LehmerRandomGenerator : IRandomGenerator
	{
		public void SRand(int seed)
		{
			this.m_seed = (seed ^ 123459876);
		}
		public void setSeed(int seed)
		{
			this.m_seed = seed;
		}
		public int Rand()
		{
			this.m_seed = 48271 * (this.m_seed % 44488) - 3399 * (this.m_seed / 44488);
			if (this.m_seed < 0)
			{
				this.m_seed += int.MaxValue;
			}
			return (int)((double)this.m_seed * 4.656612875245797E-10 * 256.0);
		}
		private int m_seed;
		private const int A = 48271;
		private const int Q = 44488;
		private const int R = 3399;
		private const int M = 2147483647;
	}
}
