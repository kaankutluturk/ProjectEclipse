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

	public void Write(byte byteValue)
	{
		window[end++] = byteValue;
		end &= 32767;
		bytesUsed++;
	}

	public void WriteLengthDistance(int length, int distance)
	{
		bytesUsed += length;
		int num = (end - distance) & 0x7FFF;
		int num2 = 32768 - length;
		if (num <= num2 && end < num2)
		{
			if (length <= distance)
			{
				Array.Copy(window, num, window, end, length);
				end += length;
			}
			else
			{
				while (length-- > 0)
				{
					window[end++] = window[num++];
				}
			}
		}
		else
		{
			while (length-- > 0)
			{
				window[end++] = window[num++];
				end &= 32767;
				num &= 0x7FFF;
			}
		}
	}

	public int CopyFrom(InputBuffer inputBuffer, int length)
	{
		length = Math.Min(Math.Min(length, 32768 - bytesUsed), inputBuffer.GetAvailableBytes());
		int num = 32768 - end;
		int num2;
		if (length > num)
		{
			num2 = inputBuffer.CopyTo(window, end, num);
			if (num2 == num)
			{
				num2 += inputBuffer.CopyTo(window, 0, length - num);
			}
		}
		else
		{
			num2 = inputBuffer.CopyTo(window, end, length);
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

	public int CopyTo(byte[] output, int outputOffset, int length)
	{
		int num;
		if (length > bytesUsed)
		{
			num = end;
			length = bytesUsed;
		}
		else
		{
			num = (end - bytesUsed + length) & 0x7FFF;
		}
		int num2 = length;
		int num3 = length - num;
		if (num3 > 0)
		{
			Array.Copy(window, 32768 - num3, output, outputOffset, num3);
			outputOffset += num3;
			length = num;
		}
		Array.Copy(window, num - length, output, outputOffset, length);
		bytesUsed -= num2;
		return num2;
	}
}
