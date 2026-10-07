using System;
using System.Diagnostics;

internal class FastEncoderWindow
{
	private byte[] window;

	private int bufPos;

	private int bufEnd;

	private const int FastEncoderHashShift = 4;

	private const int FastEncoderHashtableSize = 2048;

	private const int FastEncoderHashMask = 2047;

	private const int FastEncoderWindowSize = 8192;

	private const int FastEncoderWindowMask = 8191;

	private const int FastEncoderMatch3DistThreshold = 16384;

	internal const int MaxMatch = 258;

	internal const int MinMatch = 3;

	private const int SearchDepth = 32;

	private const int GoodLength = 4;

	private const int NiceLength = 32;

	private const int LazyMatchThreshold = 6;

	private ushort[] prev;

	private ushort[] lookup;

	public int BytesAvailable
	{
		get
		{
			return GetBytesAvailable();
		}
	}

	public DeflateInput UnprocessedInput
	{
		get
		{
			return GetUnprocessedInput();
		}
	}

	public int FreeWindowSpace
	{
		get
		{
			return GetFreeWindowSpace();
		}
	}

	public FastEncoderWindow()
	{
		ResetWindow();
	}

	public int GetBytesAvailable()
	{
		return bufEnd - bufPos;
	}

	public DeflateInput GetUnprocessedInput()
	{
		DeflateInput pGEGNLJIJFE = new DeflateInput();
		pGEGNLJIJFE.set_Buffer(window);
		pGEGNLJIJFE.SetStartIndex(bufPos);
		pGEGNLJIJFE.SetCount(bufEnd - bufPos);
		return pGEGNLJIJFE;
	}

	public void FlushWindow()
	{
		ResetWindow();
	}

	private void ResetWindow()
	{
		window = new byte[16646];
		prev = new ushort[8450];
		lookup = new ushort[2048];
		bufPos = 8192;
		bufEnd = bufPos;
	}

	public int GetFreeWindowSpace()
	{
		return 16384 - bufEnd;
	}

	public void CopyBytes(byte[] MMFIPPNMIKJ, int CAILGDNIKJD, int count)
	{
		Array.Copy(MMFIPPNMIKJ, CAILGDNIKJD, window, bufEnd, count);
		bufEnd += count;
	}

	public void MoveWindows()
	{
		Array.Copy(window, bufPos - 8192, window, 0, 8192);
		for (int i = 0; i < 2048; i++)
		{
			int num = lookup[i] - 8192;
			if (num <= 0)
			{
				lookup[i] = 0;
			}
			else
			{
				lookup[i] = (ushort)num;
			}
		}
		for (int i = 0; i < 8192; i++)
		{
			long num2 = (long)(int)prev[i] - 8192L;
			if (num2 <= 0)
			{
				prev[i] = 0;
			}
			else
			{
				prev[i] = (ushort)num2;
			}
		}
		bufPos = 8192;
		bufEnd = bufPos;
	}

	private uint HashValue(uint HDPBNCNCMOH, byte AAOIAEJJINO)
	{
		return (HDPBNCNCMOH << 4) ^ AAOIAEJJINO;
	}

	private uint InsertString(ref uint HDPBNCNCMOH)
	{
		HDPBNCNCMOH = HashValue(HDPBNCNCMOH, window[bufPos + 2]);
		uint num = lookup[HDPBNCNCMOH & 0x7FF];
		lookup[HDPBNCNCMOH & 0x7FF] = (ushort)bufPos;
		prev[bufPos & 0x1FFF] = (ushort)num;
		return num;
	}

	private void InsertStrings(ref uint HDPBNCNCMOH, int EEPFDKNNGJB)
	{
		if (bufEnd - bufPos <= EEPFDKNNGJB)
		{
			bufPos += EEPFDKNNGJB - 1;
			return;
		}
		while (--EEPFDKNNGJB > 0)
		{
			InsertString(ref HDPBNCNCMOH);
			bufPos++;
		}
	}

	internal bool GetNextSymbolOrMatch(Match MLPEJKLNAKF)
	{
		uint hDPBNCNCMOH = HashValue(0u, window[bufPos]);
		hDPBNCNCMOH = HashValue(hDPBNCNCMOH, window[bufPos + 1]);
		int MIAOKJENHOF = 0;
		int num;
		if (bufEnd - bufPos <= 3)
		{
			num = 0;
		}
		else
		{
			int num2 = (int)InsertString(ref hDPBNCNCMOH);
			if (num2 != 0)
			{
				num = FindMatch(num2, out MIAOKJENHOF, 32, 32);
				if (bufPos + num > bufEnd)
				{
					num = bufEnd - bufPos;
				}
			}
			else
			{
				num = 0;
			}
		}
		if (num < 3)
		{
			MLPEJKLNAKF.set_State(MatchState.HasSymbol);
			MLPEJKLNAKF.set_Symbol(window[bufPos]);
			bufPos++;
		}
		else
		{
			bufPos++;
			if (num <= 6)
			{
				int MIAOKJENHOF2 = 0;
				int num3 = (int)InsertString(ref hDPBNCNCMOH);
				int num4;
				if (num3 != 0)
				{
					num4 = FindMatch(num3, out MIAOKJENHOF2, (num >= 4) ? 8 : 32, 32);
					if (bufPos + num4 > bufEnd)
					{
						num4 = bufEnd - bufPos;
					}
				}
				else
				{
					num4 = 0;
				}
				if (num4 > num)
				{
					MLPEJKLNAKF.set_State(MatchState.HasSymbolAndMatch);
					MLPEJKLNAKF.set_Symbol(window[bufPos - 1]);
					MLPEJKLNAKF.set_Position(MIAOKJENHOF2);
					MLPEJKLNAKF.set_Length(num4);
					bufPos++;
					num = num4;
					InsertStrings(ref hDPBNCNCMOH, num);
				}
				else
				{
					MLPEJKLNAKF.set_State(MatchState.HasMatch);
					MLPEJKLNAKF.set_Position(MIAOKJENHOF);
					MLPEJKLNAKF.set_Length(num);
					num--;
					bufPos++;
					InsertStrings(ref hDPBNCNCMOH, num);
				}
			}
			else
			{
				MLPEJKLNAKF.set_State(MatchState.HasMatch);
				MLPEJKLNAKF.set_Position(MIAOKJENHOF);
				MLPEJKLNAKF.set_Length(num);
				InsertStrings(ref hDPBNCNCMOH, num);
			}
		}
		if (bufPos == 16384)
		{
			MoveWindows();
		}
		return true;
	}

	private int FindMatch(int AFMNFDABMGF, out int MIAOKJENHOF, int KJPBPEOKMBG, int AFEAHPCPHHI)
	{
		int num = 0;
		int num2 = 0;
		int num3 = bufPos - 8192;
		byte b = window[bufPos];
		while (AFMNFDABMGF > num3)
		{
			if (window[AFMNFDABMGF + num] == b)
			{
				int i;
				for (i = 0; i < 258 && window[bufPos + i] == window[AFMNFDABMGF + i]; i++)
				{
				}
				if (i > num)
				{
					num = i;
					num2 = AFMNFDABMGF;
					if (i > 32)
					{
						break;
					}
					b = window[bufPos + i];
				}
			}
			if (--KJPBPEOKMBG == 0)
			{
				break;
			}
			AFMNFDABMGF = prev[AFMNFDABMGF & 0x1FFF];
		}
		MIAOKJENHOF = bufPos - num2 - 1;
		if (num == 3 && MIAOKJENHOF >= 16384)
		{
			return 0;
		}
		return num;
	}

	[Conditional("DEBUG")]
	private void VerifyHashes()
	{
		for (int i = 0; i < 2048; i++)
		{
			ushort num = lookup[i];
			while (num != 0 && bufPos - num < 8192)
			{
				ushort num2 = prev[num & 0x1FFF];
				if (bufPos - num2 >= 8192)
				{
					break;
				}
				num = num2;
			}
		}
	}

	private uint RecalculateHash(int MGMMDGFPBLP)
	{
		return (uint)(((window[MGMMDGFPBLP] << 8) ^ (window[MGMMDGFPBLP + 1] << 4) ^ window[MGMMDGFPBLP + 2]) & 0x7FF);
	}
}
