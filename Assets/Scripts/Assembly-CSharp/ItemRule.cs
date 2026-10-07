using System.Xml;

public class ItemRule : Rule
{
	protected UserItem item;

	protected bool isEquipRequirement;

	private bool noAttributeChange;

	protected RuleAppliance appliance;

	public UserItem ItemEntry
	{
		get
		{
			return get_Item();
		}
	}

	public bool IsEquipRule
	{
		get
		{
			return GetIsEquipRule();
		}
	}

	public bool IgnoresAttributeChange
	{
		get
		{
			return GetNoAttributeChange();
		}
	}

	public ItemRule(XmlNode node, bool shouldParseItem = true)
		: base(RuleType.RuleItem, node)
	{
		item = null;
		noAttributeChange = false;
		isEquipRequirement = false;
		appliance = RuleAppliance.AppliancePlayer;
		if (shouldParseItem)
		{
			ParseItem(node);
		}
		noAttributeChange = node.Attributes["NoAttributeChange"].ParseBool();
		ParseAppliance(node);
	}

	public UserItem get_Item()
	{
		return item;
	}

	public bool GetIsEquipRule()
	{
		return isEquipRequirement;
	}

	public bool GetNoAttributeChange()
	{
		return noAttributeChange;
	}

	public override bool Compare(object data)
	{
		UserItem userItem = data as UserItem;
		if (userItem == null)
		{
			return true;
		}
		ItemInfo ruleItemInfo = item.GetInfo();
		ItemInfo compareItemInfo = userItem.GetInfo();
		if (ruleItemInfo.Name != string.Empty && ruleItemInfo.Name == compareItemInfo.Name)
		{
			return false;
		}
		if (ruleItemInfo.Type != string.Empty && ruleItemInfo.Type == compareItemInfo.Type)
		{
			return false;
		}
		if (ruleItemInfo.SubType != string.Empty && ruleItemInfo.SubType == compareItemInfo.SubType)
		{
			return false;
		}
		int num = ((userItem.GetCurrentUpgradeItem() == null) ? compareItemInfo.ItemLevel : userItem.GetCurrentUpgradeItem().ItemLevel);
		if (item.GetUpgradeLevel() > num)
		{
			return false;
		}
		return true;
	}

	public RuleAppliance GetAppliance()
	{
		return appliance;
	}

	public bool IsEntryRequirement()
	{
		XmlNode node = GetXmlSource().GetNode();
		return node != null && node.Name == "RequireItem";
	}

	public bool IsSatisfiedBy(ModelParameters parameters)
	{
		if (parameters == null || item == null)
		{
			return false;
		}
		ItemInfo required = item.GetInfo();
		if (required == null || string.IsNullOrEmpty(required.Type))
		{
			return false;
		}
		ItemInfo equipped = parameters.GetItemByType(required.Type);
		if (equipped == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(required.Name) && required.Name != equipped.Name)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(required.SubType) && required.SubType != equipped.SubType)
		{
			return false;
		}
		return equipped.ItemLevel >= item.GetUpgradeLevel();
	}

	public void SetAppliance(RuleAppliance ruleAppliance)
	{
		appliance = ruleAppliance;
	}

	protected void ParseAppliance(XmlNode node)
	{
		RuleAppliance parsedAppliance = RuleAppliance.AppliancePlayer;
		switch (node.Attributes["ApplyTo"].GetStringOrDefault(string.Empty))
		{
		case "Player":
			parsedAppliance = RuleAppliance.AppliancePlayer;
			break;
		case "Bot":
			parsedAppliance = RuleAppliance.ApplianceOpponent;
			break;
		case "All":
			parsedAppliance = RuleAppliance.ApplianceAll;
			break;
		}
		appliance = parsedAppliance;
	}

	protected virtual void ParseItem(XmlNode node)
	{
		ItemInfo itemInfo = new ItemInfo(node);
		item = new UserItem(node, itemInfo.Name, false, 1, node.Attributes["MinLevel"].ParseInt(), 0L, 0);
		item.SetInfo(itemInfo);
	}
}
