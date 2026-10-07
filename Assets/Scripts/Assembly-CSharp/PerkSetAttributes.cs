using System.Collections.Generic;

public class PerkSetAttributes
{
	public Dictionary<string, string> Values;

	public PerkSetAttributes()
	{
		Values = new Dictionary<string, string>();
	}

	public PerkSetAttributes(PerkSetAttributes NOLFMPDGCOC)
	{
		Values = new Dictionary<string, string>();
		foreach (KeyValuePair<string, string> item in NOLFMPDGCOC.Values)
		{
			Values.Add(item.Key, item.Value);
		}
	}

	public void AddRange(PerkSetAttributes NOLFMPDGCOC)
	{
		if (NOLFMPDGCOC == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> item in NOLFMPDGCOC.Values)
		{
			Values[item.Key] = item.Value;
		}
	}

	public void SetValue(string name, string value)
	{
		Values[name] = value;
	}

	public string GetValue(string name)
	{
		if (Values.ContainsKey(name))
		{
			return Values[name];
		}
		return name;
	}
}
