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

	public void Parse(XmlNode itemsNode)
	{
		_missingModItemIds.Clear();
		if (itemsNode == null)
		{
			return;
		}
		foreach (XmlNode childNode in itemsNode.ChildNodes)
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
				XmlAttribute equipped = itemsNode.ParentNode?.Attributes?[definition.Type];
				if (equipped != null) item.SetIsEquipped(equipped.Value == item.get_Name());
			}
			AddItem(item);
		}
		if (_missingModItemIds.Count > 0)
			UnityEngine.Debug.LogWarning("[ModSave] Preserved " + _missingModItemIds.Count +
				" unavailable mod item(s) in the save: " + string.Join(", ", _missingModItemIds));
	}

	public UserItem AddItem(UserItem value, bool replaceExisting = false)
	{
		if (value.GetInfo() != null && value.GetInfo().Type == "Seal")
		{
			value.GetInfo().SetIsNew(true);
		}
		if (replaceExisting)
		{
			UserItem existingItem = _items.Find((UserItem userItem) => userItem.get_Name().Equals(value.get_Name()));
			if (existingItem != null)
			{
				int index = _items.IndexOf(existingItem);
				_items.Insert(index, value);
				_items.Remove(existingItem);
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

	public List<UserItem> FindItemsByType(string itemType, string subType = "", bool isActive = true)
	{
		List<UserItem> list = new List<UserItem>();
		foreach (UserItem item in _items)
		{
			ItemInfo info = item.GetInfo();
			if (info != null)
			{
				bool flag = info.Type.Equals(itemType);
				bool flag2 = info.SubType.Equals(subType) || subType.Equals(string.Empty);
				bool flag3 = info.IsShopVisible || !isActive;
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
		UserItem userItem = FindItem(item);
		return userItem != null && userItem.GetCount() > 0;
	}

	public void ResetToDefaultItem(ItemInfo item, bool applyEquip)
	{
		if (item == null)
		{
			return;
		}
		string defaultItemName = GameUtils.GetDefaultItem(item.Type);
		UserItem defaultUserItem = FindItem(defaultItemName);
		if (defaultUserItem == null || defaultUserItem.GetInfo() == null)
		{
			return;
		}
		ListSF.GetRoster().get_Parameters().SetItemByType(defaultUserItem.GetInfo().Type, defaultUserItem.GetInfo());
		if (applyEquip)
		{
			UserItem existingItem = FindItem(item);
			if (existingItem != null)
			{
				var activeSets = IsPlayerItems() ? Eclipse.UI.SetBonusNotice.ActiveCombos() : null;
				existingItem.SetIsEquipped(false);
				defaultUserItem.SetIsEquipped(true);
				ListSF.GetRoster().EquipItem(defaultUserItem);
				ListSF.GetRoster().RequestSave();
				Eclipse.UI.SetBonusNotice.AnnounceNew(activeSets);
			}
		}
	}

	public void EquipItem(ItemInfo item, bool applyEquip)
	{
		if (item == null)
		{
			return;
		}
		ListSF.GetRoster().get_Parameters().SetItemByType(item.Type, item);
		if (!applyEquip)
		{
			return;
		}
		UserItem userItem = FindItem(item);
		if (userItem != null && userItem.GetIsOwned())
		{
			var activeSets = IsPlayerItems() ? Eclipse.UI.SetBonusNotice.ActiveCombos() : null;
			List<UserItem> list = FindItemsByType(item.Type, string.Empty);
			list.ForEach((UserItem userItem) =>
			{
				userItem.SetIsEquipped(false);
			});
			userItem.SetIsEquipped(true);
			ListSF.GetRoster().EquipItem(userItem);
			ListSF.GetRoster().RequestSave();
			Eclipse.UI.SetBonusNotice.AnnounceNew(activeSets);
		}
	}

	private bool IsPlayerItems()
	{
		Roster roster = ListSF.GetRoster();
		return roster != null && ReferenceEquals(this, roster.GetInventory());
	}

	public void ApplyItemInfos(List<ItemInfo> itemInfos)
	{
		ModelParameters parameters = ListSF.GetRoster().get_Parameters();
		if (parameters == null)
		{
			return;
		}
		parameters.DecorateItems.Clear();
		foreach (ItemInfo item in itemInfos)
		{
			UserItem userItem = FindItem(item);
			if (userItem != null)
			{
				userItem.SetInfo(item);
				if (userItem.GetIsEquipped() && item.Type.Equals("Decorate"))
				{
					parameters.DecorateItems.Add(item);
				}
			}
		}
		RefreshUpgradeStates();
	}

	public void RefreshUpgradeStates()
	{
		_items.ForEach((UserItem userItem) =>
		{
			userItem.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
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
			list.ForEach((UserItem userItem) =>
			{
				pendingDeliveries.Remove(userItem);
			});
		}
	}

	public void CompleteDelivery(UserItem userItem)
	{
		if (userItem != null && userItem.GetDeliveryTimestamp() > 0)
		{
			Roster acquisitionRoster = ListSF.GetRoster();
			int acquisitionProfile = Eclipse.Modding.ModRuntime.StoryEvents.ProfileGeneration;
			int previousCount = -1;
			if (userItem.GetIsUpgrade())
			{
				GetDeliveredUpgrades().Add(userItem);
			}
			else
			{
				GetDeliveredItems().Add(userItem);
			}
			ItemDelivered.Invoke(userItem);
			QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
			questParameters.purchasedItem = userItem.GetInfo();
			if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_DELIVERY))
			{
				ListSF.GetInstance().RunQuestActions();
			}
			if (userItem.GetCount() <= 0)
			{
				previousCount = userItem.GetCount();
				userItem.SetCount(1);
				userItem.set_DeliveryTime(-1L);
			}
			if (userItem.GetDeliveryUpgradeLevel() > 0 && userItem.GetDeliveryUpgradeLevel() > userItem.GetUpgradeLevel())
			{
				userItem.SetUpgradeLevel(userItem.GetDeliveryUpgradeLevel());
				userItem.set_DeliveryTime(-1L);
			}
			userItem.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
			ListSF.GetRoster().RequestSave();
			if (previousCount >= 0 && ReferenceEquals(this, acquisitionRoster.GetInventory()))
				Eclipse.Modding.ModRuntime.PublishItemAcquired(acquisitionRoster, userItem.GetInfo(), previousCount, 1, acquisitionProfile);
		}
	}

	public UserItem GetRandomItemOfType(string itemType)
	{
		List<UserItem> list = new List<UserItem>();
		foreach (UserItem item in _items)
		{
			ItemInfo info = item.GetInfo();
			if (info != null && info.Type == itemType)
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

	public void UpdateLockItems(int playerLevel)
	{
		List<ItemInfo> list = ListSF.GetItems().GetAllItems();
		foreach (ItemInfo item in list)
		{
			if (item.IsShopVisible)
			{
				ListSF.GetItems().SetNewAddItem(item, true, playerLevel);
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

	public void CorrectUpgradeLevel(UserItem userItem, ItemInfo info)
	{
		int num = userItem.GetUpgradeLevel();
		if (info == null || info.UpgradeLevel == num)
		{
			return;
		}
		int num2 = int.MaxValue;
		int num3 = int.MinValue;
		int num4 = 0;
		bool flag = false;
		userItem.SetAcquireType("Upgrade");
		List<UpgradeData> list = info.GetUpgrades();
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
		userItem.SetUpgradeLevel((!flag) ? num3 : num2);
		userItem.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
	}

	private void OnTimerTick(ExtentionBehaviour.CallEventArgs eventArgs)
	{
		ProcessDeliveries();
	}

	public void ClearDeliveredRecipes()
	{
		DeliveredRecipes.Clear();
	}
}
