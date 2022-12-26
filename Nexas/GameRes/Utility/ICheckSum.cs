using System;

namespace GameRes.Utility
{
	public interface ICheckSum
	{
		uint Value { get; }
		void Update(byte[] buf, int pos, int len);
	}
}
