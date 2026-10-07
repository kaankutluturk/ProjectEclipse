internal interface IFileFormatReader
{
	bool ReadHeader(InputBuffer NILNDHEKNLJ);

	bool ReadFooter(InputBuffer NILNDHEKNLJ);

	void UpdateWithBytesRead(byte[] buffer, int IPCOBJBKNAO, int OGAPEFFEHIH);

	void Validate();
}
