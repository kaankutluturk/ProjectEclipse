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

	public Scalar(string KOLNNNLOCFE, string EDLADAAKMDF, string value, ScalarStyle KIGNIBIMLKK, bool OCBIEJBMFJN, bool FAKBCOKEHGP, Mark ILENLCMAMBH, Mark PCLFFOBJJFO)
		: base(KOLNNNLOCFE, EDLADAAKMDF, ILENLCMAMBH, PCLFFOBJJFO)
	{
		this.value = value;
		this.style = KIGNIBIMLKK;
		this.isPlainImplicit = OCBIEJBMFJN;
		this.isQuotedImplicit = FAKBCOKEHGP;
	}

	public Scalar(string KOLNNNLOCFE, string EDLADAAKMDF, string value, ScalarStyle KIGNIBIMLKK, bool OCBIEJBMFJN, bool FAKBCOKEHGP)
		: this(KOLNNNLOCFE, EDLADAAKMDF, value, KIGNIBIMLKK, OCBIEJBMFJN, FAKBCOKEHGP, Mark.Empty, Mark.Empty)
	{
	}

	public Scalar(string value)
		: this(null, null, value, ScalarStyle.Any, true, true, Mark.Empty, Mark.Empty)
	{
	}

	public Scalar(string EDLADAAKMDF, string value)
		: this(null, EDLADAAKMDF, value, ScalarStyle.Any, true, true, Mark.Empty, Mark.Empty)
	{
	}

	public Scalar(string KOLNNNLOCFE, string EDLADAAKMDF, string value)
		: this(KOLNNNLOCFE, EDLADAAKMDF, value, ScalarStyle.Any, true, true, Mark.Empty, Mark.Empty)
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

	public override void Accept(IParsingEventVisitor NKECMANOOEM)
	{
		NKECMANOOEM.Visit(this);
	}
}
