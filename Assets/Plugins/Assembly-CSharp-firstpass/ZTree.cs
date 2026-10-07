using System;

internal sealed class ZTree
{
	private static readonly int HEAP_SIZE = 2 * InternalConstants.L_CODES + 1;

	internal static readonly int[] ExtraLengthBits = new int[29]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
		1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
		4, 4, 4, 4, 5, 5, 5, 5, 0
	};

	internal static readonly int[] ExtraDistanceBits = new int[30]
	{
		0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
		4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
		9, 9, 10, 10, 11, 11, 12, 12, 13, 13
	};

	internal static readonly int[] ExtraBlbits = new int[19]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 2, 3, 7
	};

	internal static readonly sbyte[] BlOrder = new sbyte[19]
	{
		16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
		11, 4, 12, 3, 13, 2, 14, 1, 15
	};

	internal const int Buf_size = 16;

	private static readonly sbyte[] distCodeTable = new sbyte[512]
	{
		0, 1, 2, 3, 4, 4, 5, 5, 6, 6,
		6, 6, 7, 7, 7, 7, 8, 8, 8, 8,
		8, 8, 8, 8, 9, 9, 9, 9, 9, 9,
		9, 9, 10, 10, 10, 10, 10, 10, 10, 10,
		10, 10, 10, 10, 10, 10, 10, 10, 11, 11,
		11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
		11, 11, 11, 11, 12, 12, 12, 12, 12, 12,
		12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
		12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
		12, 12, 12, 12, 12, 12, 13, 13, 13, 13,
		13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
		13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
		13, 13, 13, 13, 13, 13, 13, 13, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 0, 0, 16, 17,
		18, 18, 19, 19, 20, 20, 20, 20, 21, 21,
		21, 21, 22, 22, 22, 22, 22, 22, 22, 22,
		23, 23, 23, 23, 23, 23, 23, 23, 24, 24,
		24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
		24, 24, 24, 24, 25, 25, 25, 25, 25, 25,
		25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
		26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 27, 27, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29
	};

	internal static readonly sbyte[] LengthCode = new sbyte[256]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 8,
		9, 9, 10, 10, 11, 11, 12, 12, 12, 12,
		13, 13, 13, 13, 14, 14, 14, 14, 15, 15,
		15, 15, 16, 16, 16, 16, 16, 16, 16, 16,
		17, 17, 17, 17, 17, 17, 17, 17, 18, 18,
		18, 18, 18, 18, 18, 18, 19, 19, 19, 19,
		19, 19, 19, 19, 20, 20, 20, 20, 20, 20,
		20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
		21, 21, 21, 21, 21, 21, 21, 21, 21, 21,
		21, 21, 21, 21, 21, 21, 22, 22, 22, 22,
		22, 22, 22, 22, 22, 22, 22, 22, 22, 22,
		22, 22, 23, 23, 23, 23, 23, 23, 23, 23,
		23, 23, 23, 23, 23, 23, 23, 23, 24, 24,
		24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
		24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
		24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
		25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
		25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
		25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
		25, 25, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 26, 26, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 27, 28
	};

	internal static readonly int[] LengthBase = new int[29]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 10,
		12, 14, 16, 20, 24, 28, 32, 40, 48, 56,
		64, 80, 96, 112, 128, 160, 192, 224, 0
	};

	internal static readonly int[] DistanceBase = new int[30]
	{
		0, 1, 2, 3, 4, 6, 8, 12, 16, 24,
		32, 48, 64, 96, 128, 192, 256, 384, 512, 768,
		1024, 1536, 2048, 3072, 4096, 6144, 8192, 12288, 16384, 24576
	};

	internal short[] dyn_tree;

	internal int max_code;

	internal StaticTree staticTree;

	internal static int DistanceCode(int distance)
	{
		return (distance >= 256) ? distCodeTable[256 + SharedUtils.URShift(distance, 7)] : distCodeTable[distance];
	}

	internal void GenerateBitLengths(DeflateManager deflateManager)
	{
		short[] tree = dyn_tree;
		short[] staticCodes = staticTree.treeCodes;
		int[] extraBitsTable = staticTree.extraBits;
		int extraBase = staticTree.extraBase;
		int maxLength = staticTree.maxLength;
		int num = 0;
		for (int i = 0; i <= InternalConstants.MAX_BITS; i++)
		{
			deflateManager.bl_count[i] = 0;
		}
		tree[deflateManager.heap[deflateManager.heap_max] * 2 + 1] = 0;
		int j;
		for (j = deflateManager.heap_max + 1; j < HEAP_SIZE; j++)
		{
			int num2 = deflateManager.heap[j];
			int i = tree[tree[num2 * 2 + 1] * 2 + 1] + 1;
			if (i > maxLength)
			{
				i = maxLength;
				num++;
			}
			tree[num2 * 2 + 1] = (short)i;
			if (num2 <= max_code)
			{
				deflateManager.bl_count[i]++;
				int num3 = 0;
				if (num2 >= extraBase)
				{
					num3 = extraBitsTable[num2 - extraBase];
				}
				short num4 = tree[num2 * 2];
				deflateManager.opt_len += num4 * (i + num3);
				if (staticCodes != null)
				{
					deflateManager.static_len += num4 * (staticCodes[num2 * 2 + 1] + num3);
				}
			}
		}
		if (num == 0)
		{
			return;
		}
		do
		{
			int i = maxLength - 1;
			while (deflateManager.bl_count[i] == 0)
			{
				i--;
			}
			deflateManager.bl_count[i]--;
			deflateManager.bl_count[i + 1] = (short)(deflateManager.bl_count[i + 1] + 2);
			deflateManager.bl_count[maxLength]--;
			num -= 2;
		}
		while (num > 0);
		for (int i = maxLength; i != 0; i--)
		{
			int num2 = deflateManager.bl_count[i];
			while (num2 != 0)
			{
				int num5 = deflateManager.heap[--j];
				if (num5 <= max_code)
				{
					if (tree[num5 * 2 + 1] != i)
					{
						deflateManager.opt_len = (int)(deflateManager.opt_len + ((long)i - (long)tree[num5 * 2 + 1]) * tree[num5 * 2]);
						tree[num5 * 2 + 1] = (short)i;
					}
					num2--;
				}
			}
		}
	}

	internal void BuildTree(DeflateManager deflateManager)
	{
		short[] tree = dyn_tree;
		short[] staticCodes = staticTree.treeCodes;
		int elementCount = staticTree.elems;
		int num = -1;
		deflateManager.heap_len = 0;
		deflateManager.heap_max = HEAP_SIZE;
		for (int i = 0; i < elementCount; i++)
		{
			if (tree[i * 2] != 0)
			{
				num = (deflateManager.heap[++deflateManager.heap_len] = i);
				deflateManager.depth[i] = 0;
			}
			else
			{
				tree[i * 2 + 1] = 0;
			}
		}
		int num2;
		while (deflateManager.heap_len < 2)
		{
			num2 = (deflateManager.heap[++deflateManager.heap_len] = ((num < 2) ? (++num) : 0));
			tree[num2 * 2] = 1;
			deflateManager.depth[num2] = 0;
			deflateManager.opt_len--;
			if (staticCodes != null)
			{
				deflateManager.static_len -= staticCodes[num2 * 2 + 1];
			}
		}
		max_code = num;
		for (int i = deflateManager.heap_len / 2; i >= 1; i--)
		{
			deflateManager.pqdownheap(tree, i);
		}
		num2 = elementCount;
		do
		{
			int i = deflateManager.heap[1];
			deflateManager.heap[1] = deflateManager.heap[deflateManager.heap_len--];
			deflateManager.pqdownheap(tree, 1);
			int num3 = deflateManager.heap[1];
			deflateManager.heap[--deflateManager.heap_max] = i;
			deflateManager.heap[--deflateManager.heap_max] = num3;
			tree[num2 * 2] = (short)(tree[i * 2] + tree[num3 * 2]);
			deflateManager.depth[num2] = (sbyte)(Math.Max((byte)deflateManager.depth[i], (byte)deflateManager.depth[num3]) + 1);
			tree[i * 2 + 1] = (tree[num3 * 2 + 1] = (short)num2);
			deflateManager.heap[1] = num2++;
			deflateManager.pqdownheap(tree, 1);
		}
		while (deflateManager.heap_len >= 2);
		deflateManager.heap[--deflateManager.heap_max] = deflateManager.heap[1];
		GenerateBitLengths(deflateManager);
		GenerateCodes(tree, num, deflateManager.bl_count);
	}

	internal static void GenerateCodes(short[] tree, int max_code, short[] blCount)
	{
		short[] array = new short[InternalConstants.MAX_BITS + 1];
		short num = 0;
		for (int i = 1; i <= InternalConstants.MAX_BITS; i++)
		{
			num = (array[i] = (short)(num + blCount[i - 1] << 1));
		}
		for (int j = 0; j <= max_code; j++)
		{
			int num2 = tree[j * 2 + 1];
			if (num2 != 0)
			{
				tree[j * 2] = (short)BitReverse(array[num2]++, num2);
			}
		}
	}

	internal static int BitReverse(int code, int length)
	{
		int num = 0;
		do
		{
			num |= code & 1;
			code >>= 1;
			num <<= 1;
		}
		while (--length > 0);
		return num >> 1;
	}
}
