using System.Reflection;

[DefaultMember("Item")]
public class PlistElement
{
	// C# has no syntax for parameterized property 'Item'.
	public PlistElement get_DLKPBAJDHBO(string key)
	{
		return get_Item(key);
	}

	public void set_DLKPBAJDHBO(string key, PlistElement value)
	{
		SetItem(key, value);
	}

	protected PlistElement()
	{
	}

	public string AsString()
	{
		return ((PlistElementString)this).value;
	}

	public int AsInteger()
	{
		return ((PlistElementInteger)this).value;
	}

	public bool AsBoolean()
	{
		return ((PlistElementBoolean)this).value;
	}

	public PlistElementArray AsArray()
	{
		return (PlistElementArray)this;
	}

	public PlistElementDict AsDict()
	{
		return (PlistElementDict)this;
	}

	public PlistElement get_Item(string key)
	{
		return AsDict().get_Item(key);
	}

	public void SetItem(string key, PlistElement value)
	{
		AsDict().SetItem(key, value);
	}
}
