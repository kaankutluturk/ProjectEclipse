internal struct BitTreeEncoder
{
	private BitEncoder[] Models;

	private int NumBitLevels;

	public BitTreeEncoder(int PLIPKMLGGIP)
	{
		NumBitLevels = PLIPKMLGGIP;
		Models = new BitEncoder[1 << PLIPKMLGGIP];
	}

	public void Init()
	{
		for (uint num = 1u; num < 1 << NumBitLevels; num++)
		{
			Models[num].Init();
		}
	}

	public void Encode(RangeEncoder JHAAEJNODIF, uint symbol)
	{
		uint num = 1u;
		int num2 = NumBitLevels;
		while (num2 > 0)
		{
			num2--;
			uint num3 = (symbol >> num2) & 1;
			Models[num].Encode(JHAAEJNODIF, num3);
			num = (num << 1) | num3;
		}
	}

	public void ReverseEncode(RangeEncoder JHAAEJNODIF, uint symbol)
	{
		uint num = 1u;
		for (uint num2 = 0u; num2 < NumBitLevels; num2++)
		{
			uint num3 = symbol & 1;
			Models[num].Encode(JHAAEJNODIF, num3);
			num = (num << 1) | num3;
			symbol >>= 1;
		}
	}

	public uint GetPrice(uint symbol)
	{
		uint num = 0u;
		uint num2 = 1u;
		int num3 = NumBitLevels;
		while (num3 > 0)
		{
			num3--;
			uint num4 = (symbol >> num3) & 1;
			num += Models[num2].GetPrice(num4);
			num2 = (num2 << 1) + num4;
		}
		return num;
	}

	public uint ReverseGetPrice(uint symbol)
	{
		uint num = 0u;
		uint num2 = 1u;
		for (int num3 = NumBitLevels; num3 > 0; num3--)
		{
			uint num4 = symbol & 1;
			symbol >>= 1;
			num += Models[num2].GetPrice(num4);
			num2 = (num2 << 1) | num4;
		}
		return num;
	}

	public static uint ReverseGetPrice(BitEncoder[] LNDLFINJHDB, uint CAILGDNIKJD, int NumBitLevels, uint symbol)
	{
		uint num = 0u;
		uint num2 = 1u;
		for (int num3 = NumBitLevels; num3 > 0; num3--)
		{
			uint num4 = symbol & 1;
			symbol >>= 1;
			num += LNDLFINJHDB[CAILGDNIKJD + num2].GetPrice(num4);
			num2 = (num2 << 1) | num4;
		}
		return num;
	}

	public static void ReverseEncode(BitEncoder[] LNDLFINJHDB, uint CAILGDNIKJD, RangeEncoder JHAAEJNODIF, int NumBitLevels, uint symbol)
	{
		uint num = 1u;
		for (int i = 0; i < NumBitLevels; i++)
		{
			uint num2 = symbol & 1;
			LNDLFINJHDB[CAILGDNIKJD + num].Encode(JHAAEJNODIF, num2);
			num = (num << 1) | num2;
			symbol >>= 1;
		}
	}
}
