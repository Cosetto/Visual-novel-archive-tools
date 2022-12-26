using System;

namespace EAGLS
{
	public class CRuntimeRandomGenerator : IRandomGenerator
	{
		public void SRand(int seed)
		{
			this.m_seed = (uint)seed;
		}
		public int Rand()
		{
			this.m_seed = this.m_seed * 214013U + 2531011U;
			return (int)(this.m_seed >> 16 & 32767U);
		}
		public uint m_seed;
	}
}
