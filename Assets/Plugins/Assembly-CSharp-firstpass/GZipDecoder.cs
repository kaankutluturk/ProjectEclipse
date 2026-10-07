using System;
using Unity.IO.Compression;

internal class GZipDecoder : IFileFormatReader
{
	internal enum GzipHeaderState
	{
		ReadingID1 = 0,
		ReadingID2 = 1,
		ReadingCM = 2,
		ReadingFLG = 3,
		ReadingMMTime = 4,
		ReadingXFL = 5,
		ReadingOS = 6,
		ReadingXLen1 = 7,
		ReadingXLen2 = 8,
		ReadingXLenData = 9,
		ReadingFileName = 10,
		ReadingComment = 11,
		ReadingCRC16Part1 = 12,
		ReadingCRC16Part2 = 13,
		Done = 14,
		ReadingCRC = 15,
		ReadingFileSize = 16
	}

	[Flags]
	internal enum GZipOptionalHeaderFlags
	{
		CRCFlag = 2,
		ExtraFieldsFlag = 4,
		FileNameFlag = 8,
		CommentFlag = 0x10
	}

	private GzipHeaderState gzipHeaderSubstate;

	private GzipHeaderState gzipFooterSubstate;

	private int gzip_header_flag;

	private int gzip_header_xlen;

	private uint expectedCrc32;

	private uint expectedOutputStreamSizeModulo;

	private int loopCounter;

	private uint actualCrc32;

	private long actualStreamSizeModulo;

	public GZipDecoder()
	{
		Reset();
	}

	public void Reset()
	{
		gzipHeaderSubstate = GzipHeaderState.ReadingID1;
		gzipFooterSubstate = GzipHeaderState.ReadingCRC;
		expectedCrc32 = 0u;
		expectedOutputStreamSizeModulo = 0u;
	}

	public bool ReadHeader(InputBuffer NILNDHEKNLJ)
	{
		switch (gzipHeaderSubstate)
		{
		case GzipHeaderState.ReadingID1:
		{
			int num = NILNDHEKNLJ.GetBits(8);
			if (num < 0)
			{
				return false;
			}
			if (num != 31)
			{
				throw new InvalidDataException(SR.GetString("Corrupted gzip header"));
			}
			gzipHeaderSubstate = GzipHeaderState.ReadingID2;
			goto case GzipHeaderState.ReadingID2;
		}
		case GzipHeaderState.ReadingID2:
		{
			int num = NILNDHEKNLJ.GetBits(8);
			if (num < 0)
			{
				return false;
			}
			if (num != 139)
			{
				throw new InvalidDataException(SR.GetString("Corrupted gzip header"));
			}
			gzipHeaderSubstate = GzipHeaderState.ReadingCM;
			goto case GzipHeaderState.ReadingCM;
		}
		case GzipHeaderState.ReadingCM:
		{
			int num = NILNDHEKNLJ.GetBits(8);
			if (num < 0)
			{
				return false;
			}
			if (num != 8)
			{
				throw new InvalidDataException(SR.GetString("Unknown compression mode"));
			}
			gzipHeaderSubstate = GzipHeaderState.ReadingFLG;
			goto case GzipHeaderState.ReadingFLG;
		}
		case GzipHeaderState.ReadingFLG:
		{
			int num = NILNDHEKNLJ.GetBits(8);
			if (num < 0)
			{
				return false;
			}
			gzip_header_flag = num;
			gzipHeaderSubstate = GzipHeaderState.ReadingMMTime;
			loopCounter = 0;
			goto case GzipHeaderState.ReadingMMTime;
		}
		case GzipHeaderState.ReadingMMTime:
		{
			int num = 0;
			while (loopCounter < 4)
			{
				num = NILNDHEKNLJ.GetBits(8);
				if (num < 0)
				{
					return false;
				}
				loopCounter++;
			}
			gzipHeaderSubstate = GzipHeaderState.ReadingXFL;
			loopCounter = 0;
			goto case GzipHeaderState.ReadingXFL;
		}
		case GzipHeaderState.ReadingXFL:
		{
			int num = NILNDHEKNLJ.GetBits(8);
			if (num < 0)
			{
				return false;
			}
			gzipHeaderSubstate = GzipHeaderState.ReadingOS;
			goto case GzipHeaderState.ReadingOS;
		}
		case GzipHeaderState.ReadingOS:
		{
			int num = NILNDHEKNLJ.GetBits(8);
			if (num < 0)
			{
				return false;
			}
			gzipHeaderSubstate = GzipHeaderState.ReadingXLen1;
			goto case GzipHeaderState.ReadingXLen1;
		}
		case GzipHeaderState.ReadingXLen1:
		{
			if ((gzip_header_flag & 4) == 0)
			{
				goto case GzipHeaderState.ReadingFileName;
			}
			int num = NILNDHEKNLJ.GetBits(8);
			if (num < 0)
			{
				return false;
			}
			gzip_header_xlen = num;
			gzipHeaderSubstate = GzipHeaderState.ReadingXLen2;
			goto case GzipHeaderState.ReadingXLen2;
		}
		case GzipHeaderState.ReadingXLen2:
		{
			int num = NILNDHEKNLJ.GetBits(8);
			if (num < 0)
			{
				return false;
			}
			gzip_header_xlen |= num << 8;
			gzipHeaderSubstate = GzipHeaderState.ReadingXLenData;
			loopCounter = 0;
			goto case GzipHeaderState.ReadingXLenData;
		}
		case GzipHeaderState.ReadingXLenData:
		{
			int num = 0;
			while (loopCounter < gzip_header_xlen)
			{
				num = NILNDHEKNLJ.GetBits(8);
				if (num < 0)
				{
					return false;
				}
				loopCounter++;
			}
			gzipHeaderSubstate = GzipHeaderState.ReadingFileName;
			loopCounter = 0;
			goto case GzipHeaderState.ReadingFileName;
		}
		case GzipHeaderState.ReadingFileName:
			if ((gzip_header_flag & 8) == 0)
			{
				gzipHeaderSubstate = GzipHeaderState.ReadingComment;
			}
			else
			{
				int num;
				do
				{
					num = NILNDHEKNLJ.GetBits(8);
					if (num < 0)
					{
						return false;
					}
				}
				while (num != 0);
				gzipHeaderSubstate = GzipHeaderState.ReadingComment;
			}
			goto case GzipHeaderState.ReadingComment;
		case GzipHeaderState.ReadingComment:
			if ((gzip_header_flag & 0x10) == 0)
			{
				gzipHeaderSubstate = GzipHeaderState.ReadingCRC16Part1;
			}
			else
			{
				int num;
				do
				{
					num = NILNDHEKNLJ.GetBits(8);
					if (num < 0)
					{
						return false;
					}
				}
				while (num != 0);
				gzipHeaderSubstate = GzipHeaderState.ReadingCRC16Part1;
			}
			goto case GzipHeaderState.ReadingCRC16Part1;
		case GzipHeaderState.ReadingCRC16Part1:
		{
			if ((gzip_header_flag & 2) == 0)
			{
				gzipHeaderSubstate = GzipHeaderState.Done;
				goto case GzipHeaderState.Done;
			}
			int num = NILNDHEKNLJ.GetBits(8);
			if (num < 0)
			{
				return false;
			}
			gzipHeaderSubstate = GzipHeaderState.ReadingCRC16Part2;
			goto case GzipHeaderState.ReadingCRC16Part2;
		}
		case GzipHeaderState.ReadingCRC16Part2:
		{
			int num = NILNDHEKNLJ.GetBits(8);
			if (num < 0)
			{
				return false;
			}
			gzipHeaderSubstate = GzipHeaderState.Done;
			goto case GzipHeaderState.Done;
		}
		case GzipHeaderState.Done:
			return true;
		default:
			throw new InvalidDataException(SR.GetString("Unknown state"));
		}
	}

	public bool ReadFooter(InputBuffer NILNDHEKNLJ)
	{
		NILNDHEKNLJ.SkipToByteBoundary();
		if (gzipFooterSubstate == GzipHeaderState.ReadingCRC)
		{
			while (loopCounter < 4)
			{
				int num = NILNDHEKNLJ.GetBits(8);
				if (num < 0)
				{
					return false;
				}
				expectedCrc32 |= (uint)(num << 8 * loopCounter);
				loopCounter++;
			}
			gzipFooterSubstate = GzipHeaderState.ReadingFileSize;
			loopCounter = 0;
		}
		if (gzipFooterSubstate == GzipHeaderState.ReadingFileSize)
		{
			if (loopCounter == 0)
			{
				expectedOutputStreamSizeModulo = 0u;
			}
			while (loopCounter < 4)
			{
				int num2 = NILNDHEKNLJ.GetBits(8);
				if (num2 < 0)
				{
					return false;
				}
				expectedOutputStreamSizeModulo |= (uint)(num2 << 8 * loopCounter);
				loopCounter++;
			}
		}
		return true;
	}

	public void UpdateWithBytesRead(byte[] buffer, int IPCOBJBKNAO, int KKBGGFLOLMB)
	{
		actualCrc32 = Crc32Helper.UpdateCrc32(actualCrc32, buffer, IPCOBJBKNAO, KKBGGFLOLMB);
		long num = actualStreamSizeModulo + (uint)KKBGGFLOLMB;
		if (num >= 4294967296L)
		{
			num %= 4294967296L;
		}
		actualStreamSizeModulo = num;
	}

	public void Validate()
	{
		if (expectedCrc32 != actualCrc32)
		{
			throw new InvalidDataException(SR.GetString("Invalid CRC"));
		}
		if (actualStreamSizeModulo != expectedOutputStreamSizeModulo)
		{
			throw new InvalidDataException(SR.GetString("Invalid stream size"));
		}
	}
}
