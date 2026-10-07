using System.Globalization;
using System.IO;
using YamlDotNet.Core;

public class EventReader
{
	private readonly IParser parser;

	private bool endOfStream;

	public IParser Parser
	{
		get
		{
			return GetParser();
		}
	}

	public EventReader(IParser parser)
	{
		this.parser = parser;
		MoveNext();
	}

	public IParser GetParser()
	{
		return parser;
	}

	public T Expect<T>() where T : ParsingEvent
	{
		T val = Allow<T>();
		if (val == null)
		{
			ParsingEvent currentEvent = parser.GetCurrent();
			throw new YamlException(currentEvent.GetStart(), currentEvent.GetEnd(), string.Format(CultureInfo.InvariantCulture, "Expected '{0}', got '{1}' (at {2}).", typeof(T).Name, currentEvent.GetType().Name, currentEvent.GetStart()));
		}
		return val;
	}

	public bool Accept<T>() where T : ParsingEvent
	{
		ThrowIfAtEndOfStream();
		return parser.GetCurrent() is T;
	}

	private void ThrowIfAtEndOfStream()
	{
		if (endOfStream)
		{
			throw new EndOfStreamException();
		}
	}

	public T Allow<T>() where T : ParsingEvent
	{
		if (!Accept<T>())
		{
			return (T)null;
		}
		T result = (T)parser.GetCurrent();
		MoveNext();
		return result;
	}

	public T Peek<T>() where T : ParsingEvent
	{
		if (!Accept<T>())
		{
			return (T)null;
		}
		return (T)parser.GetCurrent();
	}

	public void SkipThisAndNestedEvents()
	{
		int num = 0;
		do
		{
			num += Peek<ParsingEvent>().GetNestingIncrease();
			MoveNext();
		}
		while (num > 0);
	}

	private void MoveNext()
	{
		endOfStream = !parser.MoveNext();
	}
}
