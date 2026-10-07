using System;

internal class OutputBuffer
{
	internal struct BufferState
	{
		internal int pos;

		internal uint bitBuf;

		internal int bitCount;
	}

	private byte[] byteBuffer;

	private int pos;

	private uint bitBuf;

	private int bitCount;

	internal int BytesWritten
	{
		get
		{
			return GetBytesWritten();
		}
	}

	internal int FreeBytes
	{
		get
		{
			return GetFreeBytes();
		}
	}

	internal int BitsInBuffer
	{
		get
		{
			return GetBitsInBuffer();
		}
	}

	internal void UpdateBuffer(byte[] output)
	{
		byteBuffer = output;
		pos = 0;
	}

	internal int GetBytesWritten()
	{
		return pos;
	}

	internal int GetFreeBytes()
	{
		return byteBuffer.Length - pos;
	}

	internal void WriteUInt16(ushort value)
	{
		byteBuffer[pos++] = (byte)value;
		byteBuffer[pos++] = (byte)(value >> 8);
	}

	internal void WriteBits(int HDKKKCDKFEE, uint HLFOKLCKNEE)
	{
		bitBuf |= HLFOKLCKNEE << bitCount;
		bitCount += HDKKKCDKFEE;
		if (bitCount >= 16)
		{
			byteBuffer[pos++] = (byte)bitBuf;
			byteBuffer[pos++] = (byte)(bitBuf >> 8);
			bitCount -= 16;
			bitBuf >>= 16;
		}
	}

	internal void FlushBits()
	{
		while (bitCount >= 8)
		{
			byteBuffer[pos++] = (byte)bitBuf;
			bitCount -= 8;
			bitBuf >>= 8;
		}
		if (bitCount > 0)
		{
			byteBuffer[pos++] = (byte)bitBuf;
			bitBuf = 0u;
			bitCount = 0;
		}
	}

	internal void WriteBytes(byte[] HFADMOEOHFA, int IPCOBJBKNAO, int count)
	{
		if (bitCount == 0)
		{
			Array.Copy(HFADMOEOHFA, IPCOBJBKNAO, byteBuffer, pos, count);
			pos += count;
		}
		else
		{
			WriteBytesUnaligned(HFADMOEOHFA, IPCOBJBKNAO, count);
		}
	}

	private void WriteBytesUnaligned(byte[] HFADMOEOHFA, int IPCOBJBKNAO, int count)
	{
		for (int i = 0; i < count; i++)
		{
			byte aAOIAEJJINO = HFADMOEOHFA[IPCOBJBKNAO + i];
			WriteByteUnaligned(aAOIAEJJINO);
		}
	}

	private void WriteByteUnaligned(byte AAOIAEJJINO)
	{
		WriteBits(8, AAOIAEJJINO);
	}

	internal int GetBitsInBuffer()
	{
		return bitCount / 8 + 1;
	}

	internal BufferState DumpState()
	{
		BufferState result = default(BufferState);
		result.pos = pos;
		result.bitBuf = bitBuf;
		result.bitCount = bitCount;
		return result;
	}

	internal void RestoreState(BufferState state)
	{
		pos = state.pos;
		bitBuf = state.bitBuf;
		bitCount = state.bitCount;
	}
}
