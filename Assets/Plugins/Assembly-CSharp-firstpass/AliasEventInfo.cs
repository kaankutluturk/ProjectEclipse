using System.Diagnostics;

public class AliasEventInfo : EventInfo
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string alias;

	public string AliasName
	{
		get
		{
			return GetAlias();
		}
		set
		{
			set_Alias(value);
		}
	}

	public AliasEventInfo(IObjectDescriptor descriptor)
		: base(descriptor)
	{
	}

	public string GetAlias()
	{
		return alias;
	}

	public void set_Alias(string value)
	{
		alias = value;
	}
}
