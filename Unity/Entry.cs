using System;

namespace Unity
{
	public class Entry
	{
		public virtual string Name { get; set; }
		public virtual string SubName { get; set; }
		public virtual string Type { get; set; }
		public int filename_offset { get; set; }
		public int filename_length { get; set; }
		public int Offset { get; set; }
		public int Size { get; set; }
		public bool IsPacked { get; set; }
		public int UnpackedSize { get; set; }
		public long StructPosition { get; set; }
		public byte[] fileBuffer { get; set; }
		public Entry()
		{
			this.Type = "";
			this.Offset = -1;
		}
	}
}
