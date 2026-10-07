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

	public UserItem(XmlNode EMOEJIOAKEG)
	{
		string gOHIIMFFFJI = EMOEJIOAKEG.Attributes["Name"].GetStringOrDefault(string.Empty);
		int bLJGEOEHIGP = EMOEJIOAKEG.Attributes["Count"].ParseInt();
		int gNLOCMLBNHF = EMOEJIOAKEG.Attributes["UpgradeLevel"].ParseInt(-1);
		bool cBDBANOPFDM = EMOEJIOAKEG.Attributes["Equipped"].ParseBool();
		long bMNFPNBAMAF = EMOEJIOAKEG.Attributes["DeliveryTime"].ParseLong(0L);
		int gIPFIKDILKL = EMOEJIOAKEG.Attributes["DeliveryUpgradeLevel"].ParseInt(-1);
		string aFGFKAANGLL = EMOEJIOAKEG.Attributes["AcquireType"].GetStringOrDefault("Item");
		if (EMOEJIOAKEG.Attributes["IsUpgrade"] != null)
		{
			bool flag = EMOEJIOAKEG.Attributes["IsUpgrade"].ParseBool();
			EMOEJIOAKEG.Attributes.RemoveNamedItem("IsUpgrade");
			aFGFKAANGLL = ((!flag) ? "Item" : "Upgrade");
		}
		Init(EMOEJIOAKEG, gOHIIMFFFJI, cBDBANOPFDM, bLJGEOEHIGP, gNLOCMLBNHF, bMNFPNBAMAF, gIPFIKDILKL, true, aFGFKAANGLL);
	}

	public UserItem(XmlNode FMBDAPOMFGN, string name, bool CBDBANOPFDM, int count, int GNLOCMLBNHF = -1, long time = 0L, int MDFLLEJODHJ = -1, bool KNGJACCPGPA = true, string AFGFKAANGLL = "Item")
	{
		XmlNode hKPPBKPJOEO = FMBDAPOMFGN.AppendElement("Item");
		Init(hKPPBKPJOEO, name, CBDBANOPFDM, count, GNLOCMLBNHF, time, MDFLLEJODHJ, KNGJACCPGPA, AFGFKAANGLL);
	}

	public UserItem(ItemInfo PJDAGCBPLJE, bool CBDBANOPFDM, int count, int GNLOCMLBNHF = -1, long time = 0L, int MDFLLEJODHJ = -1, bool KNGJACCPGPA = true, string AFGFKAANGLL = "Item")
	{
		if (ListSF.GetRoster().GetItemsNode() != null)
		{
			XmlNode hKPPBKPJOEO = ListSF.GetRoster().GetItemsNode().AppendElement("Item");
			Init(hKPPBKPJOEO, PJDAGCBPLJE.Name, CBDBANOPFDM, count, GNLOCMLBNHF, time, MDFLLEJODHJ, KNGJACCPGPA, AFGFKAANGLL);
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
		string bAINMLLIKOL = ((!value) ? "Item" : "Upgrade");
		SetAcquireType(bAINMLLIKOL);
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

	private void Init(XmlNode node, string name, bool CBDBANOPFDM, int count, int GNLOCMLBNHF, long BMNFPNBAMAF, int GIPFIKDILKL, bool KNGJACCPGPA, string AFGFKAANGLL)
	{
		_Node = node;
		writesToNode = KNGJACCPGPA;
		itemInfo = null;
		recipeDelivery = null;
		currentUpgradeItem = null;
		nextUpgradeItem = null;
		hasUpgrades = false;
		hasUpgradedItem = false;
		isMaxUpgrade = false;
		set_Name(name);
		SetIsEquipped(CBDBANOPFDM);
		SetCount(count);
		SetUpgradeLevel(GNLOCMLBNHF);
		set_DeliveryTime(BMNFPNBAMAF);
		SetDeliveryUpgradeLevel(GIPFIKDILKL);
		SetAcquireType(AFGFKAANGLL);
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
			PerkInfoItem aCONCDFDNJH = ItemInfo.ParsePerk(childNode);
			if (aCONCDFDNJH != null)
			{
				enchantments.Add(aCONCDFDNJH);
				EnsureExternalEnchantmentKind(childNode, aCONCDFDNJH);
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

	private void RemoveEnchantments(bool KBLMKFKJHCE = true, bool removeNodes = false)
	{
		List<PerkInfoItem> list = new List<PerkInfoItem>();
		foreach (PerkInfoItem item in enchantments)
		{
			if (item.Kind != PerkInfoItem.PerkKind.COMBO || KBLMKFKJHCE)
			{
				list.Add(item);
			}
		}
		list.ForEach((PerkInfoItem DHDMNHCIPEH) =>
		{
			enchantments.Remove(DHDMNHCIPEH);
			if (removeNodes)
			{
				XmlNode xmlNode = _Node["Enchantments"];
				XmlNode oldChild = xmlNode.FindChildWithAttribute("Perk", "Name", DHDMNHCIPEH.Name);
				xmlNode.RemoveChild(oldChild);
			}
		});
	}

	public void RefreshUpgradeState(int OMHDLKNHNMJ)
	{
		currentUpgradeItem = null;
		nextUpgradeItem = null;
		hasUpgrades = false;
		hasUpgradedItem = false;
		isMaxUpgrade = false;
		hasNextUpgrade = false;
		if (itemInfo != null)
		{
			itemInfo.FindNextUpgradeItems(OMHDLKNHNMJ, upgradeLevel, ref currentUpgradeItem, ref nextUpgradeItem);
			hasUpgrades = itemInfo.GetUpgrades().Count > 0;
			hasUpgradedItem = hasUpgrades && currentUpgradeItem != null;
			isMaxUpgrade = hasUpgrades && nextUpgradeItem == null;
			hasNextUpgrade = !isMaxUpgrade && nextUpgradeItem != null;
		}
	}

	public ItemInfo GetDisplayInfo(bool EECHKLPPCKH)
	{
		ItemInfo dJKEECEOCJB = null;
		if (EECHKLPPCKH && GetHasUpgrades() && GetIsOwned())
		{
			dJKEECEOCJB = GetNextUpgradeItem();
		}
		if (dJKEECEOCJB == null && GetIsUpgrade())
		{
			dJKEECEOCJB = GetCurrentUpgradeItem();
		}
		return (dJKEECEOCJB == null) ? GetInfo() : dJKEECEOCJB;
	}

	public ItemInfo GetEffectiveInfo()
	{
		ItemInfo dJKEECEOCJB = null;
		if (GetDeliveryUpgradeLevel() > 0)
		{
			dJKEECEOCJB = itemInfo.GetUpgradeItemByUpgradeLevel(GetDeliveryUpgradeLevel());
		}
		return (dJKEECEOCJB == null) ? GetDisplayInfo(ListSF.GetRoster().GetShowUpgrades()) : dJKEECEOCJB;
	}

	public void ApplyDefaultEnchantments()
	{
		int mHNCENBCECJ = ListSF.GetRoster().GetLevel();
		int mHGODOLNDLE = itemInfo.ItemLevel;
		ApplyEnchantments(itemInfo.DefaultEnchantments, mHGODOLNDLE, mHNCENBCECJ);
	}

	public void ApplyEnchantments(List<PerkStruct> HALHGEGADKA, int MPAGFAKIEJG, int MHNCENBCECJ)
	{
		if (!writesToNode)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (PerkStruct item in HALHGEGADKA)
		{
			PerkInfoItem aCONCDFDNJH = GameUtils.PerkItemList.FindBasePerk(item.get_Name());
			if (aCONCDFDNJH != null)
			{
				if (aCONCDFDNJH.Kind == PerkInfoItem.PerkKind.SINGLE)
				{
					flag = true;
				}
				if (aCONCDFDNJH.Kind == PerkInfoItem.PerkKind.COMBO)
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
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		nKGLHEGIKKP.LevelOverride = MHNCENBCECJ;
		nKGLHEGIKKP.UseLevelOverride = true;
		if (HALHGEGADKA.Count > 0)
		{
			XmlNode mEEAKLDGLDF = ((_Node["Enchantments"] != null) ? _Node["Enchantments"] : _Node.AppendElement("Enchantments"));
			foreach (PerkStruct item2 in HALHGEGADKA)
			{
					XmlNode xmlNode = mEEAKLDGLDF.AppendNewNode("Perk");
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
						XmlNode mEEAKLDGLDF2 = xmlNode.AppendNewNode("Set");
					PerkStruct jLFJOECODOF = new PerkStruct(item2);
					jLFJOECODOF.EvaluatePairValues();
					foreach (KeyValuePair<string, string> item3 in jLFJOECODOF.GetPairs())
					{
						mEEAKLDGLDF2.AppendAttribute(item3.Key).Value = item3.Value;
					}
				}
				PerkInfoItem aCONCDFDNJH2 = ItemInfo.ParsePerk(xmlNode);
				if (aCONCDFDNJH2 != null)
				{
					enchantments.Add(aCONCDFDNJH2);
				}
			}
		}
		nKGLHEGIKKP.UseLevelOverride = false;
	}

	private void RemoveSingleEnchantments()
	{
		int num = 0;
		while (num < enchantments.Count)
		{
			PerkInfoItem aCONCDFDNJH = enchantments[num];
			if (aCONCDFDNJH.Kind != PerkInfoItem.PerkKind.COMBO)
			{
				enchantments.Remove(aCONCDFDNJH);
				XmlNode xmlNode = _Node["Enchantments"];
				XmlNode oldChild = xmlNode.FindChildWithAttribute("Perk", "Name", aCONCDFDNJH.Name);
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
			PerkInfoItem aCONCDFDNJH = enchantments[num];
			if (aCONCDFDNJH.Kind == PerkInfoItem.PerkKind.COMBO)
			{
				enchantments.Remove(aCONCDFDNJH);
				XmlNode xmlNode = _Node["Enchantments"];
				XmlNode oldChild = xmlNode.FindChildWithAttribute("Perk", "Name", aCONCDFDNJH.Name);
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

	public bool HasEnchantment(PerkInfoItem AEFFHJGMNFI)
	{
		for (int i = 0; i < enchantments.Count; i++)
		{
			if (enchantments[i].Name == AEFFHJGMNFI.Name)
			{
				return true;
			}
		}
		return false;
	}

	public void ReplaceEnchantments(List<PerkStruct> HALHGEGADKA, int MPAGFAKIEJG, int MHNCENBCECJ)
	{
		RemoveEnchantmentsByName(HALHGEGADKA);
		ApplyEnchantments(HALHGEGADKA, MPAGFAKIEJG, MHNCENBCECJ);
	}

	private void RemoveEnchantmentsByName(List<PerkStruct> NIBJKBMNOKG)
	{
		XmlNode xmlNode = _Node["Enchantments"];
		for (int i = 0; i < NIBJKBMNOKG.Count; i++)
		{
			PerkStruct jLFJOECODOF = NIBJKBMNOKG[i];
			foreach (PerkInfoItem item in enchantments)
			{
				if (enchantments[i].Name == jLFJOECODOF.get_Name())
				{
					enchantments.Remove(item);
					break;
				}
			}
			XmlNode xmlNode2 = xmlNode.FindChildWithAttribute("Perk", "Name", jLFJOECODOF.get_Name());
			if (xmlNode2 != null)
			{
				xmlNode.RemoveChild(xmlNode2);
			}
		}
	}
}
