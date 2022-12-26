using System;

namespace VALKYRIA
{
	public class Entry
	{
		public virtual string Name { get; set; }
		public virtual string Type { get; set; }
		public long Offset { get; set; }
		public uint Size { get; set; }
		public bool IsPacked { get; set; }
		public int UnpackedSize { get; set; }
		public long StructPosition { get; set; }
		public byte[] fileBuffer { get; set; }
		public Entry()
		{
			this.Type = "";
			this.Offset = -1L;
		}
	}
}
