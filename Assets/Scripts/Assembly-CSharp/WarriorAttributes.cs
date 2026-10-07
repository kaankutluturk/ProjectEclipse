using System.Collections.Generic;
using System.Xml;

public class WarriorAttributes
{
	public Dictionary<string, WarriorAttribute> AttributesByName = new Dictionary<string, WarriorAttribute>();

	public List<WarriorAttribute> AttributeList = new List<WarriorAttribute>();

	public void Parse(XmlNode node)
	{
		AttributeList.Clear();
		AttributesByName.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			WarriorAttribute bCNOAOPGAEI = new WarriorAttribute();
			bCNOAOPGAEI.set_Name(XmlUtils.ParseString(childNode.Attributes["Name"]));
			bCNOAOPGAEI.IconName = XmlUtils.ParseString(childNode.Attributes["Icon"]);
			bCNOAOPGAEI.Alias = XmlUtils.ParseString(childNode.Attributes["Alias"]);
			bCNOAOPGAEI.Point = XmlUtils.ParseInt(childNode.Attributes["Point"]);
			bCNOAOPGAEI.FormatSuffix = XmlUtils.ParseString(childNode.Attributes["Format"]);
			bCNOAOPGAEI.IsHidden = XmlUtils.ParseBool(childNode.Attributes["Hidden"]);
			bCNOAOPGAEI.IsShopHidden = XmlUtils.ParseBool(childNode.Attributes["ShopHidden"]);
			bCNOAOPGAEI.IsProfileHidden = XmlUtils.ParseBool(childNode.Attributes["ProfileHidden"]);
			bCNOAOPGAEI.BarScale = XmlUtils.ParseString(childNode.Attributes["BarScale"]);
			if (bCNOAOPGAEI.FormatSuffix == "Percent")
			{
				bCNOAOPGAEI.FormatSuffix = "%";
			}
			AttributeList.Add(bCNOAOPGAEI);
			AttributesByName.Add(bCNOAOPGAEI.get_Name(), bCNOAOPGAEI);
		}
	}

	public WarriorAttribute GetAttribute(string name)
	{
		return (!AttributesByName.ContainsKey(name)) ? null : AttributesByName[name];
	}
}
