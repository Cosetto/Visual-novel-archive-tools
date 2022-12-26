using System;
using System.IO;

namespace HuffmanCompressor
{
	internal class HuffmanEncoder
	{
		private static uint parent(uint index)
		{
			return index - 1U >> 1;
		}
		private static void swap(MinHeap[] heap, uint ia, uint ib)
		{
			HeapNode data = heap[(int)ia].data;
			heap[(int)ia].data = heap[(int)ib].data;
			heap[(int)ib].data = data;
		}
		private static void bubbleUp(MinHeap[] heap, uint index)
		{
			while (index > 0U && heap[(int)HuffmanEncoder.parent(index)].data.weight > heap[(int)index].data.weight)
			{
				HuffmanEncoder.swap(heap, HuffmanEncoder.parent(index), index);
				index = HuffmanEncoder.parent(index);
			}
		}
		private static bool heapInsert(MinHeap[] heap, uint elem, uint weight, uint index)
		{
			heap[(int)index].data.elem = elem;
			heap[(int)index].data.weight = weight;
			HuffmanEncoder.bubbleUp(heap, index);
			return true;
		}
		private static uint lchild(uint index)
		{
			return (index << 1) + 1U;
		}
		private static uint rchild(uint index)
		{
			return (index << 1) + 2U;
		}
		private static void bubbleDown(MinHeap[] heap, uint index, uint currentIndex)
		{
			for (;;)
			{
				uint num = HuffmanEncoder.lchild(index);
				uint num2 = HuffmanEncoder.rchild(index);
				uint num3 = (num < currentIndex) ? heap[(int)num].data.weight : uint.MaxValue;
				uint num4 = (num2 < currentIndex) ? heap[(int)num2].data.weight : uint.MaxValue;
				uint num5 = (num3 < num4) ? num : num2;
				uint num6 = (num3 < num4) ? num3 : num4;
				if (heap[(int)index].data.weight <= num6)
				{
					break;
				}
				HuffmanEncoder.swap(heap, index, num5);
				index = num5;
			}
		}
		private static bool heapPopMin(MinHeap[] heap, out uint elem, out uint weight, uint currentIndex)
		{
			elem = heap[0].data.elem;
			weight = heap[0].data.weight;
			HuffmanEncoder.swap(heap, 0U, currentIndex - 1U);
			currentIndex = (currentIndex -= 1U);
			HuffmanEncoder.bubbleDown(heap, 0U, currentIndex);
			return true;
		}
		private static uint BuildTree(TreeNode[] tree, byte[] orginal)
		{
			uint num = 0U;
			while ((ulong)num < (ulong)((long)orginal.Length))
			{
				byte b = orginal[(int)num];
				tree[(int)b].weight = tree[(int)b].weight + 1U;
				num += 1U;
			}
			MinHeap[] heap = new MinHeap[256];
			uint num2 = 0U;
			for (ushort num3 = 0; num3 < 256; num3 += 1)
			{
				if (tree[(int)num3].weight > 0U)
				{
					HuffmanEncoder.heapInsert(heap, (uint)num3, tree[(int)num3].weight, num2);
					num2 += 1U;
				}
			}
			ushort num4 = 256;
			while (num2 > 1U)
			{
				uint num5;
				uint num6;
				HuffmanEncoder.heapPopMin(heap, out num5, out num6, num2);
				num2 -= 1U;
				uint num7;
				uint num8;
				HuffmanEncoder.heapPopMin(heap, out num7, out num8, num2);
				num2 -= 1U;
				tree[(int)num4].lchild = (ushort)num5;
				tree[(int)num4].rchild = (ushort)num7;
				tree[(int)num4].weight = num6 + num8;
				tree[(int)num5].parent = num4;
				tree[(int)num5].isrchild = 0;
				tree[(int)num7].parent = num4;
				tree[(int)num7].isrchild = 1;
				HuffmanEncoder.heapInsert(heap, (uint)num4, num6 + num8, num2);
				num2 += 1U;
				num4 += 1;
			}
			uint result;
			uint num9;
			HuffmanEncoder.heapPopMin(heap, out result, out num9, num2);
			num2 -= 1U;
			return result;
		}
		private static void EncodeBit2(int token, int token_width, BinaryWriter wrter)
		{
			int num = (1 << token_width) - 1;
			if (HuffmanEncoder.cache_bits < 24)
			{
				HuffmanEncoder.mm_bits <<= token_width;
				HuffmanEncoder.mm_bits |= (token & num);
				HuffmanEncoder.cache_bits += token_width;
			}
			if (HuffmanEncoder.cache_bits >= 8)
			{
				while (HuffmanEncoder.cache_bits >= token_width)
				{
					int num2 = HuffmanEncoder.cache_bits - 8;
					byte b = (byte)(HuffmanEncoder.mm_bits >> num2 & 255);
					HuffmanEncoder.cache_bits -= 8;
					int num3 = (1 << HuffmanEncoder.cache_bits) - 1;
					HuffmanEncoder.mm_bits &= num3;
					byte value = b;
					wrter.Write(value);
					wrter.Flush();
				}
			}
		}
		private static void subtreeEncodingWorker(TreeNode[] tree, int rootIndex, BinaryWriter bs)
		{
			if (rootIndex < 256)
			{
				HuffmanEncoder.EncodeBit2(0, 1, bs);
				HuffmanEncoder.EncodeBit2(rootIndex, 8, bs);
				return;
			}
			HuffmanEncoder.EncodeBit2(1, 1, bs);
			HuffmanEncoder.subtreeEncodingWorker(tree, (int)tree[rootIndex].lchild, bs);
			HuffmanEncoder.subtreeEncodingWorker(tree, (int)tree[rootIndex].rchild, bs);
		}
		private static void encodeTree(TreeNode[] tree, uint rootIndex, BinaryWriter bs)
		{
			HuffmanEncoder.subtreeEncodingWorker(tree, (int)rootIndex, bs);
		}
		private static void encodeData(TreeNode[] tree, uint rootIndex, byte[] data, uint oriLen, BinaryWriter bs)
		{
			byte[,] array = new byte[256, 256];
			byte[] array2 = new byte[256];
			for (ushort num = 0; num < 256; num += 1)
			{
				if (tree[(int)num].weight != 0U)
				{
					ushort num2 = num;
					while ((uint)num2 != rootIndex)
					{
						byte[,] array3 = array;
						int num3 = (int)num;
						byte[] array4 = array2;
						ushort num4 = num;
						byte b = array4[(int)num4];
						array4[(int)num4] = (byte)(b + 1);
						array3[num3, (int)b] = (byte)tree[(int)num2].isrchild;
						num2 = tree[(int)num2].parent;
					}
				}
			}
			for (uint num5 = 0U; num5 < oriLen; num5 += 1U)
			{
				for (int i = (int)(array2[(int)data[(int)num5]] - 1); i >= 0; i--)
				{
					HuffmanEncoder.EncodeBit2((int)array[(int)data[(int)num5], i], 1, bs);
				}
			}
		}
		public static byte[] HuffmanEncoding(byte[] orginal)
		{
			HuffmanEncoder.cache_bits = 0;
			HuffmanEncoder.mm_bits = 0;
			TreeNode[] tree = new TreeNode[512];
			uint rootIndex = HuffmanEncoder.BuildTree(tree, orginal);
			byte[] array = new byte[orginal.Length * 4];
			int num = 0;
			using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(array)))
			{
				HuffmanEncoder.encodeTree(tree, rootIndex, binaryWriter);
				HuffmanEncoder.encodeData(tree, rootIndex, orginal, (uint)orginal.Length, binaryWriter);
				for (int i = 0; i < 255; i++)
				{
					HuffmanEncoder.EncodeBit2(255, 8, binaryWriter);
				}
				num = (int)binaryWriter.BaseStream.Position;
			}
			byte[] array2 = new byte[num];
			Buffer.BlockCopy(array, 0, array2, 0, num);
			return array2;
		}
		public static int cache_bits;
		public static int mm_bits;
	}
}
