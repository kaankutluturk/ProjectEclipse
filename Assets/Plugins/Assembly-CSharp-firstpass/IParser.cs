public interface IParser
{
	ParsingEvent CurrentEvent { get; }

	ParsingEvent GetCurrent();

	bool MoveNext();
}
