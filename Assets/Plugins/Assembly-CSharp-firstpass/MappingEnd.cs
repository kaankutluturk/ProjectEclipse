using YamlDotNet.Core;

public class MappingEnd : ParsingEvent
{
	public override int NestingIncrease
	{
		get
		{
			return GetNestingIncrease();
		}
	}

	public MappingEnd(Mark start, Mark end)
		: base(start, end)
	{
	}

	public MappingEnd()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public override int GetNestingIncrease()
	{
		return -1;
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.MappingEnd;
	}

	public override string ToString()
	{
		return "Mapping end";
	}

	public override void Accept(IParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
