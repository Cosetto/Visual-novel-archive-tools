using System;
using System.IO;
using System.Security.Cryptography;

namespace Unity
{
	internal class InputCryptoStream : CryptoStream
	{
		public InputCryptoStream(Stream input, ICryptoTransform transform) : base(input, transform, CryptoStreamMode.Read)
		{
			this.m_transform = transform;
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && this.m_transform != null)
			{
				this.m_transform.Dispose();
				this.m_transform = null;
			}
		}
		private ICryptoTransform m_transform;
	}
}
