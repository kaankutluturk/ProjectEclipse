using System.Diagnostics;

public sealed class MappingStartEventInfo : ObjectEventInfo
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isImplicit;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private MappingStyle style;

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

	public MappingStyle Style
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

	public MappingStartEventInfo(IObjectDescriptor BBNKIBKPBLO)
		: base(BBNKIBKPBLO)
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

	public MappingStyle GetStyle()
	{
		return style;
	}

	public void SetStyle(MappingStyle value)
	{
		style = value;
	}
}
