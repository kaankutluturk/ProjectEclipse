using System;

internal class CopyEncoder
{
	private const int PaddingSize = 5;

	private const int MaxUncompressedBlockSize = 65536;

	public void GetBlock(DeflateInput input, OutputBuffer output, bool isFinal)
	{
		int num = 0;
		if (input != null)
		{
			num = Math.Min(input.GetCount(), output.GetFreeBytes() - 5 - output.GetBitsInBuffer());
			if (num > 65531)
			{
				num = 65531;
			}
		}
		if (isFinal)
		{
			output.WriteBits(3, 1u);
		}
		else
		{
			output.WriteBits(3, 0u);
		}
		output.FlushBits();
		WriteLenNLen((ushort)num, output);
		if (input != null && num > 0)
		{
			output.WriteBytes(input.GetBuffer(), input.GetStartIndex(), num);
			input.ConsumeBytes(num);
		}
	}

	private void WriteLenNLen(ushort length, OutputBuffer output)
	{
		output.WriteUInt16(length);
		ushort notLength = (ushort)(~length);
		output.WriteUInt16(notLength);
	}
}
