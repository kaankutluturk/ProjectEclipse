using System;

internal class CopyEncoder
{
	private const int PaddingSize = 5;

	private const int MaxUncompressedBlockSize = 65536;

	public void GetBlock(DeflateInput NILNDHEKNLJ, OutputBuffer output, bool JDHJLBBIKLM)
	{
		int num = 0;
		if (NILNDHEKNLJ != null)
		{
			num = Math.Min(NILNDHEKNLJ.GetCount(), output.GetFreeBytes() - 5 - output.GetBitsInBuffer());
			if (num > 65531)
			{
				num = 65531;
			}
		}
		if (JDHJLBBIKLM)
		{
			output.WriteBits(3, 1u);
		}
		else
		{
			output.WriteBits(3, 0u);
		}
		output.FlushBits();
		WriteLenNLen((ushort)num, output);
		if (NILNDHEKNLJ != null && num > 0)
		{
			output.WriteBytes(NILNDHEKNLJ.GetBuffer(), NILNDHEKNLJ.GetStartIndex(), num);
			NILNDHEKNLJ.ConsumeBytes(num);
		}
	}

	private void WriteLenNLen(ushort JCAJDBOMGOM, OutputBuffer output)
	{
		output.WriteUInt16(JCAJDBOMGOM);
		ushort bAINMLLIKOL = (ushort)(~JCAJDBOMGOM);
		output.WriteUInt16(bAINMLLIKOL);
	}
}
