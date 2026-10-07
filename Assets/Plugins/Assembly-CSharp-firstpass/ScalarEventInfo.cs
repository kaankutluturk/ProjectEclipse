using System.Diagnostics;

public sealed class ScalarEventInfo : ObjectEventInfo
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string renderedValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ScalarStyle style;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isPlainImplicit;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isQuotedImplicit;

	public string RenderedText
	{
		get
		{
			return GetRenderedValue();
		}
		set
		{
			set_RenderedValue(value);
		}
	}

	public ScalarStyle Style
	{
		get
		{
			return GetStyle();
		}
		set
		{
			SetStyle(value);
		}
	}

	public bool IsPlainImplicit
	{
		get
		{
			return GetIsPlainImplicit();
		}
		set
		{
			SetIsPlainImplicit(value);
		}
	}

	public bool IsQuotedImplicit
	{
		get
		{
			return GetIsQuotedImplicit();
		}
		set
		{
			SetIsQuotedImplicit(value);
		}
	}

	public ScalarEventInfo(IObjectDescriptor BBNKIBKPBLO)
		: base(BBNKIBKPBLO)
	{
	}

	public string GetRenderedValue()
	{
		return renderedValue;
	}

	public void set_RenderedValue(string value)
	{
		renderedValue = value;
	}

	public ScalarStyle GetStyle()
	{
		return style;
	}

	public void SetStyle(ScalarStyle value)
	{
		style = value;
	}

	public bool GetIsPlainImplicit()
	{
		return isPlainImplicit;
	}

	public void SetIsPlainImplicit(bool value)
	{
		isPlainImplicit = value;
	}

	public bool GetIsQuotedImplicit()
	{
		return isQuotedImplicit;
	}

	public void SetIsQuotedImplicit(bool value)
	{
		isQuotedImplicit = value;
	}
}
