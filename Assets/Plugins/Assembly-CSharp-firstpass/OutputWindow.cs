using System;

internal class OutputWindow
{
	private const int WindowSize = 32768;

	private const int WindowMask = 32767;

	private byte[] window = new byte[32768];

	private int end;

	private int bytesUsed;

	public int FreeBytes
	{
		get
		{
			return GetFreeBytes();
		}
	}

	public int AvailableBytes
	{
		get
		{
			return GetAvailableBytes();
		}
	}

	public void Write(byte AAOIAEJJINO)
	{
		window[end++] = AAOIAEJJINO;
		end &= 32767;
		bytesUsed++;
	}

	public void WriteLengthDistance(int BDBOAEGELMC, int OIOMNNFMDOO)
	{
		bytesUsed += BDBOAEGELMC;
		int num = (end - OIOMNNFMDOO) & 0x7FFF;
		int num2 = 32768 - BDBOAEGELMC;
		if (num <= num2 && end < num2)
		{
			if (BDBOAEGELMC <= OIOMNNFMDOO)
			{
				Array.Copy(window, num, window, end, BDBOAEGELMC);
				end += BDBOAEGELMC;
			}
			else
			{
				while (BDBOAEGELMC-- > 0)
				{
					window[end++] = window[num++];
				}
			}
		}
		else
		{
			while (BDBOAEGELMC-- > 0)
			{
				window[end++] = window[num++];
				end &= 32767;
				num &= 0x7FFF;
			}
		}
	}

	public int CopyFrom(InputBuffer NILNDHEKNLJ, int BDBOAEGELMC)
	{
		BDBOAEGELMC = Math.Min(Math.Min(BDBOAEGELMC, 32768 - bytesUsed), NILNDHEKNLJ.GetAvailableBytes());
		int num = 32768 - end;
		int num2;
		if (BDBOAEGELMC > num)
		{
			num2 = NILNDHEKNLJ.CopyTo(window, end, num);
			if (num2 == num)
			{
				num2 += NILNDHEKNLJ.CopyTo(window, 0, BDBOAEGELMC - num);
			}
		}
		else
		{
			num2 = NILNDHEKNLJ.CopyTo(window, end, BDBOAEGELMC);
		}
		end = (end + num2) & 0x7FFF;
		bytesUsed += num2;
		return num2;
	}

	public int GetFreeBytes()
	{
		return 32768 - bytesUsed;
	}

	public int GetAvailableBytes()
	{
		return bytesUsed;
	}

	public int CopyTo(byte[] output, int IPCOBJBKNAO, int BDBOAEGELMC)
	{
		int num;
		if (BDBOAEGELMC > bytesUsed)
		{
			num = end;
			BDBOAEGELMC = bytesUsed;
		}
		else
		{
			num = (end - bytesUsed + BDBOAEGELMC) & 0x7FFF;
		}
		int num2 = BDBOAEGELMC;
		int num3 = BDBOAEGELMC - num;
		if (num3 > 0)
		{
			Array.Copy(window, 32768 - num3, output, IPCOBJBKNAO, num3);
			IPCOBJBKNAO += num3;
			BDBOAEGELMC = num;
		}
		Array.Copy(window, num - BDBOAEGELMC, output, IPCOBJBKNAO, BDBOAEGELMC);
		bytesUsed -= num2;
		return num2;
	}
}
