using System;

namespace EAGLS
{
	public interface IRandomGenerator
	{
		void SRand(int seed);
		int Rand();
	}
}
