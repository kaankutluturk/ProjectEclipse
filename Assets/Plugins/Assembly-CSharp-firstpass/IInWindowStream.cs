using System.IO;

internal interface IInWindowStream
{
	void SetStream(Stream BHHJJHBNEKD);

	void Init();

	void ReleaseStream();

	byte GetIndexByte(int index);

	uint GetMatchLen(int index, uint OIOMNNFMDOO, uint LOHCIKNKDEI);

	uint GetNumAvailableBytes();
}
