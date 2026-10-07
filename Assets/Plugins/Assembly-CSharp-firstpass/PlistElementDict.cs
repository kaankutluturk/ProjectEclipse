using System.Collections.Generic;
using System.Reflection;

[DefaultMember("Item")]
public class PlistElementDict : PlistElement
{
	private SortedDictionary<string, PlistElement> m_PrivateValue = new SortedDictionary<string, PlistElement>();

	public IDictionary<string, PlistElement> values
	{
		get
		{
			return GetValues();
		}
	}

	// C# has no syntax for parameterized property 'Item'.
	public PlistElement get_DLKPBAJDHBO(string key)
	{
		return get_Item(key);
	}

	public void set_DLKPBAJDHBO(string key, PlistElement value)
	{
		SetItem(key, value);
	}

	public IDictionary<string, PlistElement> GetValues()
	{
		return m_PrivateValue;
	}

	public new PlistElement get_Item(string key)
	{
		if (GetValues().ContainsKey(key))
		{
			return GetValues()[key];
		}
		return null;
	}

	public new void SetItem(string key, PlistElement value)
	{
		GetValues()[key] = value;
	}

	public void SetInteger(string key, int value)
	{
		GetValues()[key] = new PlistElementInteger(value);
	}

	public void SetString(string key, string value)
	{
		GetValues()[key] = new PlistElementString(value);
	}

	public void SetBoolean(string key, bool value)
	{
		GetValues()[key] = new PlistElementBoolean(value);
	}

	public PlistElementArray CreateArray(string key)
	{
		PlistElementArray array = new PlistElementArray();
		GetValues()[key] = array;
		return array;
	}

	public PlistElementDict CreateDict(string key)
	{
		PlistElementDict dict = new PlistElementDict();
		GetValues()[key] = dict;
		return dict;
	}
}
