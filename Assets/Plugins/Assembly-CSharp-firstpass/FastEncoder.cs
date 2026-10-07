using System;

internal class FastEncoder
{
	private FastEncoderWindow inputWindow;

	private Match currentMatch;

	private double lastCompressionRatio;

	internal int BytesInHistory
	{
		get
		{
			return GetBytesInHistory();
		}
	}

	internal DeflateInput UnprocessedInput
	{
		get
		{
			return GetUnprocessedInput();
		}
	}

	internal double LastCompressionRatio
	{
		get
		{
			return GetLastCompressionRatio();
		}
	}

	public FastEncoder()
	{
		inputWindow = new FastEncoderWindow();
		currentMatch = new Match();
	}

	internal int GetBytesInHistory()
	{
		return inputWindow.GetBytesAvailable();
	}

	internal DeflateInput GetUnprocessedInput()
	{
		return inputWindow.GetUnprocessedInput();
	}

	internal void FlushInput()
	{
		inputWindow.FlushWindow();
	}

	internal double GetLastCompressionRatio()
	{
		return lastCompressionRatio;
	}

	internal void GetBlock(DeflateInput input, OutputBuffer output, int maxBytesToCopy)
	{
		WriteDeflatePreamble(output);
		GetCompressedOutput(input, output, maxBytesToCopy);
		WriteEndOfBlock(output);
	}

	internal void GetCompressedData(DeflateInput input, OutputBuffer output)
	{
		GetCompressedOutput(input, output, -1);
	}

	internal void GetBlockHeader(OutputBuffer output)
	{
		WriteDeflatePreamble(output);
	}

	internal void GetBlockFooter(OutputBuffer output)
	{
		WriteEndOfBlock(output);
	}

	private void GetCompressedOutput(DeflateInput input, OutputBuffer output, int maxBytesToCopy)
	{
		int num = output.GetBytesWritten();
		int num2 = 0;
		int num3 = GetBytesInHistory() + input.GetCount();
		do
		{
			int num4 = ((input.GetCount() >= inputWindow.GetFreeWindowSpace()) ? inputWindow.GetFreeWindowSpace() : input.GetCount());
			if (maxBytesToCopy >= 1)
			{
				num4 = Math.Min(num4, maxBytesToCopy - num2);
			}
			if (num4 > 0)
			{
				inputWindow.CopyBytes(input.GetBuffer(), input.GetStartIndex(), num4);
				input.ConsumeBytes(num4);
				num2 += num4;
			}
			GetCompressedOutput(output);
		}
		while (SafeToWriteTo(output) && InputAvailable(input) && (maxBytesToCopy < 1 || num2 < maxBytesToCopy));
		int num5 = output.GetBytesWritten();
		int num6 = num5 - num;
		int num7 = GetBytesInHistory() + input.GetCount();
		int num8 = num3 - num7;
		if (num6 != 0)
		{
			lastCompressionRatio = (double)num6 / (double)num8;
		}
	}

	private void GetCompressedOutput(OutputBuffer output)
	{
		while (inputWindow.GetBytesAvailable() > 0 && SafeToWriteTo(output))
		{
			inputWindow.GetNextSymbolOrMatch(currentMatch);
			if (currentMatch.GetState() == MatchState.HasSymbol)
			{
				WriteChar(currentMatch.GetSymbol(), output);
				continue;
			}
			if (currentMatch.GetState() == MatchState.HasMatch)
			{
				WriteMatch(currentMatch.GetLength(), currentMatch.GetPosition(), output);
				continue;
			}
			WriteChar(currentMatch.GetSymbol(), output);
			WriteMatch(currentMatch.GetLength(), currentMatch.GetPosition(), output);
		}
	}

	private bool InputAvailable(DeflateInput input)
	{
		return input.GetCount() > 0 || GetBytesInHistory() > 0;
	}

	private bool SafeToWriteTo(OutputBuffer output)
	{
		return output.GetFreeBytes() > 16;
	}

	private void WriteEndOfBlock(OutputBuffer output)
	{
		uint num = FastEncoderStatics.FastEncoderLiteralCodeInfo[256];
		int codeLength = (int)(num & 0x1F);
		output.WriteBits(codeLength, num >> 5);
	}

	internal static void WriteMatch(int length, int distance, OutputBuffer output)
	{
		uint num = FastEncoderStatics.FastEncoderLiteralCodeInfo[254 + length];
		int num2 = (int)(num & 0x1F);
		if (num2 <= 16)
		{
			output.WriteBits(num2, num >> 5);
		}
		else
		{
			output.WriteBits(16, (num >> 5) & 0xFFFF);
			output.WriteBits(num2 - 16, num >> 21);
		}
		num = FastEncoderStatics.FastEncoderDistanceCodeInfo[FastEncoderStatics.GetSlot(distance)];
		output.WriteBits((int)(num & 0xF), num >> 8);
		int num3 = (int)((num >> 4) & 0xF);
		if (num3 != 0)
		{
			output.WriteBits(num3, (uint)distance & FastEncoderStatics.BitMask[num3]);
		}
	}

	internal static void WriteChar(byte symbol, OutputBuffer output)
	{
		uint num = FastEncoderStatics.FastEncoderLiteralCodeInfo[symbol];
		output.WriteBits((int)(num & 0x1F), num >> 5);
	}

	internal static void WriteDeflatePreamble(OutputBuffer output)
	{
		output.WriteBytes(FastEncoderStatics.FastEncoderTreeStructureData, 0, FastEncoderStatics.FastEncoderTreeStructureData.Length);
		output.WriteBits(9, 34u);
	}
}
