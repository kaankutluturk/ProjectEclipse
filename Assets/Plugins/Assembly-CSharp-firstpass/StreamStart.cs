using YamlDotNet.Core;

public class StreamStart : ParsingEvent
{
	public override int NestingIncrease
	{
		get
		{
			return GetNestingIncrease();
		}
	}

	public StreamStart()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public StreamStart(Mark start, Mark end)
		: base(start, end)
	{
	}

	public override int GetNestingIncrease()
	{
		return 1;
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.StreamStart;
	}

	public override string ToString()
	{
		return "Stream start";
	}

	public override void Accept(IParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
