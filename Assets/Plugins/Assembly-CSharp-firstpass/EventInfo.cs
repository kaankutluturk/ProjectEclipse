using System.Diagnostics;

public abstract class EventInfo
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IObjectDescriptor source;

	public IObjectDescriptor SourceDescriptor
	{
		get
		{
			return GetSource();
		}
		private set
		{
			SetSource(value);
		}
	}

	protected EventInfo(IObjectDescriptor source)
	{
		SetSource(source);
	}

	public IObjectDescriptor GetSource()
	{
		return source;
	}

	private void SetSource(IObjectDescriptor value)
	{
		source = value;
	}
}
