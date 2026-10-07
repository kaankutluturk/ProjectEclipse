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

	public SequenceEnd(Mark ILENLCMAMBH, Mark PCLFFOBJJFO)
		: base(ILENLCMAMBH, PCLFFOBJJFO)
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

	public override void Accept(IParsingEventVisitor NKECMANOOEM)
	{
		NKECMANOOEM.Visit(this);
	}
}
