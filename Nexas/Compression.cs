using System;

namespace Nexas
{
	public enum Compression
	{
		None,
		Lzss,
		Huffman,
		Deflate,
		DeflateOrNone,
		zstd = 7
	}
}
