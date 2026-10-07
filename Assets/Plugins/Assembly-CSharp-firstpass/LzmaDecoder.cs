using System;
using System.IO;

public class LzmaDecoder : ICoder, ISetDecoderProperties
{
	private class LenDecoder
	{
		private BitDecoder m_Choice = default(BitDecoder);

		private BitDecoder m_Choice2 = default(BitDecoder);

		private BitTreeDecoder[] m_LowCoder = new BitTreeDecoder[16];

		private BitTreeDecoder[] m_MidCoder = new BitTreeDecoder[16];

		private BitTreeDecoder m_HighCoder = new BitTreeDecoder(8);

		private uint m_NumPosStates;

		public void Create(uint BHMGNFOKODN)
		{
			for (uint num = m_NumPosStates; num < BHMGNFOKODN; num++)
			{
				m_LowCoder[num] = new BitTreeDecoder(3);
				m_MidCoder[num] = new BitTreeDecoder(3);
			}
			m_NumPosStates = BHMGNFOKODN;
		}

		public void Init()
		{
			m_Choice.Init();
			for (uint num = 0u; num < m_NumPosStates; num++)
			{
				m_LowCoder[num].Init();
				m_MidCoder[num].Init();
			}
			m_Choice2.Init();
			m_HighCoder.Init();
		}

		public uint Decode(RangeDecoder HELKEOGALEA, uint LFOAILOHHHD)
		{
			if (m_Choice.Decode(HELKEOGALEA) == 0)
			{
				return m_LowCoder[LFOAILOHHHD].Decode(HELKEOGALEA);
			}
			uint num = 8u;
			if (m_Choice2.Decode(HELKEOGALEA) == 0)
			{
				return num + m_MidCoder[LFOAILOHHHD].Decode(HELKEOGALEA);
			}
			num += 8;
			return num + m_HighCoder.Decode(HELKEOGALEA);
		}
	}

	private class LiteralDecoder
	{
		private struct Decoder2
		{
			private BitDecoder[] m_Decoders;

			public void Create()
			{
				m_Decoders = new BitDecoder[768];
			}

			public void Init()
			{
				for (int i = 0; i < 768; i++)
				{
					m_Decoders[i].Init();
				}
			}

			public byte DecodeNormal(RangeDecoder HELKEOGALEA)
			{
				uint num = 1u;
				do
				{
					num = (num << 1) | m_Decoders[num].Decode(HELKEOGALEA);
				}
				while (num < 256);
				return (byte)num;
			}

			public byte DecodeWithMatchByte(RangeDecoder HELKEOGALEA, byte HGMKIONDDNO)
			{
				uint num = 1u;
				do
				{
					uint num2 = (uint)((HGMKIONDDNO >> 7) & 1);
					HGMKIONDDNO <<= 1;
					uint num3 = m_Decoders[(1 + num2 << 8) + num].Decode(HELKEOGALEA);
					num = (num << 1) | num3;
					if (num2 != num3)
					{
						while (num < 256)
						{
							num = (num << 1) | m_Decoders[num].Decode(HELKEOGALEA);
						}
						break;
					}
				}
				while (num < 256);
				return (byte)num;
			}
		}

		private Decoder2[] m_Coders;

		private int m_NumPrevBits;

		private int m_NumPosBits;

		private uint m_PosMask;

		public void Create(int PGIOGOCKAPN, int NNNENAADHAE)
		{
			if (m_Coders == null || m_NumPrevBits != NNNENAADHAE || m_NumPosBits != PGIOGOCKAPN)
			{
				m_NumPosBits = PGIOGOCKAPN;
				m_PosMask = (uint)((1 << PGIOGOCKAPN) - 1);
				m_NumPrevBits = NNNENAADHAE;
				uint num = (uint)(1 << m_NumPrevBits + m_NumPosBits);
				m_Coders = new Decoder2[num];
				for (uint num2 = 0u; num2 < num; num2++)
				{
					m_Coders[num2].Create();
				}
			}
		}

		public void Init()
		{
			uint num = (uint)(1 << m_NumPrevBits + m_NumPosBits);
			for (uint num2 = 0u; num2 < num; num2++)
			{
				m_Coders[num2].Init();
			}
		}

		private uint GetState(uint LCCLEFMKLPB, byte PMEIMKDGNJP)
		{
			return ((LCCLEFMKLPB & m_PosMask) << m_NumPrevBits) + (uint)(PMEIMKDGNJP >> 8 - m_NumPrevBits);
		}

		public byte DecodeNormal(RangeDecoder HELKEOGALEA, uint LCCLEFMKLPB, byte PMEIMKDGNJP)
		{
			return m_Coders[GetState(LCCLEFMKLPB, PMEIMKDGNJP)].DecodeNormal(HELKEOGALEA);
		}

		public byte DecodeWithMatchByte(RangeDecoder HELKEOGALEA, uint LCCLEFMKLPB, byte PMEIMKDGNJP, byte HGMKIONDDNO)
		{
			return m_Coders[GetState(LCCLEFMKLPB, PMEIMKDGNJP)].DecodeWithMatchByte(HELKEOGALEA, HGMKIONDDNO);
		}
	}

	private OutWindow m_OutWindow = new OutWindow();

	private RangeDecoder m_RangeDecoder = new RangeDecoder();

	private BitDecoder[] m_IsMatchDecoders = new BitDecoder[192];

	private BitDecoder[] m_IsRepDecoders = new BitDecoder[12];

	private BitDecoder[] m_IsRepG0Decoders = new BitDecoder[12];

	private BitDecoder[] m_IsRepG1Decoders = new BitDecoder[12];

	private BitDecoder[] m_IsRepG2Decoders = new BitDecoder[12];

	private BitDecoder[] m_IsRep0LongDecoders = new BitDecoder[192];

	private BitTreeDecoder[] m_PosSlotDecoder = new BitTreeDecoder[4];

	private BitDecoder[] m_PosDecoders = new BitDecoder[114];

	private BitTreeDecoder m_PosAlignDecoder = new BitTreeDecoder(4);

	private LenDecoder m_LenDecoder = new LenDecoder();

	private LenDecoder m_RepLenDecoder = new LenDecoder();

	private LiteralDecoder m_LiteralDecoder = new LiteralDecoder();

	private uint m_DictionarySize;

	private uint m_DictionarySizeCheck;

	private uint m_PosStateMask;

	private bool _solid;

	public LzmaDecoder()
	{
		m_DictionarySize = uint.MaxValue;
		for (int i = 0; (long)i < 4L; i++)
		{
			m_PosSlotDecoder[i] = new BitTreeDecoder(6);
		}
	}

	private void SetDictionarySize(uint MMNLJFOCACJ)
	{
		if (m_DictionarySize != MMNLJFOCACJ)
		{
			m_DictionarySize = MMNLJFOCACJ;
			m_DictionarySizeCheck = Math.Max(m_DictionarySize, 1u);
			uint aKOEOKJFINO = Math.Max(m_DictionarySizeCheck, 4096u);
			m_OutWindow.Create(aKOEOKJFINO);
		}
	}

	private void SetLiteralProperties(int MHNGHOGBEAE, int LMHBHHENKHG)
	{
		if (MHNGHOGBEAE > 8)
		{
			throw new InvalidParamException();
		}
		if (LMHBHHENKHG > 8)
		{
			throw new InvalidParamException();
		}
		m_LiteralDecoder.Create(MHNGHOGBEAE, LMHBHHENKHG);
	}

	private void SetPosBitsProperties(int LMJOLGGBKNL)
	{
		if (LMJOLGGBKNL > 4)
		{
			throw new InvalidParamException();
		}
		uint num = (uint)(1 << LMJOLGGBKNL);
		m_LenDecoder.Create(num);
		m_RepLenDecoder.Create(num);
		m_PosStateMask = num - 1;
	}

	private void Init(Stream BHHJJHBNEKD, Stream BBBGGJLOCPB)
	{
		m_RangeDecoder.Init(BHHJJHBNEKD);
		m_OutWindow.Init(BBBGGJLOCPB, _solid);
		for (uint num = 0u; num < 12; num++)
		{
			for (uint num2 = 0u; num2 <= m_PosStateMask; num2++)
			{
				uint num3 = (num << 4) + num2;
				m_IsMatchDecoders[num3].Init();
				m_IsRep0LongDecoders[num3].Init();
			}
			m_IsRepDecoders[num].Init();
			m_IsRepG0Decoders[num].Init();
			m_IsRepG1Decoders[num].Init();
			m_IsRepG2Decoders[num].Init();
		}
		m_LiteralDecoder.Init();
		for (uint num = 0u; num < 4; num++)
		{
			m_PosSlotDecoder[num].Init();
		}
		for (uint num = 0u; num < 114; num++)
		{
			m_PosDecoders[num].Init();
		}
		m_LenDecoder.Init();
		m_RepLenDecoder.Init();
		m_PosAlignDecoder.Init();
	}

	public void Code(Stream BHHJJHBNEKD, Stream BBBGGJLOCPB, long NCKELGLBGJN, long JNILCBKONPG, ICodeProgress progress)
	{
		Init(BHHJJHBNEKD, BBBGGJLOCPB);
		Base.CoderState iPAFOKKOCPF = default(Base.CoderState);
		iPAFOKKOCPF.Init();
		uint num = 0u;
		uint num2 = 0u;
		uint num3 = 0u;
		uint num4 = 0u;
		ulong num5 = 0uL;
		if (num5 < (ulong)JNILCBKONPG)
		{
			if (m_IsMatchDecoders[iPAFOKKOCPF.Index << 4].Decode(m_RangeDecoder) != 0)
			{
				throw new DataErrorException();
			}
			iPAFOKKOCPF.UpdateChar();
			byte aAOIAEJJINO = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, 0u, 0);
			m_OutWindow.PutByte(aAOIAEJJINO);
			num5++;
		}
		while (num5 < (ulong)JNILCBKONPG)
		{
			uint num6 = (uint)(int)num5 & m_PosStateMask;
			if (m_IsMatchDecoders[(iPAFOKKOCPF.Index << 4) + num6].Decode(m_RangeDecoder) == 0)
			{
				byte pMEIMKDGNJP = m_OutWindow.GetByte(0u);
				byte aAOIAEJJINO2 = (iPAFOKKOCPF.IsCharState() ? m_LiteralDecoder.DecodeNormal(m_RangeDecoder, (uint)num5, pMEIMKDGNJP) : m_LiteralDecoder.DecodeWithMatchByte(m_RangeDecoder, (uint)num5, pMEIMKDGNJP, m_OutWindow.GetByte(num)));
				m_OutWindow.PutByte(aAOIAEJJINO2);
				iPAFOKKOCPF.UpdateChar();
				num5++;
				continue;
			}
			uint num8;
			if (m_IsRepDecoders[iPAFOKKOCPF.Index].Decode(m_RangeDecoder) == 1)
			{
				if (m_IsRepG0Decoders[iPAFOKKOCPF.Index].Decode(m_RangeDecoder) == 0)
				{
					if (m_IsRep0LongDecoders[(iPAFOKKOCPF.Index << 4) + num6].Decode(m_RangeDecoder) == 0)
					{
						iPAFOKKOCPF.UpdateShortRep();
						m_OutWindow.PutByte(m_OutWindow.GetByte(num));
						num5++;
						continue;
					}
				}
				else
				{
					uint num7;
					if (m_IsRepG1Decoders[iPAFOKKOCPF.Index].Decode(m_RangeDecoder) == 0)
					{
						num7 = num2;
					}
					else
					{
						if (m_IsRepG2Decoders[iPAFOKKOCPF.Index].Decode(m_RangeDecoder) == 0)
						{
							num7 = num3;
						}
						else
						{
							num7 = num4;
							num4 = num3;
						}
						num3 = num2;
					}
					num2 = num;
					num = num7;
				}
				num8 = m_RepLenDecoder.Decode(m_RangeDecoder, num6) + 2;
				iPAFOKKOCPF.UpdateRep();
			}
			else
			{
				num4 = num3;
				num3 = num2;
				num2 = num;
				num8 = 2 + m_LenDecoder.Decode(m_RangeDecoder, num6);
				iPAFOKKOCPF.UpdateMatch();
				uint num9 = m_PosSlotDecoder[Base.GetLenToPosState(num8)].Decode(m_RangeDecoder);
				if (num9 >= 4)
				{
					int num10 = (int)((num9 >> 1) - 1);
					num = (2 | (num9 & 1)) << num10;
					if (num9 < 14)
					{
						num += BitTreeDecoder.ReverseDecode(m_PosDecoders, num - num9 - 1, m_RangeDecoder, num10);
					}
					else
					{
						num += m_RangeDecoder.DecodeDirectBits(num10 - 4) << 4;
						num += m_PosAlignDecoder.ReverseDecode(m_RangeDecoder);
					}
				}
				else
				{
					num = num9;
				}
			}
			if (num >= m_OutWindow.TrainSize + num5 || num >= m_DictionarySizeCheck)
			{
				if (num == uint.MaxValue)
				{
					break;
				}
				throw new DataErrorException();
			}
			m_OutWindow.CopyBlock(num, num8);
			num5 += num8;
		}
		m_OutWindow.Flush();
		m_OutWindow.ReleaseStream();
		m_RangeDecoder.ReleaseStream();
	}

	public void SetDecoderProperties(byte[] properties)
	{
		if (properties.Length < 5)
		{
			throw new InvalidParamException();
		}
		int lMHBHHENKHG = properties[0] % 9;
		int num = properties[0] / 9;
		int mHNGHOGBEAE = num % 5;
		int num2 = num / 5;
		if (num2 > 4)
		{
			throw new InvalidParamException();
		}
		uint num3 = 0u;
		for (int i = 0; i < 4; i++)
		{
			num3 += (uint)(properties[1 + i] << i * 8);
		}
		SetDictionarySize(num3);
		SetLiteralProperties(mHNGHOGBEAE, lMHBHHENKHG);
		SetPosBitsProperties(num2);
	}

	public bool Train(Stream ABJIEFMMIEK)
	{
		_solid = true;
		return m_OutWindow.Train(ABJIEFMMIEK);
	}
}
