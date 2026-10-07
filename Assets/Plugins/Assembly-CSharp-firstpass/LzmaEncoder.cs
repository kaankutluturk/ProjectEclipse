using System;
using System.IO;

public class LzmaEncoder : ICoder, ISetCoderProperties, IWriteCoderProperties
{
	private enum MatchFinderType
	{
		BT2 = 0,
		BT4 = 1
	}

	private class LiteralEncoder
	{
		public struct LiteralSubEncoder
		{
			private BitEncoder[] encoders;

			public void Create()
			{
				encoders = new BitEncoder[768];
			}

			public void Init()
			{
				for (int i = 0; i < 768; i++)
				{
					encoders[i].Init();
				}
			}

			public void Encode(RangeEncoder JHAAEJNODIF, byte symbol)
			{
				uint num = 1u;
				for (int num2 = 7; num2 >= 0; num2--)
				{
					uint num3 = (uint)((symbol >> num2) & 1);
					encoders[num].Encode(JHAAEJNODIF, num3);
					num = (num << 1) | num3;
				}
			}

			public void EncodeMatched(RangeEncoder JHAAEJNODIF, byte HGMKIONDDNO, byte symbol)
			{
				uint num = 1u;
				bool flag = true;
				for (int num2 = 7; num2 >= 0; num2--)
				{
					uint num3 = (uint)((symbol >> num2) & 1);
					uint num4 = num;
					if (flag)
					{
						uint num5 = (uint)((HGMKIONDDNO >> num2) & 1);
						num4 += 1 + num5 << 8;
						flag = num5 == num3;
					}
					encoders[num4].Encode(JHAAEJNODIF, num3);
					num = (num << 1) | num3;
				}
			}

			public uint GetPrice(bool PGBFBFEDLBH, byte HGMKIONDDNO, byte symbol)
			{
				uint num = 0u;
				uint num2 = 1u;
				int num3 = 7;
				if (PGBFBFEDLBH)
				{
					while (num3 >= 0)
					{
						uint num4 = (uint)((HGMKIONDDNO >> num3) & 1);
						uint num5 = (uint)((symbol >> num3) & 1);
						num += encoders[(1 + num4 << 8) + num2].GetPrice(num5);
						num2 = (num2 << 1) | num5;
						if (num4 != num5)
						{
							num3--;
							break;
						}
						num3--;
					}
				}
				while (num3 >= 0)
				{
					uint num6 = (uint)((symbol >> num3) & 1);
					num += encoders[num2].GetPrice(num6);
					num2 = (num2 << 1) | num6;
					num3--;
				}
				return num;
			}
		}

		private LiteralSubEncoder[] coders;

		private int numPrevBits;

		private int numPosBits;

		private uint m_PosMask;

		public void Create(int PGIOGOCKAPN, int NNNENAADHAE)
		{
			if (coders == null || numPrevBits != NNNENAADHAE || numPosBits != PGIOGOCKAPN)
			{
				numPosBits = PGIOGOCKAPN;
				m_PosMask = (uint)((1 << PGIOGOCKAPN) - 1);
				numPrevBits = NNNENAADHAE;
				uint num = (uint)(1 << numPrevBits + numPosBits);
				coders = new LiteralSubEncoder[num];
				for (uint num2 = 0u; num2 < num; num2++)
				{
					coders[num2].Create();
				}
			}
		}

		public void Init()
		{
			uint num = (uint)(1 << numPrevBits + numPosBits);
			for (uint num2 = 0u; num2 < num; num2++)
			{
				coders[num2].Init();
			}
		}

		public LiteralSubEncoder GetSubCoder(uint LCCLEFMKLPB, byte PMEIMKDGNJP)
		{
			return coders[(int)((LCCLEFMKLPB & m_PosMask) << numPrevBits) + (PMEIMKDGNJP >> 8 - numPrevBits)];
		}
	}

	private class LenEncoder
	{
		private BitEncoder choice = default(BitEncoder);

		private BitEncoder choice2 = default(BitEncoder);

		private BitTreeEncoder[] lowCoder = new BitTreeEncoder[16];

		private BitTreeEncoder[] midCoder = new BitTreeEncoder[16];

		private BitTreeEncoder highCoder = new BitTreeEncoder(8);

		public LenEncoder()
		{
			for (uint num = 0u; num < 16; num++)
			{
				lowCoder[num] = new BitTreeEncoder(3);
				midCoder[num] = new BitTreeEncoder(3);
			}
		}

		public void Init(uint BHMGNFOKODN)
		{
			choice.Init();
			choice2.Init();
			for (uint num = 0u; num < BHMGNFOKODN; num++)
			{
				lowCoder[num].Init();
				midCoder[num].Init();
			}
			highCoder.Init();
		}

		public void Encode(RangeEncoder JHAAEJNODIF, uint symbol, uint LFOAILOHHHD)
		{
			if (symbol < 8)
			{
				choice.Encode(JHAAEJNODIF, 0u);
				lowCoder[LFOAILOHHHD].Encode(JHAAEJNODIF, symbol);
				return;
			}
			symbol -= 8;
			choice.Encode(JHAAEJNODIF, 1u);
			if (symbol < 8)
			{
				choice2.Encode(JHAAEJNODIF, 0u);
				midCoder[LFOAILOHHHD].Encode(JHAAEJNODIF, symbol);
			}
			else
			{
				choice2.Encode(JHAAEJNODIF, 1u);
				highCoder.Encode(JHAAEJNODIF, symbol - 8);
			}
		}

		public void SetPrices(uint LFOAILOHHHD, uint DCFHENPGLID, uint[] LDDIFHOEMEI, uint DCOKHNMLPGJ)
		{
			uint num = choice.GetPrice0();
			uint num2 = choice.GetPrice1();
			uint num3 = num2 + choice2.GetPrice0();
			uint num4 = num2 + choice2.GetPrice1();
			uint num5 = 0u;
			for (num5 = 0u; num5 < 8; num5++)
			{
				if (num5 >= DCFHENPGLID)
				{
					return;
				}
				LDDIFHOEMEI[DCOKHNMLPGJ + num5] = num + lowCoder[LFOAILOHHHD].GetPrice(num5);
			}
			for (; num5 < 16; num5++)
			{
				if (num5 >= DCFHENPGLID)
				{
					return;
				}
				LDDIFHOEMEI[DCOKHNMLPGJ + num5] = num3 + midCoder[LFOAILOHHHD].GetPrice(num5 - 8);
			}
			for (; num5 < DCFHENPGLID; num5++)
			{
				LDDIFHOEMEI[DCOKHNMLPGJ + num5] = num4 + highCoder.GetPrice(num5 - 8 - 8);
			}
		}
	}

	private class LenPriceTableEncoder : LenEncoder
	{
		private uint[] prices = new uint[4352];

		private uint _tableSize;

		private uint[] counters = new uint[16];

		public void SetTableSize(uint ELDKFILGOIH)
		{
			_tableSize = ELDKFILGOIH;
		}

		public uint GetPrice(uint symbol, uint LFOAILOHHHD)
		{
			return prices[LFOAILOHHHD * 272 + symbol];
		}

		private void UpdateTable(uint LFOAILOHHHD)
		{
			SetPrices(LFOAILOHHHD, _tableSize, prices, LFOAILOHHHD * 272);
			counters[LFOAILOHHHD] = _tableSize;
		}

		public void UpdateTables(uint BHMGNFOKODN)
		{
			for (uint num = 0u; num < BHMGNFOKODN; num++)
			{
				UpdateTable(num);
			}
		}

		public new void Encode(RangeEncoder JHAAEJNODIF, uint symbol, uint LFOAILOHHHD)
		{
			base.Encode(JHAAEJNODIF, symbol, LFOAILOHHHD);
			if (--counters[LFOAILOHHHD] == 0)
			{
				UpdateTable(LFOAILOHHHD);
			}
		}
	}

	private class Optimal
	{
		public Base.CoderState CoderState;

		public bool Prev1IsChar;

		public bool Prev2;

		public uint PosPrev2;

		public uint BackPrev2;

		public uint Price;

		public uint PosPrev;

		public uint BackPrev;

		public uint Backs0;

		public uint Backs1;

		public uint Backs2;

		public uint Backs3;

		public void MakeAsChar()
		{
			BackPrev = uint.MaxValue;
			Prev1IsChar = false;
		}

		public void MakeAsShortRep()
		{
			BackPrev = 0u;
			Prev1IsChar = false;
		}

		public bool IsShortRep()
		{
			return BackPrev == 0;
		}
	}

	private const uint InfinityPrice = 268435455u;

	private static byte[] fastPos;

	private Base.CoderState state = default(Base.CoderState);

	private byte _previousByte;

	private uint[] repDistances = new uint[4];

	private const int kDefaultDictionaryLogSize = 22;

	private const uint NumFastBytesDefault = 32u;

	private const uint NumLenSpecSymbols = 16u;

	private const uint NumOpts = 4096u;

	private Optimal[] optimum = new Optimal[4096];

	private IMatchFinder matchFinder;

	private RangeEncoder rangeEncoder = new RangeEncoder();

	private BitEncoder[] isMatch = new BitEncoder[192];

	private BitEncoder[] isRep = new BitEncoder[12];

	private BitEncoder[] isRepG0 = new BitEncoder[12];

	private BitEncoder[] isRepG1 = new BitEncoder[12];

	private BitEncoder[] isRepG2 = new BitEncoder[12];

	private BitEncoder[] isRep0Long = new BitEncoder[192];

	private BitTreeEncoder[] posSlotEncoder = new BitTreeEncoder[4];

	private BitEncoder[] posEncoders = new BitEncoder[114];

	private BitTreeEncoder posAlignEncoder = new BitTreeEncoder(4);

	private LenPriceTableEncoder lenEncoder = new LenPriceTableEncoder();

	private LenPriceTableEncoder repMatchLenEncoder = new LenPriceTableEncoder();

	private LiteralEncoder literalEncoder = new LiteralEncoder();

	private uint[] matchDistances = new uint[548];

	private uint numFastBytes = 32u;

	private uint longestMatchLength;

	private uint numDistancePairs;

	private uint additionalOffset;

	private uint optimumEndIndex;

	private uint optimumCurrentIndex;

	private bool longestMatchWasFound;

	private uint[] posSlotPrices = new uint[256];

	private uint[] distancesPrices = new uint[512];

	private uint[] alignPrices = new uint[16];

	private uint alignPriceCount;

	private uint distTableSize = 44u;

	private int posStateBits = 2;

	private uint posStateMask = 3u;

	private int numLiteralPosStateBits;

	private int numLiteralContextBits = 3;

	private uint dictionarySize = 4194304u;

	private uint dictionarySizePrev = uint.MaxValue;

	private uint numFastBytesPrev = uint.MaxValue;

	private long nowPos64;

	private bool finished;

	private Stream _inStream;

	private MatchFinderType matchFinderType = MatchFinderType.BT4;

	private bool writeEndMark;

	private bool needReleaseMFStream;

	private uint[] reps = new uint[4];

	private uint[] repLens = new uint[4];

	private const int kPropSize = 5;

	private byte[] properties = new byte[5];

	private uint[] tempPrices = new uint[128];

	private uint matchPriceCount;

	private static string[] kMatchFinderIDs;

	private uint trainSize;

	static LzmaEncoder()
	{
		fastPos = new byte[2048];
		kMatchFinderIDs = new string[2] { "BT2", "BT4" };
		int num = 2;
		fastPos[0] = 0;
		fastPos[1] = 1;
		for (byte b = 2; b < 22; b++)
		{
			uint num2 = (uint)(1 << (b >> 1) - 1);
			uint num3 = 0u;
			while (num3 < num2)
			{
				fastPos[num] = b;
				num3++;
				num++;
			}
		}
	}

	public LzmaEncoder()
	{
		for (int i = 0; (long)i < 4096L; i++)
		{
			optimum[i] = new Optimal();
		}
		for (int j = 0; (long)j < 4L; j++)
		{
			posSlotEncoder[j] = new BitTreeEncoder(6);
		}
	}

	private static uint GetPosSlot(uint LCCLEFMKLPB)
	{
		if (LCCLEFMKLPB < 2048)
		{
			return fastPos[LCCLEFMKLPB];
		}
		if (LCCLEFMKLPB < 2097152)
		{
			return (uint)(fastPos[LCCLEFMKLPB >> 10] + 20);
		}
		return (uint)(fastPos[LCCLEFMKLPB >> 20] + 40);
	}

	private static uint GetPosSlot2(uint LCCLEFMKLPB)
	{
		if (LCCLEFMKLPB < 131072)
		{
			return (uint)(fastPos[LCCLEFMKLPB >> 6] + 12);
		}
		if (LCCLEFMKLPB < 134217728)
		{
			return (uint)(fastPos[LCCLEFMKLPB >> 16] + 32);
		}
		return (uint)(fastPos[LCCLEFMKLPB >> 26] + 52);
	}

	private void BaseInit()
	{
		state.Init();
		_previousByte = 0;
		for (uint num = 0u; num < 4; num++)
		{
			repDistances[num] = 0u;
		}
	}

	private void Create()
	{
		if (matchFinder == null)
		{
			BinTree mEEBALKDNBG = new BinTree();
			int eOKCENIBPJD = 4;
			if (matchFinderType == MatchFinderType.BT2)
			{
				eOKCENIBPJD = 2;
			}
			mEEBALKDNBG.SetType(eOKCENIBPJD);
			matchFinder = mEEBALKDNBG;
		}
		literalEncoder.Create(numLiteralPosStateBits, numLiteralContextBits);
		if (dictionarySize != dictionarySizePrev || numFastBytesPrev != numFastBytes)
		{
			matchFinder.Create(dictionarySize, 4096u, numFastBytes, 274u);
			dictionarySizePrev = dictionarySize;
			numFastBytesPrev = numFastBytes;
		}
	}

	private void SetWriteEndMarkerMode(bool KJDHMDADICC)
	{
		writeEndMark = KJDHMDADICC;
	}

	private void Init()
	{
		BaseInit();
		rangeEncoder.Init();
		for (uint num = 0u; num < 12; num++)
		{
			for (uint num2 = 0u; num2 <= posStateMask; num2++)
			{
				uint num3 = (num << 4) + num2;
				isMatch[num3].Init();
				isRep0Long[num3].Init();
			}
			isRep[num].Init();
			isRepG0[num].Init();
			isRepG1[num].Init();
			isRepG2[num].Init();
		}
		literalEncoder.Init();
		for (uint num = 0u; num < 4; num++)
		{
			posSlotEncoder[num].Init();
		}
		for (uint num = 0u; num < 114; num++)
		{
			posEncoders[num].Init();
		}
		lenEncoder.Init((uint)(1 << posStateBits));
		repMatchLenEncoder.Init((uint)(1 << posStateBits));
		posAlignEncoder.Init();
		longestMatchWasFound = false;
		optimumEndIndex = 0u;
		optimumCurrentIndex = 0u;
		additionalOffset = 0u;
	}

	private void ReadMatchDistances(out uint CEAHDKFDIOK, out uint IBGEENFNMHL)
	{
		CEAHDKFDIOK = 0u;
		IBGEENFNMHL = matchFinder.GetMatches(matchDistances);
		if (IBGEENFNMHL != 0)
		{
			CEAHDKFDIOK = matchDistances[IBGEENFNMHL - 2];
			if (CEAHDKFDIOK == numFastBytes)
			{
				CEAHDKFDIOK += matchFinder.GetMatchLen((int)(CEAHDKFDIOK - 1), matchDistances[IBGEENFNMHL - 1], 273 - CEAHDKFDIOK);
			}
		}
		additionalOffset++;
	}

	private void MovePos(uint OMEDGJMNGKE)
	{
		if (OMEDGJMNGKE != 0)
		{
			matchFinder.Skip(OMEDGJMNGKE);
			additionalOffset += OMEDGJMNGKE;
		}
	}

	private uint GetRepLen1Price(Base.CoderState state, uint LFOAILOHHHD)
	{
		return isRepG0[state.Index].GetPrice0() + isRep0Long[(state.Index << 4) + LFOAILOHHHD].GetPrice0();
	}

	private uint GetPureRepPrice(uint CBEPCAHEEMI, Base.CoderState state, uint LFOAILOHHHD)
	{
		uint num;
		if (CBEPCAHEEMI == 0)
		{
			num = isRepG0[state.Index].GetPrice0();
			return num + isRep0Long[(state.Index << 4) + LFOAILOHHHD].GetPrice1();
		}
		num = isRepG0[state.Index].GetPrice1();
		if (CBEPCAHEEMI == 1)
		{
			return num + isRepG1[state.Index].GetPrice0();
		}
		num += isRepG1[state.Index].GetPrice1();
		return num + isRepG2[state.Index].GetPrice(CBEPCAHEEMI - 2);
	}

	private uint GetRepPrice(uint CBEPCAHEEMI, uint JCAJDBOMGOM, Base.CoderState state, uint LFOAILOHHHD)
	{
		uint num = repMatchLenEncoder.GetPrice(JCAJDBOMGOM - 2, LFOAILOHHHD);
		return num + GetPureRepPrice(CBEPCAHEEMI, state, LFOAILOHHHD);
	}

	private uint GetPosLenPrice(uint LCCLEFMKLPB, uint JCAJDBOMGOM, uint LFOAILOHHHD)
	{
		uint num = Base.GetLenToPosState(JCAJDBOMGOM);
		uint num2 = ((LCCLEFMKLPB >= 128) ? (posSlotPrices[(num << 6) + GetPosSlot2(LCCLEFMKLPB)] + alignPrices[LCCLEFMKLPB & 0xF]) : distancesPrices[num * 128 + LCCLEFMKLPB]);
		return num2 + lenEncoder.GetPrice(JCAJDBOMGOM - 2, LFOAILOHHHD);
	}

	private uint Backward(out uint PHJEECBMCFO, uint MGPKJFBKOOO)
	{
		optimumEndIndex = MGPKJFBKOOO;
		uint dFLODJBKHDN = optimum[MGPKJFBKOOO].PosPrev;
		uint eKNEPEFHCIL = optimum[MGPKJFBKOOO].BackPrev;
		do
		{
			if (optimum[MGPKJFBKOOO].Prev1IsChar)
			{
				optimum[dFLODJBKHDN].MakeAsChar();
				optimum[dFLODJBKHDN].PosPrev = dFLODJBKHDN - 1;
				if (optimum[MGPKJFBKOOO].Prev2)
				{
					optimum[dFLODJBKHDN - 1].Prev1IsChar = false;
					optimum[dFLODJBKHDN - 1].PosPrev = optimum[MGPKJFBKOOO].PosPrev2;
					optimum[dFLODJBKHDN - 1].BackPrev = optimum[MGPKJFBKOOO].BackPrev2;
				}
			}
			uint num = dFLODJBKHDN;
			uint eKNEPEFHCIL2 = eKNEPEFHCIL;
			eKNEPEFHCIL = optimum[num].BackPrev;
			dFLODJBKHDN = optimum[num].PosPrev;
			optimum[num].BackPrev = eKNEPEFHCIL2;
			optimum[num].PosPrev = MGPKJFBKOOO;
			MGPKJFBKOOO = num;
		}
		while (MGPKJFBKOOO != 0);
		PHJEECBMCFO = optimum[0].BackPrev;
		optimumCurrentIndex = optimum[0].PosPrev;
		return optimumCurrentIndex;
	}

	private uint GetOptimum(uint MGMMDGFPBLP, out uint PHJEECBMCFO)
	{
		if (optimumEndIndex != optimumCurrentIndex)
		{
			uint result = optimum[optimumCurrentIndex].PosPrev - optimumCurrentIndex;
			PHJEECBMCFO = optimum[optimumCurrentIndex].BackPrev;
			optimumCurrentIndex = optimum[optimumCurrentIndex].PosPrev;
			return result;
		}
		optimumCurrentIndex = (optimumEndIndex = 0u);
		uint CEAHDKFDIOK;
		uint IBGEENFNMHL;
		if (!longestMatchWasFound)
		{
			ReadMatchDistances(out CEAHDKFDIOK, out IBGEENFNMHL);
		}
		else
		{
			CEAHDKFDIOK = longestMatchLength;
			IBGEENFNMHL = numDistancePairs;
			longestMatchWasFound = false;
		}
		uint num = matchFinder.GetNumAvailableBytes() + 1;
		if (num < 2)
		{
			PHJEECBMCFO = uint.MaxValue;
			return 1u;
		}
		if (num > 273)
		{
			num = 273u;
		}
		uint num2 = 0u;
		for (uint num3 = 0u; num3 < 4; num3++)
		{
			reps[num3] = repDistances[num3];
			repLens[num3] = matchFinder.GetMatchLen(-1, reps[num3], 273u);
			if (repLens[num3] > repLens[num2])
			{
				num2 = num3;
			}
		}
		if (repLens[num2] >= numFastBytes)
		{
			PHJEECBMCFO = num2;
			uint num4 = repLens[num2];
			MovePos(num4 - 1);
			return num4;
		}
		if (CEAHDKFDIOK >= numFastBytes)
		{
			PHJEECBMCFO = matchDistances[IBGEENFNMHL - 1] + 4;
			MovePos(CEAHDKFDIOK - 1);
			return CEAHDKFDIOK;
		}
		byte b = matchFinder.GetIndexByte(-1);
		byte b2 = matchFinder.GetIndexByte((int)(0 - repDistances[0] - 1 - 1));
		if (CEAHDKFDIOK < 2 && b != b2 && repLens[num2] < 2)
		{
			PHJEECBMCFO = uint.MaxValue;
			return 1u;
		}
		optimum[0].CoderState = state;
		uint num5 = MGMMDGFPBLP & posStateMask;
		optimum[1].Price = isMatch[(state.Index << 4) + num5].GetPrice0() + literalEncoder.GetSubCoder(MGMMDGFPBLP, _previousByte).GetPrice(!state.IsCharState(), b2, b);
		optimum[1].MakeAsChar();
		uint num6 = isMatch[(state.Index << 4) + num5].GetPrice1();
		uint num7 = num6 + isRep[state.Index].GetPrice1();
		if (b2 == b)
		{
			uint num8 = num7 + GetRepLen1Price(state, num5);
			if (num8 < optimum[1].Price)
			{
				optimum[1].Price = num8;
				optimum[1].MakeAsShortRep();
			}
		}
		uint num9 = ((CEAHDKFDIOK < repLens[num2]) ? repLens[num2] : CEAHDKFDIOK);
		if (num9 < 2)
		{
			PHJEECBMCFO = optimum[1].BackPrev;
			return 1u;
		}
		optimum[1].PosPrev = 0u;
		optimum[0].Backs0 = reps[0];
		optimum[0].Backs1 = reps[1];
		optimum[0].Backs2 = reps[2];
		optimum[0].Backs3 = reps[3];
		uint num10 = num9;
		do
		{
			optimum[num10--].Price = 268435455u;
		}
		while (num10 >= 2);
		for (uint num3 = 0u; num3 < 4; num3++)
		{
			uint num11 = repLens[num3];
			if (num11 < 2)
			{
				continue;
			}
			uint num12 = num7 + GetPureRepPrice(num3, state, num5);
			do
			{
				uint num13 = num12 + repMatchLenEncoder.GetPrice(num11 - 2, num5);
				Optimal jDOABBOPFIP = optimum[num11];
				if (num13 < jDOABBOPFIP.Price)
				{
					jDOABBOPFIP.Price = num13;
					jDOABBOPFIP.PosPrev = 0u;
					jDOABBOPFIP.BackPrev = num3;
					jDOABBOPFIP.Prev1IsChar = false;
				}
			}
			while (--num11 >= 2);
		}
		uint num14 = num6 + isRep[state.Index].GetPrice0();
		num10 = ((repLens[0] < 2) ? 2u : (repLens[0] + 1));
		if (num10 <= CEAHDKFDIOK)
		{
			uint num15;
			for (num15 = 0u; num10 > matchDistances[num15]; num15 += 2)
			{
			}
			while (true)
			{
				uint num16 = matchDistances[num15 + 1];
				uint num17 = num14 + GetPosLenPrice(num16, num10, num5);
				Optimal jDOABBOPFIP2 = optimum[num10];
				if (num17 < jDOABBOPFIP2.Price)
				{
					jDOABBOPFIP2.Price = num17;
					jDOABBOPFIP2.PosPrev = 0u;
					jDOABBOPFIP2.BackPrev = num16 + 4;
					jDOABBOPFIP2.Prev1IsChar = false;
				}
				if (num10 == matchDistances[num15])
				{
					num15 += 2;
					if (num15 == IBGEENFNMHL)
					{
						break;
					}
				}
				num10++;
			}
		}
		uint num18 = 0u;
		uint CEAHDKFDIOK2;
		while (true)
		{
			num18++;
			if (num18 == num9)
			{
				return Backward(out PHJEECBMCFO, num18);
			}
			ReadMatchDistances(out CEAHDKFDIOK2, out IBGEENFNMHL);
			if (CEAHDKFDIOK2 >= numFastBytes)
			{
				break;
			}
			MGMMDGFPBLP++;
			uint num19 = optimum[num18].PosPrev;
			Base.CoderState aFINHOBCHMC;
			if (optimum[num18].Prev1IsChar)
			{
				num19--;
				if (optimum[num18].Prev2)
				{
					aFINHOBCHMC = optimum[optimum[num18].PosPrev2].CoderState;
					if (optimum[num18].BackPrev2 < 4)
					{
						aFINHOBCHMC.UpdateRep();
					}
					else
					{
						aFINHOBCHMC.UpdateMatch();
					}
				}
				else
				{
					aFINHOBCHMC = optimum[num19].CoderState;
				}
				aFINHOBCHMC.UpdateChar();
			}
			else
			{
				aFINHOBCHMC = optimum[num19].CoderState;
			}
			if (num19 == num18 - 1)
			{
				if (optimum[num18].IsShortRep())
				{
					aFINHOBCHMC.UpdateShortRep();
				}
				else
				{
					aFINHOBCHMC.UpdateChar();
				}
			}
			else
			{
				uint num20;
				if (optimum[num18].Prev1IsChar && optimum[num18].Prev2)
				{
					num19 = optimum[num18].PosPrev2;
					num20 = optimum[num18].BackPrev2;
					aFINHOBCHMC.UpdateRep();
				}
				else
				{
					num20 = optimum[num18].BackPrev;
					if (num20 < 4)
					{
						aFINHOBCHMC.UpdateRep();
					}
					else
					{
						aFINHOBCHMC.UpdateMatch();
					}
				}
				Optimal jDOABBOPFIP3 = optimum[num19];
				switch (num20)
				{
				case 0u:
					reps[0] = jDOABBOPFIP3.Backs0;
					reps[1] = jDOABBOPFIP3.Backs1;
					reps[2] = jDOABBOPFIP3.Backs2;
					reps[3] = jDOABBOPFIP3.Backs3;
					break;
				case 1u:
					reps[0] = jDOABBOPFIP3.Backs1;
					reps[1] = jDOABBOPFIP3.Backs0;
					reps[2] = jDOABBOPFIP3.Backs2;
					reps[3] = jDOABBOPFIP3.Backs3;
					break;
				case 2u:
					reps[0] = jDOABBOPFIP3.Backs2;
					reps[1] = jDOABBOPFIP3.Backs0;
					reps[2] = jDOABBOPFIP3.Backs1;
					reps[3] = jDOABBOPFIP3.Backs3;
					break;
				case 3u:
					reps[0] = jDOABBOPFIP3.Backs3;
					reps[1] = jDOABBOPFIP3.Backs0;
					reps[2] = jDOABBOPFIP3.Backs1;
					reps[3] = jDOABBOPFIP3.Backs2;
					break;
				default:
					reps[0] = num20 - 4;
					reps[1] = jDOABBOPFIP3.Backs0;
					reps[2] = jDOABBOPFIP3.Backs1;
					reps[3] = jDOABBOPFIP3.Backs2;
					break;
				}
			}
			optimum[num18].CoderState = aFINHOBCHMC;
			optimum[num18].Backs0 = reps[0];
			optimum[num18].Backs1 = reps[1];
			optimum[num18].Backs2 = reps[2];
			optimum[num18].Backs3 = reps[3];
			uint mDAAJFBENON = optimum[num18].Price;
			b = matchFinder.GetIndexByte(-1);
			b2 = matchFinder.GetIndexByte((int)(0 - reps[0] - 1 - 1));
			num5 = MGMMDGFPBLP & posStateMask;
			uint num21 = mDAAJFBENON + isMatch[(aFINHOBCHMC.Index << 4) + num5].GetPrice0() + literalEncoder.GetSubCoder(MGMMDGFPBLP, matchFinder.GetIndexByte(-2)).GetPrice(!aFINHOBCHMC.IsCharState(), b2, b);
			Optimal jDOABBOPFIP4 = optimum[num18 + 1];
			bool flag = false;
			if (num21 < jDOABBOPFIP4.Price)
			{
				jDOABBOPFIP4.Price = num21;
				jDOABBOPFIP4.PosPrev = num18;
				jDOABBOPFIP4.MakeAsChar();
				flag = true;
			}
			num6 = mDAAJFBENON + isMatch[(aFINHOBCHMC.Index << 4) + num5].GetPrice1();
			num7 = num6 + isRep[aFINHOBCHMC.Index].GetPrice1();
			if (b2 == b && (jDOABBOPFIP4.PosPrev >= num18 || jDOABBOPFIP4.BackPrev != 0))
			{
				uint num22 = num7 + GetRepLen1Price(aFINHOBCHMC, num5);
				if (num22 <= jDOABBOPFIP4.Price)
				{
					jDOABBOPFIP4.Price = num22;
					jDOABBOPFIP4.PosPrev = num18;
					jDOABBOPFIP4.MakeAsShortRep();
					flag = true;
				}
			}
			uint val = matchFinder.GetNumAvailableBytes() + 1;
			val = Math.Min(4095 - num18, val);
			num = val;
			if (num < 2)
			{
				continue;
			}
			if (num > numFastBytes)
			{
				num = numFastBytes;
			}
			if (!flag && b2 != b)
			{
				uint lOHCIKNKDEI = Math.Min(val - 1, numFastBytes);
				uint num23 = matchFinder.GetMatchLen(0, reps[0], lOHCIKNKDEI);
				if (num23 >= 2)
				{
					Base.CoderState pIFKPLHIOFJ = aFINHOBCHMC;
					pIFKPLHIOFJ.UpdateChar();
					uint num24 = (MGMMDGFPBLP + 1) & posStateMask;
					uint num25 = num21 + isMatch[(pIFKPLHIOFJ.Index << 4) + num24].GetPrice1() + isRep[pIFKPLHIOFJ.Index].GetPrice1();
					uint num26 = num18 + 1 + num23;
					while (num9 < num26)
					{
						optimum[++num9].Price = 268435455u;
					}
					uint num27 = num25 + GetRepPrice(0u, num23, pIFKPLHIOFJ, num24);
					Optimal jDOABBOPFIP5 = optimum[num26];
					if (num27 < jDOABBOPFIP5.Price)
					{
						jDOABBOPFIP5.Price = num27;
						jDOABBOPFIP5.PosPrev = num18 + 1;
						jDOABBOPFIP5.BackPrev = 0u;
						jDOABBOPFIP5.Prev1IsChar = true;
						jDOABBOPFIP5.Prev2 = false;
					}
				}
			}
			uint num28 = 2u;
			for (uint num29 = 0u; num29 < 4; num29++)
			{
				uint num30 = matchFinder.GetMatchLen(-1, reps[num29], num);
				if (num30 < 2)
				{
					continue;
				}
				uint num31 = num30;
				while (true)
				{
					if (num9 < num18 + num30)
					{
						optimum[++num9].Price = 268435455u;
						continue;
					}
					uint num32 = num7 + GetRepPrice(num29, num30, aFINHOBCHMC, num5);
					Optimal jDOABBOPFIP6 = optimum[num18 + num30];
					if (num32 < jDOABBOPFIP6.Price)
					{
						jDOABBOPFIP6.Price = num32;
						jDOABBOPFIP6.PosPrev = num18;
						jDOABBOPFIP6.BackPrev = num29;
						jDOABBOPFIP6.Prev1IsChar = false;
					}
					if (--num30 < 2)
					{
						break;
					}
				}
				num30 = num31;
				if (num29 == 0)
				{
					num28 = num30 + 1;
				}
				if (num30 >= val)
				{
					continue;
				}
				uint lOHCIKNKDEI2 = Math.Min(val - 1 - num30, numFastBytes);
				uint num33 = matchFinder.GetMatchLen((int)num30, reps[num29], lOHCIKNKDEI2);
				if (num33 >= 2)
				{
					Base.CoderState pIFKPLHIOFJ2 = aFINHOBCHMC;
					pIFKPLHIOFJ2.UpdateRep();
					uint num34 = (MGMMDGFPBLP + num30) & posStateMask;
					uint num35 = num7 + GetRepPrice(num29, num30, aFINHOBCHMC, num5) + isMatch[(pIFKPLHIOFJ2.Index << 4) + num34].GetPrice0() + literalEncoder.GetSubCoder(MGMMDGFPBLP + num30, matchFinder.GetIndexByte((int)(num30 - 1 - 1))).GetPrice(true, matchFinder.GetIndexByte((int)(num30 - 1 - (reps[num29] + 1))), matchFinder.GetIndexByte((int)(num30 - 1)));
					pIFKPLHIOFJ2.UpdateChar();
					num34 = (MGMMDGFPBLP + num30 + 1) & posStateMask;
					uint num36 = num35 + isMatch[(pIFKPLHIOFJ2.Index << 4) + num34].GetPrice1();
					uint num37 = num36 + isRep[pIFKPLHIOFJ2.Index].GetPrice1();
					uint num38 = num30 + 1 + num33;
					while (num9 < num18 + num38)
					{
						optimum[++num9].Price = 268435455u;
					}
					uint num39 = num37 + GetRepPrice(0u, num33, pIFKPLHIOFJ2, num34);
					Optimal jDOABBOPFIP7 = optimum[num18 + num38];
					if (num39 < jDOABBOPFIP7.Price)
					{
						jDOABBOPFIP7.Price = num39;
						jDOABBOPFIP7.PosPrev = num18 + num30 + 1;
						jDOABBOPFIP7.BackPrev = 0u;
						jDOABBOPFIP7.Prev1IsChar = true;
						jDOABBOPFIP7.Prev2 = true;
						jDOABBOPFIP7.PosPrev2 = num18;
						jDOABBOPFIP7.BackPrev2 = num29;
					}
				}
			}
			if (CEAHDKFDIOK2 > num)
			{
				CEAHDKFDIOK2 = num;
				for (IBGEENFNMHL = 0u; CEAHDKFDIOK2 > matchDistances[IBGEENFNMHL]; IBGEENFNMHL += 2)
				{
				}
				matchDistances[IBGEENFNMHL] = CEAHDKFDIOK2;
				IBGEENFNMHL += 2;
			}
			if (CEAHDKFDIOK2 < num28)
			{
				continue;
			}
			num14 = num6 + isRep[aFINHOBCHMC.Index].GetPrice0();
			while (num9 < num18 + CEAHDKFDIOK2)
			{
				optimum[++num9].Price = 268435455u;
			}
			uint num40;
			for (num40 = 0u; num28 > matchDistances[num40]; num40 += 2)
			{
			}
			uint num41 = num28;
			while (true)
			{
				uint num42 = matchDistances[num40 + 1];
				uint num43 = num14 + GetPosLenPrice(num42, num41, num5);
				Optimal jDOABBOPFIP8 = optimum[num18 + num41];
				if (num43 < jDOABBOPFIP8.Price)
				{
					jDOABBOPFIP8.Price = num43;
					jDOABBOPFIP8.PosPrev = num18;
					jDOABBOPFIP8.BackPrev = num42 + 4;
					jDOABBOPFIP8.Prev1IsChar = false;
				}
				if (num41 == matchDistances[num40])
				{
					if (num41 < val)
					{
						uint lOHCIKNKDEI3 = Math.Min(val - 1 - num41, numFastBytes);
						uint num44 = matchFinder.GetMatchLen((int)num41, num42, lOHCIKNKDEI3);
						if (num44 >= 2)
						{
							Base.CoderState pIFKPLHIOFJ3 = aFINHOBCHMC;
							pIFKPLHIOFJ3.UpdateMatch();
							uint num45 = (MGMMDGFPBLP + num41) & posStateMask;
							uint num46 = num43 + isMatch[(pIFKPLHIOFJ3.Index << 4) + num45].GetPrice0() + literalEncoder.GetSubCoder(MGMMDGFPBLP + num41, matchFinder.GetIndexByte((int)(num41 - 1 - 1))).GetPrice(true, matchFinder.GetIndexByte((int)(num41 - (num42 + 1) - 1)), matchFinder.GetIndexByte((int)(num41 - 1)));
							pIFKPLHIOFJ3.UpdateChar();
							num45 = (MGMMDGFPBLP + num41 + 1) & posStateMask;
							uint num47 = num46 + isMatch[(pIFKPLHIOFJ3.Index << 4) + num45].GetPrice1();
							uint num48 = num47 + isRep[pIFKPLHIOFJ3.Index].GetPrice1();
							uint num49 = num41 + 1 + num44;
							while (num9 < num18 + num49)
							{
								optimum[++num9].Price = 268435455u;
							}
							num43 = num48 + GetRepPrice(0u, num44, pIFKPLHIOFJ3, num45);
							jDOABBOPFIP8 = optimum[num18 + num49];
							if (num43 < jDOABBOPFIP8.Price)
							{
								jDOABBOPFIP8.Price = num43;
								jDOABBOPFIP8.PosPrev = num18 + num41 + 1;
								jDOABBOPFIP8.BackPrev = 0u;
								jDOABBOPFIP8.Prev1IsChar = true;
								jDOABBOPFIP8.Prev2 = true;
								jDOABBOPFIP8.PosPrev2 = num18;
								jDOABBOPFIP8.BackPrev2 = num42 + 4;
							}
						}
					}
					num40 += 2;
					if (num40 == IBGEENFNMHL)
					{
						break;
					}
				}
				num41++;
			}
		}
		numDistancePairs = IBGEENFNMHL;
		longestMatchLength = CEAHDKFDIOK2;
		longestMatchWasFound = true;
		return Backward(out PHJEECBMCFO, num18);
	}

	private bool ChangePair(uint NBNEODKIPFO, uint FKEJHBLHOBL)
	{
		return NBNEODKIPFO < 33554432 && FKEJHBLHOBL >= NBNEODKIPFO << 7;
	}

	private void WriteEndMarker(uint LFOAILOHHHD)
	{
		if (writeEndMark)
		{
			isMatch[(state.Index << 4) + LFOAILOHHHD].Encode(rangeEncoder, 1u);
			isRep[state.Index].Encode(rangeEncoder, 0u);
			state.UpdateMatch();
			uint num = 2u;
			lenEncoder.Encode(rangeEncoder, num - 2, LFOAILOHHHD);
			uint iIFFPBLOKKC = 63u;
			uint num2 = Base.GetLenToPosState(num);
			posSlotEncoder[num2].Encode(rangeEncoder, iIFFPBLOKKC);
			int num3 = 30;
			uint num4 = (uint)((1 << num3) - 1);
			rangeEncoder.EncodeDirectBits(num4 >> 4, num3 - 4);
			posAlignEncoder.ReverseEncode(rangeEncoder, num4 & 0xF);
		}
	}

	private void Flush(uint KGBFLBENJJG)
	{
		ReleaseMFStream();
		WriteEndMarker(KGBFLBENJJG & posStateMask);
		rangeEncoder.FlushData();
		rangeEncoder.FlushStream();
	}

	public void CodeOneBlock(out long NCKELGLBGJN, out long JNILCBKONPG, out bool IAAOKDKLNGH)
	{
		NCKELGLBGJN = 0L;
		JNILCBKONPG = 0L;
		IAAOKDKLNGH = true;
		if (_inStream != null)
		{
			matchFinder.SetStream(_inStream);
			matchFinder.Init();
			needReleaseMFStream = true;
			_inStream = null;
			if (trainSize != 0)
			{
				matchFinder.Skip(trainSize);
			}
		}
		if (finished)
		{
			return;
		}
		finished = true;
		long hMCMFCDHGIG = nowPos64;
		if (nowPos64 == 0)
		{
			if (matchFinder.GetNumAvailableBytes() == 0)
			{
				Flush((uint)nowPos64);
				return;
			}
			uint CEAHDKFDIOK;
			uint IBGEENFNMHL;
			ReadMatchDistances(out CEAHDKFDIOK, out IBGEENFNMHL);
			uint num = (uint)(int)nowPos64 & posStateMask;
			isMatch[(state.Index << 4) + num].Encode(rangeEncoder, 0u);
			state.UpdateChar();
			byte b = matchFinder.GetIndexByte((int)(0 - additionalOffset));
			literalEncoder.GetSubCoder((uint)nowPos64, _previousByte).Encode(rangeEncoder, b);
			_previousByte = b;
			additionalOffset--;
			nowPos64++;
		}
		if (matchFinder.GetNumAvailableBytes() == 0)
		{
			Flush((uint)nowPos64);
			return;
		}
		while (true)
		{
			uint PHJEECBMCFO;
			uint num2 = GetOptimum((uint)nowPos64, out PHJEECBMCFO);
			uint num3 = (uint)(int)nowPos64 & posStateMask;
			uint num4 = (state.Index << 4) + num3;
			if (num2 == 1 && PHJEECBMCFO == uint.MaxValue)
			{
				isMatch[num4].Encode(rangeEncoder, 0u);
				byte b2 = matchFinder.GetIndexByte((int)(0 - additionalOffset));
				LiteralEncoder.LiteralSubEncoder eKANDKFGMGL = literalEncoder.GetSubCoder((uint)nowPos64, _previousByte);
				if (!state.IsCharState())
				{
					byte hGMKIONDDNO = matchFinder.GetIndexByte((int)(0 - repDistances[0] - 1 - additionalOffset));
					eKANDKFGMGL.EncodeMatched(rangeEncoder, hGMKIONDDNO, b2);
				}
				else
				{
					eKANDKFGMGL.Encode(rangeEncoder, b2);
				}
				_previousByte = b2;
				state.UpdateChar();
			}
			else
			{
				isMatch[num4].Encode(rangeEncoder, 1u);
				if (PHJEECBMCFO < 4)
				{
					isRep[state.Index].Encode(rangeEncoder, 1u);
					if (PHJEECBMCFO == 0)
					{
						isRepG0[state.Index].Encode(rangeEncoder, 0u);
						if (num2 == 1)
						{
							isRep0Long[num4].Encode(rangeEncoder, 0u);
						}
						else
						{
							isRep0Long[num4].Encode(rangeEncoder, 1u);
						}
					}
					else
					{
						isRepG0[state.Index].Encode(rangeEncoder, 1u);
						if (PHJEECBMCFO == 1)
						{
							isRepG1[state.Index].Encode(rangeEncoder, 0u);
						}
						else
						{
							isRepG1[state.Index].Encode(rangeEncoder, 1u);
							isRepG2[state.Index].Encode(rangeEncoder, PHJEECBMCFO - 2);
						}
					}
					if (num2 == 1)
					{
						state.UpdateShortRep();
					}
					else
					{
						repMatchLenEncoder.Encode(rangeEncoder, num2 - 2, num3);
						state.UpdateRep();
					}
					uint num5 = repDistances[PHJEECBMCFO];
					if (PHJEECBMCFO != 0)
					{
						for (uint num6 = PHJEECBMCFO; num6 >= 1; num6--)
						{
							repDistances[num6] = repDistances[num6 - 1];
						}
						repDistances[0] = num5;
					}
				}
				else
				{
					isRep[state.Index].Encode(rangeEncoder, 0u);
					state.UpdateMatch();
					lenEncoder.Encode(rangeEncoder, num2 - 2, num3);
					PHJEECBMCFO -= 4;
					uint num7 = GetPosSlot(PHJEECBMCFO);
					uint num8 = Base.GetLenToPosState(num2);
					posSlotEncoder[num8].Encode(rangeEncoder, num7);
					if (num7 >= 4)
					{
						int num9 = (int)((num7 >> 1) - 1);
						uint num10 = (2 | (num7 & 1)) << num9;
						uint num11 = PHJEECBMCFO - num10;
						if (num7 < 14)
						{
							BitTreeEncoder.ReverseEncode(posEncoders, num10 - num7 - 1, rangeEncoder, num9, num11);
						}
						else
						{
							rangeEncoder.EncodeDirectBits(num11 >> 4, num9 - 4);
							posAlignEncoder.ReverseEncode(rangeEncoder, num11 & 0xF);
							alignPriceCount++;
						}
					}
					uint num12 = PHJEECBMCFO;
					for (uint num13 = 3u; num13 >= 1; num13--)
					{
						repDistances[num13] = repDistances[num13 - 1];
					}
					repDistances[0] = num12;
					matchPriceCount++;
				}
				_previousByte = matchFinder.GetIndexByte((int)(num2 - 1 - additionalOffset));
			}
			additionalOffset -= num2;
			nowPos64 += num2;
			if (additionalOffset == 0)
			{
				if (matchPriceCount >= 128)
				{
					FillDistancesPrices();
				}
				if (alignPriceCount >= 16)
				{
					FillAlignPrices();
				}
				NCKELGLBGJN = nowPos64;
				JNILCBKONPG = rangeEncoder.GetProcessedSizeAdd();
				if (matchFinder.GetNumAvailableBytes() == 0)
				{
					Flush((uint)nowPos64);
					return;
				}
				if (nowPos64 - hMCMFCDHGIG >= 4096)
				{
					break;
				}
			}
		}
		finished = false;
		IAAOKDKLNGH = false;
	}

	private void ReleaseMFStream()
	{
		if (matchFinder != null && needReleaseMFStream)
		{
			matchFinder.ReleaseStream();
			needReleaseMFStream = false;
		}
	}

	private void SetOutStream(Stream BBBGGJLOCPB)
	{
		rangeEncoder.SetStream(BBBGGJLOCPB);
	}

	private void ReleaseOutStream()
	{
		rangeEncoder.ReleaseStream();
	}

	private void ReleaseStreams()
	{
		ReleaseMFStream();
		ReleaseOutStream();
	}

	private void SetStreams(Stream BHHJJHBNEKD, Stream BBBGGJLOCPB, long NCKELGLBGJN, long JNILCBKONPG)
	{
		_inStream = BHHJJHBNEKD;
		finished = false;
		Create();
		SetOutStream(BBBGGJLOCPB);
		Init();
		FillDistancesPrices();
		FillAlignPrices();
		lenEncoder.SetTableSize(numFastBytes + 1 - 2);
		lenEncoder.UpdateTables((uint)(1 << posStateBits));
		repMatchLenEncoder.SetTableSize(numFastBytes + 1 - 2);
		repMatchLenEncoder.UpdateTables((uint)(1 << posStateBits));
		nowPos64 = 0L;
	}

	public void Code(Stream BHHJJHBNEKD, Stream BBBGGJLOCPB, long NCKELGLBGJN, long JNILCBKONPG, ICodeProgress progress)
	{
		needReleaseMFStream = false;
		try
		{
			SetStreams(BHHJJHBNEKD, BBBGGJLOCPB, NCKELGLBGJN, JNILCBKONPG);
			while (true)
			{
				long NCKELGLBGJN2;
				long JNILCBKONPG2;
				bool IAAOKDKLNGH;
				CodeOneBlock(out NCKELGLBGJN2, out JNILCBKONPG2, out IAAOKDKLNGH);
				if (IAAOKDKLNGH)
				{
					break;
				}
				if (progress != null)
				{
					progress.SetProgress(NCKELGLBGJN2, JNILCBKONPG2);
				}
			}
		}
		finally
		{
			ReleaseStreams();
		}
	}

	public void WriteCoderProperties(Stream BBBGGJLOCPB)
	{
		properties[0] = (byte)((posStateBits * 5 + numLiteralPosStateBits) * 9 + numLiteralContextBits);
		for (int i = 0; i < 4; i++)
		{
			properties[1 + i] = (byte)((dictionarySize >> 8 * i) & 0xFF);
		}
		BBBGGJLOCPB.Write(properties, 0, 5);
	}

	private void FillDistancesPrices()
	{
		for (uint num = 4u; num < 128; num++)
		{
			uint num2 = GetPosSlot(num);
			int num3 = (int)((num2 >> 1) - 1);
			uint num4 = (2 | (num2 & 1)) << num3;
			tempPrices[num] = BitTreeEncoder.ReverseGetPrice(posEncoders, num4 - num2 - 1, num3, num - num4);
		}
		for (uint num5 = 0u; num5 < 4; num5++)
		{
			BitTreeEncoder fLKFKPEKKPD = posSlotEncoder[num5];
			uint num6 = num5 << 6;
			for (uint num7 = 0u; num7 < distTableSize; num7++)
			{
				posSlotPrices[num6 + num7] = fLKFKPEKKPD.GetPrice(num7);
			}
			for (uint num7 = 14u; num7 < distTableSize; num7++)
			{
				posSlotPrices[num6 + num7] += (num7 >> 1) - 1 - 4 << 6;
			}
			uint num8 = num5 * 128;
			uint num9;
			for (num9 = 0u; num9 < 4; num9++)
			{
				distancesPrices[num8 + num9] = posSlotPrices[num6 + num9];
			}
			for (; num9 < 128; num9++)
			{
				distancesPrices[num8 + num9] = posSlotPrices[num6 + GetPosSlot(num9)] + tempPrices[num9];
			}
		}
		matchPriceCount = 0u;
	}

	private void FillAlignPrices()
	{
		for (uint num = 0u; num < 16; num++)
		{
			alignPrices[num] = posAlignEncoder.ReverseGetPrice(num);
		}
		alignPriceCount = 0u;
	}

	private static int FindMatchFinder(string JDCCBCNFENK)
	{
		for (int i = 0; i < kMatchFinderIDs.Length; i++)
		{
			if (JDCCBCNFENK == kMatchFinderIDs[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void SetCoderProperties(CoderPropID[] JPIKKLMCDNM, object[] properties)
	{
		for (uint num = 0u; num < properties.Length; num++)
		{
			object obj = properties[num];
			switch (JPIKKLMCDNM[num])
			{
			case CoderPropID.NumFastBytes:
			{
				if (!(obj is int))
				{
					throw new InvalidParamException();
				}
				int num2 = (int)obj;
				if (num2 < 5 || (long)num2 > 273L)
				{
					throw new InvalidParamException();
				}
				numFastBytes = (uint)num2;
				break;
			}
			case CoderPropID.MatchFinder:
			{
				if (!(obj is string))
				{
					throw new InvalidParamException();
				}
				MatchFinderType bFDNPAJNFFK = matchFinderType;
				int num6 = FindMatchFinder(((string)obj).ToUpper());
				if (num6 < 0)
				{
					throw new InvalidParamException();
				}
				matchFinderType = (MatchFinderType)num6;
				if (matchFinder != null && bFDNPAJNFFK != matchFinderType)
				{
					dictionarySizePrev = uint.MaxValue;
					matchFinder = null;
				}
				break;
			}
			case CoderPropID.DictionarySize:
			{
				if (!(obj is int))
				{
					throw new InvalidParamException();
				}
				int num7 = (int)obj;
				if ((long)num7 < 1L || (long)num7 > 1073741824L)
				{
					throw new InvalidParamException();
				}
				dictionarySize = (uint)num7;
				int i;
				for (i = 0; (long)i < 30L && num7 > (uint)(1 << i); i++)
				{
				}
				distTableSize = (uint)(i * 2);
				break;
			}
			case CoderPropID.PosStateBits:
			{
				if (!(obj is int))
				{
					throw new InvalidParamException();
				}
				int num3 = (int)obj;
				if (num3 < 0 || (long)num3 > 4L)
				{
					throw new InvalidParamException();
				}
				posStateBits = num3;
				posStateMask = (uint)((1 << posStateBits) - 1);
				break;
			}
			case CoderPropID.LitPosBits:
			{
				if (!(obj is int))
				{
					throw new InvalidParamException();
				}
				int num5 = (int)obj;
				if (num5 < 0 || (long)num5 > 4L)
				{
					throw new InvalidParamException();
				}
				numLiteralPosStateBits = num5;
				break;
			}
			case CoderPropID.LitContextBits:
			{
				if (!(obj is int))
				{
					throw new InvalidParamException();
				}
				int num4 = (int)obj;
				if (num4 < 0 || (long)num4 > 8L)
				{
					throw new InvalidParamException();
				}
				numLiteralContextBits = num4;
				break;
			}
			case CoderPropID.EndMarker:
				if (!(obj is bool))
				{
					throw new InvalidParamException();
				}
				SetWriteEndMarkerMode((bool)obj);
				break;
			default:
				throw new InvalidParamException();
			case CoderPropID.Algorithm:
				break;
			}
		}
	}

	public void SetTrainSize(uint CIGKDGKAADK)
	{
		trainSize = CIGKDGKAADK;
	}
}
