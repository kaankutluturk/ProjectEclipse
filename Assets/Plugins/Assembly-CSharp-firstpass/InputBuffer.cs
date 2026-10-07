using System;

internal class InputBuffer
{
	private byte[] buffer;

	private int start;

	private int end;

	private uint bitBuffer;

	private int bitsInBuffer;

	public int AvailableBits
	{
		get
		{
			return GetAvailableBits();
		}
	}

	public int AvailableBytes
	{
		get
		{
			return GetAvailableBytes();
		}
	}

	public int GetAvailableBits()
	{
		return bitsInBuffer;
	}

	public int GetAvailableBytes()
	{
		return end - start + bitsInBuffer / 8;
	}

	public bool EnsureBitsAvailable(int count)
	{
		if (bitsInBuffer < count)
		{
			if (NeedsInput())
			{
				return false;
			}
			bitBuffer |= (uint)(buffer[start++] << bitsInBuffer);
			bitsInBuffer += 8;
			if (bitsInBuffer < count)
			{
				if (NeedsInput())
				{
					return false;
				}
				bitBuffer |= (uint)(buffer[start++] << bitsInBuffer);
				bitsInBuffer += 8;
			}
		}
		return true;
	}

	public uint TryLoad16Bits()
	{
		if (bitsInBuffer < 8)
		{
			if (start < end)
			{
				bitBuffer |= (uint)(buffer[start++] << bitsInBuffer);
				bitsInBuffer += 8;
			}
			if (start < end)
			{
				bitBuffer |= (uint)(buffer[start++] << bitsInBuffer);
				bitsInBuffer += 8;
			}
		}
		else if (bitsInBuffer < 16 && start < end)
		{
			bitBuffer |= (uint)(buffer[start++] << bitsInBuffer);
			bitsInBuffer += 8;
		}
		return bitBuffer;
	}

	private uint GetBitMask(int count)
	{
		return (uint)((1 << count) - 1);
	}

	public int GetBits(int count)
	{
		if (!EnsureBitsAvailable(count))
		{
			return -1;
		}
		int result = (int)(bitBuffer & GetBitMask(count));
		bitBuffer >>= count;
		bitsInBuffer -= count;
		return result;
	}

	public int CopyTo(byte[] output, int IPCOBJBKNAO, int BDBOAEGELMC)
	{
		int num = 0;
		while (bitsInBuffer > 0 && BDBOAEGELMC > 0)
		{
			output[IPCOBJBKNAO++] = (byte)bitBuffer;
			bitBuffer >>= 8;
			bitsInBuffer -= 8;
			BDBOAEGELMC--;
			num++;
		}
		if (BDBOAEGELMC == 0)
		{
			return num;
		}
		int num2 = end - start;
		if (BDBOAEGELMC > num2)
		{
			BDBOAEGELMC = num2;
		}
		Array.Copy(buffer, start, output, IPCOBJBKNAO, BDBOAEGELMC);
		start += BDBOAEGELMC;
		return num + BDBOAEGELMC;
	}

	public bool NeedsInput()
	{
		return start == end;
	}

	public void SetInput(byte[] buffer, int IPCOBJBKNAO, int BDBOAEGELMC)
	{
		this.buffer = buffer;
		start = IPCOBJBKNAO;
		end = IPCOBJBKNAO + BDBOAEGELMC;
	}

	public void SkipBits(int HDKKKCDKFEE)
	{
		bitBuffer >>= HDKKKCDKFEE;
		bitsInBuffer -= HDKKKCDKFEE;
	}

	public void SkipToByteBoundary()
	{
		bitBuffer >>= bitsInBuffer % 8;
		bitsInBuffer -= bitsInBuffer % 8;
	}
}
