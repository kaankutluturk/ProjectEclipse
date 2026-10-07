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
			WarriorAttribute attribute = new WarriorAttribute();
			attribute.set_Name(XmlUtils.ParseString(childNode.Attributes["Name"]));
			attribute.IconName = XmlUtils.ParseString(childNode.Attributes["Icon"]);
			attribute.Alias = XmlUtils.ParseString(childNode.Attributes["Alias"]);
			attribute.Point = XmlUtils.ParseInt(childNode.Attributes["Point"]);
			attribute.FormatSuffix = XmlUtils.ParseString(childNode.Attributes["Format"]);
			attribute.IsHidden = XmlUtils.ParseBool(childNode.Attributes["Hidden"]);
			attribute.IsShopHidden = XmlUtils.ParseBool(childNode.Attributes["ShopHidden"]);
			attribute.IsProfileHidden = XmlUtils.ParseBool(childNode.Attributes["ProfileHidden"]);
			attribute.BarScale = XmlUtils.ParseString(childNode.Attributes["BarScale"]);
			if (attribute.FormatSuffix == "Percent")
			{
				attribute.FormatSuffix = "%";
			}
			AttributeList.Add(attribute);
			AttributesByName.Add(attribute.get_Name(), attribute);
		}
	}

	public WarriorAttribute GetAttribute(string name)
	{
		return (!AttributesByName.ContainsKey(name)) ? null : AttributesByName[name];
	}
}
