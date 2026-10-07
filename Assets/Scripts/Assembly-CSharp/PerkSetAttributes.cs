using System.Collections.Generic;

public class PerkSetAttributes
{
	public Dictionary<string, string> Values;

	public PerkSetAttributes()
	{
		Values = new Dictionary<string, string>();
	}

	public PerkSetAttributes(PerkSetAttributes source)
	{
		Values = new Dictionary<string, string>();
		foreach (KeyValuePair<string, string> item in source.Values)
		{
			Values.Add(item.Key, item.Value);
		}
	}

	public void AddRange(PerkSetAttributes other)
	{
		if (other == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> item in other.Values)
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
