using System;

internal sealed class InflateBlocks
{
	private enum InflateBlockMode
	{
		TYPE = 0,
		LENS = 1,
		STORED = 2,
		TABLE = 3,
		BTREE = 4,
		DTREE = 5,
		CODES = 6,
		DRY = 7,
		DONE = 8,
		BAD = 9
	}

	private const int MANY = 1440;

	internal static readonly int[] border = new int[19]
	{
		16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
		11, 4, 12, 3, 13, 2, 14, 1, 15
	};

	private InflateBlockMode mode;

	internal int left;

	internal int table;

	internal int index;

	internal int[] blens;

	internal int[] bb = new int[1];

	internal int[] tb = new int[1];

	internal InflateCodes codes = new InflateCodes();

	internal int last;

	internal ZlibCodec _codec;

	internal int bitk;

	internal int bitb;

	internal int[] hufts;

	internal byte[] window;

	internal int end;

	internal int readAt;

	internal int writeAt;

	internal object checkfn;

	internal uint check;

	internal InfTree inftree = new InfTree();

	internal InflateBlocks(ZlibCodec codec, object checkfn, int windowSize)
	{
		_codec = codec;
		hufts = new int[4320];
		window = new byte[windowSize];
		end = windowSize;
		this.checkfn = checkfn;
		mode = InflateBlockMode.TYPE;
		Reset();
	}

	internal uint Reset()
	{
		uint previousCheck = check;
		mode = InflateBlockMode.TYPE;
		bitk = 0;
		bitb = 0;
		readAt = (writeAt = 0);
		if (checkfn != null)
		{
			_codec._Adler32 = (check = Adler.Adler32(0u, null, 0, 0));
		}
		return previousCheck;
	}

	internal int Process(int result)
	{
		int num = _codec.NextIn;
		int num2 = _codec.AvailableBytesIn;
		int num3 = bitb;
		int i = bitk;
		int num4 = writeAt;
		int num5 = ((num4 >= readAt) ? (end - num4) : (readAt - num4 - 1));
		while (true)
		{
			switch (mode)
			{
			case InflateBlockMode.TYPE:
			{
				for (; i < 3; i += 8)
				{
					if (num2 != 0)
					{
						result = 0;
						num2--;
						num3 |= (_codec.InputBuffer[num++] & 0xFF) << i;
						continue;
					}
					bitb = num3;
					bitk = i;
					_codec.AvailableBytesIn = num2;
					_codec.TotalBytesIn += num - _codec.NextIn;
					_codec.NextIn = num;
					writeAt = num4;
					return Flush(result);
				}
				int num6 = num3 & 7;
				last = num6 & 1;
				switch ((uint)num6 >> 1)
				{
				case 0u:
					num3 >>= 3;
					i -= 3;
					num6 = i & 7;
					num3 >>= num6;
					i -= num6;
					mode = InflateBlockMode.LENS;
					break;
				case 1u:
				{
					int[] array = new int[1];
					int[] array2 = new int[1];
					int[][] array3 = new int[1][];
					int[][] array4 = new int[1][];
					InfTree.inflate_trees_fixed(array, array2, array3, array4, _codec);
					codes.Init(array[0], array2[0], array3[0], 0, array4[0], 0);
					num3 >>= 3;
					i -= 3;
					mode = InflateBlockMode.CODES;
					break;
				}
				case 2u:
					num3 >>= 3;
					i -= 3;
					mode = InflateBlockMode.TABLE;
					break;
				case 3u:
					num3 >>= 3;
					i -= 3;
					mode = InflateBlockMode.BAD;
					_codec.Message = "invalid block type";
					result = -3;
					bitb = num3;
					bitk = i;
					_codec.AvailableBytesIn = num2;
					_codec.TotalBytesIn += num - _codec.NextIn;
					_codec.NextIn = num;
					writeAt = num4;
					return Flush(result);
				}
				break;
			}
			case InflateBlockMode.LENS:
				for (; i < 32; i += 8)
				{
					if (num2 != 0)
					{
						result = 0;
						num2--;
						num3 |= (_codec.InputBuffer[num++] & 0xFF) << i;
						continue;
					}
					bitb = num3;
					bitk = i;
					_codec.AvailableBytesIn = num2;
					_codec.TotalBytesIn += num - _codec.NextIn;
					_codec.NextIn = num;
					writeAt = num4;
					return Flush(result);
				}
				if (((~num3 >> 16) & 0xFFFF) != (num3 & 0xFFFF))
				{
					mode = InflateBlockMode.BAD;
					_codec.Message = "invalid stored block lengths";
					result = -3;
					bitb = num3;
					bitk = i;
					_codec.AvailableBytesIn = num2;
					_codec.TotalBytesIn += num - _codec.NextIn;
					_codec.NextIn = num;
					writeAt = num4;
					return Flush(result);
				}
				left = num3 & 0xFFFF;
				num3 = (i = 0);
				mode = ((left != 0) ? InflateBlockMode.STORED : ((last != 0) ? InflateBlockMode.DRY : InflateBlockMode.TYPE));
				break;
			case InflateBlockMode.STORED:
			{
				if (num2 == 0)
				{
					bitb = num3;
					bitk = i;
					_codec.AvailableBytesIn = num2;
					_codec.TotalBytesIn += num - _codec.NextIn;
					_codec.NextIn = num;
					writeAt = num4;
					return Flush(result);
				}
				if (num5 == 0)
				{
					if (num4 == end && readAt != 0)
					{
						num4 = 0;
						num5 = ((num4 >= readAt) ? (end - num4) : (readAt - num4 - 1));
					}
					if (num5 == 0)
					{
						writeAt = num4;
						result = Flush(result);
						num4 = writeAt;
						num5 = ((num4 >= readAt) ? (end - num4) : (readAt - num4 - 1));
						if (num4 == end && readAt != 0)
						{
							num4 = 0;
							num5 = ((num4 >= readAt) ? (end - num4) : (readAt - num4 - 1));
						}
						if (num5 == 0)
						{
							bitb = num3;
							bitk = i;
							_codec.AvailableBytesIn = num2;
							_codec.TotalBytesIn += num - _codec.NextIn;
							_codec.NextIn = num;
							writeAt = num4;
							return Flush(result);
						}
					}
				}
				result = 0;
				int num6 = left;
				if (num6 > num2)
				{
					num6 = num2;
				}
				if (num6 > num5)
				{
					num6 = num5;
				}
				Array.Copy(_codec.InputBuffer, num, window, num4, num6);
				num += num6;
				num2 -= num6;
				num4 += num6;
				num5 -= num6;
				if ((left -= num6) == 0)
				{
					mode = ((last != 0) ? InflateBlockMode.DRY : InflateBlockMode.TYPE);
				}
				break;
			}
			case InflateBlockMode.TABLE:
			{
				for (; i < 14; i += 8)
				{
					if (num2 != 0)
					{
						result = 0;
						num2--;
						num3 |= (_codec.InputBuffer[num++] & 0xFF) << i;
						continue;
					}
					bitb = num3;
					bitk = i;
					_codec.AvailableBytesIn = num2;
					_codec.TotalBytesIn += num - _codec.NextIn;
					_codec.NextIn = num;
					writeAt = num4;
					return Flush(result);
				}
				int num6 = (table = num3 & 0x3FFF);
				if ((num6 & 0x1F) > 29 || ((num6 >> 5) & 0x1F) > 29)
				{
					mode = InflateBlockMode.BAD;
					_codec.Message = "too many length or distance symbols";
					result = -3;
					bitb = num3;
					bitk = i;
					_codec.AvailableBytesIn = num2;
					_codec.TotalBytesIn += num - _codec.NextIn;
					_codec.NextIn = num;
					writeAt = num4;
					return Flush(result);
				}
				num6 = 258 + (num6 & 0x1F) + ((num6 >> 5) & 0x1F);
				if (blens == null || blens.Length < num6)
				{
					blens = new int[num6];
				}
				else
				{
					Array.Clear(blens, 0, num6);
				}
				num3 >>= 14;
				i -= 14;
				index = 0;
				mode = InflateBlockMode.BTREE;
				goto case InflateBlockMode.BTREE;
			}
			case InflateBlockMode.BTREE:
			{
				while (index < 4 + (table >> 10))
				{
					for (; i < 3; i += 8)
					{
						if (num2 != 0)
						{
							result = 0;
							num2--;
							num3 |= (_codec.InputBuffer[num++] & 0xFF) << i;
							continue;
						}
						bitb = num3;
						bitk = i;
						_codec.AvailableBytesIn = num2;
						_codec.TotalBytesIn += num - _codec.NextIn;
						_codec.NextIn = num;
						writeAt = num4;
						return Flush(result);
					}
					blens[border[index++]] = num3 & 7;
					num3 >>= 3;
					i -= 3;
				}
				while (index < 19)
				{
					blens[border[index++]] = 0;
				}
				bb[0] = 7;
				int num6 = inftree.inflate_trees_bits(blens, bb, tb, hufts, _codec);
				if (num6 != 0)
				{
					result = num6;
					if (result == -3)
					{
						blens = null;
						mode = InflateBlockMode.BAD;
					}
					bitb = num3;
					bitk = i;
					_codec.AvailableBytesIn = num2;
					_codec.TotalBytesIn += num - _codec.NextIn;
					_codec.NextIn = num;
					writeAt = num4;
					return Flush(result);
				}
				index = 0;
				mode = InflateBlockMode.DTREE;
				goto case InflateBlockMode.DTREE;
			}
			case InflateBlockMode.DTREE:
			{
				int num6;
				while (true)
				{
					num6 = table;
					if (index >= 258 + (num6 & 0x1F) + ((num6 >> 5) & 0x1F))
					{
						break;
					}
					for (num6 = bb[0]; i < num6; i += 8)
					{
						if (num2 != 0)
						{
							result = 0;
							num2--;
							num3 |= (_codec.InputBuffer[num++] & 0xFF) << i;
							continue;
						}
						bitb = num3;
						bitk = i;
						_codec.AvailableBytesIn = num2;
						_codec.TotalBytesIn += num - _codec.NextIn;
						_codec.NextIn = num;
						writeAt = num4;
						return Flush(result);
					}
					num6 = hufts[(tb[0] + (num3 & InternalInflateConstants.InflateMask[num6])) * 3 + 1];
					int num7 = hufts[(tb[0] + (num3 & InternalInflateConstants.InflateMask[num6])) * 3 + 2];
					if (num7 < 16)
					{
						num3 >>= num6;
						i -= num6;
						blens[index++] = num7;
						continue;
					}
					int num8 = ((num7 != 18) ? (num7 - 14) : 7);
					int num9 = ((num7 != 18) ? 3 : 11);
					for (; i < num6 + num8; i += 8)
					{
						if (num2 != 0)
						{
							result = 0;
							num2--;
							num3 |= (_codec.InputBuffer[num++] & 0xFF) << i;
							continue;
						}
						bitb = num3;
						bitk = i;
						_codec.AvailableBytesIn = num2;
						_codec.TotalBytesIn += num - _codec.NextIn;
						_codec.NextIn = num;
						writeAt = num4;
						return Flush(result);
					}
					num3 >>= num6;
					i -= num6;
					num9 += num3 & InternalInflateConstants.InflateMask[num8];
					num3 >>= num8;
					i -= num8;
					num8 = index;
					num6 = table;
					if (num8 + num9 > 258 + (num6 & 0x1F) + ((num6 >> 5) & 0x1F) || (num7 == 16 && num8 < 1))
					{
						blens = null;
						mode = InflateBlockMode.BAD;
						_codec.Message = "invalid bit length repeat";
						result = -3;
						bitb = num3;
						bitk = i;
						_codec.AvailableBytesIn = num2;
						_codec.TotalBytesIn += num - _codec.NextIn;
						_codec.NextIn = num;
						writeAt = num4;
						return Flush(result);
					}
					num7 = ((num7 == 16) ? blens[num8 - 1] : 0);
					do
					{
						blens[num8++] = num7;
					}
					while (--num9 != 0);
					index = num8;
				}
				tb[0] = -1;
				int[] array5 = new int[1] { 9 };
				int[] array6 = new int[1] { 6 };
				int[] array7 = new int[1];
				int[] array8 = new int[1];
				num6 = table;
				num6 = inftree.inflate_trees_dynamic(257 + (num6 & 0x1F), 1 + ((num6 >> 5) & 0x1F), blens, array5, array6, array7, array8, hufts, _codec);
				if (num6 != 0)
				{
					if (num6 == -3)
					{
						blens = null;
						mode = InflateBlockMode.BAD;
					}
					result = num6;
					bitb = num3;
					bitk = i;
					_codec.AvailableBytesIn = num2;
					_codec.TotalBytesIn += num - _codec.NextIn;
					_codec.NextIn = num;
					writeAt = num4;
					return Flush(result);
				}
				codes.Init(array5[0], array6[0], hufts, array7[0], hufts, array8[0]);
				mode = InflateBlockMode.CODES;
				goto case InflateBlockMode.CODES;
			}
			case InflateBlockMode.CODES:
				bitb = num3;
				bitk = i;
				_codec.AvailableBytesIn = num2;
				_codec.TotalBytesIn += num - _codec.NextIn;
				_codec.NextIn = num;
				writeAt = num4;
				result = codes.Process(this, result);
				if (result != 1)
				{
					return Flush(result);
				}
				result = 0;
				num = _codec.NextIn;
				num2 = _codec.AvailableBytesIn;
				num3 = bitb;
				i = bitk;
				num4 = writeAt;
				num5 = ((num4 >= readAt) ? (end - num4) : (readAt - num4 - 1));
				if (last == 0)
				{
					mode = InflateBlockMode.TYPE;
					break;
				}
				mode = InflateBlockMode.DRY;
				goto case InflateBlockMode.DRY;
			case InflateBlockMode.DRY:
				writeAt = num4;
				result = Flush(result);
				num4 = writeAt;
				num5 = ((num4 >= readAt) ? (end - num4) : (readAt - num4 - 1));
				if (readAt != writeAt)
				{
					bitb = num3;
					bitk = i;
					_codec.AvailableBytesIn = num2;
					_codec.TotalBytesIn += num - _codec.NextIn;
					_codec.NextIn = num;
					writeAt = num4;
					return Flush(result);
				}
				mode = InflateBlockMode.DONE;
				goto case InflateBlockMode.DONE;
			case InflateBlockMode.DONE:
				result = 1;
				bitb = num3;
				bitk = i;
				_codec.AvailableBytesIn = num2;
				_codec.TotalBytesIn += num - _codec.NextIn;
				_codec.NextIn = num;
				writeAt = num4;
				return Flush(result);
			case InflateBlockMode.BAD:
				result = -3;
				bitb = num3;
				bitk = i;
				_codec.AvailableBytesIn = num2;
				_codec.TotalBytesIn += num - _codec.NextIn;
				_codec.NextIn = num;
				writeAt = num4;
				return Flush(result);
			default:
				result = -2;
				bitb = num3;
				bitk = i;
				_codec.AvailableBytesIn = num2;
				_codec.TotalBytesIn += num - _codec.NextIn;
				_codec.NextIn = num;
				writeAt = num4;
				return Flush(result);
			}
		}
	}

	internal void Free()
	{
		Reset();
		window = null;
		hufts = null;
	}

	internal void SetDictionary(byte[] d, int offset, int length)
	{
		Array.Copy(d, offset, window, 0, length);
		readAt = (writeAt = length);
	}

	internal int SyncPoint()
	{
		return (mode == InflateBlockMode.LENS) ? 1 : 0;
	}

	internal int Flush(int result)
	{
		for (int i = 0; i < 2; i++)
		{
			int num = ((i != 0) ? (writeAt - readAt) : (((readAt > writeAt) ? end : writeAt) - readAt));
			if (num == 0)
			{
				if (result == -5)
				{
					result = 0;
				}
				return result;
			}
			if (num > _codec.AvailableBytesOut)
			{
				num = _codec.AvailableBytesOut;
			}
			if (num != 0 && result == -5)
			{
				result = 0;
			}
			_codec.AvailableBytesOut -= num;
			_codec.TotalBytesOut += num;
			if (checkfn != null)
			{
				_codec._Adler32 = (check = Adler.Adler32(check, window, readAt, num));
			}
			Array.Copy(window, readAt, _codec.OutputBuffer, _codec.NextOut, num);
			_codec.NextOut += num;
			readAt += num;
			if (readAt == end && i == 0)
			{
				readAt = 0;
				if (writeAt == end)
				{
					writeAt = 0;
				}
			}
			else
			{
				i++;
			}
		}
		return result;
	}
}
