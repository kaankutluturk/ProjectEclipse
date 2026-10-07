using System.Diagnostics;

public class ObjectEventInfo : EventInfo
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string anchor;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string tag;

	public string Anchor
	{
		get
		{
			return GetAnchor();
		}
		set
		{
			SetAnchor(value);
		}
	}

	public string EventTag
	{
		get
		{
			return GetTag();
		}
		set
		{
			set_Tag(value);
		}
	}

	protected ObjectEventInfo(IObjectDescriptor objectDescriptor)
		: base(objectDescriptor)
	{
	}

	public string GetAnchor()
	{
		return anchor;
	}

	public void SetAnchor(string value)
	{
		anchor = value;
	}

	public string GetTag()
	{
		return tag;
	}

	public void set_Tag(string value)
	{
		tag = value;
	}
}
