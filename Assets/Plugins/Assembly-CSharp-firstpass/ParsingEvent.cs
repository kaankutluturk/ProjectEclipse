using YamlDotNet.Core;

public abstract class ParsingEvent
{
	private readonly Mark start;

	private readonly Mark end;

	public virtual int NestingIncrease
	{
		get
		{
			return GetNestingIncrease();
		}
	}

	public Mark Start
	{
		get
		{
			return GetStart();
		}
	}

	public Mark End
	{
		get
		{
			return GetEnd();
		}
	}

	internal ParsingEvent(Mark ILENLCMAMBH, Mark PCLFFOBJJFO)
	{
		this.start = ILENLCMAMBH;
		this.end = PCLFFOBJJFO;
	}

	public virtual int GetNestingIncrease()
	{
		return 0;
	}

	internal abstract ParsingEventType get_Type();

	public Mark GetStart()
	{
		return start;
	}

	public Mark GetEnd()
	{
		return end;
	}

	public abstract void Accept(IParsingEventVisitor NKECMANOOEM);
}
