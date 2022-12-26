using System;

namespace GPX
{
	public class PackEntry
	{
		public virtual string Name { get; set; }
		public virtual string Type { get; set; }
		public long Offset { get; set; }
		public uint Size { get; set; }
		public bool IsPacked { get; set; }
		public int UnpackedSize { get; set; }
		public long StructPosition { get; set; }
		public int c1 { get; set; }
		public int c2 { get; set; }
		public byte[] fileBuffer { get; set; }
		public PackEntry()
		{
			this.Type = "";
			this.Offset = -1L;
		}
	}
}
