using System.Reflection;

[DefaultMember("Item")]
public class PlistElement
{
	// C# has no syntax for parameterized property 'DLKPBAJDHBO'.
	public PlistElement get_DLKPBAJDHBO(string KGBGENDIMBC)
	{
		return get_Item(KGBGENDIMBC);
	}

	public void set_DLKPBAJDHBO(string KGBGENDIMBC, PlistElement value)
	{
		SetItem(KGBGENDIMBC, value);
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

	public PlistElement get_Item(string KGBGENDIMBC)
	{
		return AsDict().get_Item(KGBGENDIMBC);
	}

	public void SetItem(string KGBGENDIMBC, PlistElement value)
	{
		AsDict().SetItem(KGBGENDIMBC, value);
	}
}
