using YamlDotNet.Core;

public class StreamEndEvent : ParsingEvent
{
	public override int NestingIncrease
	{
		get
		{
			return GetNestingIncrease();
		}
	}

	public StreamEndEvent(Mark start, Mark end)
		: base(start, end)
	{
	}

	public StreamEndEvent()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public override int GetNestingIncrease()
	{
		return -1;
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.StreamEnd;
	}

	public override string ToString()
	{
		return "Stream end";
	}

	public override void Accept(IParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
