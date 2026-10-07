using System.Collections.Generic;
using System.Diagnostics;

public class KeyValuePairList
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<KeyValuePair> values;

	public List<KeyValuePair> Values
	{
		get
		{
			return GetValues();
		}
		protected set
		{
			SetValues(value);
		}
	}

	public List<KeyValuePair> GetValues()
	{
		return values;
	}

	protected void SetValues(List<KeyValuePair> value)
	{
		values = value;
	}

	public bool TryGet(string value, out KeyValuePair pair)
	{
		pair = null;
		for (int i = 0; i < GetValues().Count; i++)
		{
			if (string.CompareOrdinal(GetValues()[i].GetKey(), value) == 0)
			{
				pair = GetValues()[i];
				return true;
			}
		}
		return false;
	}

	public bool HasAny(string key, string alternateKey = "")
	{
		for (int i = 0; i < GetValues().Count; i++)
		{
			if (string.CompareOrdinal(GetValues()[i].GetKey(), key) == 0 || string.CompareOrdinal(GetValues()[i].GetKey(), alternateKey) == 0)
			{
				return true;
			}
		}
		return false;
	}
}
