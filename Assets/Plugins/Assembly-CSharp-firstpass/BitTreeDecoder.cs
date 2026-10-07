internal struct BitTreeDecoder
{
	private BitDecoder[] Models;

	private int NumBitLevels;

	public BitTreeDecoder(int PLIPKMLGGIP)
	{
		NumBitLevels = PLIPKMLGGIP;
		Models = new BitDecoder[1 << PLIPKMLGGIP];
	}

	public void Init()
	{
		for (uint num = 1u; num < 1 << NumBitLevels; num++)
		{
			Models[num].Init();
		}
	}

	public uint Decode(RangeDecoder HELKEOGALEA)
	{
		uint num = 1u;
		for (int num2 = NumBitLevels; num2 > 0; num2--)
		{
			num = (num << 1) + Models[num].Decode(HELKEOGALEA);
		}
		return num - (uint)(1 << NumBitLevels);
	}

	public uint ReverseDecode(RangeDecoder HELKEOGALEA)
	{
		uint num = 1u;
		uint num2 = 0u;
		for (int i = 0; i < NumBitLevels; i++)
		{
			uint num3 = Models[num].Decode(HELKEOGALEA);
			num <<= 1;
			num += num3;
			num2 |= num3 << i;
		}
		return num2;
	}

	public static uint ReverseDecode(BitDecoder[] LNDLFINJHDB, uint CAILGDNIKJD, RangeDecoder HELKEOGALEA, int NumBitLevels)
	{
		uint num = 1u;
		uint num2 = 0u;
		for (int i = 0; i < NumBitLevels; i++)
		{
			uint num3 = LNDLFINJHDB[CAILGDNIKJD + num].Decode(HELKEOGALEA);
			num <<= 1;
			num += num3;
			num2 |= num3 << i;
		}
		return num2;
	}
}
