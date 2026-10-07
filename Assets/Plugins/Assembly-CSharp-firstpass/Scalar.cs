using System.Globalization;
using YamlDotNet.Core;

public class Scalar : NodeEvent
{
	private readonly string value;

	private readonly ScalarStyle style;

	private readonly bool isPlainImplicit;

	private readonly bool isQuotedImplicit;

	public string Value
	{
		get
		{
			return GetValue();
		}
	}

	public ScalarStyle Style
	{
		get
		{
			return GetStyle();
		}
	}

	public bool IsPlainImplicit
	{
		get
		{
			return GetIsPlainImplicit();
		}
	}

	public bool IsQuotedImplicit
	{
		get
		{
			return GetIsQuotedImplicit();
		}
	}

	public override bool IsCanonical
	{
		get
		{
			return GetIsCanonical();
		}
	}

	public Scalar(string anchor, string tag, string value, ScalarStyle style, bool isPlainImplicit, bool isQuotedImplicit, Mark start, Mark end)
		: base(anchor, tag, start, end)
	{
		this.value = value;
		this.style = style;
		this.isPlainImplicit = isPlainImplicit;
		this.isQuotedImplicit = isQuotedImplicit;
	}

	public Scalar(string anchor, string tag, string value, ScalarStyle style, bool isPlainImplicit, bool isQuotedImplicit)
		: this(anchor, tag, value, style, isPlainImplicit, isQuotedImplicit, Mark.Empty, Mark.Empty)
	{
	}

	public Scalar(string value)
		: this(null, null, value, ScalarStyle.Any, true, true, Mark.Empty, Mark.Empty)
	{
	}

	public Scalar(string tag, string value)
		: this(null, tag, value, ScalarStyle.Any, true, true, Mark.Empty, Mark.Empty)
	{
	}

	public Scalar(string anchor, string tag, string value)
		: this(anchor, tag, value, ScalarStyle.Any, true, true, Mark.Empty, Mark.Empty)
	{
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.Scalar;
	}

	public string GetValue()
	{
		return value;
	}

	public ScalarStyle GetStyle()
	{
		return style;
	}

	public bool GetIsPlainImplicit()
	{
		return isPlainImplicit;
	}

	public bool GetIsQuotedImplicit()
	{
		return isQuotedImplicit;
	}

	public override bool GetIsCanonical()
	{
		return !isPlainImplicit && !isQuotedImplicit;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Scalar [anchor = {0}, tag = {1}, value = {2}, style = {3}, isPlainImplicit = {4}, isQuotedImplicit = {5}]", GetAnchor(), GetTag(), value, style, isPlainImplicit, isQuotedImplicit);
	}

	public override void Accept(IParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
