using System;
using System.IO;

namespace Escu
{
	public class BitStream : IDisposable
	{
		public Stream Input
		{
			get
			{
				return this.m_input;
			}
		}
		public int CacheSize
		{
			get
			{
				return this.m_cached_bits;
			}
		}
		protected BitStream(Stream file, bool leave_open)
		{
			this.m_input = file;
			this.m_should_dispose = !leave_open;
		}
		public void Reset()
		{
			this.m_cached_bits = 0;
		}
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (!this.m_disposed)
			{
				if (disposing && this.m_should_dispose && this.m_input != null)
				{
					this.m_input.Dispose();
				}
				this.m_disposed = true;
			}
		}
		protected Stream m_input;
		private bool m_should_dispose;
		protected int m_bits;
		protected int m_cached_bits;
		private bool m_disposed;
	}
}
