using System.IO;

internal interface IInWindowStream
{
	void SetStream(Stream stream);

	void Init();

	void ReleaseStream();

	byte GetIndexByte(int index);

	uint GetMatchLen(int index, uint distance, uint limit);

	uint GetNumAvailableBytes();
}
