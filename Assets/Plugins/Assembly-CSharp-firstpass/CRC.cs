internal class CRC
{
	public static readonly uint[] Table;

	private uint _value = uint.MaxValue;

	static CRC()
	{
		Table = new uint[256];
		for (uint num = 0u; num < 256; num++)
		{
			uint num2 = num;
			for (int i = 0; i < 8; i++)
			{
				num2 = (((num2 & 1) == 0) ? (num2 >> 1) : ((num2 >> 1) ^ 0xEDB88320u));
			}
			Table[num] = num2;
		}
	}

	public void Init()
	{
		_value = uint.MaxValue;
	}

	public void UpdateByte(byte AAOIAEJJINO)
	{
		_value = Table[(byte)_value ^ AAOIAEJJINO] ^ (_value >> 8);
	}

	public void Update(byte[] data, uint IPCOBJBKNAO, uint PEEOEOMEBFG)
	{
		for (uint num = 0u; num < PEEOEOMEBFG; num++)
		{
			_value = Table[(byte)_value ^ data[IPCOBJBKNAO + num]] ^ (_value >> 8);
		}
	}

	public uint GetDigest()
	{
		return _value ^ 0xFFFFFFFFu;
	}

	private static uint CalculateDigest(byte[] data, uint IPCOBJBKNAO, uint PEEOEOMEBFG)
	{
		CRC nEOAHLMJHKC = new CRC();
		nEOAHLMJHKC.Update(data, IPCOBJBKNAO, PEEOEOMEBFG);
		return nEOAHLMJHKC.GetDigest();
	}

	private static bool VerifyDigest(uint MODPGIJPPMP, byte[] data, uint IPCOBJBKNAO, uint PEEOEOMEBFG)
	{
		return CalculateDigest(data, IPCOBJBKNAO, PEEOEOMEBFG) == MODPGIJPPMP;
	}
}
