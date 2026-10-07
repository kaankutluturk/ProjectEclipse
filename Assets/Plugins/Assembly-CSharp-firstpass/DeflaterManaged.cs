using System;

internal class DeflaterManaged : IDisposable, IDeflater
{
	private enum DeflaterState
	{
		NotStarted = 0,
		SlowDownForIncompressible1 = 1,
		SlowDownForIncompressible2 = 2,
		StartingSmallData = 3,
		CompressThenCheck = 4,
		CheckingForIncompressible = 5,
		HandlingSmallData = 6
	}

	private const int MinBlockSize = 256;

	private const int MaxHeaderFooterGoo = 120;

	private const int CleanCopySize = 8072;

	private const double BadCompressionThreshold = 1.0;

	private FastEncoder deflateEncoder;

	private CopyEncoder copyEncoder;

	private DeflateInput input;

	private OutputBuffer output;

	private DeflaterState processingState;

	private DeflateInput inputFromHistory;

	internal DeflaterManaged()
	{
		deflateEncoder = new FastEncoder();
		copyEncoder = new CopyEncoder();
		input = new DeflateInput();
		output = new OutputBuffer();
		processingState = DeflaterState.NotStarted;
	}

	private bool NeedsInput()
	{
		return ((IDeflater)this).NeedsInput();
	}

	bool IDeflater.NeedsInput()
	{
		return input.GetCount() == 0 && deflateEncoder.GetBytesInHistory() == 0;
	}

	void IDeflater.SetInput(byte[] buffer, int startIndex, int count)
	{
		input.set_Buffer(buffer);
		input.SetCount(count);
		input.SetStartIndex(startIndex);
		if (count > 0 && count < 256)
		{
			switch (processingState)
			{
			case DeflaterState.NotStarted:
			case DeflaterState.CheckingForIncompressible:
				processingState = DeflaterState.StartingSmallData;
				break;
			case DeflaterState.CompressThenCheck:
				processingState = DeflaterState.HandlingSmallData;
				break;
			}
		}
	}

	int IDeflater.GetDeflateOutput(byte[] outputBuffer)
	{
		output.UpdateBuffer(outputBuffer);
		switch (processingState)
		{
		case DeflaterState.NotStarted:
		{
			DeflateInput.InputState initialInputState = input.DumpState();
			OutputBuffer.BufferState initialOutputState = output.DumpState();
			deflateEncoder.GetBlockHeader(output);
			deflateEncoder.GetCompressedData(input, output);
			if (!UseCompressed(deflateEncoder.GetLastCompressionRatio()))
			{
				input.RestoreState(initialInputState);
				output.RestoreState(initialOutputState);
				copyEncoder.GetBlock(input, output, false);
				FlushInputWindows();
				processingState = DeflaterState.CheckingForIncompressible;
			}
			else
			{
				processingState = DeflaterState.CompressThenCheck;
			}
			break;
		}
		case DeflaterState.CompressThenCheck:
			deflateEncoder.GetCompressedData(input, output);
			if (!UseCompressed(deflateEncoder.GetLastCompressionRatio()))
			{
				processingState = DeflaterState.SlowDownForIncompressible1;
				inputFromHistory = deflateEncoder.GetUnprocessedInput();
			}
			break;
		case DeflaterState.SlowDownForIncompressible1:
			deflateEncoder.GetBlockFooter(output);
			processingState = DeflaterState.SlowDownForIncompressible2;
			goto case DeflaterState.SlowDownForIncompressible2;
		case DeflaterState.SlowDownForIncompressible2:
			if (inputFromHistory.GetCount() > 0)
			{
				copyEncoder.GetBlock(inputFromHistory, output, false);
			}
			if (inputFromHistory.GetCount() == 0)
			{
				deflateEncoder.FlushInput();
				processingState = DeflaterState.CheckingForIncompressible;
			}
			break;
		case DeflaterState.CheckingForIncompressible:
		{
			DeflateInput.InputState inputState = input.DumpState();
			OutputBuffer.BufferState outputState = output.DumpState();
			deflateEncoder.GetBlock(input, output, 8072);
			if (!UseCompressed(deflateEncoder.GetLastCompressionRatio()))
			{
				input.RestoreState(inputState);
				output.RestoreState(outputState);
				copyEncoder.GetBlock(input, output, false);
				FlushInputWindows();
			}
			break;
		}
		case DeflaterState.StartingSmallData:
			deflateEncoder.GetBlockHeader(output);
			processingState = DeflaterState.HandlingSmallData;
			goto case DeflaterState.HandlingSmallData;
		case DeflaterState.HandlingSmallData:
			deflateEncoder.GetCompressedData(input, output);
			break;
		}
		return output.GetBytesWritten();
	}

	bool IDeflater.Finish(byte[] outputBuffer, out int bytesRead)
	{
		if (processingState == DeflaterState.NotStarted)
		{
			bytesRead = 0;
			return true;
		}
		output.UpdateBuffer(outputBuffer);
		if (processingState == DeflaterState.CompressThenCheck || processingState == DeflaterState.HandlingSmallData || processingState == DeflaterState.SlowDownForIncompressible1)
		{
			deflateEncoder.GetBlockFooter(output);
		}
		WriteFinal();
		bytesRead = output.GetBytesWritten();
		return true;
	}

	void IDisposable.Dispose()
	{
	}

	protected void Dispose(bool disposing)
	{
	}

	private bool UseCompressed(double ratio)
	{
		return ratio <= 1.0;
	}

	private void FlushInputWindows()
	{
		deflateEncoder.FlushInput();
	}

	private void WriteFinal()
	{
		copyEncoder.GetBlock(null, output, true);
	}
}
