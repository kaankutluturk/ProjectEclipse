internal struct BitDecoder
{
	public const int kNumBitModelTotalBits = 11;

	public const uint kBitModelTotal = 2048u;

	private const int kNumMoveBits = 5;

	private uint Prob;

	public void UpdateModel(int EGMENJEPBNH, uint symbol)
	{
		if (symbol == 0)
		{
			Prob += 2048 - Prob >> EGMENJEPBNH;
		}
		else
		{
			Prob -= Prob >> EGMENJEPBNH;
		}
	}

	public void Init()
	{
		Prob = 1024u;
	}

	public uint Decode(RangeDecoder HELKEOGALEA)
	{
		uint num = (HELKEOGALEA.Range >> 11) * Prob;
		if (HELKEOGALEA.Code < num)
		{
			HELKEOGALEA.Range = num;
			Prob += 2048 - Prob >> 5;
			if (HELKEOGALEA.Range < 16777216)
			{
				HELKEOGALEA.Code = (HELKEOGALEA.Code << 8) | (byte)HELKEOGALEA.Stream.ReadByte();
				HELKEOGALEA.Range <<= 8;
			}
			return 0u;
		}
		HELKEOGALEA.Range -= num;
		HELKEOGALEA.Code -= num;
		Prob -= Prob >> 5;
		if (HELKEOGALEA.Range < 16777216)
		{
			HELKEOGALEA.Code = (HELKEOGALEA.Code << 8) | (byte)HELKEOGALEA.Stream.ReadByte();
			HELKEOGALEA.Range <<= 8;
		}
		return 1u;
	}
}
