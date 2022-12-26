using System;

namespace HuffmanCompressor
{
	internal struct TreeNode
	{
		public ushort parent;
		public ushort isrchild;
		public ushort lchild;
		public ushort rchild;
		public uint weight;
	}
}
