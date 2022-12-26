using System;
using System.Collections.Generic;

namespace EAGLS
{
	internal class Package
	{
		public byte[] buffer;
		public List<PackEntry> dir = new List<PackEntry>();
	}
}
