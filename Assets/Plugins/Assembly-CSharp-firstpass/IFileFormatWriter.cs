internal interface IFileFormatWriter
{
	byte[] GetHeader();

	void UpdateWithBytesRead(byte[] buffer, int IPCOBJBKNAO, int OGAPEFFEHIH);

	byte[] GetFooter();
}
