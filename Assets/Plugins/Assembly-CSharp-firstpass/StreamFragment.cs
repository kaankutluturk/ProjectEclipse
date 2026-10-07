using System;
using System.Collections.Generic;

public sealed class StreamFragment : IYamlSerializable
{
	private readonly List<ParsingEvent> events = new List<ParsingEvent>();

	public IList<ParsingEvent> Events
	{
		get
		{
			return GetEvents();
		}
	}

	public IList<ParsingEvent> GetEvents()
	{
		return events;
	}

	void IYamlSerializable.ReadYaml(IParser parser)
	{
		events.Clear();
		int num = 0;
		do
		{
			if (!parser.MoveNext())
			{
				throw new InvalidOperationException("The parser has reached the end before deserialization completed.");
			}
			events.Add(parser.GetCurrent());
			num += parser.GetCurrent().GetNestingIncrease();
		}
		while (num > 0);
	}

	void IYamlSerializable.WriteYaml(IEmitter emitter)
	{
		foreach (ParsingEvent item in events)
		{
			emitter.Emit(item);
		}
	}
}
