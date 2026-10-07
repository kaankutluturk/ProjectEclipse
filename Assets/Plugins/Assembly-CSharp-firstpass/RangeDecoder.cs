using System.IO;

internal class RangeDecoder
{
	public const uint kTopValue = 16777216u;

	public uint Range;

	public uint Code;

	public Stream Stream;

	public void Init(Stream ABJIEFMMIEK)
	{
		Stream = ABJIEFMMIEK;
		Code = 0u;
		Range = uint.MaxValue;
		for (int i = 0; i < 5; i++)
		{
			Code = (Code << 8) | (byte)Stream.ReadByte();
		}
	}

	public void ReleaseStream()
	{
		Stream = null;
	}

	public void CloseStream()
	{
		Stream.Close();
	}

	public void Normalize()
	{
		while (Range < 16777216)
		{
			Code = (Code << 8) | (byte)Stream.ReadByte();
			Range <<= 8;
		}
	}

	public void Normalize2()
	{
		if (Range < 16777216)
		{
			Code = (Code << 8) | (byte)Stream.ReadByte();
			Range <<= 8;
		}
	}

	public uint GetThreshold(uint ADLMOFDBBMG)
	{
		return Code / (Range /= ADLMOFDBBMG);
	}

	public void Decode(uint ILENLCMAMBH, uint PEEOEOMEBFG, uint ADLMOFDBBMG)
	{
		Code -= ILENLCMAMBH * Range;
		Range *= PEEOEOMEBFG;
		Normalize();
	}

	public uint DecodeDirectBits(int HEGEFMNECOF)
	{
		uint num = Range;
		uint num2 = Code;
		uint num3 = 0u;
		for (int num4 = HEGEFMNECOF; num4 > 0; num4--)
		{
			num >>= 1;
			uint num5 = num2 - num >> 31;
			num2 -= num & (num5 - 1);
			num3 = (num3 << 1) | (1 - num5);
			if (num < 16777216)
			{
				num2 = (num2 << 8) | (byte)Stream.ReadByte();
				num <<= 8;
			}
		}
		Range = num;
		Code = num2;
		return num3;
	}

	public uint DecodeBit(uint DOONDFDPDFH, int HEGEFMNECOF)
	{
		uint num = (Range >> HEGEFMNECOF) * DOONDFDPDFH;
		uint result;
		if (Code < num)
		{
			result = 0u;
			Range = num;
		}
		else
		{
			result = 1u;
			Code -= num;
			Range -= num;
		}
		Normalize();
		return result;
	}
}
