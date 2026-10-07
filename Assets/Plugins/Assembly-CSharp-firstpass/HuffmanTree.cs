using Unity.IO.Compression;

internal class HuffmanTree
{
	internal const int MaxLiteralTreeElements = 288;

	internal const int MaxDistTreeElements = 32;

	internal const int EndOfBlockCode = 256;

	internal const int NumberOfCodeLengthTreeElements = 19;

	private int tableBits;

	private short[] table;

	private short[] left;

	private short[] right;

	private byte[] codeLengthArray;

	private int tableMask;

	private static HuffmanTree staticLiteralLengthTree;

	private static HuffmanTree staticDistanceTree;

	public static HuffmanTree StaticLiteralLengthTree
	{
		get
		{
			return GetStaticLiteralLengthTree();
		}
	}

	public static HuffmanTree StaticDistanceTree
	{
		get
		{
			return GetStaticDistanceTree();
		}
	}

	static HuffmanTree()
	{
		staticLiteralLengthTree = new HuffmanTree(GetStaticLiteralTreeLength());
		staticDistanceTree = new HuffmanTree(GetStaticDistanceTreeLength());
	}

	public HuffmanTree(byte[] codeLengths)
	{
		codeLengthArray = codeLengths;
		if (codeLengthArray.Length == 288)
		{
			tableBits = 9;
		}
		else
		{
			tableBits = 7;
		}
		tableMask = (1 << tableBits) - 1;
		CreateTable();
	}

	public static HuffmanTree GetStaticLiteralLengthTree()
	{
		return staticLiteralLengthTree;
	}

	public static HuffmanTree GetStaticDistanceTree()
	{
		return staticDistanceTree;
	}

	private static byte[] GetStaticLiteralTreeLength()
	{
		byte[] array = new byte[288];
		for (int i = 0; i <= 143; i++)
		{
			array[i] = 8;
		}
		for (int j = 144; j <= 255; j++)
		{
			array[j] = 9;
		}
		for (int k = 256; k <= 279; k++)
		{
			array[k] = 7;
		}
		for (int l = 280; l <= 287; l++)
		{
			array[l] = 8;
		}
		return array;
	}

	private static byte[] GetStaticDistanceTreeLength()
	{
		byte[] array = new byte[32];
		for (int i = 0; i < 32; i++)
		{
			array[i] = 5;
		}
		return array;
	}

	private uint[] CalculateHuffmanCode()
	{
		uint[] array = new uint[17];
		byte[] lengths = codeLengthArray;
		foreach (int num in lengths)
		{
			array[num]++;
		}
		array[0] = 0u;
		uint[] array2 = new uint[17];
		uint num2 = 0u;
		for (int j = 1; j <= 16; j++)
		{
			num2 = (array2[j] = num2 + array[j - 1] << 1);
		}
		uint[] array3 = new uint[288];
		for (int k = 0; k < codeLengthArray.Length; k++)
		{
			int num3 = codeLengthArray[k];
			if (num3 > 0)
			{
				array3[k] = FastEncoderStatics.BitReverse(array2[num3], num3);
				array2[num3]++;
			}
		}
		return array3;
	}

	private void CreateTable()
	{
		uint[] array = CalculateHuffmanCode();
		table = new short[1 << tableBits];
		left = new short[2 * codeLengthArray.Length];
		right = new short[2 * codeLengthArray.Length];
		short num = (short)codeLengthArray.Length;
		for (int i = 0; i < codeLengthArray.Length; i++)
		{
			int num2 = codeLengthArray[i];
			if (num2 <= 0)
			{
				continue;
			}
			int num3 = (int)array[i];
			if (num2 <= tableBits)
			{
				int num4 = 1 << num2;
				if (num3 >= num4)
				{
					throw new InvalidDataException(SR.GetString("Invalid Huffman data"));
				}
				int num5 = 1 << tableBits - num2;
				for (int j = 0; j < num5; j++)
				{
					table[num3] = (short)i;
					num3 += num4;
				}
				continue;
			}
			int num6 = num2 - tableBits;
			int num7 = 1 << tableBits;
			int num8 = num3 & ((1 << tableBits) - 1);
			short[] array2 = table;
			do
			{
				short num9 = array2[num8];
				if (num9 == 0)
				{
					array2[num8] = (short)(-num);
					num9 = (short)(-num);
					num++;
				}
				if (num9 > 0)
				{
					throw new InvalidDataException(SR.GetString("Invalid Huffman data"));
				}
				array2 = (((num3 & num7) != 0) ? right : left);
				num8 = -num9;
				num7 <<= 1;
				num6--;
			}
			while (num6 != 0);
			array2[num8] = (short)i;
		}
	}

	public int GetNextSymbol(InputBuffer input)
	{
		uint num = input.TryLoad16Bits();
		if (input.GetAvailableBits() == 0)
		{
			return -1;
		}
		int num2 = table[num & tableMask];
		if (num2 < 0)
		{
			uint num3 = (uint)(1 << tableBits);
			do
			{
				num2 = -num2;
				num2 = (((num & num3) != 0) ? right[num2] : left[num2]);
				num3 <<= 1;
			}
			while (num2 < 0);
		}
		int num4 = codeLengthArray[num2];
		if (num4 <= 0)
		{
			throw new InvalidDataException(SR.GetString("Invalid Huffman data"));
		}
		if (num4 > input.GetAvailableBits())
		{
			return -1;
		}
		input.SkipBits(num4);
		return num2;
	}
}
