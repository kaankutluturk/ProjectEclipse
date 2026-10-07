using System.Diagnostics;

public sealed class KeyValuePair
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string key;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string valueText;

	public string PairName
	{
		get
		{
			return GetKey();
		}
		set
		{
			set_Key(value);
		}
	}

	public KeyValuePair(string KGBGENDIMBC)
	{
		set_Key(KGBGENDIMBC);
	}

	public string GetKey()
	{
		return key;
	}

	public void set_Key(string value)
	{
		key = value;
	}

	public string GetValue()
	{
		return valueText;
	}

	public void set_Value(string value)
	{
		valueText = value;
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(GetValue()))
		{
			return GetKey() + '=' + GetValue();
		}
		return GetKey();
	}
}
