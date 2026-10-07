using System;

internal sealed class InflateCodes
{
	private const int START = 0;

	private const int LEN = 1;

	private const int LENEXT = 2;

	private const int DIST = 3;

	private const int DISTEXT = 4;

	private const int COPY = 5;

	private const int LIT = 6;

	private const int WASH = 7;

	private const int END = 8;

	private const int BADCODE = 9;

	internal int mode;

	internal int len;

	internal int[] tree;

	internal int tree_index;

	internal int need;

	internal int lit;

	internal int bitsToGet;

	internal int dist;

	internal byte lbits;

	internal byte dbits;

	internal int[] ltree;

	internal int ltree_index;

	internal int[] dtree;

	internal int dtree_index;

	internal InflateCodes()
	{
	}

	internal void Init(int GGEJHHHGPKN, int NBHIKILKMED, int[] AEFHBJIMPHM, int HLDNDJKELJE, int[] GICLKGGKJAG, int KMKIMJDIKHC)
	{
		mode = 0;
		lbits = (byte)GGEJHHHGPKN;
		dbits = (byte)NBHIKILKMED;
		ltree = AEFHBJIMPHM;
		ltree_index = HLDNDJKELJE;
		dtree = GICLKGGKJAG;
		dtree_index = KMKIMJDIKHC;
		tree = null;
	}

	internal int Process(InflateBlocks CGKHDGJKOMG, int BOPODEAIEBJ)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		ZlibCodec cJMKCEHHMCH = CGKHDGJKOMG._codec;
		num3 = cJMKCEHHMCH.NextIn;
		int num4 = cJMKCEHHMCH.AvailableBytesIn;
		num = CGKHDGJKOMG.bitb;
		num2 = CGKHDGJKOMG.bitk;
		int num5 = CGKHDGJKOMG.writeAt;
		int num6 = ((num5 >= CGKHDGJKOMG.readAt) ? (CGKHDGJKOMG.end - num5) : (CGKHDGJKOMG.readAt - num5 - 1));
		while (true)
		{
			switch (mode)
			{
			case 0:
				if (num6 >= 258 && num4 >= 10)
				{
					CGKHDGJKOMG.bitb = num;
					CGKHDGJKOMG.bitk = num2;
					cJMKCEHHMCH.AvailableBytesIn = num4;
					cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
					cJMKCEHHMCH.NextIn = num3;
					CGKHDGJKOMG.writeAt = num5;
					BOPODEAIEBJ = InflateFast(lbits, dbits, ltree, ltree_index, dtree, dtree_index, CGKHDGJKOMG, cJMKCEHHMCH);
					num3 = cJMKCEHHMCH.NextIn;
					num4 = cJMKCEHHMCH.AvailableBytesIn;
					num = CGKHDGJKOMG.bitb;
					num2 = CGKHDGJKOMG.bitk;
					num5 = CGKHDGJKOMG.writeAt;
					num6 = ((num5 >= CGKHDGJKOMG.readAt) ? (CGKHDGJKOMG.end - num5) : (CGKHDGJKOMG.readAt - num5 - 1));
					if (BOPODEAIEBJ != 0)
					{
						mode = ((BOPODEAIEBJ != 1) ? 9 : 7);
						break;
					}
				}
				need = lbits;
				tree = ltree;
				tree_index = ltree_index;
				mode = 1;
				goto case 1;
			case 1:
			{
				int aCLJBJJAENG;
				for (aCLJBJJAENG = need; num2 < aCLJBJJAENG; num2 += 8)
				{
					if (num4 != 0)
					{
						BOPODEAIEBJ = 0;
						num4--;
						num |= (cJMKCEHHMCH.InputBuffer[num3++] & 0xFF) << num2;
						continue;
					}
					CGKHDGJKOMG.bitb = num;
					CGKHDGJKOMG.bitk = num2;
					cJMKCEHHMCH.AvailableBytesIn = num4;
					cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
					cJMKCEHHMCH.NextIn = num3;
					CGKHDGJKOMG.writeAt = num5;
					return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
				}
				int num7 = (tree_index + (num & InternalInflateConstants.InflateMask[aCLJBJJAENG])) * 3;
				num >>= tree[num7 + 1];
				num2 -= tree[num7 + 1];
				int num8 = tree[num7];
				if (num8 == 0)
				{
					lit = tree[num7 + 2];
					mode = 6;
					break;
				}
				if ((num8 & 0x10) != 0)
				{
					bitsToGet = num8 & 0xF;
					len = tree[num7 + 2];
					mode = 2;
					break;
				}
				if ((num8 & 0x40) == 0)
				{
					need = num8;
					tree_index = num7 / 3 + tree[num7 + 2];
					break;
				}
				if ((num8 & 0x20) != 0)
				{
					mode = 7;
					break;
				}
				mode = 9;
				cJMKCEHHMCH.Message = "invalid literal/length code";
				BOPODEAIEBJ = -3;
				CGKHDGJKOMG.bitb = num;
				CGKHDGJKOMG.bitk = num2;
				cJMKCEHHMCH.AvailableBytesIn = num4;
				cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
				cJMKCEHHMCH.NextIn = num3;
				CGKHDGJKOMG.writeAt = num5;
				return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
			}
			case 2:
			{
				int aCLJBJJAENG;
				for (aCLJBJJAENG = bitsToGet; num2 < aCLJBJJAENG; num2 += 8)
				{
					if (num4 != 0)
					{
						BOPODEAIEBJ = 0;
						num4--;
						num |= (cJMKCEHHMCH.InputBuffer[num3++] & 0xFF) << num2;
						continue;
					}
					CGKHDGJKOMG.bitb = num;
					CGKHDGJKOMG.bitk = num2;
					cJMKCEHHMCH.AvailableBytesIn = num4;
					cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
					cJMKCEHHMCH.NextIn = num3;
					CGKHDGJKOMG.writeAt = num5;
					return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
				}
				len += num & InternalInflateConstants.InflateMask[aCLJBJJAENG];
				num >>= aCLJBJJAENG;
				num2 -= aCLJBJJAENG;
				need = dbits;
				tree = dtree;
				tree_index = dtree_index;
				mode = 3;
				goto case 3;
			}
			case 3:
			{
				int aCLJBJJAENG;
				for (aCLJBJJAENG = need; num2 < aCLJBJJAENG; num2 += 8)
				{
					if (num4 != 0)
					{
						BOPODEAIEBJ = 0;
						num4--;
						num |= (cJMKCEHHMCH.InputBuffer[num3++] & 0xFF) << num2;
						continue;
					}
					CGKHDGJKOMG.bitb = num;
					CGKHDGJKOMG.bitk = num2;
					cJMKCEHHMCH.AvailableBytesIn = num4;
					cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
					cJMKCEHHMCH.NextIn = num3;
					CGKHDGJKOMG.writeAt = num5;
					return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
				}
				int num7 = (tree_index + (num & InternalInflateConstants.InflateMask[aCLJBJJAENG])) * 3;
				num >>= tree[num7 + 1];
				num2 -= tree[num7 + 1];
				int num8 = tree[num7];
				if ((num8 & 0x10) != 0)
				{
					bitsToGet = num8 & 0xF;
					dist = tree[num7 + 2];
					mode = 4;
					break;
				}
				if ((num8 & 0x40) == 0)
				{
					need = num8;
					tree_index = num7 / 3 + tree[num7 + 2];
					break;
				}
				mode = 9;
				cJMKCEHHMCH.Message = "invalid distance code";
				BOPODEAIEBJ = -3;
				CGKHDGJKOMG.bitb = num;
				CGKHDGJKOMG.bitk = num2;
				cJMKCEHHMCH.AvailableBytesIn = num4;
				cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
				cJMKCEHHMCH.NextIn = num3;
				CGKHDGJKOMG.writeAt = num5;
				return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
			}
			case 4:
			{
				int aCLJBJJAENG;
				for (aCLJBJJAENG = bitsToGet; num2 < aCLJBJJAENG; num2 += 8)
				{
					if (num4 != 0)
					{
						BOPODEAIEBJ = 0;
						num4--;
						num |= (cJMKCEHHMCH.InputBuffer[num3++] & 0xFF) << num2;
						continue;
					}
					CGKHDGJKOMG.bitb = num;
					CGKHDGJKOMG.bitk = num2;
					cJMKCEHHMCH.AvailableBytesIn = num4;
					cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
					cJMKCEHHMCH.NextIn = num3;
					CGKHDGJKOMG.writeAt = num5;
					return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
				}
				dist += num & InternalInflateConstants.InflateMask[aCLJBJJAENG];
				num >>= aCLJBJJAENG;
				num2 -= aCLJBJJAENG;
				mode = 5;
				goto case 5;
			}
			case 5:
			{
				int i;
				for (i = num5 - dist; i < 0; i += CGKHDGJKOMG.end)
				{
				}
				while (len != 0)
				{
					if (num6 == 0)
					{
						if (num5 == CGKHDGJKOMG.end && CGKHDGJKOMG.readAt != 0)
						{
							num5 = 0;
							num6 = ((num5 >= CGKHDGJKOMG.readAt) ? (CGKHDGJKOMG.end - num5) : (CGKHDGJKOMG.readAt - num5 - 1));
						}
						if (num6 == 0)
						{
							CGKHDGJKOMG.writeAt = num5;
							BOPODEAIEBJ = CGKHDGJKOMG.Flush(BOPODEAIEBJ);
							num5 = CGKHDGJKOMG.writeAt;
							num6 = ((num5 >= CGKHDGJKOMG.readAt) ? (CGKHDGJKOMG.end - num5) : (CGKHDGJKOMG.readAt - num5 - 1));
							if (num5 == CGKHDGJKOMG.end && CGKHDGJKOMG.readAt != 0)
							{
								num5 = 0;
								num6 = ((num5 >= CGKHDGJKOMG.readAt) ? (CGKHDGJKOMG.end - num5) : (CGKHDGJKOMG.readAt - num5 - 1));
							}
							if (num6 == 0)
							{
								CGKHDGJKOMG.bitb = num;
								CGKHDGJKOMG.bitk = num2;
								cJMKCEHHMCH.AvailableBytesIn = num4;
								cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
								cJMKCEHHMCH.NextIn = num3;
								CGKHDGJKOMG.writeAt = num5;
								return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
							}
						}
					}
					CGKHDGJKOMG.window[num5++] = CGKHDGJKOMG.window[i++];
					num6--;
					if (i == CGKHDGJKOMG.end)
					{
						i = 0;
					}
					len--;
				}
				mode = 0;
				break;
			}
			case 6:
				if (num6 == 0)
				{
					if (num5 == CGKHDGJKOMG.end && CGKHDGJKOMG.readAt != 0)
					{
						num5 = 0;
						num6 = ((num5 >= CGKHDGJKOMG.readAt) ? (CGKHDGJKOMG.end - num5) : (CGKHDGJKOMG.readAt - num5 - 1));
					}
					if (num6 == 0)
					{
						CGKHDGJKOMG.writeAt = num5;
						BOPODEAIEBJ = CGKHDGJKOMG.Flush(BOPODEAIEBJ);
						num5 = CGKHDGJKOMG.writeAt;
						num6 = ((num5 >= CGKHDGJKOMG.readAt) ? (CGKHDGJKOMG.end - num5) : (CGKHDGJKOMG.readAt - num5 - 1));
						if (num5 == CGKHDGJKOMG.end && CGKHDGJKOMG.readAt != 0)
						{
							num5 = 0;
							num6 = ((num5 >= CGKHDGJKOMG.readAt) ? (CGKHDGJKOMG.end - num5) : (CGKHDGJKOMG.readAt - num5 - 1));
						}
						if (num6 == 0)
						{
							CGKHDGJKOMG.bitb = num;
							CGKHDGJKOMG.bitk = num2;
							cJMKCEHHMCH.AvailableBytesIn = num4;
							cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
							cJMKCEHHMCH.NextIn = num3;
							CGKHDGJKOMG.writeAt = num5;
							return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
						}
					}
				}
				BOPODEAIEBJ = 0;
				CGKHDGJKOMG.window[num5++] = (byte)lit;
				num6--;
				mode = 0;
				break;
			case 7:
				if (num2 > 7)
				{
					num2 -= 8;
					num4++;
					num3--;
				}
				CGKHDGJKOMG.writeAt = num5;
				BOPODEAIEBJ = CGKHDGJKOMG.Flush(BOPODEAIEBJ);
				num5 = CGKHDGJKOMG.writeAt;
				num6 = ((num5 >= CGKHDGJKOMG.readAt) ? (CGKHDGJKOMG.end - num5) : (CGKHDGJKOMG.readAt - num5 - 1));
				if (CGKHDGJKOMG.readAt != CGKHDGJKOMG.writeAt)
				{
					CGKHDGJKOMG.bitb = num;
					CGKHDGJKOMG.bitk = num2;
					cJMKCEHHMCH.AvailableBytesIn = num4;
					cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
					cJMKCEHHMCH.NextIn = num3;
					CGKHDGJKOMG.writeAt = num5;
					return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
				}
				mode = 8;
				goto case 8;
			case 8:
				BOPODEAIEBJ = 1;
				CGKHDGJKOMG.bitb = num;
				CGKHDGJKOMG.bitk = num2;
				cJMKCEHHMCH.AvailableBytesIn = num4;
				cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
				cJMKCEHHMCH.NextIn = num3;
				CGKHDGJKOMG.writeAt = num5;
				return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
			case 9:
				BOPODEAIEBJ = -3;
				CGKHDGJKOMG.bitb = num;
				CGKHDGJKOMG.bitk = num2;
				cJMKCEHHMCH.AvailableBytesIn = num4;
				cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
				cJMKCEHHMCH.NextIn = num3;
				CGKHDGJKOMG.writeAt = num5;
				return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
			default:
				BOPODEAIEBJ = -2;
				CGKHDGJKOMG.bitb = num;
				CGKHDGJKOMG.bitk = num2;
				cJMKCEHHMCH.AvailableBytesIn = num4;
				cJMKCEHHMCH.TotalBytesIn += num3 - cJMKCEHHMCH.NextIn;
				cJMKCEHHMCH.NextIn = num3;
				CGKHDGJKOMG.writeAt = num5;
				return CGKHDGJKOMG.Flush(BOPODEAIEBJ);
			}
		}
	}

	internal int InflateFast(int GGEJHHHGPKN, int NBHIKILKMED, int[] AEFHBJIMPHM, int HLDNDJKELJE, int[] GICLKGGKJAG, int KMKIMJDIKHC, InflateBlocks JDCCBCNFENK, ZlibCodec LKPCKJOLJDO)
	{
		int lMIPBGGILEJ = LKPCKJOLJDO.NextIn;
		int num = LKPCKJOLJDO.AvailableBytesIn;
		int num2 = JDCCBCNFENK.bitb;
		int num3 = JDCCBCNFENK.bitk;
		int num4 = JDCCBCNFENK.writeAt;
		int num5 = ((num4 >= JDCCBCNFENK.readAt) ? (JDCCBCNFENK.end - num4) : (JDCCBCNFENK.readAt - num4 - 1));
		int num6 = InternalInflateConstants.InflateMask[GGEJHHHGPKN];
		int num7 = InternalInflateConstants.InflateMask[NBHIKILKMED];
		int num12;
		while (true)
		{
			if (num3 < 20)
			{
				num--;
				num2 |= (LKPCKJOLJDO.InputBuffer[lMIPBGGILEJ++] & 0xFF) << num3;
				num3 += 8;
				continue;
			}
			int num8 = num2 & num6;
			int[] array = AEFHBJIMPHM;
			int num9 = HLDNDJKELJE;
			int num10 = (num9 + num8) * 3;
			int num11;
			if ((num11 = array[num10]) == 0)
			{
				num2 >>= array[num10 + 1];
				num3 -= array[num10 + 1];
				JDCCBCNFENK.window[num4++] = (byte)array[num10 + 2];
				num5--;
			}
			else
			{
				while (true)
				{
					num2 >>= array[num10 + 1];
					num3 -= array[num10 + 1];
					if ((num11 & 0x10) != 0)
					{
						num11 &= 0xF;
						num12 = array[num10 + 2] + (num2 & InternalInflateConstants.InflateMask[num11]);
						num2 >>= num11;
						for (num3 -= num11; num3 < 15; num3 += 8)
						{
							num--;
							num2 |= (LKPCKJOLJDO.InputBuffer[lMIPBGGILEJ++] & 0xFF) << num3;
						}
						num8 = num2 & num7;
						array = GICLKGGKJAG;
						num9 = KMKIMJDIKHC;
						num10 = (num9 + num8) * 3;
						num11 = array[num10];
						while (true)
						{
							num2 >>= array[num10 + 1];
							num3 -= array[num10 + 1];
							if ((num11 & 0x10) != 0)
							{
								break;
							}
							if ((num11 & 0x40) == 0)
							{
								num8 += array[num10 + 2];
								num8 += num2 & InternalInflateConstants.InflateMask[num11];
								num10 = (num9 + num8) * 3;
								num11 = array[num10];
								continue;
							}
							LKPCKJOLJDO.Message = "invalid distance code";
							num12 = LKPCKJOLJDO.AvailableBytesIn - num;
							num12 = ((num3 >> 3 >= num12) ? num12 : (num3 >> 3));
							num += num12;
							lMIPBGGILEJ -= num12;
							num3 -= num12 << 3;
							JDCCBCNFENK.bitb = num2;
							JDCCBCNFENK.bitk = num3;
							LKPCKJOLJDO.AvailableBytesIn = num;
							LKPCKJOLJDO.TotalBytesIn += lMIPBGGILEJ - LKPCKJOLJDO.NextIn;
							LKPCKJOLJDO.NextIn = lMIPBGGILEJ;
							JDCCBCNFENK.writeAt = num4;
							return -3;
						}
						for (num11 &= 0xF; num3 < num11; num3 += 8)
						{
							num--;
							num2 |= (LKPCKJOLJDO.InputBuffer[lMIPBGGILEJ++] & 0xFF) << num3;
						}
						int num13 = array[num10 + 2] + (num2 & InternalInflateConstants.InflateMask[num11]);
						num2 >>= num11;
						num3 -= num11;
						num5 -= num12;
						int num14;
						if (num4 >= num13)
						{
							num14 = num4 - num13;
							if (num4 - num14 > 0 && 2 > num4 - num14)
							{
								JDCCBCNFENK.window[num4++] = JDCCBCNFENK.window[num14++];
								JDCCBCNFENK.window[num4++] = JDCCBCNFENK.window[num14++];
								num12 -= 2;
							}
							else
							{
								Array.Copy(JDCCBCNFENK.window, num14, JDCCBCNFENK.window, num4, 2);
								num4 += 2;
								num14 += 2;
								num12 -= 2;
							}
						}
						else
						{
							num14 = num4 - num13;
							do
							{
								num14 += JDCCBCNFENK.end;
							}
							while (num14 < 0);
							num11 = JDCCBCNFENK.end - num14;
							if (num12 > num11)
							{
								num12 -= num11;
								if (num4 - num14 > 0 && num11 > num4 - num14)
								{
									do
									{
										JDCCBCNFENK.window[num4++] = JDCCBCNFENK.window[num14++];
									}
									while (--num11 != 0);
								}
								else
								{
									Array.Copy(JDCCBCNFENK.window, num14, JDCCBCNFENK.window, num4, num11);
									num4 += num11;
									num14 += num11;
									num11 = 0;
								}
								num14 = 0;
							}
						}
						if (num4 - num14 > 0 && num12 > num4 - num14)
						{
							do
							{
								JDCCBCNFENK.window[num4++] = JDCCBCNFENK.window[num14++];
							}
							while (--num12 != 0);
							break;
						}
						Array.Copy(JDCCBCNFENK.window, num14, JDCCBCNFENK.window, num4, num12);
						num4 += num12;
						num14 += num12;
						num12 = 0;
						break;
					}
					if ((num11 & 0x40) == 0)
					{
						num8 += array[num10 + 2];
						num8 += num2 & InternalInflateConstants.InflateMask[num11];
						num10 = (num9 + num8) * 3;
						if ((num11 = array[num10]) == 0)
						{
							num2 >>= array[num10 + 1];
							num3 -= array[num10 + 1];
							JDCCBCNFENK.window[num4++] = (byte)array[num10 + 2];
							num5--;
							break;
						}
						continue;
					}
					if ((num11 & 0x20) != 0)
					{
						num12 = LKPCKJOLJDO.AvailableBytesIn - num;
						num12 = ((num3 >> 3 >= num12) ? num12 : (num3 >> 3));
						num += num12;
						lMIPBGGILEJ -= num12;
						num3 -= num12 << 3;
						JDCCBCNFENK.bitb = num2;
						JDCCBCNFENK.bitk = num3;
						LKPCKJOLJDO.AvailableBytesIn = num;
						LKPCKJOLJDO.TotalBytesIn += lMIPBGGILEJ - LKPCKJOLJDO.NextIn;
						LKPCKJOLJDO.NextIn = lMIPBGGILEJ;
						JDCCBCNFENK.writeAt = num4;
						return 1;
					}
					LKPCKJOLJDO.Message = "invalid literal/length code";
					num12 = LKPCKJOLJDO.AvailableBytesIn - num;
					num12 = ((num3 >> 3 >= num12) ? num12 : (num3 >> 3));
					num += num12;
					lMIPBGGILEJ -= num12;
					num3 -= num12 << 3;
					JDCCBCNFENK.bitb = num2;
					JDCCBCNFENK.bitk = num3;
					LKPCKJOLJDO.AvailableBytesIn = num;
					LKPCKJOLJDO.TotalBytesIn += lMIPBGGILEJ - LKPCKJOLJDO.NextIn;
					LKPCKJOLJDO.NextIn = lMIPBGGILEJ;
					JDCCBCNFENK.writeAt = num4;
					return -3;
				}
			}
			if (num5 < 258 || num < 10)
			{
				break;
			}
		}
		num12 = LKPCKJOLJDO.AvailableBytesIn - num;
		num12 = ((num3 >> 3 >= num12) ? num12 : (num3 >> 3));
		num += num12;
		lMIPBGGILEJ -= num12;
		num3 -= num12 << 3;
		JDCCBCNFENK.bitb = num2;
		JDCCBCNFENK.bitk = num3;
		LKPCKJOLJDO.AvailableBytesIn = num;
		LKPCKJOLJDO.TotalBytesIn += lMIPBGGILEJ - LKPCKJOLJDO.NextIn;
		LKPCKJOLJDO.NextIn = lMIPBGGILEJ;
		JDCCBCNFENK.writeAt = num4;
		return 0;
	}
}
