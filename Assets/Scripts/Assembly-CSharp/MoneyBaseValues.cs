using System.Collections.Generic;
using System.Xml;

public class MoneyBaseValues
{
	private List<CharProgLevel> levels = new List<CharProgLevel>();

	public void Parse(XmlNode node)
	{
		levels.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "Level")
			{
				CharProgLevel item = new CharProgLevel(childNode);
				levels.Add(item);
			}
		}
	}

	public long GetBaseValue(int level)
	{
		foreach (CharProgLevel item in levels)
		{
			if (level >= item.Min && level <= item.Max)
			{
				return item.value;
			}
		}
		return 0L;
	}
}
