using System.Collections.Generic;

public class PlistElementArray : PlistElement
{
	public List<PlistElement> values = new List<PlistElement>();

	public void AddString(string value)
	{
		values.Add(new PlistElementString(value));
	}

	public void AddInteger(int value)
	{
		values.Add(new PlistElementInteger(value));
	}

	public void AddBoolean(bool value)
	{
		values.Add(new PlistElementBoolean(value));
	}

	public PlistElementArray AddArray()
	{
		PlistElementArray array = new PlistElementArray();
		values.Add(array);
		return array;
	}

	public PlistElementDict AddDict()
	{
		PlistElementDict dict = new PlistElementDict();
		values.Add(dict);
		return dict;
	}
}
