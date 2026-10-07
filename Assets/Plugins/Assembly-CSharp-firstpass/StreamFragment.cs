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

	void IYamlSerializable.ReadYaml(IParser BPGMNGAJMKK)
	{
		events.Clear();
		int num = 0;
		do
		{
			if (!BPGMNGAJMKK.MoveNext())
			{
				throw new InvalidOperationException("The parser has reached the end before deserialization completed.");
			}
			events.Add(BPGMNGAJMKK.GetCurrent());
			num += BPGMNGAJMKK.GetCurrent().GetNestingIncrease();
		}
		while (num > 0);
	}

	void IYamlSerializable.WriteYaml(IEmitter NPIDIMCLNEM)
	{
		foreach (ParsingEvent item in events)
		{
			NPIDIMCLNEM.Emit(item);
		}
	}
}
