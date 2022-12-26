using System;
using System.Collections.Generic;

namespace GPX
{
	internal class Package
	{
		public byte[] buffer;
		
		public List<PackEntry> dir = new List<PackEntry>();
	}
}
