using System.Diagnostics;

public sealed class SequenceStartEventInfo : ObjectEventInfo
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isImplicit;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SequenceStyle style;

	public bool IsImplicit
	{
		get
		{
			return GetIsImplicit();
		}
		set
		{
			SetIsImplicit(value);
		}
	}

	public SequenceStyle Style
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

	public SequenceStartEventInfo(IObjectDescriptor source)
		: base(source)
	{
	}

	public bool GetIsImplicit()
	{
		return isImplicit;
	}

	public void SetIsImplicit(bool value)
	{
		isImplicit = value;
	}

	public SequenceStyle GetStyle()
	{
		return style;
	}

	public void SetStyle(SequenceStyle value)
	{
		style = value;
	}
}
