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

	internal void WriteBits(int count, uint bits)
	{
		bitBuf |= bits << bitCount;
		bitCount += count;
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

	internal void WriteBytes(byte[] buffer, int offset, int count)
	{
		if (bitCount == 0)
		{
			Array.Copy(buffer, offset, byteBuffer, pos, count);
			pos += count;
		}
		else
		{
			WriteBytesUnaligned(buffer, offset, count);
		}
	}

	private void WriteBytesUnaligned(byte[] buffer, int offset, int count)
	{
		for (int i = 0; i < count; i++)
		{
			byte byteValue = buffer[offset + i];
			WriteByteUnaligned(byteValue);
		}
	}

	private void WriteByteUnaligned(byte byteValue)
	{
		WriteBits(8, byteValue);
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
