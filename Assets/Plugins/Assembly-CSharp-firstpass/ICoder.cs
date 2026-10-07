using System.IO;

public interface ICoder
{
	void Code(Stream inStream, Stream outStream, long inSize, long outSize, ICodeProgress progress);
}
