using System.Collections.Generic;
using System.Xml;

public class BarScale
{
	public List<Limit> AttributeLimits = new List<Limit>();

	public List<Limit> ItemLimits = new List<Limit>();

	public string Name;

	public string Type;

	public float Power;

	public float MinPower;

	public Limit GetItemLimitForLevel(int level)
	{
		return ItemLimits.Find((Limit limit) => limit.Levels.Contains(level));
	}

	public Limit GetDefaultItemLimit()
	{
		return ItemLimits.Find((Limit limit) => limit.Levels.Count == 0);
	}

	public Limit GetAttributeLimitForLevel(int level)
	{
		return AttributeLimits.Find((Limit limit) => limit.Levels.Contains(level));
	}

	public Limit GetDefaultAttributeLimit()
	{
		return AttributeLimits.Find((Limit limit) => limit.Levels.Count == 0);
	}

	public void ParseLimits(XmlNode node, List<Limit> limits)
	{
		if (node == null)
		{
			return;
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			Limit limit = new Limit();
			XmlAttribute leftLimitAttribute = childNode.Attributes["LeftLimit"];
			XmlAttribute cJBEMNNNHDM2 = childNode.Attributes["RightLimit"];
			XmlAttribute xmlAttribute = childNode.Attributes["Level"];
			XmlAttribute cJBEMNNNHDM3 = childNode.Attributes["LevelMultiplier"];
			XmlAttribute cJBEMNNNHDM4 = childNode.Attributes["Shift"];
			limit.LeftLimit = leftLimitAttribute.ParseInt(-1);
			limit.RightLimit = cJBEMNNNHDM2.ParseInt(-1);
			if (xmlAttribute != null)
			{
				string text = xmlAttribute.GetStringOrDefault();
				if (!string.IsNullOrEmpty(text))
				{
					string[] array = text.Split('|');
					string[] array2 = array;
					foreach (string s in array2)
					{
						int result;
						if (int.TryParse(s, out result))
						{
							limit.Levels.Add(result);
						}
					}
				}
			}
			limit.LevelMultiplier = cJBEMNNNHDM3.ParseFloat(-1f);
			limit.Shift = cJBEMNNNHDM4.ParseInt(-1);
			limits.Add(limit);
		}
	}
}
