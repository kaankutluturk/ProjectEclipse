using System.Collections.Generic;
using System.Xml;

public class Defense
{
	public string DefenseName;

	public float Weight;

	public string CancellingItem;

	public List<Evaluation> Evaluations = new List<Evaluation>();

	public static int Parse(XmlNode node, List<Defense> defenses)
	{
		int count = defenses.Count;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "Defense")
			{
				Defense defense = new Defense();
				defense.Parse(childNode);
				defenses.Add(defense);
			}
		}
		return defenses.Count - count;
	}

	public void Parse(XmlNode node)
	{
		DefenseName = XmlUtils.ParseString(node.Attributes["Name"]);
		Weight = XmlUtils.ParseFloat(node.Attributes["Weight"]);
		CancellingItem = XmlUtils.ParseString(node.Attributes["CancellingItem"]);
		Evaluation.ParseAttributes(node, Evaluations);
	}
}
