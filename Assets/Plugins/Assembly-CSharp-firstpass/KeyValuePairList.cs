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

	public bool TryGet(string value, out KeyValuePair KKNOCIPBIIK)
	{
		KKNOCIPBIIK = null;
		for (int i = 0; i < GetValues().Count; i++)
		{
			if (string.CompareOrdinal(GetValues()[i].GetKey(), value) == 0)
			{
				KKNOCIPBIIK = GetValues()[i];
				return true;
			}
		}
		return false;
	}

	public bool HasAny(string OIPHDFDAOFN, string DBALKBDCIKJ = "")
	{
		for (int i = 0; i < GetValues().Count; i++)
		{
			if (string.CompareOrdinal(GetValues()[i].GetKey(), OIPHDFDAOFN) == 0 || string.CompareOrdinal(GetValues()[i].GetKey(), DBALKBDCIKJ) == 0)
			{
				return true;
			}
		}
		return false;
	}
}
