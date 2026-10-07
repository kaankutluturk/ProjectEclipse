using System.Globalization;
using YamlDotNet.Core;

public class MappingStart : NodeEvent
{
	private readonly bool isImplicit;

	private readonly MappingStyle style;

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

	public override bool IsCanonical
	{
		get
		{
			return GetIsCanonical();
		}
	}

	public MappingStyle Style
	{
		get
		{
			return GetStyle();
		}
	}

	public MappingStart(string KOLNNNLOCFE, string EDLADAAKMDF, bool isImplicit, MappingStyle KIGNIBIMLKK, Mark ILENLCMAMBH, Mark PCLFFOBJJFO)
		: base(KOLNNNLOCFE, EDLADAAKMDF, ILENLCMAMBH, PCLFFOBJJFO)
	{
		this.isImplicit = isImplicit;
		this.style = KIGNIBIMLKK;
	}

	public MappingStart(string KOLNNNLOCFE, string EDLADAAKMDF, bool isImplicit, MappingStyle KIGNIBIMLKK)
		: this(KOLNNNLOCFE, EDLADAAKMDF, isImplicit, KIGNIBIMLKK, Mark.Empty, Mark.Empty)
	{
	}

	public MappingStart()
		: this(null, null, true, MappingStyle.Any, Mark.Empty, Mark.Empty)
	{
	}

	public override int GetNestingIncrease()
	{
		return 1;
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.MappingStart;
	}

	public bool GetIsImplicit()
	{
		return isImplicit;
	}

	public override bool GetIsCanonical()
	{
		return !isImplicit;
	}

	public MappingStyle GetStyle()
	{
		return style;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Mapping start [anchor = {0}, tag = {1}, isImplicit = {2}, style = {3}]", GetAnchor(), GetTag(), isImplicit, style);
	}

	public override void Accept(IParsingEventVisitor NKECMANOOEM)
	{
		NKECMANOOEM.Visit(this);
	}
}
