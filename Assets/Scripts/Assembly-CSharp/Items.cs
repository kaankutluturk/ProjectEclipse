using System.Collections.Generic;
using System.Xml;
using SF2.Offline;

public class Items
{
	private string _fileName = "/list.xml";

	private List<ItemInfo> weapons = new List<ItemInfo>();

	private List<ItemInfo> armors = new List<ItemInfo>();

	private List<ItemInfo> helms = new List<ItemInfo>();

	private List<ItemInfo> rangedWeapons = new List<ItemInfo>();

	private List<ItemInfo> magicItems = new List<ItemInfo>();

	private List<ItemInfo> seals = new List<ItemInfo>();

	private List<ItemInfo> realMoneyItems = new List<ItemInfo>();

	private List<ItemInfo> consumables = new List<ItemInfo>();

	private List<ItemInfo> freeItems = new List<ItemInfo>();

	private List<ItemInfo> allItems = new List<ItemInfo>();

	private List<UpgradeDataContainer> upgradeContainers = new List<UpgradeDataContainer>();

	private ItemSets itemSets = new ItemSets();

	public List<ItemInfo> Weapons
	{
		get
		{
			return GetWeapons();
		}
	}

	public List<ItemInfo> Armors
	{
		get
		{
			return GetArmors();
		}
	}

	public List<ItemInfo> Helms
	{
		get
		{
			return GetHelms();
		}
	}

	public List<ItemInfo> RangedWeapons
	{
		get
		{
			return GetRangedWeapons();
		}
	}

	public List<ItemInfo> MagicItems
	{
		get
		{
			return GetMagicItems();
		}
	}

	public List<ItemInfo> Seals
	{
		get
		{
			return GetSeals();
		}
	}

	public List<ItemInfo> RealMoneyItems
	{
		get
		{
			return GetRealMoneyItems();
		}
	}

	public List<ItemInfo> Consumables
	{
		get
		{
			return GetConsumables();
		}
	}

	public List<ItemInfo> FreeItems
	{
		get
		{
			return GetFreeItems();
		}
	}

	// best guess for name
	public List<ItemInfo> AllItems
	{
		get
		{
			return GetAllItems();
		}
	}

	public List<UpgradeDataContainer> UpgradeContainers
	{
		get
		{
			return GetUpgradeContainers();
		}
	}

	public ItemSets Sets
	{
		get
		{
			return GetItemSets();
		}
	}

	private List<ItemInfo> ProductItems
	{
		get
		{
			return GetProductItems();
		}
	}

	public int NewItemsCount
	{
		get
		{
			return GetNewItemsCount();
		}
	}

	public List<ItemInfo> GetWeapons()
	{
		return weapons;
	}

	public List<ItemInfo> GetArmors()
	{
		return armors;
	}

	public List<ItemInfo> GetHelms()
	{
		return helms;
	}

	public List<ItemInfo> GetRangedWeapons()
	{
		return rangedWeapons;
	}

	public List<ItemInfo> GetMagicItems()
	{
		return magicItems;
	}

	public List<ItemInfo> GetSeals()
	{
		return seals;
	}

	public List<ItemInfo> GetRealMoneyItems()
	{
		return realMoneyItems;
	}

	public List<ItemInfo> GetConsumables()
	{
		return consumables;
	}

	public List<ItemInfo> GetFreeItems()
	{
		return freeItems;
	}

	public List<ItemInfo> GetAllItems()
	{
		return allItems;
	}

	public List<UpgradeDataContainer> GetUpgradeContainers()
	{
		return upgradeContainers;
	}

	public ItemSets GetItemSets()
	{
		return itemSets;
	}

	private List<ItemInfo> GetProductItems()
	{
		List<ItemInfo> list = new List<ItemInfo>();
		foreach (ItemInfo item in allItems)
		{
			if (item.Type == "RealMoneyItem" && !string.IsNullOrEmpty(item.GetMarketId()))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public int GetNewItemsCount()
	{
		int count = 0;
		allItems.ForEach((ItemInfo itemInfo) =>
		{
			if (!itemInfo.Type.Equals("Seal") && itemInfo.GetIsNew())
			{
				count++;
			}
		});
		return count;
	}

	public int GetCountNewItemsByType(string itemType)
	{
		return allItems.FindAll((ItemInfo candidate) => candidate.Type.Equals(itemType) && candidate.GetIsNew()).Count;
	}

	// best guess for name
	public ItemInfo GetItemByName(string name)
	{
		ItemInfo item = allItems.Find((ItemInfo candidate) => candidate.Name.Equals(name));
		if (item != null) return item;
		Eclipse.Modding.DefinitionId id;
		Eclipse.Modding.ItemDefinition definition;
		Eclipse.Modding.ModScriptSession scripts = Eclipse.Modding.ModRuntime.Scripts;
		if (scripts != null && Eclipse.Modding.DefinitionId.TryParse(name, out id) &&
			scripts.Content.TryResolveItem(id, out definition))
		{
			if (definition.IsCore && definition.LegacyName != null)
			{
				ItemInfo legacy = allItems.Find(value => value.Name == definition.LegacyName && value.NodeXML != null &&
					definition.LegacyItemXml != null && value.NodeXML.OuterXml == definition.LegacyItemXml);
				return legacy ?? allItems.Find(value => value.Name == definition.LegacyName);
			}
			return allItems.Find(value => value.Name == definition.Id.ToString());
		}
		return null;
	}

	public List<ItemInfo> GetItemsByType(string itemType)
	{
		switch (itemType)
		{
		case "Weapon":
			return GetWeapons();
		case "Armor":
			return GetArmors();
		case "Helm":
			return GetHelms();
		case "Ranged":
			return GetRangedWeapons();
		case "Magic":
			return GetMagicItems();
		case "RealMoneyItem":
			return GetRealMoneyItems();
		case "Consumable":
			return GetConsumables();
		case "Free":
			return GetFreeItems();
		case "Seal":
			return GetSeals();
		default:
			return null;
		}
	}

	public List<ItemInfo> GetItemsByMarketId(string marketId)
	{
		List<ItemInfo> list = new List<ItemInfo>();
		int i = 0;
		for (int count = allItems.Count; i < count; i++)
		{
			if (allItems[i].GetMarketId() == marketId)
			{
				list.Add(allItems[i]);
			}
		}
		return list;
	}

	private ItemInfo ParseItem(XmlNode node, int index)
	{
		ItemInfo itemInfo = new ItemInfo(node);
		itemInfo.NodeXML = node.CloneNode(true);
		XmlNode xmlNode = node["Upgrades"];
		if (xmlNode != null)
		{
			string templateName = xmlNode.Attributes["Template"].GetStringOrDefault(string.Empty);
			foreach (XmlNode item in xmlNode)
			{
				UpgradeData upgradeData = new UpgradeData(item, templateName);
				itemInfo.AddLocalUpgrade(upgradeData);
			}
		}
		itemInfo.SortLocalUpgrades();
		itemInfo.Index = index;
		return itemInfo;
	}

	// Eclipse-owned mod content is validated before reaching this recovered container.
	// Keep the integration seam here deliberately tiny so the original item parser remains
	// authoritative for ItemInfo defaults, attributes and upgrade-template semantics.
	public ItemInfo AddExternalItem(XmlNode node)
	{
		if (node == null)
		{
			throw new System.ArgumentNullException("node");
		}
		XmlAttribute nameAttribute = node.Attributes["Name"];
		string name = (nameAttribute == null) ? string.Empty : nameAttribute.Value;
		if (string.IsNullOrEmpty(name))
		{
			throw new System.InvalidOperationException("External item requires a Name attribute.");
		}
		if (GetItemByName(name) != null)
		{
			throw new System.InvalidOperationException("Item already exists: " + name);
		}

		ItemInfo item = ParseItem(node, GetAllItems().Count);
		List<ItemInfo> category = GetItemsByType(item.Type);
		if (category == null || (item.Type != "Weapon" && item.Type != "Armor" && item.Type != "Helm" &&
			item.Type != "Ranged" && item.Type != "Magic" && item.Type != "Consumable" &&
			item.Type != "Free" && item.Type != "Seal"))
		{
			throw new System.InvalidOperationException("Unsupported external item type '" + item.Type + "': " + name);
		}
		GetAllItems().Add(item);
		category.Add(item);
		return item;
	}

	public bool RemoveExternalItem(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		ItemInfo item = GetItemByName(name);
		if (item == null)
		{
			return false;
		}
		List<ItemInfo> category = GetItemsByType(item.Type);
		if (category == null) return false;
		category.Remove(item);
		GetAllItems().Remove(item);
		return true;
	}

	public ItemInfo AddExternalWeapon(XmlNode node)
	{
		if (node == null || node.Attributes?["Type"]?.Value != "Weapon")
			throw new System.InvalidOperationException("External weapon seam requires Type=Weapon.");
		return AddExternalItem(node);
	}

	public bool RemoveExternalWeapon(string name)
	{
		ItemInfo item = GetItemByName(name);
		return item != null && item.Type == "Weapon" && RemoveExternalItem(name);
	}

	private void ParseUpgradeList(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			UpgradeDataContainer upgradeContainer = new UpgradeDataContainer();
			upgradeContainer.Type = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			foreach (XmlNode childNode2 in childNode.ChildNodes)
			{
				UpgradeData item = new UpgradeData(childNode2, upgradeContainer.Type);
				upgradeContainer.Upgrades.Add(item);
			}
			upgradeContainers.Add(upgradeContainer);
		}
	}

	// best guess for name
	public UpgradeDataContainer GetUpgradeDataContainerByName(string containerName)
	{
		foreach (UpgradeDataContainer item in upgradeContainers)
		{
			if (item.Type.Equals(containerName))
			{
				return item;
			}
		}
		return null;
	}

	public void LoadItems(string path)
	{
		XmlDocument xmlDocument = null;
		xmlDocument = XmlUtils.OpenXMLDocument(path + _fileName, string.Empty, XmlUtils.XmlSourceMode.Normal, true, XmlCryptoUtils.GetIsEncryptionEnabled());
		if (xmlDocument == null)
		{
			GameLog.Error("Items.ParseItems xmlDocument == null");
			return;
		}
		XmlNode upgradeListNode = xmlDocument["List"]["UpgradeList"];
		ParseUpgradeList(upgradeListNode);
		XmlNode xmlNode = xmlDocument["List"]["Items"];
		int num = 0;
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			ItemInfo itemInfo = ParseItem(childNode, num);
			GetAllItems().Add(itemInfo);
			num++;
			switch (itemInfo.Type)
			{
			case "Weapon":
				GetWeapons().Add(itemInfo);
				break;
			case "Armor":
				GetArmors().Add(itemInfo);
				break;
			case "Helm":
				GetHelms().Add(itemInfo);
				break;
			case "Ranged":
				GetRangedWeapons().Add(itemInfo);
				break;
			case "Magic":
				GetMagicItems().Add(itemInfo);
				break;
			case "RealMoneyItem":
				GetRealMoneyItems().Add(itemInfo);
				break;
			case "Consumable":
				GetConsumables().Add(itemInfo);
				break;
			case "Free":
				GetFreeItems().Add(itemInfo);
				break;
			case "Seal":
				GetSeals().Add(itemInfo);
				break;
			}
		}
		XmlNode hKPPBKPJOEO3 = xmlDocument["List"]["ItemSets"];
		GetItemSets().Parse(hKPPBKPJOEO3);
	}

	public void ClearNewItemFlags()
	{
		foreach (ItemInfo item in allItems)
		{
			if (item.GetIsNew())
			{
				item.SetIsNew(false);
			}
		}
	}

	public void SetNewAddItem(string itemName, bool value, int itemLevel)
	{
		SetNewAddItem(GetItemByName(itemName), value, itemLevel);
	}

	public void SetNewAddItem(ItemInfo item, bool value, int itemLevel)
	{
		bool flag = item.GroupId == string.Empty || ListSF.GetRoster().HasShopLock(item.GroupId);
		bool flag2 = itemLevel == item.ItemLevel;
		if (!item.IsHidden() && flag && flag2)
		{
			item.SetIsNew(value);
		}
	}

	public void ClearNewFlagsForGroup(string groupId)
	{
		List<ItemInfo> list = GetAllItems();
		foreach (ItemInfo item in list)
		{
			if (item.IsShopVisible && item.GetIsNew() && item.GroupId == groupId)
			{
				item.SetIsNew(false);
			}
		}
	}

	public ProductDefinition[] GetProductDefinitions()
	{
		List<ProductDefinition> list = new List<ProductDefinition>();
		HashSet<string> hashSet = new HashSet<string>();
		foreach (ItemInfo item in allItems)
		{
			if (item.Type == "RealMoneyItem" && !string.IsNullOrEmpty(item.GetMarketId()) && !hashSet.Contains(item.GetMarketId()))
			{
				hashSet.Add(item.GetMarketId());
				list.Add(new ProductDefinition(item.GetMarketId(), (!item.GetIsConsumable()) ? ProductType.NonConsumable : ProductType.Consumable));
			}
		}
		return list.ToArray();
	}

	public void ApplyStoreProducts(Product[] products)
	{
		if (products != null && products.Length != 0)
		{
			List<ItemInfo> productItems = GetProductItems();
			foreach (Product product in products)
			{
				ApplyStoreProduct(product, productItems);
			}
		}
	}

	private void ApplyStoreProduct(Product product, List<ItemInfo> productItems)
	{
		foreach (ItemInfo item in productItems)
		{
			if (item.GetMarketId() == product.definition.id)
			{
				item.ApplyProductMetadata(product.metadata);
			}
		}
	}

	public void RandomizeObscuredVars()
	{
		GetAllItems().ForEach((ItemInfo itemInfo) =>
		{
			itemInfo.RandomizeObscuredVars();
		});
		GetUpgradeContainers().ForEach((UpgradeDataContainer container) =>
		{
			container.RandomizeObscuredVars();
		});
	}
}
