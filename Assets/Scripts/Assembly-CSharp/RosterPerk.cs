using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class RosterPerk
{
	private int _level;

	private int upgradeLevel;

	private string _name = string.Empty;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private PerkInfoItem perkInfo;

	private XmlNode _node;

	public XmlNode Node
	{
		get { return _node; }
	}

	public int Level
	{
		get
		{
			return GetLevel();
		}
		set
		{
			SetLevel(value);
		}
	}

	public int UpgradeLevel
	{
		get
		{
			return GetUpgradeLevel();
		}
		set
		{
			SetUpgradeLevel(value);
		}
	}

	public PerkInfoItem PerkInfo
	{
		get
		{
			return GetPerkInfo();
		}
		set
		{
			SetPerkInfo(value);
		}
	}

	public RosterPerk(XmlNode node)
	{
		_node = node;
		XmlAttribute xmlAttribute = _node.Attributes["Level"];
		if (xmlAttribute == null || string.IsNullOrEmpty(xmlAttribute.Value))
		{
			xmlAttribute = _node.AppendAttribute("Level");
			xmlAttribute.Value = "0";
		}
		XmlAttribute xmlAttribute2 = _node.Attributes["Name"];
		if (xmlAttribute2 == null || string.IsNullOrEmpty(xmlAttribute2.Value))
		{
			xmlAttribute2 = _node.AppendAttribute("Name");
		}
		_level = xmlAttribute.ParseInt();
		_name = xmlAttribute2.GetStringOrDefault(string.Empty);
		upgradeLevel = _node.Attributes["UpgradeLevel"].ParseInt();
	}

	public int GetLevel()
	{
		return _level;
	}

	public void SetLevel(int value)
	{
		_level = value;
		XmlAttribute xmlAttribute = _node.Attributes["Level"];
		if (xmlAttribute == null)
		{
			xmlAttribute = _node.AppendAttribute("Level");
		}
		xmlAttribute.Value = _level.ToString();
	}

	public int GetUpgradeLevel()
	{
		return upgradeLevel;
	}

	public void SetUpgradeLevel(int value)
	{
		upgradeLevel = value;
		XmlAttribute xmlAttribute = _node.Attributes["UpgradeLevel"];
		if (xmlAttribute == null)
		{
			xmlAttribute = _node.AppendAttribute("UpgradeLevel");
		}
		xmlAttribute.Value = upgradeLevel.ToString();
	}

	public string get_Name()
	{
		return _name;
	}

	public void set_Name(string value)
	{
		_name = value;
		XmlAttribute xmlAttribute = _node.Attributes["Name"];
		if (xmlAttribute == null)
		{
			xmlAttribute = _node.AppendAttribute("Name");
		}
		xmlAttribute.Value = _name;
	}

	public PerkInfoItem GetPerkInfo()
	{
		return perkInfo;
	}

	public void SetPerkInfo(PerkInfoItem value)
	{
		perkInfo = value;
	}

	public void AppendNodeChild(Dictionary<string, string> attributes)
	{
		XmlNode xmlNode = _node["Set"];
		if (xmlNode != null)
		{
			_node.RemoveChild(xmlNode);
		}
		if (attributes.Count == 0)
		{
			return;
		}
		XmlNode setNode = _node.AppendElement("Set");
		foreach (KeyValuePair<string, string> item in attributes)
		{
			setNode.AppendAttribute(item.Key).Value = item.Value;
		}
	}
}
