using System.Collections.Generic;
using System.Xml;

public class UserItem
{
	private string _Name;

	private ItemInfo itemInfo;

	private bool isEquipped;

	private int count;

	private long _DeliveryTime;

	private int deliveryUpgradeLevel;

	private int upgradeLevel;

	private XmlNode _Node;

	private bool writesToNode;

	private RecipeItemInfo recipeDelivery;

	private ItemInfo currentUpgradeItem;

	private ItemInfo nextUpgradeItem;

	private bool hasUpgrades;

	private bool hasUpgradedItem;

	private bool isMaxUpgrade;

	private bool hasNextUpgrade;

	private string acquireType;

	private List<PerkInfoItem> enchantments = new List<PerkInfoItem>();

	public ItemInfo Info
	{
		get
		{
			return GetInfo();
		}
		set
		{
			SetInfo(value);
		}
	}

	public bool IsEquipped
	{
		get
		{
			return GetIsEquipped();
		}
		set
		{
			SetIsEquipped(value);
		}
	}

	public int Count
	{
		get
		{
			return GetCount();
		}
		set
		{
			SetCount(value);
		}
	}

	public long DeliveryTimestamp
	{
		get
		{
			return GetDeliveryTimestamp();
		}
		set
		{
			set_DeliveryTime(value);
		}
	}

	public int DeliveryUpgradeLevel
	{
		get
		{
			return GetDeliveryUpgradeLevel();
		}
		set
		{
			SetDeliveryUpgradeLevel(value);
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

	public XmlNode Node
	{
		get
		{
			return GetNode();
		}
	}

	public bool WritesToNode
	{
		get
		{
			return GetWritesToNode();
		}
	}

	public RecipeItemInfo RecipeDelivery
	{
		get
		{
			return GetRecipeDelivery();
		}
	}

	public ItemInfo CurrentUpgradeItem
	{
		get
		{
			return GetCurrentUpgradeItem();
		}
	}

	public ItemInfo NextUpgradeItem
	{
		get
		{
			return GetNextUpgradeItem();
		}
	}

	public bool HasUpgrades
	{
		get
		{
			return GetHasUpgrades();
		}
	}

	public bool IsUpgrade
	{
		get
		{
			return GetIsUpgrade();
		}
		set
		{
			SetIsUpgrade(value);
		}
	}

	public bool HasUpgradedItem
	{
		get
		{
			return GetHasUpgradedItem();
		}
	}

	public bool IsMaxUpgrade
	{
		get
		{
			return GetIsMaxUpgrade();
		}
	}

	public bool HasNextUpgrade
	{
		get
		{
			return GetHasNextUpgrade();
		}
	}

	public string AcquireType
	{
		get
		{
			return GetAcquireType();
		}
		set
		{
			SetAcquireType(value);
		}
	}

	public List<PerkInfoItem> Enchantments
	{
		get
		{
			return GetEnchantments();
		}
	}

	public bool IsOwned
	{
		get
		{
			return GetIsOwned();
		}
	}

	public UserItem(XmlNode node)
	{
		string itemName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		int itemCount = node.Attributes["Count"].ParseInt();
		int savedUpgradeLevel = node.Attributes["UpgradeLevel"].ParseInt(-1);
		bool equipped = node.Attributes["Equipped"].ParseBool();
		long deliveryTime = node.Attributes["DeliveryTime"].ParseLong(0L);
		int savedDeliveryUpgradeLevel = node.Attributes["DeliveryUpgradeLevel"].ParseInt(-1);
		string itemAcquireType = node.Attributes["AcquireType"].GetStringOrDefault("Item");
		if (node.Attributes["IsUpgrade"] != null)
		{
			bool flag = node.Attributes["IsUpgrade"].ParseBool();
			node.Attributes.RemoveNamedItem("IsUpgrade");
			itemAcquireType = ((!flag) ? "Item" : "Upgrade");
		}
		Init(node, itemName, equipped, itemCount, savedUpgradeLevel, deliveryTime, savedDeliveryUpgradeLevel, true, itemAcquireType);
	}

	public UserItem(XmlNode parentNode, string name, bool equipped, int count, int upgradeLevelValue = -1, long time = 0L, int deliveryUpgradeLevelValue = -1, bool shouldWriteToNode = true, string itemAcquireType = "Item")
	{
		XmlNode itemNode = parentNode.AppendElement("Item");
		Init(itemNode, name, equipped, count, upgradeLevelValue, time, deliveryUpgradeLevelValue, shouldWriteToNode, itemAcquireType);
	}

	public UserItem(ItemInfo sourceInfo, bool equipped, int count, int upgradeLevelValue = -1, long time = 0L, int deliveryUpgradeLevelValue = -1, bool shouldWriteToNode = true, string itemAcquireType = "Item")
	{
		if (ListSF.GetRoster().GetItemsNode() != null)
		{
			XmlNode itemNode = ListSF.GetRoster().GetItemsNode().AppendElement("Item");
			Init(itemNode, sourceInfo.Name, equipped, count, upgradeLevelValue, time, deliveryUpgradeLevelValue, shouldWriteToNode, itemAcquireType);
		}
	}

	public string get_Name()
	{
		return _Name;
	}

	public void set_Name(string value)
	{
		_Name = value;
		if (writesToNode)
		{
			if (_Node.Attributes["Name"] == null)
			{
				_Node.AppendAttribute("Name");
			}
			_Node.Attributes["Name"].Value = value;
		}
	}

	public ItemInfo GetInfo()
	{
		return itemInfo;
	}

	public void SetInfo(ItemInfo value)
	{
		// Preview inventory must not unlock shared shop definitions or mark seals new.
		// It can outlive the title through recovered timer listeners, so keep its
		// item instances detached even after the sandbox directory is released.
		itemInfo = Eclipse.Saves.CampaignSaveSession.PreviewDirectory != null ? value.Clone() : value;
		itemInfo.IsShopVisible = true;
		if (upgradeLevel == -1)
		{
			SetUpgradeLevel(itemInfo.UpgradeLevel);
		}
		else
		{
			SetUpgradeLevel(upgradeLevel);
		}
	}

	public bool GetIsEquipped()
	{
		return isEquipped;
	}

	public void SetIsEquipped(bool value)
	{
		isEquipped = value;
		if (writesToNode)
		{
			if (_Node.Attributes["Equipped"] == null)
			{
				_Node.AppendAttribute("Equipped");
			}
			_Node.Attributes["Equipped"].Value = ((!value) ? "0" : "1");
		}
	}

	public int GetCount()
	{
		return count;
	}

	public void SetCount(int value)
	{
		count = value;
		if (writesToNode)
		{
			if (_Node.Attributes["Count"] == null)
			{
				_Node.AppendAttribute("Count");
			}
			_Node.Attributes["Count"].Value = value.ToString();
		}
	}

	public long GetDeliveryTimestamp()
	{
		return _DeliveryTime;
	}

	public void set_DeliveryTime(long value)
	{
		_DeliveryTime = value;
		if (writesToNode)
		{
			if (_Node.Attributes["DeliveryTime"] == null)
			{
				_Node.AppendAttribute("DeliveryTime");
			}
			_Node.Attributes["DeliveryTime"].Value = value.ToString();
		}
	}

	public int GetDeliveryUpgradeLevel()
	{
		return deliveryUpgradeLevel;
	}

	public void SetDeliveryUpgradeLevel(int value)
	{
		deliveryUpgradeLevel = value;
		if (writesToNode)
		{
			if (_Node.Attributes["DeliveryUpgradeLevel"] == null)
			{
				_Node.AppendAttribute("DeliveryUpgradeLevel");
			}
			_Node.Attributes["DeliveryUpgradeLevel"].Value = value.ToString();
		}
	}

	public int GetUpgradeLevel()
	{
		return upgradeLevel;
	}

	public void SetUpgradeLevel(int value)
	{
		upgradeLevel = value;
		if (writesToNode)
		{
			if (_Node.Attributes["UpgradeLevel"] == null)
			{
				_Node.AppendAttribute("UpgradeLevel");
			}
			_Node.Attributes["UpgradeLevel"].Value = value.ToString();
		}
	}

	public XmlNode GetNode()
	{
		return _Node;
	}

	public bool GetWritesToNode()
	{
		return writesToNode;
	}

	public RecipeItemInfo GetRecipeDelivery()
	{
		return recipeDelivery;
	}

	public bool SetRecipeDelivery(RecipeItemInfo recipeItem)
	{
		if (recipeItem == null || recipeItem.GetUserItem() != this || recipeDelivery != null || !writesToNode)
			return false;
		Recipe recipe = recipeItem.GetRecipe();
		if (recipe == null) return false;
		XmlNode node = _Node["RecipeDelivery"] ?? _Node.AppendElement("RecipeDelivery");
		SetNodeAttribute(node, "Name", recipe.Name);
		SetNodeAttribute(node, "ItemLevel", recipeItem.ItemLevel.ToString());
		SetNodeAttribute(node, "PlayerLevel", recipeItem.PlayerLevel.ToString());
		SetNodeAttribute(node, "DeliveryTime", recipeItem.RecipeDeliveryTime.ToString());
		recipeDelivery = recipeItem;
		return true;
	}

	public void ClearRecipeDelivery()
	{
		if (writesToNode && _Node != null)
		{
			XmlNode node = _Node["RecipeDelivery"];
			if (node != null) _Node.RemoveChild(node);
		}
		recipeDelivery = null;
	}

	private static void SetNodeAttribute(XmlNode node, string name, string value)
	{
		XmlAttribute attribute = node.Attributes[name] ?? node.AppendAttribute(name);
		attribute.Value = value ?? string.Empty;
	}

	public ItemInfo GetCurrentUpgradeItem()
	{
		if (currentUpgradeItem == null)
		{
			return itemInfo;
		}
		return currentUpgradeItem;
	}

	public ItemInfo GetNextUpgradeItem()
	{
		return nextUpgradeItem;
	}

	public bool GetHasUpgrades()
	{
		return hasUpgrades;
	}

	public bool GetIsUpgrade()
	{
		return GetAcquireType().Equals("Upgrade");
	}

	public void SetIsUpgrade(bool value)
	{
		string newAcquireType = ((!value) ? "Item" : "Upgrade");
		SetAcquireType(newAcquireType);
	}

	public bool GetHasUpgradedItem()
	{
		return hasUpgradedItem;
	}

	public bool GetIsMaxUpgrade()
	{
		return isMaxUpgrade;
	}

	public bool GetHasNextUpgrade()
	{
		return hasNextUpgrade;
	}

	public string GetAcquireType()
	{
		return acquireType;
	}

	public void SetAcquireType(string value)
	{
		acquireType = value;
		if (writesToNode)
		{
			if (_Node.Attributes["AcquireType"] == null)
			{
				_Node.AppendAttribute("AcquireType");
			}
			_Node.Attributes["AcquireType"].Value = acquireType;
		}
	}

	// best guess for name
	public List<PerkInfoItem> GetEnchantments()
	{
		return enchantments;
	}

	public bool GetIsOwned()
	{
		return GetCount() > 0;
	}

	private void Init(XmlNode node, string name, bool equipped, int count, int upgradeLevelValue, long deliveryTime, int deliveryUpgradeLevelValue, bool shouldWriteToNode, string itemAcquireType)
	{
		_Node = node;
		writesToNode = shouldWriteToNode;
		itemInfo = null;
		recipeDelivery = null;
		currentUpgradeItem = null;
		nextUpgradeItem = null;
		hasUpgrades = false;
		hasUpgradedItem = false;
		isMaxUpgrade = false;
		set_Name(name);
		SetIsEquipped(equipped);
		SetCount(count);
		SetUpgradeLevel(upgradeLevelValue);
		set_DeliveryTime(deliveryTime);
		SetDeliveryUpgradeLevel(deliveryUpgradeLevelValue);
		SetAcquireType(itemAcquireType);
		if (node["Enchantments"] != null)
		{
			ParseEnchantments(node["Enchantments"]);
		}
		if (node["RecipeDelivery"] != null)
		{
			ParseRecipeDelivery(node["RecipeDelivery"]);
		}
	}

	private void ParseRecipeDelivery(XmlNode node)
	{
		recipeDelivery = new RecipeItemInfo(node, this);
	}

	private void ParseEnchantments(XmlNode node)
	{
		RemoveEnchantments();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkInfoItem perk = ItemInfo.ParsePerk(childNode);
			if (perk != null)
			{
				enchantments.Add(perk);
				EnsureExternalEnchantmentKind(childNode, perk);
			}
		}
	}

	private static void EnsureExternalEnchantmentKind(XmlNode node, PerkInfoItem perk)
	{
		if (node == null || perk == null || node.Name != "Perk") return;
		string name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		Eclipse.Modding.DefinitionId definitionId;
		if (!Eclipse.Modding.DefinitionId.TryParse(name, out definitionId) || definitionId.Category != "perks" ||
			definitionId.Namespace.Value == "core") return;
		string expectedKind = perk.Kind == PerkInfoItem.PerkKind.COMBO ? "Combo" : "Single";
		XmlAttribute kind = node.Attributes[PerkStruct.EclipseKindAttribute];
		if (kind == null || !string.Equals(kind.Value, expectedKind, System.StringComparison.Ordinal))
			node.AppendAttribute(PerkStruct.EclipseKindAttribute).Value = expectedKind;
	}

	private void RemoveEnchantments(bool includeCombo = true, bool removeNodes = false)
	{
		List<PerkInfoItem> list = new List<PerkInfoItem>();
		foreach (PerkInfoItem item in enchantments)
		{
			if (item.Kind != PerkInfoItem.PerkKind.COMBO || includeCombo)
			{
				list.Add(item);
			}
		}
		list.ForEach((PerkInfoItem perk) =>
		{
			enchantments.Remove(perk);
			if (removeNodes)
			{
				XmlNode xmlNode = _Node["Enchantments"];
				XmlNode oldChild = xmlNode.FindChildWithAttribute("Perk", "Name", perk.Name);
				xmlNode.RemoveChild(oldChild);
			}
		});
	}

	public void RefreshUpgradeState(int playerLevel)
	{
		currentUpgradeItem = null;
		nextUpgradeItem = null;
		hasUpgrades = false;
		hasUpgradedItem = false;
		isMaxUpgrade = false;
		hasNextUpgrade = false;
		if (itemInfo != null)
		{
			itemInfo.FindNextUpgradeItems(playerLevel, upgradeLevel, ref currentUpgradeItem, ref nextUpgradeItem);
			hasUpgrades = itemInfo.GetUpgrades().Count > 0;
			hasUpgradedItem = hasUpgrades && currentUpgradeItem != null;
			isMaxUpgrade = hasUpgrades && nextUpgradeItem == null;
			hasNextUpgrade = !isMaxUpgrade && nextUpgradeItem != null;
		}
	}

	public ItemInfo GetDisplayInfo(bool showUpgrades)
	{
		ItemInfo displayInfo = null;
		if (showUpgrades && GetHasUpgrades() && GetIsOwned())
		{
			displayInfo = GetNextUpgradeItem();
		}
		if (displayInfo == null && GetIsUpgrade())
		{
			displayInfo = GetCurrentUpgradeItem();
		}
		return (displayInfo == null) ? GetInfo() : displayInfo;
	}

	public ItemInfo GetEffectiveInfo()
	{
		ItemInfo deliveryInfo = null;
		if (GetDeliveryUpgradeLevel() > 0)
		{
			deliveryInfo = itemInfo.GetUpgradeItemByUpgradeLevel(GetDeliveryUpgradeLevel());
		}
		return (deliveryInfo == null) ? GetDisplayInfo(ListSF.GetRoster().GetShowUpgrades()) : deliveryInfo;
	}

	public void ApplyDefaultEnchantments()
	{
		int playerLevel = ListSF.GetRoster().GetLevel();
		int itemLevel = itemInfo.ItemLevel;
		ApplyEnchantments(itemInfo.DefaultEnchantments, itemLevel, playerLevel);
	}

	public void ApplyEnchantments(List<PerkStruct> perkStructs, int itemLevel, int playerLevel)
	{
		if (!writesToNode)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (PerkStruct item in perkStructs)
		{
			PerkInfoItem basePerk = GameUtils.PerkItemList.FindBasePerk(item.get_Name());
			if (basePerk != null)
			{
				if (basePerk.Kind == PerkInfoItem.PerkKind.SINGLE)
				{
					flag = true;
				}
				if (basePerk.Kind == PerkInfoItem.PerkKind.COMBO)
				{
					flag2 = true;
				}
			}
		}
		if (flag)
		{
			RemoveSingleEnchantments();
		}
		if (flag2)
		{
			RemoveComboEnchantments();
		}
		Roster roster = ListSF.GetRoster();
		roster.LevelOverride = playerLevel;
		roster.UseLevelOverride = true;
		if (perkStructs.Count > 0)
		{
			XmlNode enchantmentsNode = ((_Node["Enchantments"] != null) ? _Node["Enchantments"] : _Node.AppendElement("Enchantments"));
			foreach (PerkStruct item2 in perkStructs)
			{
					XmlNode xmlNode = enchantmentsNode.AppendNewNode("Perk");
					xmlNode.AppendAttribute("Name").Value = item2.get_Name();
					if (!string.IsNullOrEmpty(item2.EclipseEnchantment))
						xmlNode.AppendAttribute(PerkStruct.EclipseEnchantmentAttribute).Value = item2.EclipseEnchantment;
					if (!string.IsNullOrEmpty(item2.EclipseKind))
						xmlNode.AppendAttribute(PerkStruct.EclipseKindAttribute).Value = item2.EclipseKind;
					if (item2.EclipseParameters.Count > 0)
					{
						XmlNode parameters = xmlNode.AppendNewNode(Eclipse.Modding.ModEffectSaveData.NodeName);
						parameters.AppendAttribute("Format").Value = Eclipse.Modding.ModEffectSaveData.Format;
						foreach (KeyValuePair<string, string> parameter in item2.EclipseParameters)
						{
							XmlNode value = parameters.AppendNewNode(Eclipse.Modding.ModEffectSaveData.ParameterNodeName);
							value.AppendAttribute("Name").Value = parameter.Key;
							value.AppendAttribute("Value").Value = parameter.Value;
						}
					}
					if (item2.GetPairs().Count > 0)
					{
						XmlNode enchantmentNode = xmlNode.AppendNewNode("Set");
					PerkStruct perkStruct = new PerkStruct(item2);
					perkStruct.EvaluatePairValues();
					foreach (KeyValuePair<string, string> item3 in perkStruct.GetPairs())
					{
						enchantmentNode.AppendAttribute(item3.Key).Value = item3.Value;
					}
				}
				PerkInfoItem enchantmentPerk = ItemInfo.ParsePerk(xmlNode);
				if (enchantmentPerk != null)
				{
					enchantments.Add(enchantmentPerk);
				}
			}
		}
		roster.UseLevelOverride = false;
	}

	private void RemoveSingleEnchantments()
	{
		int num = 0;
		while (num < enchantments.Count)
		{
			PerkInfoItem enchantment = enchantments[num];
			if (enchantment.Kind != PerkInfoItem.PerkKind.COMBO)
			{
				enchantments.Remove(enchantment);
				XmlNode xmlNode = _Node["Enchantments"];
				XmlNode oldChild = xmlNode.FindChildWithAttribute("Perk", "Name", enchantment.Name);
				xmlNode.RemoveChild(oldChild);
			}
			else
			{
				num++;
			}
		}
		RemoveSavedExternalEnchantmentsByKind("Single");
	}

	private void RemoveComboEnchantments()
	{
		int num = 0;
		while (num < enchantments.Count)
		{
			PerkInfoItem enchantment = enchantments[num];
			if (enchantment.Kind == PerkInfoItem.PerkKind.COMBO)
			{
				enchantments.Remove(enchantment);
				XmlNode xmlNode = _Node["Enchantments"];
				XmlNode oldChild = xmlNode.FindChildWithAttribute("Perk", "Name", enchantment.Name);
				xmlNode.RemoveChild(oldChild);
			}
			else
			{
				num++;
			}
		}
		RemoveSavedExternalEnchantmentsByKind("Combo");
	}

	private void RemoveSavedExternalEnchantmentsByKind(string kind)
	{
		XmlNode enchantments = _Node["Enchantments"];
		if (enchantments == null) return;
		var remove = new List<XmlNode>();
		foreach (XmlNode child in enchantments.ChildNodes)
		{
			if (child.NodeType != XmlNodeType.Element || child.Name != "Perk") continue;
			string savedKind = child.Attributes[PerkStruct.EclipseKindAttribute].GetStringOrDefault(string.Empty);
			if (string.Equals(savedKind, kind, System.StringComparison.Ordinal) && IsEclipseOwnedSavedEnchantment(child))
				remove.Add(child);
		}
		for (int i = 0; i < remove.Count; i++) enchantments.RemoveChild(remove[i]);
	}

	private static bool IsEclipseOwnedSavedEnchantment(XmlNode node)
	{
		Eclipse.Modding.DefinitionId id;
		string enchantmentId = node.Attributes[PerkStruct.EclipseEnchantmentAttribute].GetStringOrDefault(string.Empty);
		if (Eclipse.Modding.DefinitionId.TryParse(enchantmentId, out id) && id.Namespace.Value != "core" &&
			id.Category == "enchantments") return true;
		string runtimeName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		return Eclipse.Modding.DefinitionId.TryParse(runtimeName, out id) && id.Namespace.Value != "core" &&
			id.Category == "perks";
	}

	public bool HasEnchantment(PerkInfoItem perk)
	{
		for (int i = 0; i < enchantments.Count; i++)
		{
			if (enchantments[i].Name == perk.Name)
			{
				return true;
			}
		}
		return false;
	}

	public void ReplaceEnchantments(List<PerkStruct> perkStructs, int itemLevel, int playerLevel)
	{
		RemoveEnchantmentsByName(perkStructs);
		ApplyEnchantments(perkStructs, itemLevel, playerLevel);
	}

	private void RemoveEnchantmentsByName(List<PerkStruct> perkStructs)
	{
		XmlNode xmlNode = _Node["Enchantments"];
		for (int i = 0; i < perkStructs.Count; i++)
		{
			PerkStruct perkStruct = perkStructs[i];
			foreach (PerkInfoItem item in enchantments)
			{
				if (enchantments[i].Name == perkStruct.get_Name())
				{
					enchantments.Remove(item);
					break;
				}
			}
			XmlNode xmlNode2 = xmlNode.FindChildWithAttribute("Perk", "Name", perkStruct.get_Name());
			if (xmlNode2 != null)
			{
				xmlNode.RemoveChild(xmlNode2);
			}
		}
	}
}
