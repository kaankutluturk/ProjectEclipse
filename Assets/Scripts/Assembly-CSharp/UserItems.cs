using System.Collections.Generic;
using System.Xml;
using Nekki.Utils;
using UnityEngine.Events;

public class UserItems
{
	public class ItemsDeliveredEvent : UnityEvent<List<UserItem>>
	{
	}

	public class ItemDeliveredEvent : UnityEvent<UserItem>
	{
	}

	public ItemsDeliveredEvent ItemsDelivered = new ItemsDeliveredEvent();

	public ItemDeliveredEvent ItemDelivered = new ItemDeliveredEvent();

	private List<UserItem> _items = new List<UserItem>();

	private readonly List<string> _missingModItemIds = new List<string>();

	public IReadOnlyList<string> MissingModItemIds => _missingModItemIds.AsReadOnly();

	private List<UserItem> pendingDeliveries = new List<UserItem>();

	public List<UserItem> DeliveredItems = new List<UserItem>();

	public List<UserItem> DeliveredUpgrades = new List<UserItem>();

	public List<RecipeItemInfo> DeliveredRecipes = new List<RecipeItemInfo>();

	public List<UserItem> Items
	{
		get
		{
			return GetItems();
		}
	}

	public List<UserItem> DeliveredItemList
	{
		get
		{
			return GetDeliveredItems();
		}
	}

	public List<UserItem> DeliveredUpgradeList
	{
		get
		{
			return GetDeliveredUpgrades();
		}
	}

	public UserItems()
	{
		GlobalTimer.get_Instance().addEventListener(0, OnTimerTick);
	}

	public List<UserItem> GetItems()
	{
		return _items;
	}

	public List<UserItem> GetDeliveredItems()
	{
		return DeliveredItems;
	}

	public List<UserItem> GetDeliveredUpgrades()
	{
		return DeliveredUpgrades;
	}

	public void Parse(XmlNode EPGOOPEHFMO)
	{
		_missingModItemIds.Clear();
		if (EPGOOPEHFMO == null)
		{
			return;
		}
		foreach (XmlNode childNode in EPGOOPEHFMO.ChildNodes)
		{
			if (childNode.NodeType != XmlNodeType.Element) continue;
			if (Eclipse.Modding.ModSaveData.IsMissingItem(childNode, name => ListSF.GetItems().GetItemByName(name) != null))
			{
				_missingModItemIds.Add(childNode.Attributes["Name"].Value);
				continue; // Preserve the entire save node, but do not run delivery/equipment logic on it.
			}
			UserItem item = new UserItem(childNode);
			if (Eclipse.Modding.ModSaveData.IsExternalItem(item.get_Name()))
			{
				ItemInfo definition = ListSF.GetItems().GetItemByName(item.get_Name());
				XmlAttribute equipped = EPGOOPEHFMO.ParentNode?.Attributes?[definition.Type];
				if (equipped != null) item.SetIsEquipped(equipped.Value == item.get_Name());
			}
			AddItem(item);
		}
		if (_missingModItemIds.Count > 0)
			UnityEngine.Debug.LogWarning("[ModSave] Preserved " + _missingModItemIds.Count +
				" unavailable mod item(s) in the save: " + string.Join(", ", _missingModItemIds));
	}

	public UserItem AddItem(UserItem value, bool HPCLCADMKCG = false)
	{
		if (value.GetInfo() != null && value.GetInfo().Type == "Seal")
		{
			value.GetInfo().SetIsNew(true);
		}
		if (HPCLCADMKCG)
		{
			UserItem dKCHDHMLKHN = _items.Find((UserItem DHDMNHCIPEH) => DHDMNHCIPEH.get_Name().Equals(value.get_Name()));
			if (dKCHDHMLKHN != null)
			{
				int index = _items.IndexOf(dKCHDHMLKHN);
				_items.Insert(index, value);
				_items.Remove(dKCHDHMLKHN);
			}
			else
			{
				_items.Add(value);
			}
		}
		else
		{
			_items.Add(value);
		}
		if (value.GetDeliveryTimestamp() > 0)
		{
			pendingDeliveries.Add(value);
		}
		return value;
	}

	public UserItem FindItem(ItemInfo item)
	{
		return (item == null) ? null : FindItem(item.Name);
	}

	public UserItem FindItem(string name)
	{
		foreach (UserItem item in _items)
		{
			if (item.get_Name() == name)
			{
				return item;
			}
		}
		if (Eclipse.Modding.ModSaveData.IsExternalItem(name))
		{
			ItemInfo requested = ListSF.GetItems().GetItemByName(name);
			if (requested != null)
			{
				foreach (UserItem item in _items)
				{
					if (!Eclipse.Modding.ModSaveData.IsExternalItem(item.get_Name())) continue;
					if (ListSF.GetItems().GetItemByName(item.get_Name()) == requested) return item;
				}
			}
		}
		return null;
	}

	public List<UserItem> FindItemsByType(string LFLGCDNKNJI, string GIGAFKGDKNH = "", bool isActive = true)
	{
		List<UserItem> list = new List<UserItem>();
		foreach (UserItem item in _items)
		{
			ItemInfo dJKEECEOCJB = item.GetInfo();
			if (dJKEECEOCJB != null)
			{
				bool flag = dJKEECEOCJB.Type.Equals(LFLGCDNKNJI);
				bool flag2 = dJKEECEOCJB.SubType.Equals(GIGAFKGDKNH) || GIGAFKGDKNH.Equals(string.Empty);
				bool flag3 = dJKEECEOCJB.IsShopVisible || !isActive;
				if (flag && flag2 && flag3)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public bool HasItem(ItemInfo item)
	{
		UserItem dKCHDHMLKHN = FindItem(item);
		return dKCHDHMLKHN != null && dKCHDHMLKHN.GetCount() > 0;
	}

	public void ResetToDefaultItem(ItemInfo item, bool GHLLDFNGMAE)
	{
		if (item == null)
		{
			return;
		}
		string gOHIIMFFFJI = GameUtils.GetDefaultItem(item.Type);
		UserItem dKCHDHMLKHN = FindItem(gOHIIMFFFJI);
		if (dKCHDHMLKHN == null || dKCHDHMLKHN.GetInfo() == null)
		{
			return;
		}
		ListSF.GetRoster().get_Parameters().SetItemByType(dKCHDHMLKHN.GetInfo().Type, dKCHDHMLKHN.GetInfo());
		if (GHLLDFNGMAE)
		{
			UserItem dKCHDHMLKHN2 = FindItem(item);
			if (dKCHDHMLKHN2 != null)
			{
				var activeSets = IsPlayerItems() ? Eclipse.UI.SetBonusNotice.ActiveCombos() : null;
				dKCHDHMLKHN2.SetIsEquipped(false);
				dKCHDHMLKHN.SetIsEquipped(true);
				ListSF.GetRoster().EquipItem(dKCHDHMLKHN);
				ListSF.GetRoster().RequestSave();
				Eclipse.UI.SetBonusNotice.AnnounceNew(activeSets);
			}
		}
	}

	public void EquipItem(ItemInfo item, bool GHLLDFNGMAE)
	{
		if (item == null)
		{
			return;
		}
		ListSF.GetRoster().get_Parameters().SetItemByType(item.Type, item);
		if (!GHLLDFNGMAE)
		{
			return;
		}
		UserItem dKCHDHMLKHN = FindItem(item);
		if (dKCHDHMLKHN != null && dKCHDHMLKHN.GetIsOwned())
		{
			var activeSets = IsPlayerItems() ? Eclipse.UI.SetBonusNotice.ActiveCombos() : null;
			List<UserItem> list = FindItemsByType(item.Type, string.Empty);
			list.ForEach((UserItem DHDMNHCIPEH) =>
			{
				DHDMNHCIPEH.SetIsEquipped(false);
			});
			dKCHDHMLKHN.SetIsEquipped(true);
			ListSF.GetRoster().EquipItem(dKCHDHMLKHN);
			ListSF.GetRoster().RequestSave();
			Eclipse.UI.SetBonusNotice.AnnounceNew(activeSets);
		}
	}

	private bool IsPlayerItems()
	{
		Roster roster = ListSF.GetRoster();
		return roster != null && ReferenceEquals(this, roster.GetInventory());
	}

	public void ApplyItemInfos(List<ItemInfo> HELFDCAIJNE)
	{
		ModelParameters kIKOGDEPGHB = ListSF.GetRoster().get_Parameters();
		if (kIKOGDEPGHB == null)
		{
			return;
		}
		kIKOGDEPGHB.DecorateItems.Clear();
		foreach (ItemInfo item in HELFDCAIJNE)
		{
			UserItem dKCHDHMLKHN = FindItem(item);
			if (dKCHDHMLKHN != null)
			{
				dKCHDHMLKHN.SetInfo(item);
				if (dKCHDHMLKHN.GetIsEquipped() && item.Type.Equals("Decorate"))
				{
					kIKOGDEPGHB.DecorateItems.Add(item);
				}
			}
		}
		RefreshUpgradeStates();
	}

	public void RefreshUpgradeStates()
	{
		_items.ForEach((UserItem DHDMNHCIPEH) =>
		{
			DHDMNHCIPEH.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
		});
	}

	public void ProcessDeliveries()
	{
		List<UserItem> list = new List<UserItem>();
		bool flag = false;
		foreach (UserItem item in pendingDeliveries)
		{
			if (item.GetDeliveryTimestamp() <= 0)
			{
				list.Add(item);
			}
			else
			{
				// Migrate any delivery already persisted by an older build on the next
				// timer tick. New shop purchases no longer enter this queue.
				CompleteDelivery(item);
				list.Add(item);
				flag = true;
			}
		}
		if (list.Count > 0)
		{
			if (flag)
			{
				ItemsDelivered.Invoke(list);
			}
			list.ForEach((UserItem DHDMNHCIPEH) =>
			{
				pendingDeliveries.Remove(DHDMNHCIPEH);
			});
		}
	}

	public void CompleteDelivery(UserItem NDMCFNGEPOA)
	{
		if (NDMCFNGEPOA != null && NDMCFNGEPOA.GetDeliveryTimestamp() > 0)
		{
			Roster acquisitionRoster = ListSF.GetRoster();
			int acquisitionProfile = Eclipse.Modding.ModRuntime.StoryEvents.ProfileGeneration;
			int previousCount = -1;
			if (NDMCFNGEPOA.GetIsUpgrade())
			{
				GetDeliveredUpgrades().Add(NDMCFNGEPOA);
			}
			else
			{
				GetDeliveredItems().Add(NDMCFNGEPOA);
			}
			ItemDelivered.Invoke(NDMCFNGEPOA);
			QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
			hHKLFIIBIFF.purchasedItem = NDMCFNGEPOA.GetInfo();
			if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_DELIVERY))
			{
				ListSF.GetInstance().RunQuestActions();
			}
			if (NDMCFNGEPOA.GetCount() <= 0)
			{
				previousCount = NDMCFNGEPOA.GetCount();
				NDMCFNGEPOA.SetCount(1);
				NDMCFNGEPOA.set_DeliveryTime(-1L);
			}
			if (NDMCFNGEPOA.GetDeliveryUpgradeLevel() > 0 && NDMCFNGEPOA.GetDeliveryUpgradeLevel() > NDMCFNGEPOA.GetUpgradeLevel())
			{
				NDMCFNGEPOA.SetUpgradeLevel(NDMCFNGEPOA.GetDeliveryUpgradeLevel());
				NDMCFNGEPOA.set_DeliveryTime(-1L);
			}
			NDMCFNGEPOA.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
			ListSF.GetRoster().RequestSave();
			if (previousCount >= 0 && ReferenceEquals(this, acquisitionRoster.GetInventory()))
				Eclipse.Modding.ModRuntime.PublishItemAcquired(acquisitionRoster, NDMCFNGEPOA.GetInfo(), previousCount, 1, acquisitionProfile);
		}
	}

	public UserItem GetRandomItemOfType(string LFLGCDNKNJI)
	{
		List<UserItem> list = new List<UserItem>();
		foreach (UserItem item in _items)
		{
			ItemInfo dJKEECEOCJB = item.GetInfo();
			if (dJKEECEOCJB != null && dJKEECEOCJB.Type == LFLGCDNKNJI)
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			int index = NekkiMath.randomInt(list.Count);
			return list[index];
		}
		return null;
	}

	public List<UserItem> GetItemsInDelivery()
	{
		List<UserItem> list = new List<UserItem>();
		foreach (UserItem item in _items)
		{
			if (item.GetDeliveryTimestamp() > 0)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<UserItem> GetEquippedItems()
	{
		List<UserItem> list = new List<UserItem>();
		foreach (UserItem item in _items)
		{
			if (item.GetIsEquipped())
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<RecipeItemInfo> GetRecipeDeliveries()
	{
		List<RecipeItemInfo> list = new List<RecipeItemInfo>();
		foreach (UserItem item in _items)
		{
			if (item.GetRecipeDelivery() != null)
			{
				list.Add(item.GetRecipeDelivery());
			}
		}
		return list;
	}

	public bool FinishDeliveryRecipe(RecipeItemInfo recipeItem, bool runEvent = true)
	{
		if (recipeItem == null || recipeItem.GetUserItem() == null) return false;
		RecipeItemInfo pending = recipeItem.GetUserItem().GetRecipeDelivery();
		if (pending == null || pending.ItemAndRecipeInfo != recipeItem.ItemAndRecipeInfo) return false;
		if (pending.IsStillInOrder && !Eclipse.Modding.ModPolicies.SkipEnabled("forge")) return false;
		if (!ForgeManager.GetInstance().FinishEnchant(pending)) return false;
		if (!DeliveredRecipes.Contains(pending)) DeliveredRecipes.Add(pending);
		if (runEvent)
		{
			Roster roster = ListSF.GetRoster();
			if (roster != null) roster.CallEvent(1, pending);
		}
		return true;
	}

	public List<RecipeItemInfo> GetRecipesDelivered()
	{
		return DeliveredRecipes;
	}

	public void UpdateLockItems(int OMHDLKNHNMJ)
	{
		List<ItemInfo> list = ListSF.GetItems().GetAllItems();
		foreach (ItemInfo item in list)
		{
			if (item.IsShopVisible)
			{
				ListSF.GetItems().SetNewAddItem(item, true, OMHDLKNHNMJ);
			}
		}
	}

	public void CorrectUpgradeLevels()
	{
		foreach (UserItem item in _items)
		{
			CorrectUpgradeLevel(item, item.GetInfo());
		}
	}

	public void CorrectUpgradeLevel(UserItem NDMCFNGEPOA, ItemInfo PJDAGCBPLJE)
	{
		int num = NDMCFNGEPOA.GetUpgradeLevel();
		if (PJDAGCBPLJE == null || PJDAGCBPLJE.UpgradeLevel == num)
		{
			return;
		}
		int num2 = int.MaxValue;
		int num3 = int.MinValue;
		int num4 = 0;
		bool flag = false;
		NDMCFNGEPOA.SetAcquireType("Upgrade");
		List<UpgradeData> list = PJDAGCBPLJE.GetUpgrades();
		foreach (UpgradeData item in list)
		{
			num4 = item.Values.UpgradeLevel;
			if (num4 == num)
			{
				return;
			}
			if (num4 > num && num4 <= num2)
			{
				num2 = num4;
				flag = true;
			}
			if (num4 >= num3)
			{
				num3 = num4;
			}
		}
		NDMCFNGEPOA.SetUpgradeLevel((!flag) ? num3 : num2);
		NDMCFNGEPOA.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
	}

	private void OnTimerTick(ExtentionBehaviour.CallEventArgs JKOCDNPPJDG)
	{
		ProcessDeliveries();
	}

	public void ClearDeliveredRecipes()
	{
		DeliveredRecipes.Clear();
	}
}
