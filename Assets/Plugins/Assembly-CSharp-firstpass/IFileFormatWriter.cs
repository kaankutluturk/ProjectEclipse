internal interface IFileFormatWriter
{
	byte[] GetHeader();

	void UpdateWithBytesRead(byte[] buffer, int offset, int count);

	byte[] GetFooter();
}
