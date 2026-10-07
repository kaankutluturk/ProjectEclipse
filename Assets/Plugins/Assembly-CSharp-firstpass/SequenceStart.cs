using System.Globalization;
using YamlDotNet.Core;

public class SequenceStart : NodeEvent
{
	private readonly bool isImplicit;

	private readonly SequenceStyle style;

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

	public SequenceStyle Style
	{
		get
		{
			return GetStyle();
		}
	}

	public SequenceStart(string anchor, string tag, bool isImplicit, SequenceStyle style, Mark start, Mark end)
		: base(anchor, tag, start, end)
	{
		this.isImplicit = isImplicit;
		this.style = style;
	}

	public SequenceStart(string anchor, string tag, bool isImplicit, SequenceStyle style)
		: this(anchor, tag, isImplicit, style, Mark.Empty, Mark.Empty)
	{
	}

	public override int GetNestingIncrease()
	{
		return 1;
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.SequenceStart;
	}

	public bool GetIsImplicit()
	{
		return isImplicit;
	}

	public override bool GetIsCanonical()
	{
		return !isImplicit;
	}

	public SequenceStyle GetStyle()
	{
		return style;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Sequence start [anchor = {0}, tag = {1}, isImplicit = {2}, style = {3}]", GetAnchor(), GetTag(), isImplicit, style);
	}

	public override void Accept(IParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
