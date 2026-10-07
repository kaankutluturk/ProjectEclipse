using System.Globalization;
using YamlDotNet.Core;

public class DocumentEnd : ParsingEvent
{
	private readonly bool isImplicit;

	public override int NestingIncrease
	{
		get
		{
			return GetNestingIncrease();
		}
	}

	public bool IsImplicit
	{
		get
		{
			return GetIsImplicit();
		}
	}

	public DocumentEnd(bool isImplicit, Mark ILENLCMAMBH, Mark PCLFFOBJJFO)
		: base(ILENLCMAMBH, PCLFFOBJJFO)
	{
		this.isImplicit = isImplicit;
	}

	public DocumentEnd(bool isImplicit)
		: this(isImplicit, Mark.Empty, Mark.Empty)
	{
	}

	public override int GetNestingIncrease()
	{
		return -1;
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.DocumentEnd;
	}

	public bool GetIsImplicit()
	{
		return isImplicit;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Document end [isImplicit = {0}]", isImplicit);
	}

	public override void Accept(IParsingEventVisitor NKECMANOOEM)
	{
		NKECMANOOEM.Visit(this);
	}
}
