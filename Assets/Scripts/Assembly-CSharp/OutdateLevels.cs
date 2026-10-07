using System.Collections.Generic;
using System.Xml;

public class OutdateLevels
{
	private List<OutdateLevelItem> Types = new List<OutdateLevelItem>();

	public void Parse(XmlNode node)
	{
		Types.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			OutdateLevelItem outdateItem = new OutdateLevelItem();
			outdateItem.Parse(childNode);
			Types.Add(outdateItem);
		}
	}

	public float GetValue(string typeName)
	{
		foreach (OutdateLevelItem item in Types)
		{
			if (item.IsType(typeName))
			{
				return item.Value;
			}
		}
		return (Types.Count <= 0) ? 0f : Types[0].Value;
	}
}
