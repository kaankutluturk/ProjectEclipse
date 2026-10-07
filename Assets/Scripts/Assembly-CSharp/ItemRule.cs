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

	public ItemRule(XmlNode node, bool NPBEDEFLCAE = true)
		: base(RuleType.RuleItem, node)
	{
		item = null;
		noAttributeChange = false;
		isEquipRequirement = false;
		appliance = RuleAppliance.AppliancePlayer;
		if (NPBEDEFLCAE)
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
		UserItem dKCHDHMLKHN = data as UserItem;
		if (dKCHDHMLKHN == null)
		{
			return true;
		}
		ItemInfo dJKEECEOCJB = item.GetInfo();
		ItemInfo dJKEECEOCJB2 = dKCHDHMLKHN.GetInfo();
		if (dJKEECEOCJB.Name != string.Empty && dJKEECEOCJB.Name == dJKEECEOCJB2.Name)
		{
			return false;
		}
		if (dJKEECEOCJB.Type != string.Empty && dJKEECEOCJB.Type == dJKEECEOCJB2.Type)
		{
			return false;
		}
		if (dJKEECEOCJB.SubType != string.Empty && dJKEECEOCJB.SubType == dJKEECEOCJB2.SubType)
		{
			return false;
		}
		int num = ((dKCHDHMLKHN.GetCurrentUpgradeItem() == null) ? dJKEECEOCJB2.ItemLevel : dKCHDHMLKHN.GetCurrentUpgradeItem().ItemLevel);
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

	public void SetAppliance(RuleAppliance IGFNCCEHFEK)
	{
		appliance = IGFNCCEHFEK;
	}

	protected void ParseAppliance(XmlNode node)
	{
		RuleAppliance bJHBHKKHENM = RuleAppliance.AppliancePlayer;
		switch (node.Attributes["ApplyTo"].GetStringOrDefault(string.Empty))
		{
		case "Player":
			bJHBHKKHENM = RuleAppliance.AppliancePlayer;
			break;
		case "Bot":
			bJHBHKKHENM = RuleAppliance.ApplianceOpponent;
			break;
		case "All":
			bJHBHKKHENM = RuleAppliance.ApplianceAll;
			break;
		}
		appliance = bJHBHKKHENM;
	}

	protected virtual void ParseItem(XmlNode node)
	{
		ItemInfo dJKEECEOCJB = new ItemInfo(node);
		item = new UserItem(node, dJKEECEOCJB.Name, false, 1, node.Attributes["MinLevel"].ParseInt(), 0L, 0);
		item.SetInfo(dJKEECEOCJB);
	}
}
