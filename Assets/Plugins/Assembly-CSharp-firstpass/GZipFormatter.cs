internal class GZipFormatter : IFileFormatWriter
{
	private byte[] headerBytes = new byte[10] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };

	private uint _crc32;

	private long _inputStreamSizeModulo;

	internal GZipFormatter()
		: this(3)
	{
	}

	internal GZipFormatter(int compressionLevel)
	{
		if (compressionLevel == 10)
		{
			headerBytes[8] = 2;
		}
	}

	public byte[] GetHeader()
	{
		return headerBytes;
	}

	public void UpdateWithBytesRead(byte[] buffer, int offset, int count)
	{
		_crc32 = Crc32Helper.UpdateCrc32(_crc32, buffer, offset, count);
		long num = _inputStreamSizeModulo + (uint)count;
		if (num >= 4294967296L)
		{
			num %= 4294967296L;
		}
		_inputStreamSizeModulo = num;
	}

	public byte[] GetFooter()
	{
		byte[] array = new byte[8];
		WriteUInt32(array, _crc32, 0);
		WriteUInt32(array, (uint)_inputStreamSizeModulo, 4);
		return array;
	}

	internal void WriteUInt32(byte[] buffer, uint value, int offset)
	{
		buffer[offset] = (byte)value;
		buffer[offset + 1] = (byte)(value >> 8);
		buffer[offset + 2] = (byte)(value >> 16);
		buffer[offset + 3] = (byte)(value >> 24);
	}
}
