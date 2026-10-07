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

	internal static int DistanceCode(int CGIBMHPALCO)
	{
		return (CGIBMHPALCO >= 256) ? distCodeTable[256 + SharedUtils.URShift(CGIBMHPALCO, 7)] : distCodeTable[CGIBMHPALCO];
	}

	internal void GenerateBitLengths(DeflateManager JDCCBCNFENK)
	{
		short[] kAMMGDHOHKA = dyn_tree;
		short[] cNKMPIHKGLL = staticTree.treeCodes;
		int[] lILPPGCPPGO = staticTree.extraBits;
		int gFDLABEMBHB = staticTree.extraBase;
		int aFKIJFBEHCN = staticTree.maxLength;
		int num = 0;
		for (int i = 0; i <= InternalConstants.MAX_BITS; i++)
		{
			JDCCBCNFENK.bl_count[i] = 0;
		}
		kAMMGDHOHKA[JDCCBCNFENK.heap[JDCCBCNFENK.heap_max] * 2 + 1] = 0;
		int j;
		for (j = JDCCBCNFENK.heap_max + 1; j < HEAP_SIZE; j++)
		{
			int num2 = JDCCBCNFENK.heap[j];
			int i = kAMMGDHOHKA[kAMMGDHOHKA[num2 * 2 + 1] * 2 + 1] + 1;
			if (i > aFKIJFBEHCN)
			{
				i = aFKIJFBEHCN;
				num++;
			}
			kAMMGDHOHKA[num2 * 2 + 1] = (short)i;
			if (num2 <= max_code)
			{
				JDCCBCNFENK.bl_count[i]++;
				int num3 = 0;
				if (num2 >= gFDLABEMBHB)
				{
					num3 = lILPPGCPPGO[num2 - gFDLABEMBHB];
				}
				short num4 = kAMMGDHOHKA[num2 * 2];
				JDCCBCNFENK.opt_len += num4 * (i + num3);
				if (cNKMPIHKGLL != null)
				{
					JDCCBCNFENK.static_len += num4 * (cNKMPIHKGLL[num2 * 2 + 1] + num3);
				}
			}
		}
		if (num == 0)
		{
			return;
		}
		do
		{
			int i = aFKIJFBEHCN - 1;
			while (JDCCBCNFENK.bl_count[i] == 0)
			{
				i--;
			}
			JDCCBCNFENK.bl_count[i]--;
			JDCCBCNFENK.bl_count[i + 1] = (short)(JDCCBCNFENK.bl_count[i + 1] + 2);
			JDCCBCNFENK.bl_count[aFKIJFBEHCN]--;
			num -= 2;
		}
		while (num > 0);
		for (int i = aFKIJFBEHCN; i != 0; i--)
		{
			int num2 = JDCCBCNFENK.bl_count[i];
			while (num2 != 0)
			{
				int num5 = JDCCBCNFENK.heap[--j];
				if (num5 <= max_code)
				{
					if (kAMMGDHOHKA[num5 * 2 + 1] != i)
					{
						JDCCBCNFENK.opt_len = (int)(JDCCBCNFENK.opt_len + ((long)i - (long)kAMMGDHOHKA[num5 * 2 + 1]) * kAMMGDHOHKA[num5 * 2]);
						kAMMGDHOHKA[num5 * 2 + 1] = (short)i;
					}
					num2--;
				}
			}
		}
	}

	internal void BuildTree(DeflateManager JDCCBCNFENK)
	{
		short[] kAMMGDHOHKA = dyn_tree;
		short[] cNKMPIHKGLL = staticTree.treeCodes;
		int pNNMNEEJEGD = staticTree.elems;
		int num = -1;
		JDCCBCNFENK.heap_len = 0;
		JDCCBCNFENK.heap_max = HEAP_SIZE;
		for (int i = 0; i < pNNMNEEJEGD; i++)
		{
			if (kAMMGDHOHKA[i * 2] != 0)
			{
				num = (JDCCBCNFENK.heap[++JDCCBCNFENK.heap_len] = i);
				JDCCBCNFENK.depth[i] = 0;
			}
			else
			{
				kAMMGDHOHKA[i * 2 + 1] = 0;
			}
		}
		int num2;
		while (JDCCBCNFENK.heap_len < 2)
		{
			num2 = (JDCCBCNFENK.heap[++JDCCBCNFENK.heap_len] = ((num < 2) ? (++num) : 0));
			kAMMGDHOHKA[num2 * 2] = 1;
			JDCCBCNFENK.depth[num2] = 0;
			JDCCBCNFENK.opt_len--;
			if (cNKMPIHKGLL != null)
			{
				JDCCBCNFENK.static_len -= cNKMPIHKGLL[num2 * 2 + 1];
			}
		}
		max_code = num;
		for (int i = JDCCBCNFENK.heap_len / 2; i >= 1; i--)
		{
			JDCCBCNFENK.pqdownheap(kAMMGDHOHKA, i);
		}
		num2 = pNNMNEEJEGD;
		do
		{
			int i = JDCCBCNFENK.heap[1];
			JDCCBCNFENK.heap[1] = JDCCBCNFENK.heap[JDCCBCNFENK.heap_len--];
			JDCCBCNFENK.pqdownheap(kAMMGDHOHKA, 1);
			int num3 = JDCCBCNFENK.heap[1];
			JDCCBCNFENK.heap[--JDCCBCNFENK.heap_max] = i;
			JDCCBCNFENK.heap[--JDCCBCNFENK.heap_max] = num3;
			kAMMGDHOHKA[num2 * 2] = (short)(kAMMGDHOHKA[i * 2] + kAMMGDHOHKA[num3 * 2]);
			JDCCBCNFENK.depth[num2] = (sbyte)(Math.Max((byte)JDCCBCNFENK.depth[i], (byte)JDCCBCNFENK.depth[num3]) + 1);
			kAMMGDHOHKA[i * 2 + 1] = (kAMMGDHOHKA[num3 * 2 + 1] = (short)num2);
			JDCCBCNFENK.heap[1] = num2++;
			JDCCBCNFENK.pqdownheap(kAMMGDHOHKA, 1);
		}
		while (JDCCBCNFENK.heap_len >= 2);
		JDCCBCNFENK.heap[--JDCCBCNFENK.heap_max] = JDCCBCNFENK.heap[1];
		GenerateBitLengths(JDCCBCNFENK);
		GenerateCodes(kAMMGDHOHKA, num, JDCCBCNFENK.bl_count);
	}

	internal static void GenerateCodes(short[] EDBPBGAMMDO, int max_code, short[] OOJOJFEKPEL)
	{
		short[] array = new short[InternalConstants.MAX_BITS + 1];
		short num = 0;
		for (int i = 1; i <= InternalConstants.MAX_BITS; i++)
		{
			num = (array[i] = (short)(num + OOJOJFEKPEL[i - 1] << 1));
		}
		for (int j = 0; j <= max_code; j++)
		{
			int num2 = EDBPBGAMMDO[j * 2 + 1];
			if (num2 != 0)
			{
				EDBPBGAMMDO[j * 2] = (short)BitReverse(array[num2]++, num2);
			}
		}
	}

	internal static int BitReverse(int KJPGKHJNOMC, int JCAJDBOMGOM)
	{
		int num = 0;
		do
		{
			num |= KJPGKHJNOMC & 1;
			KJPGKHJNOMC >>= 1;
			num <<= 1;
		}
		while (--JCAJDBOMGOM > 0);
		return num >> 1;
	}
}
