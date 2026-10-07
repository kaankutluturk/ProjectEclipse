using YamlDotNet.Core;

public class SequenceEnd : ParsingEvent
{
	public override int NestingIncrease
	{
		get
		{
			return GetNestingIncrease();
		}
	}

	public SequenceEnd(Mark start, Mark end)
		: base(start, end)
	{
	}

	public SequenceEnd()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public override int GetNestingIncrease()
	{
		return -1;
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.SequenceEnd;
	}

	public override string ToString()
	{
		return "Sequence end";
	}

	public override void Accept(IParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
