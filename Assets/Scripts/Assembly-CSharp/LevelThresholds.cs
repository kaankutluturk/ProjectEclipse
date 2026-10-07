using System.Collections.Generic;
using System.Xml;

public class LevelThresholds
{
	public List<global::Pair<int, uint>> Thresholds = new List<global::Pair<int, uint>>();

	public void Parse(XmlNode node)
	{
		Thresholds.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			int level = childNode.Attributes["Level"].ParseInt();
			uint experience = childNode.Attributes["Exp"].ParseUint();
			Thresholds.Add(new global::Pair<int, uint>(level, experience));
		}
	}
}
