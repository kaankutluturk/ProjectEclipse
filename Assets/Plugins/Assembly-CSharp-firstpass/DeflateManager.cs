using System;

internal sealed class DeflateManager
{
	internal delegate BlockState CompressFunc(FlushType NGBJDNFAPKC);

	internal class Config
	{
		internal int GoodLength;

		internal int MaxLazy;

		internal int NiceLength;

		internal int MaxChainLength;

		internal DeflateFlavor Flavor;

		private static readonly Config[] Table;

		private Config(int IAOAPAPPECC, int DEHAFDGLEOP, int AFEAHPCPHHI, int ENOGJBAJMCJ, DeflateFlavor CENOEIJNIAG)
		{
			GoodLength = IAOAPAPPECC;
			MaxLazy = DEHAFDGLEOP;
			NiceLength = AFEAHPCPHHI;
			MaxChainLength = ENOGJBAJMCJ;
			Flavor = CENOEIJNIAG;
		}

		static Config()
		{
			Table = new Config[10]
			{
				new Config(0, 0, 0, 0, DeflateFlavor.Store),
				new Config(4, 4, 8, 4, DeflateFlavor.Fast),
				new Config(4, 5, 16, 8, DeflateFlavor.Fast),
				new Config(4, 6, 32, 32, DeflateFlavor.Fast),
				new Config(4, 4, 16, 16, DeflateFlavor.Slow),
				new Config(8, 16, 32, 32, DeflateFlavor.Slow),
				new Config(8, 16, 128, 128, DeflateFlavor.Slow),
				new Config(8, 32, 128, 256, DeflateFlavor.Slow),
				new Config(32, 128, 258, 1024, DeflateFlavor.Slow),
				new Config(32, 258, 258, 4096, DeflateFlavor.Slow)
			};
		}

		public static Config Lookup(ZlibCompressionLevel GNLOCMLBNHF)
		{
			return Table[(int)GNLOCMLBNHF];
		}
	}

	private static readonly int MEM_LEVEL_MAX = 9;

	private static readonly int MEM_LEVEL_DEFAULT = 8;

	private CompressFunc DeflateFunction;

	private static readonly string[] _ErrorMessage = new string[10]
	{
		"need dictionary",
		"stream end",
		string.Empty,
		"file error",
		"stream error",
		"data error",
		"insufficient memory",
		"buffer error",
		"incompatible version",
		string.Empty
	};

	private static readonly int PRESET_DICT = 32;

	private static readonly int INIT_STATE = 42;

	private static readonly int BUSY_STATE = 113;

	private static readonly int FINISH_STATE = 666;

	private static readonly int Z_DEFLATED = 8;

	private static readonly int STORED_BLOCK = 0;

	private static readonly int STATIC_TREES = 1;

	private static readonly int DYN_TREES = 2;

	private static readonly int Z_BINARY = 0;

	private static readonly int Z_ASCII = 1;

	private static readonly int Z_UNKNOWN = 2;

	private static readonly int Buf_size = 16;

	private static readonly int MIN_MATCH = 3;

	private static readonly int MAX_MATCH = 258;

	private static readonly int MIN_LOOKAHEAD = MAX_MATCH + MIN_MATCH + 1;

	private static readonly int HEAP_SIZE = 2 * InternalConstants.L_CODES + 1;

	private static readonly int END_BLOCK = 256;

	internal ZlibCodec _codec;

	internal int status;

	internal byte[] pending;

	internal int nextPending;

	internal int pendingCount;

	internal sbyte data_type;

	internal int last_flush;

	internal int w_size;

	internal int w_bits;

	internal int w_mask;

	internal byte[] window;

	internal int window_size;

	internal short[] prev;

	internal short[] head;

	internal int ins_h;

	internal int hash_size;

	internal int hash_bits;

	internal int hash_mask;

	internal int hash_shift;

	internal int block_start;

	private Config config;

	internal int match_length;

	internal int prev_match;

	internal int match_available;

	internal int strstart;

	internal int match_start;

	internal int lookahead;

	internal int prev_length;

	internal ZlibCompressionLevel compressionLevel;

	internal CompressionStrategy compressionStrategy;

	internal short[] dyn_ltree;

	internal short[] dyn_dtree;

	internal short[] bl_tree;

	internal ZTree treeLiterals = new ZTree();

	internal ZTree treeDistances = new ZTree();

	internal ZTree treeBitLengths = new ZTree();

	internal short[] bl_count = new short[InternalConstants.MAX_BITS + 1];

	internal int[] heap = new int[2 * InternalConstants.L_CODES + 1];

	internal int heap_len;

	internal int heap_max;

	internal sbyte[] depth = new sbyte[2 * InternalConstants.L_CODES + 1];

	internal int _lengthOffset;

	internal int lit_bufsize;

	internal int last_lit;

	internal int _distanceOffset;

	internal int opt_len;

	internal int static_len;

	internal int matches;

	internal int last_eob_len;

	internal short bi_buf;

	internal int bi_valid;

	private bool Rfc1950BytesEmitted;

	private bool _WantRfc1950HeaderBytes = true;

	internal bool WantRfc1950HeaderBytes
	{
		get
		{
			return GetWantRfc1950HeaderBytes();
		}
		set
		{
			SetWantRfc1950HeaderBytes(value);
		}
	}

	internal DeflateManager()
	{
		dyn_ltree = new short[HEAP_SIZE * 2];
		dyn_dtree = new short[(2 * InternalConstants.D_CODES + 1) * 2];
		bl_tree = new short[(2 * InternalConstants.BL_CODES + 1) * 2];
	}

	private void _InitializeLazyMatch()
	{
		window_size = 2 * w_size;
		Array.Clear(head, 0, hash_size);
		config = Config.Lookup(compressionLevel);
		SetDeflater();
		strstart = 0;
		block_start = 0;
		lookahead = 0;
		match_length = (prev_length = MIN_MATCH - 1);
		match_available = 0;
		ins_h = 0;
	}

	private void _InitializeTreeData()
	{
		treeLiterals.dyn_tree = dyn_ltree;
		treeLiterals.staticTree = StaticTree.Literals;
		treeDistances.dyn_tree = dyn_dtree;
		treeDistances.staticTree = StaticTree.Distances;
		treeBitLengths.dyn_tree = bl_tree;
		treeBitLengths.staticTree = StaticTree.BitLengths;
		bi_buf = 0;
		bi_valid = 0;
		last_eob_len = 8;
		_InitializeBlocks();
	}

	internal void _InitializeBlocks()
	{
		for (int i = 0; i < InternalConstants.L_CODES; i++)
		{
			dyn_ltree[i * 2] = 0;
		}
		for (int j = 0; j < InternalConstants.D_CODES; j++)
		{
			dyn_dtree[j * 2] = 0;
		}
		for (int k = 0; k < InternalConstants.BL_CODES; k++)
		{
			bl_tree[k * 2] = 0;
		}
		dyn_ltree[END_BLOCK * 2] = 1;
		opt_len = (static_len = 0);
		last_lit = (matches = 0);
	}

	internal void pqdownheap(short[] EDBPBGAMMDO, int KJBMNAEJIHG)
	{
		int num = heap[KJBMNAEJIHG];
		for (int num2 = KJBMNAEJIHG << 1; num2 <= heap_len; num2 <<= 1)
		{
			if (num2 < heap_len && _IsSmaller(EDBPBGAMMDO, heap[num2 + 1], heap[num2], depth))
			{
				num2++;
			}
			if (_IsSmaller(EDBPBGAMMDO, num, heap[num2], depth))
			{
				break;
			}
			heap[KJBMNAEJIHG] = heap[num2];
			KJBMNAEJIHG = num2;
		}
		heap[KJBMNAEJIHG] = num;
	}

	internal static bool _IsSmaller(short[] EDBPBGAMMDO, int HDKKKCDKFEE, int OFBGCEPCNOL, sbyte[] depth)
	{
		short num = EDBPBGAMMDO[HDKKKCDKFEE * 2];
		short num2 = EDBPBGAMMDO[OFBGCEPCNOL * 2];
		return num < num2 || (num == num2 && depth[HDKKKCDKFEE] <= depth[OFBGCEPCNOL]);
	}

	internal void scan_tree(short[] EDBPBGAMMDO, int max_code)
	{
		int num = -1;
		int num2 = EDBPBGAMMDO[1];
		int num3 = 0;
		int num4 = 7;
		int num5 = 4;
		if (num2 == 0)
		{
			num4 = 138;
			num5 = 3;
		}
		EDBPBGAMMDO[(max_code + 1) * 2 + 1] = short.MaxValue;
		for (int i = 0; i <= max_code; i++)
		{
			int num6 = num2;
			num2 = EDBPBGAMMDO[(i + 1) * 2 + 1];
			if (++num3 < num4 && num6 == num2)
			{
				continue;
			}
			if (num3 < num5)
			{
				bl_tree[num6 * 2] = (short)(bl_tree[num6 * 2] + num3);
			}
			else if (num6 != 0)
			{
				if (num6 != num)
				{
					bl_tree[num6 * 2]++;
				}
				bl_tree[InternalConstants.REP_3_6 * 2]++;
			}
			else if (num3 <= 10)
			{
				bl_tree[InternalConstants.REPZ_3_10 * 2]++;
			}
			else
			{
				bl_tree[InternalConstants.REPZ_11_138 * 2]++;
			}
			num3 = 0;
			num = num6;
			if (num2 == 0)
			{
				num4 = 138;
				num5 = 3;
			}
			else if (num6 == num2)
			{
				num4 = 6;
				num5 = 3;
			}
			else
			{
				num4 = 7;
				num5 = 4;
			}
		}
	}

	internal int build_bl_tree()
	{
		scan_tree(dyn_ltree, treeLiterals.max_code);
		scan_tree(dyn_dtree, treeDistances.max_code);
		treeBitLengths.BuildTree(this);
		int num = InternalConstants.BL_CODES - 1;
		while (num >= 3 && bl_tree[ZTree.BlOrder[num] * 2 + 1] == 0)
		{
			num--;
		}
		opt_len += 3 * (num + 1) + 5 + 5 + 4;
		return num;
	}

	internal void send_all_trees(int EHFOCDNBFHI, int GNCJDCINAFE, int LPLODDOBOJK)
	{
		send_bits(EHFOCDNBFHI - 257, 5);
		send_bits(GNCJDCINAFE - 1, 5);
		send_bits(LPLODDOBOJK - 4, 4);
		for (int i = 0; i < LPLODDOBOJK; i++)
		{
			send_bits(bl_tree[ZTree.BlOrder[i] * 2 + 1], 3);
		}
		send_tree(dyn_ltree, EHFOCDNBFHI - 1);
		send_tree(dyn_dtree, GNCJDCINAFE - 1);
	}

	internal void send_tree(short[] EDBPBGAMMDO, int max_code)
	{
		int num = -1;
		int num2 = EDBPBGAMMDO[1];
		int num3 = 0;
		int num4 = 7;
		int num5 = 4;
		if (num2 == 0)
		{
			num4 = 138;
			num5 = 3;
		}
		for (int i = 0; i <= max_code; i++)
		{
			int num6 = num2;
			num2 = EDBPBGAMMDO[(i + 1) * 2 + 1];
			if (++num3 < num4 && num6 == num2)
			{
				continue;
			}
			if (num3 < num5)
			{
				do
				{
					send_code(num6, bl_tree);
				}
				while (--num3 != 0);
			}
			else if (num6 != 0)
			{
				if (num6 != num)
				{
					send_code(num6, bl_tree);
					num3--;
				}
				send_code(InternalConstants.REP_3_6, bl_tree);
				send_bits(num3 - 3, 2);
			}
			else if (num3 <= 10)
			{
				send_code(InternalConstants.REPZ_3_10, bl_tree);
				send_bits(num3 - 3, 3);
			}
			else
			{
				send_code(InternalConstants.REPZ_11_138, bl_tree);
				send_bits(num3 - 11, 7);
			}
			num3 = 0;
			num = num6;
			if (num2 == 0)
			{
				num4 = 138;
				num5 = 3;
			}
			else if (num6 == num2)
			{
				num4 = 6;
				num5 = 3;
			}
			else
			{
				num4 = 7;
				num5 = 4;
			}
		}
	}

	private void put_bytes(byte[] PIIEECCHMAC, int ILENLCMAMBH, int JCAJDBOMGOM)
	{
		Array.Copy(PIIEECCHMAC, ILENLCMAMBH, pending, pendingCount, JCAJDBOMGOM);
		pendingCount += JCAJDBOMGOM;
	}

	internal void send_code(int ILHDJDNPFKH, short[] EDBPBGAMMDO)
	{
		int num = ILHDJDNPFKH * 2;
		send_bits(EDBPBGAMMDO[num] & 0xFFFF, EDBPBGAMMDO[num + 1] & 0xFFFF);
	}

	internal void send_bits(int value, int BDBOAEGELMC)
	{
		if (bi_valid > Buf_size - BDBOAEGELMC)
		{
			bi_buf |= (short)((value << bi_valid) & 0xFFFF);
			pending[pendingCount++] = (byte)bi_buf;
			pending[pendingCount++] = (byte)(bi_buf >> 8);
			bi_buf = (short)((uint)value >> Buf_size - bi_valid);
			bi_valid += BDBOAEGELMC - Buf_size;
		}
		else
		{
			bi_buf |= (short)((value << bi_valid) & 0xFFFF);
			bi_valid += BDBOAEGELMC;
		}
	}

	internal void _tr_align()
	{
		send_bits(STATIC_TREES << 1, 3);
		send_code(END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
		bi_flush();
		if (1 + last_eob_len + 10 - bi_valid < 9)
		{
			send_bits(STATIC_TREES << 1, 3);
			send_code(END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
			bi_flush();
		}
		last_eob_len = 7;
	}

	internal bool _tr_tally(int CGIBMHPALCO, int LMHBHHENKHG)
	{
		pending[_distanceOffset + last_lit * 2] = (byte)((uint)CGIBMHPALCO >> 8);
		pending[_distanceOffset + last_lit * 2 + 1] = (byte)CGIBMHPALCO;
		pending[_lengthOffset + last_lit] = (byte)LMHBHHENKHG;
		last_lit++;
		if (CGIBMHPALCO == 0)
		{
			dyn_ltree[LMHBHHENKHG * 2]++;
		}
		else
		{
			matches++;
			CGIBMHPALCO--;
			dyn_ltree[(ZTree.LengthCode[LMHBHHENKHG] + InternalConstants.LITERALS + 1) * 2]++;
			dyn_dtree[ZTree.DistanceCode(CGIBMHPALCO) * 2]++;
		}
		if ((last_lit & 0x1FFF) == 0 && compressionLevel > ZlibCompressionLevel.Level2)
		{
			int num = last_lit << 3;
			int num2 = strstart - block_start;
			for (int i = 0; i < InternalConstants.D_CODES; i++)
			{
				num = (int)(num + dyn_dtree[i * 2] * (5L + (long)ZTree.ExtraDistanceBits[i]));
			}
			num >>= 3;
			if (matches < last_lit / 2 && num < num2 / 2)
			{
				return true;
			}
		}
		return last_lit == lit_bufsize - 1 || last_lit == lit_bufsize;
	}

	internal void send_compressed_block(short[] JKLBBEFFMID, short[] JDFLHKFAEOD)
	{
		int num = 0;
		if (last_lit != 0)
		{
			do
			{
				int num2 = _distanceOffset + num * 2;
				int num3 = ((pending[num2] << 8) & 0xFF00) | (pending[num2 + 1] & 0xFF);
				int num4 = pending[_lengthOffset + num] & 0xFF;
				num++;
				if (num3 == 0)
				{
					send_code(num4, JKLBBEFFMID);
					continue;
				}
				int num5 = ZTree.LengthCode[num4];
				send_code(num5 + InternalConstants.LITERALS + 1, JKLBBEFFMID);
				int num6 = ZTree.ExtraLengthBits[num5];
				if (num6 != 0)
				{
					num4 -= ZTree.LengthBase[num5];
					send_bits(num4, num6);
				}
				num3--;
				num5 = ZTree.DistanceCode(num3);
				send_code(num5, JDFLHKFAEOD);
				num6 = ZTree.ExtraDistanceBits[num5];
				if (num6 != 0)
				{
					num3 -= ZTree.DistanceBase[num5];
					send_bits(num3, num6);
				}
			}
			while (num < last_lit);
		}
		send_code(END_BLOCK, JKLBBEFFMID);
		last_eob_len = JKLBBEFFMID[END_BLOCK * 2 + 1];
	}

	internal void set_data_type()
	{
		int i = 0;
		int num = 0;
		int num2 = 0;
		for (; i < 7; i++)
		{
			num2 += dyn_ltree[i * 2];
		}
		for (; i < 128; i++)
		{
			num += dyn_ltree[i * 2];
		}
		for (; i < InternalConstants.LITERALS; i++)
		{
			num2 += dyn_ltree[i * 2];
		}
		data_type = (sbyte)((num2 <= num >> 2) ? Z_ASCII : Z_BINARY);
	}

	internal void bi_flush()
	{
		if (bi_valid == 16)
		{
			pending[pendingCount++] = (byte)bi_buf;
			pending[pendingCount++] = (byte)(bi_buf >> 8);
			bi_buf = 0;
			bi_valid = 0;
		}
		else if (bi_valid >= 8)
		{
			pending[pendingCount++] = (byte)bi_buf;
			bi_buf >>= 8;
			bi_valid -= 8;
		}
	}

	internal void bi_windup()
	{
		if (bi_valid > 8)
		{
			pending[pendingCount++] = (byte)bi_buf;
			pending[pendingCount++] = (byte)(bi_buf >> 8);
		}
		else if (bi_valid > 0)
		{
			pending[pendingCount++] = (byte)bi_buf;
		}
		bi_buf = 0;
		bi_valid = 0;
	}

	internal void copy_block(int HLDLIFPJMOA, int JCAJDBOMGOM, bool HHAAFADDOJB)
	{
		bi_windup();
		last_eob_len = 8;
		if (HHAAFADDOJB)
		{
			pending[pendingCount++] = (byte)JCAJDBOMGOM;
			pending[pendingCount++] = (byte)(JCAJDBOMGOM >> 8);
			pending[pendingCount++] = (byte)(~JCAJDBOMGOM);
			pending[pendingCount++] = (byte)(~JCAJDBOMGOM >> 8);
		}
		put_bytes(window, HLDLIFPJMOA, JCAJDBOMGOM);
	}

	internal void flush_block_only(bool MMCDBIAEFHO)
	{
		_tr_flush_block((block_start < 0) ? (-1) : block_start, strstart - block_start, MMCDBIAEFHO);
		block_start = strstart;
		_codec.FlushPending();
	}

	internal BlockState DeflateNone(FlushType NGBJDNFAPKC)
	{
		int num = 65535;
		if (num > pending.Length - 5)
		{
			num = pending.Length - 5;
		}
		while (true)
		{
			if (lookahead <= 1)
			{
				_fillWindow();
				if (lookahead == 0 && NGBJDNFAPKC == FlushType.None)
				{
					return BlockState.NeedMore;
				}
				if (lookahead == 0)
				{
					break;
				}
			}
			strstart += lookahead;
			lookahead = 0;
			int num2 = block_start + num;
			if (strstart == 0 || strstart >= num2)
			{
				lookahead = strstart - num2;
				strstart = num2;
				flush_block_only(false);
				if (_codec.AvailableBytesOut == 0)
				{
					return BlockState.NeedMore;
				}
			}
			if (strstart - block_start >= w_size - MIN_LOOKAHEAD)
			{
				flush_block_only(false);
				if (_codec.AvailableBytesOut == 0)
				{
					return BlockState.NeedMore;
				}
			}
		}
		flush_block_only(NGBJDNFAPKC == FlushType.Finish);
		if (_codec.AvailableBytesOut == 0)
		{
			return (NGBJDNFAPKC == FlushType.Finish) ? BlockState.FinishStarted : BlockState.NeedMore;
		}
		return (NGBJDNFAPKC != FlushType.Finish) ? BlockState.BlockDone : BlockState.FinishDone;
	}

	internal void _tr_stored_block(int HLDLIFPJMOA, int PNFGGDAMONJ, bool MMCDBIAEFHO)
	{
		send_bits((STORED_BLOCK << 1) + (MMCDBIAEFHO ? 1 : 0), 3);
		copy_block(HLDLIFPJMOA, PNFGGDAMONJ, true);
	}

	internal void _tr_flush_block(int HLDLIFPJMOA, int PNFGGDAMONJ, bool MMCDBIAEFHO)
	{
		int num = 0;
		int num2;
		int num3;
		if (compressionLevel > ZlibCompressionLevel.None)
		{
			if (data_type == Z_UNKNOWN)
			{
				set_data_type();
			}
			treeLiterals.BuildTree(this);
			treeDistances.BuildTree(this);
			num = build_bl_tree();
			num2 = opt_len + 3 + 7 >> 3;
			num3 = static_len + 3 + 7 >> 3;
			if (num3 <= num2)
			{
				num2 = num3;
			}
		}
		else
		{
			num2 = (num3 = PNFGGDAMONJ + 5);
		}
		if (PNFGGDAMONJ + 4 <= num2 && HLDLIFPJMOA != -1)
		{
			_tr_stored_block(HLDLIFPJMOA, PNFGGDAMONJ, MMCDBIAEFHO);
		}
		else if (num3 == num2)
		{
			send_bits((STATIC_TREES << 1) + (MMCDBIAEFHO ? 1 : 0), 3);
			send_compressed_block(StaticTree.lengthAndLiteralsTreeCodes, StaticTree.distTreeCodes);
		}
		else
		{
			send_bits((DYN_TREES << 1) + (MMCDBIAEFHO ? 1 : 0), 3);
			send_all_trees(treeLiterals.max_code + 1, treeDistances.max_code + 1, num + 1);
			send_compressed_block(dyn_ltree, dyn_dtree);
		}
		_InitializeBlocks();
		if (MMCDBIAEFHO)
		{
			bi_windup();
		}
	}

	private void _fillWindow()
	{
		do
		{
			int num = window_size - lookahead - strstart;
			int num2;
			if (num == 0 && strstart == 0 && lookahead == 0)
			{
				num = w_size;
			}
			else if (num == -1)
			{
				num--;
			}
			else if (strstart >= w_size + w_size - MIN_LOOKAHEAD)
			{
				Array.Copy(window, w_size, window, 0, w_size);
				match_start -= w_size;
				strstart -= w_size;
				block_start -= w_size;
				num2 = hash_size;
				int num3 = num2;
				do
				{
					int num4 = head[--num3] & 0xFFFF;
					head[num3] = (short)((num4 >= w_size) ? (num4 - w_size) : 0);
				}
				while (--num2 != 0);
				num2 = w_size;
				num3 = num2;
				do
				{
					int num4 = prev[--num3] & 0xFFFF;
					prev[num3] = (short)((num4 >= w_size) ? (num4 - w_size) : 0);
				}
				while (--num2 != 0);
				num += w_size;
			}
			if (_codec.AvailableBytesIn == 0)
			{
				break;
			}
			num2 = _codec.read_buf(window, strstart + lookahead, num);
			lookahead += num2;
			if (lookahead >= MIN_MATCH)
			{
				ins_h = window[strstart] & 0xFF;
				ins_h = ((ins_h << hash_shift) ^ (window[strstart + 1] & 0xFF)) & hash_mask;
			}
		}
		while (lookahead < MIN_LOOKAHEAD && _codec.AvailableBytesIn != 0);
	}

	internal BlockState DeflateFast(FlushType NGBJDNFAPKC)
	{
		int num = 0;
		while (true)
		{
			if (lookahead < MIN_LOOKAHEAD)
			{
				_fillWindow();
				if (lookahead < MIN_LOOKAHEAD && NGBJDNFAPKC == FlushType.None)
				{
					return BlockState.NeedMore;
				}
				if (lookahead == 0)
				{
					break;
				}
			}
			if (lookahead >= MIN_MATCH)
			{
				ins_h = ((ins_h << hash_shift) ^ (window[strstart + (MIN_MATCH - 1)] & 0xFF)) & hash_mask;
				num = head[ins_h] & 0xFFFF;
				prev[strstart & w_mask] = head[ins_h];
				head[ins_h] = (short)strstart;
			}
			if ((long)num != 0 && ((strstart - num) & 0xFFFF) <= w_size - MIN_LOOKAHEAD && compressionStrategy != CompressionStrategy.HuffmanOnly)
			{
				match_length = longest_match(num);
			}
			bool flag;
			if (match_length >= MIN_MATCH)
			{
				flag = _tr_tally(strstart - match_start, match_length - MIN_MATCH);
				lookahead -= match_length;
				if (match_length <= config.MaxLazy && lookahead >= MIN_MATCH)
				{
					match_length--;
					do
					{
						strstart++;
						ins_h = ((ins_h << hash_shift) ^ (window[strstart + (MIN_MATCH - 1)] & 0xFF)) & hash_mask;
						num = head[ins_h] & 0xFFFF;
						prev[strstart & w_mask] = head[ins_h];
						head[ins_h] = (short)strstart;
					}
					while (--match_length != 0);
					strstart++;
				}
				else
				{
					strstart += match_length;
					match_length = 0;
					ins_h = window[strstart] & 0xFF;
					ins_h = ((ins_h << hash_shift) ^ (window[strstart + 1] & 0xFF)) & hash_mask;
				}
			}
			else
			{
				flag = _tr_tally(0, window[strstart] & 0xFF);
				lookahead--;
				strstart++;
			}
			if (flag)
			{
				flush_block_only(false);
				if (_codec.AvailableBytesOut == 0)
				{
					return BlockState.NeedMore;
				}
			}
		}
		flush_block_only(NGBJDNFAPKC == FlushType.Finish);
		if (_codec.AvailableBytesOut == 0)
		{
			if (NGBJDNFAPKC == FlushType.Finish)
			{
				return BlockState.FinishStarted;
			}
			return BlockState.NeedMore;
		}
		return (NGBJDNFAPKC != FlushType.Finish) ? BlockState.BlockDone : BlockState.FinishDone;
	}

	internal BlockState DeflateSlow(FlushType NGBJDNFAPKC)
	{
		int num = 0;
		while (true)
		{
			if (lookahead < MIN_LOOKAHEAD)
			{
				_fillWindow();
				if (lookahead < MIN_LOOKAHEAD && NGBJDNFAPKC == FlushType.None)
				{
					return BlockState.NeedMore;
				}
				if (lookahead == 0)
				{
					break;
				}
			}
			if (lookahead >= MIN_MATCH)
			{
				ins_h = ((ins_h << hash_shift) ^ (window[strstart + (MIN_MATCH - 1)] & 0xFF)) & hash_mask;
				num = head[ins_h] & 0xFFFF;
				prev[strstart & w_mask] = head[ins_h];
				head[ins_h] = (short)strstart;
			}
			prev_length = match_length;
			prev_match = match_start;
			match_length = MIN_MATCH - 1;
			if (num != 0 && prev_length < config.MaxLazy && ((strstart - num) & 0xFFFF) <= w_size - MIN_LOOKAHEAD)
			{
				if (compressionStrategy != CompressionStrategy.HuffmanOnly)
				{
					match_length = longest_match(num);
				}
				if (match_length <= 5 && (compressionStrategy == CompressionStrategy.Filtered || (match_length == MIN_MATCH && strstart - match_start > 4096)))
				{
					match_length = MIN_MATCH - 1;
				}
			}
			if (prev_length >= MIN_MATCH && match_length <= prev_length)
			{
				int num2 = strstart + lookahead - MIN_MATCH;
				bool flag = _tr_tally(strstart - 1 - prev_match, prev_length - MIN_MATCH);
				lookahead -= prev_length - 1;
				prev_length -= 2;
				do
				{
					if (++strstart <= num2)
					{
						ins_h = ((ins_h << hash_shift) ^ (window[strstart + (MIN_MATCH - 1)] & 0xFF)) & hash_mask;
						num = head[ins_h] & 0xFFFF;
						prev[strstart & w_mask] = head[ins_h];
						head[ins_h] = (short)strstart;
					}
				}
				while (--prev_length != 0);
				match_available = 0;
				match_length = MIN_MATCH - 1;
				strstart++;
				if (flag)
				{
					flush_block_only(false);
					if (_codec.AvailableBytesOut == 0)
					{
						return BlockState.NeedMore;
					}
				}
			}
			else if (match_available != 0)
			{
				if (_tr_tally(0, window[strstart - 1] & 0xFF))
				{
					flush_block_only(false);
				}
				strstart++;
				lookahead--;
				if (_codec.AvailableBytesOut == 0)
				{
					return BlockState.NeedMore;
				}
			}
			else
			{
				match_available = 1;
				strstart++;
				lookahead--;
			}
		}
		if (match_available != 0)
		{
			bool flag = _tr_tally(0, window[strstart - 1] & 0xFF);
			match_available = 0;
		}
		flush_block_only(NGBJDNFAPKC == FlushType.Finish);
		if (_codec.AvailableBytesOut == 0)
		{
			if (NGBJDNFAPKC == FlushType.Finish)
			{
				return BlockState.FinishStarted;
			}
			return BlockState.NeedMore;
		}
		return (NGBJDNFAPKC != FlushType.Finish) ? BlockState.BlockDone : BlockState.FinishDone;
	}

	internal int longest_match(int FGDAGFCDECP)
	{
		int num = config.MaxChainLength;
		int num2 = strstart;
		int num3 = prev_length;
		int num4 = ((strstart > w_size - MIN_LOOKAHEAD) ? (strstart - (w_size - MIN_LOOKAHEAD)) : 0);
		int num5 = config.NiceLength;
		int bHAHBAHNDHM = w_mask;
		int num6 = strstart + MAX_MATCH;
		byte b = window[num2 + num3 - 1];
		byte b2 = window[num2 + num3];
		if (prev_length >= config.GoodLength)
		{
			num >>= 2;
		}
		if (num5 > lookahead)
		{
			num5 = lookahead;
		}
		do
		{
			int num7 = FGDAGFCDECP;
			if (window[num7 + num3] != b2 || window[num7 + num3 - 1] != b || window[num7] != window[num2] || window[++num7] != window[num2 + 1])
			{
				continue;
			}
			num2 += 2;
			num7++;
			while (window[++num2] == window[++num7] && window[++num2] == window[++num7] && window[++num2] == window[++num7] && window[++num2] == window[++num7] && window[++num2] == window[++num7] && window[++num2] == window[++num7] && window[++num2] == window[++num7] && window[++num2] == window[++num7] && num2 < num6)
			{
			}
			int num8 = MAX_MATCH - (num6 - num2);
			num2 = num6 - MAX_MATCH;
			if (num8 > num3)
			{
				match_start = FGDAGFCDECP;
				num3 = num8;
				if (num8 >= num5)
				{
					break;
				}
				b = window[num2 + num3 - 1];
				b2 = window[num2 + num3];
			}
		}
		while ((FGDAGFCDECP = prev[FGDAGFCDECP & bHAHBAHNDHM] & 0xFFFF) > num4 && --num != 0);
		if (num3 <= lookahead)
		{
			return num3;
		}
		return lookahead;
	}

	internal bool GetWantRfc1950HeaderBytes()
	{
		return _WantRfc1950HeaderBytes;
	}

	internal void SetWantRfc1950HeaderBytes(bool value)
	{
		_WantRfc1950HeaderBytes = value;
	}

	internal int Initialize(ZlibCodec HNJFOALABOA, ZlibCompressionLevel GNLOCMLBNHF)
	{
		return Initialize(HNJFOALABOA, GNLOCMLBNHF, 15);
	}

	internal int Initialize(ZlibCodec HNJFOALABOA, ZlibCompressionLevel GNLOCMLBNHF, int HLFOKLCKNEE)
	{
		return Initialize(HNJFOALABOA, GNLOCMLBNHF, HLFOKLCKNEE, MEM_LEVEL_DEFAULT, CompressionStrategy.Default);
	}

	internal int Initialize(ZlibCodec HNJFOALABOA, ZlibCompressionLevel GNLOCMLBNHF, int HLFOKLCKNEE, CompressionStrategy IDOIMLPCFNP)
	{
		return Initialize(HNJFOALABOA, GNLOCMLBNHF, HLFOKLCKNEE, MEM_LEVEL_DEFAULT, IDOIMLPCFNP);
	}

	internal int Initialize(ZlibCodec HNJFOALABOA, ZlibCompressionLevel GNLOCMLBNHF, int KGFELFAKFIA, int GLEJJCGAOMO, CompressionStrategy FNLGJNHJCPL)
	{
		_codec = HNJFOALABOA;
		_codec.Message = null;
		if (KGFELFAKFIA < 9 || KGFELFAKFIA > 15)
		{
			throw new ZlibException("windowBits must be in the range 9..15.");
		}
		if (GLEJJCGAOMO < 1 || GLEJJCGAOMO > MEM_LEVEL_MAX)
		{
			throw new ZlibException(string.Format("memLevel must be in the range 1.. {0}", MEM_LEVEL_MAX));
		}
		_codec.DeflateState = this;
		w_bits = KGFELFAKFIA;
		w_size = 1 << w_bits;
		w_mask = w_size - 1;
		hash_bits = GLEJJCGAOMO + 7;
		hash_size = 1 << hash_bits;
		hash_mask = hash_size - 1;
		hash_shift = (hash_bits + MIN_MATCH - 1) / MIN_MATCH;
		window = new byte[w_size * 2];
		prev = new short[w_size];
		head = new short[hash_size];
		lit_bufsize = 1 << GLEJJCGAOMO + 6;
		pending = new byte[lit_bufsize * 4];
		_distanceOffset = lit_bufsize;
		_lengthOffset = 3 * lit_bufsize;
		compressionLevel = GNLOCMLBNHF;
		compressionStrategy = FNLGJNHJCPL;
		Reset();
		return 0;
	}

	internal void Reset()
	{
		_codec.TotalBytesIn = (_codec.TotalBytesOut = 0L);
		_codec.Message = null;
		pendingCount = 0;
		nextPending = 0;
		Rfc1950BytesEmitted = false;
		status = ((!GetWantRfc1950HeaderBytes()) ? BUSY_STATE : INIT_STATE);
		_codec._Adler32 = Adler.Adler32(0u, null, 0, 0);
		last_flush = 0;
		_InitializeTreeData();
		_InitializeLazyMatch();
	}

	internal int End()
	{
		if (status != INIT_STATE && status != BUSY_STATE && status != FINISH_STATE)
		{
			return -2;
		}
		pending = null;
		head = null;
		prev = null;
		window = null;
		return (status == BUSY_STATE) ? (-3) : 0;
	}

	private void SetDeflater()
	{
		switch (config.Flavor)
		{
		case DeflateFlavor.Store:
			DeflateFunction = DeflateNone;
			break;
		case DeflateFlavor.Fast:
			DeflateFunction = DeflateFast;
			break;
		case DeflateFlavor.Slow:
			DeflateFunction = DeflateSlow;
			break;
		}
	}

	internal int SetParams(ZlibCompressionLevel GNLOCMLBNHF, CompressionStrategy FNLGJNHJCPL)
	{
		int result = 0;
		if (compressionLevel != GNLOCMLBNHF)
		{
			Config cLOGLEGLGGF = Config.Lookup(GNLOCMLBNHF);
			if (cLOGLEGLGGF.Flavor != config.Flavor && _codec.TotalBytesIn != 0)
			{
				result = _codec.Deflate(FlushType.Partial);
			}
			compressionLevel = GNLOCMLBNHF;
			config = cLOGLEGLGGF;
			SetDeflater();
		}
		compressionStrategy = FNLGJNHJCPL;
		return result;
	}

	internal int SetDictionary(byte[] dictionary)
	{
		int num = dictionary.Length;
		int sourceIndex = 0;
		if (dictionary == null || status != INIT_STATE)
		{
			throw new ZlibException("Stream error.");
		}
		_codec._Adler32 = Adler.Adler32(_codec._Adler32, dictionary, 0, dictionary.Length);
		if (num < MIN_MATCH)
		{
			return 0;
		}
		if (num > w_size - MIN_LOOKAHEAD)
		{
			num = w_size - MIN_LOOKAHEAD;
			sourceIndex = dictionary.Length - num;
		}
		Array.Copy(dictionary, sourceIndex, window, 0, num);
		strstart = num;
		block_start = num;
		ins_h = window[0] & 0xFF;
		ins_h = ((ins_h << hash_shift) ^ (window[1] & 0xFF)) & hash_mask;
		for (int i = 0; i <= num - MIN_MATCH; i++)
		{
			ins_h = ((ins_h << hash_shift) ^ (window[i + (MIN_MATCH - 1)] & 0xFF)) & hash_mask;
			prev[i & w_mask] = head[ins_h];
			head[ins_h] = (short)i;
		}
		return 0;
	}

	internal int Deflate(FlushType NGBJDNFAPKC)
	{
		if (_codec.OutputBuffer == null || (_codec.InputBuffer == null && _codec.AvailableBytesIn != 0) || (status == FINISH_STATE && NGBJDNFAPKC != FlushType.Finish))
		{
			_codec.Message = _ErrorMessage[4];
			throw new ZlibException(string.Format("Something is fishy. [{0}]", _codec.Message));
		}
		if (_codec.AvailableBytesOut == 0)
		{
			_codec.Message = _ErrorMessage[7];
			throw new ZlibException("OutputBuffer is full (AvailableBytesOut == 0)");
		}
		int kNACOPCPMJK = last_flush;
		last_flush = (int)NGBJDNFAPKC;
		if (status == INIT_STATE)
		{
			int num = Z_DEFLATED + (w_bits - 8 << 4) << 8;
			int num2 = (int)((compressionLevel - 1) & (ZlibCompressionLevel)0xFF) >> 1;
			if (num2 > 3)
			{
				num2 = 3;
			}
			num |= num2 << 6;
			if (strstart != 0)
			{
				num |= PRESET_DICT;
			}
			num += 31 - num % 31;
			status = BUSY_STATE;
			pending[pendingCount++] = (byte)(num >> 8);
			pending[pendingCount++] = (byte)num;
			if (strstart != 0)
			{
				pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF000000u) >> 24);
				pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF0000) >> 16);
				pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF00) >> 8);
				pending[pendingCount++] = (byte)(_codec._Adler32 & 0xFF);
			}
			_codec._Adler32 = Adler.Adler32(0u, null, 0, 0);
		}
		if (pendingCount != 0)
		{
			_codec.FlushPending();
			if (_codec.AvailableBytesOut == 0)
			{
				last_flush = -1;
				return 0;
			}
		}
		else if (_codec.AvailableBytesIn == 0 && (int)NGBJDNFAPKC <= kNACOPCPMJK && NGBJDNFAPKC != FlushType.Finish)
		{
			return 0;
		}
		if (status == FINISH_STATE && _codec.AvailableBytesIn != 0)
		{
			_codec.Message = _ErrorMessage[7];
			throw new ZlibException("status == FINISH_STATE && _codec.AvailableBytesIn != 0");
		}
		if (_codec.AvailableBytesIn != 0 || lookahead != 0 || (NGBJDNFAPKC != FlushType.None && status != FINISH_STATE))
		{
			BlockState hHLELELECLA = DeflateFunction(NGBJDNFAPKC);
			if (hHLELELECLA == BlockState.FinishStarted || hHLELELECLA == BlockState.FinishDone)
			{
				status = FINISH_STATE;
			}
			switch (hHLELELECLA)
			{
			case BlockState.NeedMore:
			case BlockState.FinishStarted:
				if (_codec.AvailableBytesOut == 0)
				{
					last_flush = -1;
				}
				return 0;
			case BlockState.BlockDone:
				if (NGBJDNFAPKC == FlushType.Partial)
				{
					_tr_align();
				}
				else
				{
					_tr_stored_block(0, 0, false);
					if (NGBJDNFAPKC == FlushType.Full)
					{
						for (int i = 0; i < hash_size; i++)
						{
							head[i] = 0;
						}
					}
				}
				_codec.FlushPending();
				if (_codec.AvailableBytesOut == 0)
				{
					last_flush = -1;
					return 0;
				}
				break;
			}
		}
		if (NGBJDNFAPKC != FlushType.Finish)
		{
			return 0;
		}
		if (!GetWantRfc1950HeaderBytes() || Rfc1950BytesEmitted)
		{
			return 1;
		}
		pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF000000u) >> 24);
		pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF0000) >> 16);
		pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF00) >> 8);
		pending[pendingCount++] = (byte)(_codec._Adler32 & 0xFF);
		_codec.FlushPending();
		Rfc1950BytesEmitted = true;
		return (pendingCount == 0) ? 1 : 0;
	}
}
