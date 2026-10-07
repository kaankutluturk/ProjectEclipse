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

	internal void Init(int literalBits, int distanceBits, int[] literalTable, int literalTableIndex, int[] distanceTable, int distanceTableIndex)
	{
		mode = 0;
		lbits = (byte)literalBits;
		dbits = (byte)distanceBits;
		ltree = literalTable;
		ltree_index = literalTableIndex;
		dtree = distanceTable;
		dtree_index = distanceTableIndex;
		tree = null;
	}

	internal int Process(InflateBlocks blocks, int result)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		ZlibCodec codec = blocks._codec;
		num3 = codec.NextIn;
		int num4 = codec.AvailableBytesIn;
		num = blocks.bitb;
		num2 = blocks.bitk;
		int num5 = blocks.writeAt;
		int num6 = ((num5 >= blocks.readAt) ? (blocks.end - num5) : (blocks.readAt - num5 - 1));
		while (true)
		{
			switch (mode)
			{
			case 0:
				if (num6 >= 258 && num4 >= 10)
				{
					blocks.bitb = num;
					blocks.bitk = num2;
					codec.AvailableBytesIn = num4;
					codec.TotalBytesIn += num3 - codec.NextIn;
					codec.NextIn = num3;
					blocks.writeAt = num5;
					result = InflateFast(lbits, dbits, ltree, ltree_index, dtree, dtree_index, blocks, codec);
					num3 = codec.NextIn;
					num4 = codec.AvailableBytesIn;
					num = blocks.bitb;
					num2 = blocks.bitk;
					num5 = blocks.writeAt;
					num6 = ((num5 >= blocks.readAt) ? (blocks.end - num5) : (blocks.readAt - num5 - 1));
					if (result != 0)
					{
						mode = ((result != 1) ? 9 : 7);
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
				int bitsNeeded;
				for (bitsNeeded = need; num2 < bitsNeeded; num2 += 8)
				{
					if (num4 != 0)
					{
						result = 0;
						num4--;
						num |= (codec.InputBuffer[num3++] & 0xFF) << num2;
						continue;
					}
					blocks.bitb = num;
					blocks.bitk = num2;
					codec.AvailableBytesIn = num4;
					codec.TotalBytesIn += num3 - codec.NextIn;
					codec.NextIn = num3;
					blocks.writeAt = num5;
					return blocks.Flush(result);
				}
				int num7 = (tree_index + (num & InternalInflateConstants.InflateMask[bitsNeeded])) * 3;
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
				codec.Message = "invalid literal/length code";
				result = -3;
				blocks.bitb = num;
				blocks.bitk = num2;
				codec.AvailableBytesIn = num4;
				codec.TotalBytesIn += num3 - codec.NextIn;
				codec.NextIn = num3;
				blocks.writeAt = num5;
				return blocks.Flush(result);
			}
			case 2:
			{
				int bitsNeeded;
				for (bitsNeeded = bitsToGet; num2 < bitsNeeded; num2 += 8)
				{
					if (num4 != 0)
					{
						result = 0;
						num4--;
						num |= (codec.InputBuffer[num3++] & 0xFF) << num2;
						continue;
					}
					blocks.bitb = num;
					blocks.bitk = num2;
					codec.AvailableBytesIn = num4;
					codec.TotalBytesIn += num3 - codec.NextIn;
					codec.NextIn = num3;
					blocks.writeAt = num5;
					return blocks.Flush(result);
				}
				len += num & InternalInflateConstants.InflateMask[bitsNeeded];
				num >>= bitsNeeded;
				num2 -= bitsNeeded;
				need = dbits;
				tree = dtree;
				tree_index = dtree_index;
				mode = 3;
				goto case 3;
			}
			case 3:
			{
				int bitsNeeded;
				for (bitsNeeded = need; num2 < bitsNeeded; num2 += 8)
				{
					if (num4 != 0)
					{
						result = 0;
						num4--;
						num |= (codec.InputBuffer[num3++] & 0xFF) << num2;
						continue;
					}
					blocks.bitb = num;
					blocks.bitk = num2;
					codec.AvailableBytesIn = num4;
					codec.TotalBytesIn += num3 - codec.NextIn;
					codec.NextIn = num3;
					blocks.writeAt = num5;
					return blocks.Flush(result);
				}
				int num7 = (tree_index + (num & InternalInflateConstants.InflateMask[bitsNeeded])) * 3;
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
				codec.Message = "invalid distance code";
				result = -3;
				blocks.bitb = num;
				blocks.bitk = num2;
				codec.AvailableBytesIn = num4;
				codec.TotalBytesIn += num3 - codec.NextIn;
				codec.NextIn = num3;
				blocks.writeAt = num5;
				return blocks.Flush(result);
			}
			case 4:
			{
				int bitsNeeded;
				for (bitsNeeded = bitsToGet; num2 < bitsNeeded; num2 += 8)
				{
					if (num4 != 0)
					{
						result = 0;
						num4--;
						num |= (codec.InputBuffer[num3++] & 0xFF) << num2;
						continue;
					}
					blocks.bitb = num;
					blocks.bitk = num2;
					codec.AvailableBytesIn = num4;
					codec.TotalBytesIn += num3 - codec.NextIn;
					codec.NextIn = num3;
					blocks.writeAt = num5;
					return blocks.Flush(result);
				}
				dist += num & InternalInflateConstants.InflateMask[bitsNeeded];
				num >>= bitsNeeded;
				num2 -= bitsNeeded;
				mode = 5;
				goto case 5;
			}
			case 5:
			{
				int i;
				for (i = num5 - dist; i < 0; i += blocks.end)
				{
				}
				while (len != 0)
				{
					if (num6 == 0)
					{
						if (num5 == blocks.end && blocks.readAt != 0)
						{
							num5 = 0;
							num6 = ((num5 >= blocks.readAt) ? (blocks.end - num5) : (blocks.readAt - num5 - 1));
						}
						if (num6 == 0)
						{
							blocks.writeAt = num5;
							result = blocks.Flush(result);
							num5 = blocks.writeAt;
							num6 = ((num5 >= blocks.readAt) ? (blocks.end - num5) : (blocks.readAt - num5 - 1));
							if (num5 == blocks.end && blocks.readAt != 0)
							{
								num5 = 0;
								num6 = ((num5 >= blocks.readAt) ? (blocks.end - num5) : (blocks.readAt - num5 - 1));
							}
							if (num6 == 0)
							{
								blocks.bitb = num;
								blocks.bitk = num2;
								codec.AvailableBytesIn = num4;
								codec.TotalBytesIn += num3 - codec.NextIn;
								codec.NextIn = num3;
								blocks.writeAt = num5;
								return blocks.Flush(result);
							}
						}
					}
					blocks.window[num5++] = blocks.window[i++];
					num6--;
					if (i == blocks.end)
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
					if (num5 == blocks.end && blocks.readAt != 0)
					{
						num5 = 0;
						num6 = ((num5 >= blocks.readAt) ? (blocks.end - num5) : (blocks.readAt - num5 - 1));
					}
					if (num6 == 0)
					{
						blocks.writeAt = num5;
						result = blocks.Flush(result);
						num5 = blocks.writeAt;
						num6 = ((num5 >= blocks.readAt) ? (blocks.end - num5) : (blocks.readAt - num5 - 1));
						if (num5 == blocks.end && blocks.readAt != 0)
						{
							num5 = 0;
							num6 = ((num5 >= blocks.readAt) ? (blocks.end - num5) : (blocks.readAt - num5 - 1));
						}
						if (num6 == 0)
						{
							blocks.bitb = num;
							blocks.bitk = num2;
							codec.AvailableBytesIn = num4;
							codec.TotalBytesIn += num3 - codec.NextIn;
							codec.NextIn = num3;
							blocks.writeAt = num5;
							return blocks.Flush(result);
						}
					}
				}
				result = 0;
				blocks.window[num5++] = (byte)lit;
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
				blocks.writeAt = num5;
				result = blocks.Flush(result);
				num5 = blocks.writeAt;
				num6 = ((num5 >= blocks.readAt) ? (blocks.end - num5) : (blocks.readAt - num5 - 1));
				if (blocks.readAt != blocks.writeAt)
				{
					blocks.bitb = num;
					blocks.bitk = num2;
					codec.AvailableBytesIn = num4;
					codec.TotalBytesIn += num3 - codec.NextIn;
					codec.NextIn = num3;
					blocks.writeAt = num5;
					return blocks.Flush(result);
				}
				mode = 8;
				goto case 8;
			case 8:
				result = 1;
				blocks.bitb = num;
				blocks.bitk = num2;
				codec.AvailableBytesIn = num4;
				codec.TotalBytesIn += num3 - codec.NextIn;
				codec.NextIn = num3;
				blocks.writeAt = num5;
				return blocks.Flush(result);
			case 9:
				result = -3;
				blocks.bitb = num;
				blocks.bitk = num2;
				codec.AvailableBytesIn = num4;
				codec.TotalBytesIn += num3 - codec.NextIn;
				codec.NextIn = num3;
				blocks.writeAt = num5;
				return blocks.Flush(result);
			default:
				result = -2;
				blocks.bitb = num;
				blocks.bitk = num2;
				codec.AvailableBytesIn = num4;
				codec.TotalBytesIn += num3 - codec.NextIn;
				codec.NextIn = num3;
				blocks.writeAt = num5;
				return blocks.Flush(result);
			}
		}
	}

	internal int InflateFast(int literalBits, int distanceBits, int[] literalTable, int literalTableIndex, int[] distanceTable, int distanceTableIndex, InflateBlocks blocks, ZlibCodec codec)
	{
		int inputIndex = codec.NextIn;
		int num = codec.AvailableBytesIn;
		int num2 = blocks.bitb;
		int num3 = blocks.bitk;
		int num4 = blocks.writeAt;
		int num5 = ((num4 >= blocks.readAt) ? (blocks.end - num4) : (blocks.readAt - num4 - 1));
		int num6 = InternalInflateConstants.InflateMask[literalBits];
		int num7 = InternalInflateConstants.InflateMask[distanceBits];
		int num12;
		while (true)
		{
			if (num3 < 20)
			{
				num--;
				num2 |= (codec.InputBuffer[inputIndex++] & 0xFF) << num3;
				num3 += 8;
				continue;
			}
			int num8 = num2 & num6;
			int[] array = literalTable;
			int num9 = literalTableIndex;
			int num10 = (num9 + num8) * 3;
			int num11;
			if ((num11 = array[num10]) == 0)
			{
				num2 >>= array[num10 + 1];
				num3 -= array[num10 + 1];
				blocks.window[num4++] = (byte)array[num10 + 2];
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
							num2 |= (codec.InputBuffer[inputIndex++] & 0xFF) << num3;
						}
						num8 = num2 & num7;
						array = distanceTable;
						num9 = distanceTableIndex;
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
							codec.Message = "invalid distance code";
							num12 = codec.AvailableBytesIn - num;
							num12 = ((num3 >> 3 >= num12) ? num12 : (num3 >> 3));
							num += num12;
							inputIndex -= num12;
							num3 -= num12 << 3;
							blocks.bitb = num2;
							blocks.bitk = num3;
							codec.AvailableBytesIn = num;
							codec.TotalBytesIn += inputIndex - codec.NextIn;
							codec.NextIn = inputIndex;
							blocks.writeAt = num4;
							return -3;
						}
						for (num11 &= 0xF; num3 < num11; num3 += 8)
						{
							num--;
							num2 |= (codec.InputBuffer[inputIndex++] & 0xFF) << num3;
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
								blocks.window[num4++] = blocks.window[num14++];
								blocks.window[num4++] = blocks.window[num14++];
								num12 -= 2;
							}
							else
							{
								Array.Copy(blocks.window, num14, blocks.window, num4, 2);
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
								num14 += blocks.end;
							}
							while (num14 < 0);
							num11 = blocks.end - num14;
							if (num12 > num11)
							{
								num12 -= num11;
								if (num4 - num14 > 0 && num11 > num4 - num14)
								{
									do
									{
										blocks.window[num4++] = blocks.window[num14++];
									}
									while (--num11 != 0);
								}
								else
								{
									Array.Copy(blocks.window, num14, blocks.window, num4, num11);
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
								blocks.window[num4++] = blocks.window[num14++];
							}
							while (--num12 != 0);
							break;
						}
						Array.Copy(blocks.window, num14, blocks.window, num4, num12);
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
							blocks.window[num4++] = (byte)array[num10 + 2];
							num5--;
							break;
						}
						continue;
					}
					if ((num11 & 0x20) != 0)
					{
						num12 = codec.AvailableBytesIn - num;
						num12 = ((num3 >> 3 >= num12) ? num12 : (num3 >> 3));
						num += num12;
						inputIndex -= num12;
						num3 -= num12 << 3;
						blocks.bitb = num2;
						blocks.bitk = num3;
						codec.AvailableBytesIn = num;
						codec.TotalBytesIn += inputIndex - codec.NextIn;
						codec.NextIn = inputIndex;
						blocks.writeAt = num4;
						return 1;
					}
					codec.Message = "invalid literal/length code";
					num12 = codec.AvailableBytesIn - num;
					num12 = ((num3 >> 3 >= num12) ? num12 : (num3 >> 3));
					num += num12;
					inputIndex -= num12;
					num3 -= num12 << 3;
					blocks.bitb = num2;
					blocks.bitk = num3;
					codec.AvailableBytesIn = num;
					codec.TotalBytesIn += inputIndex - codec.NextIn;
					codec.NextIn = inputIndex;
					blocks.writeAt = num4;
					return -3;
				}
			}
			if (num5 < 258 || num < 10)
			{
				break;
			}
		}
		num12 = codec.AvailableBytesIn - num;
		num12 = ((num3 >> 3 >= num12) ? num12 : (num3 >> 3));
		num += num12;
		inputIndex -= num12;
		num3 -= num12 << 3;
		blocks.bitb = num2;
		blocks.bitk = num3;
		codec.AvailableBytesIn = num;
		codec.TotalBytesIn += inputIndex - codec.NextIn;
		codec.NextIn = inputIndex;
		blocks.writeAt = num4;
		return 0;
	}
}
