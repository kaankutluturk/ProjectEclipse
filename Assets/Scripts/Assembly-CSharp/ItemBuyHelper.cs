using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;
using Nekki.Utils;

public static class ItemBuyHelper
{
	private static bool SettleImmediatePurchase(ItemInfo item, bool gems, System.Func<bool> apply)
	{
		if (item == null) return false;
		Roster roster = ListSF.GetRoster();
		if (roster == null) return false;
		long price = gems ? (long)item.GemPrice : (long)item.CoinPrice;
		long balance = gems ? roster.GetBonus() : roster.GetMoney();
		if (price < 0 || balance < price) return false;
		UserItem existing = roster.GetInventory().FindItem(item);
		if (!ListSF.CanIncrementItemCount(existing == null ? 0 : existing.GetCount(), 1)) return false;
		return Eclipse.Modding.ModRuntime.SettleItemPurchase(item, 1, apply);
	}

	private static bool AddPurchasedItem(ItemInfo item)
	{
		UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(item);
		if (!ListSF.CanIncrementItemCount(userItem == null ? 0 : userItem.GetCount(), 1)) return false;
		if (userItem == null)
		{
			XmlNode itemsNode = ListSF.GetRoster().GetItemsNode();
			UserItem newUserItem = new UserItem(itemsNode, item.Name, false, 1, -1, -1L);
			newUserItem.SetInfo(item);
			newUserItem.SetIsUpgrade(false);
			newUserItem.ApplyDefaultEnchantments();
			ListSF.GetRoster().GetInventory().AddItem(newUserItem);
			newUserItem.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
			Sound.PlaySound("snd_buy");
			return true;
		}
		userItem.SetCount(userItem.GetCount() + 1);
		return true;
	}

	private static bool AddItemWithDelivery(ItemInfo item)
	{
		UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(item);
		if (userItem == null)
		{
			long deliveryTimestamp = GlobalTimer.get_LocalTimeUTC() + item.DeliveryTime;
			XmlNode itemsNode = ListSF.GetRoster().GetItemsNode();
			UserItem newUserItem = new UserItem(itemsNode, item.Name, false, 0, -1, deliveryTimestamp);
			newUserItem.SetInfo(item);
			newUserItem.SetIsUpgrade(false);
			newUserItem.ApplyDefaultEnchantments();
			ListSF.GetRoster().GetInventory().AddItem(newUserItem);
			newUserItem.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
			Sound.PlaySound("snd_upgrade");
			return true;
		}
		return false;
	}

	private static bool ApplyUpgrade(ItemInfo item, UserItem userItem)
	{
		if (userItem != null)
		{
			userItem.SetIsUpgrade(true);
			userItem.SetUpgradeLevel(item.UpgradeLevel);
			userItem.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
			Sound.PlaySound("snd_upgrade");
			return true;
		}
		return false;
	}

	private static bool ApplyUpgradeWithDelivery(ItemInfo item, UserItem userItem)
	{
		if (userItem != null)
		{
			long deliveryTimestamp = GlobalTimer.get_LocalTimeUTC() + item.DeliveryTime;
			userItem.set_DeliveryTime(deliveryTimestamp);
			userItem.SetDeliveryUpgradeLevel(item.UpgradeLevel);
			userItem.SetIsUpgrade(true);
			userItem.ApplyDefaultEnchantments();
			ListSF.GetRoster().GetInventory().AddItem(userItem, true);
			Sound.PlaySound("snd_upgrade");
			return true;
		}
		return false;
	}

	public static bool BuyItemWithCoins(ItemInfo item)
	{
		return SettleImmediatePurchase(item, false, () => ApplyImmediateCoinPurchase(item));
	}

	private static bool ApplyImmediateCoinPurchase(ItemInfo item)
	{
		if (item == null)
		{
			return false;
		}
		if (ListSF.GetRoster().GetMoney() >= (ObscuredLong)(item.CoinPrice))
		{
			long newBalance = ListSF.GetRoster().GetMoney() - (ObscuredLong)(item.CoinPrice);
			bool flag = false;
			// Desktop/offline builds have no reliable server-backed delivery clock.
			// Complete coin purchases immediately so an order cannot strand the item.
			flag = AddPurchasedItem(item);
			if (flag)
			{
				ListSF.GetRoster().SetMoney(newBalance);
				ListSF.GetRoster().RequestSave(true);
				ReportPurchaseStatistics(item, StatisticsCollector.CurrencyType.Money, false);
				NotifyPurchaseQuestEvent(item);
			}
			return flag;
		}
		return false;
	}

	public static bool BuyItemWithGems(ItemInfo item)
	{
		return SettleImmediatePurchase(item, true, () => ApplyImmediateGemPurchase(item));
	}

	private static bool ApplyImmediateGemPurchase(ItemInfo item)
	{
		if (item == null)
		{
			return false;
		}
		if (ListSF.GetRoster().GetBonus() >= (ObscuredLong)(item.GemPrice))
		{
			long newBalance = ListSF.GetRoster().GetBonus() - (ObscuredLong)(item.GemPrice);
			bool flag = AddPurchasedItem(item);
			if (flag)
			{
				ListSF.GetRoster().SetBonus(newBalance, Roster.BalanceChangeType.CHANGE_BUY_ITEM);
				ListSF.GetRoster().RequestSave(true);
				ReportPurchaseStatistics(item, StatisticsCollector.CurrencyType.Bonus, false);
				NotifyPurchaseQuestEvent(item);
			}
			return flag;
		}
		return false;
	}

	public static bool UpgradeItemWithCoins(ItemInfo item)
	{
		if (item == null)
		{
			return false;
		}
		UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(item);
		if (userItem == null)
		{
			return false;
		}
		ItemInfo upgradeItem = userItem.GetNextUpgradeItem();
		if (upgradeItem == null)
		{
			return false;
		}
		if (ListSF.GetRoster().GetMoney() >= (ObscuredLong)(upgradeItem.CoinPrice))
		{
			long newBalance = ListSF.GetRoster().GetMoney() - (ObscuredLong)(upgradeItem.CoinPrice);
			bool flag = false;
			// Shop upgrades are immediate in the offline runtime. This also avoids
			// entering the legacy delivery branch without reporting success.
			flag = ApplyUpgrade(upgradeItem, userItem);
			if (flag)
			{
				ListSF.GetRoster().SetMoney(newBalance);
				ListSF.GetRoster().RequestSave(true);
				ReportPurchaseStatistics(upgradeItem, StatisticsCollector.CurrencyType.Money, false);
				NotifyPurchaseQuestEvent(upgradeItem);
			}
			return flag;
		}
		return false;
	}

	public static bool UpgradeItemWithGems(ItemInfo item)
	{
		if (item == null)
		{
			return false;
		}
		UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(item);
		if (userItem == null)
		{
			return false;
		}
		ItemInfo upgradeItem = userItem.GetNextUpgradeItem();
		if (upgradeItem == null)
		{
			return false;
		}
		if (ListSF.GetRoster().GetBonus() >= (ObscuredLong)(upgradeItem.GemPrice))
		{
			long newBalance = ListSF.GetRoster().GetBonus() - (ObscuredLong)(upgradeItem.GemPrice);
			bool flag = ApplyUpgrade(upgradeItem, userItem);
			if (flag)
			{
				ListSF.GetRoster().SetBonus(newBalance, Roster.BalanceChangeType.CHANGE_BUY_ITEM);
				ListSF.GetRoster().RequestSave(true);
				ReportPurchaseStatistics(upgradeItem, StatisticsCollector.CurrencyType.Bonus, false);
				NotifyPurchaseQuestEvent(upgradeItem);
			}
			return flag;
		}
		return false;
	}

	public static bool BuyImmediatelyDelivery(string itemName)
	{
		ItemInfo item = ListSF.GetItems().GetItemByName(itemName);
		return BuyImmediatelyDelivery(item);
	}

	public static bool BuyImmediatelyDelivery(ItemInfo item)
	{
		if (item == null)
		{
			return false;
		}
		UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(item);
		if (userItem == null)
		{
			return false;
		}
		ItemInfo upgradeItem = userItem.GetNextUpgradeItem();
		if (upgradeItem == null)
		{
			return false;
		}
		bool flag = ListSF.GetRoster().GetBonus() >= (ObscuredLong)(upgradeItem.DeliveryGemPrice);
		bool flag2 = userItem.GetDeliveryTimestamp() > GlobalTimer.get_LocalTimeUTC();
		if (flag && flag2)
		{
			long newBalance = ListSF.GetRoster().GetBonus() - (ObscuredLong)(upgradeItem.DeliveryGemPrice);
			ListSF.GetRoster().GetInventory().CompleteDelivery(userItem);
			ListSF.GetRoster().SetBonus(newBalance, Roster.BalanceChangeType.CHANGE_BUY_DELIVERY);
			ListSF.GetRoster().RequestSave(true);
			ReportPurchaseStatistics(upgradeItem, StatisticsCollector.CurrencyType.Bonus, true);
			NotifyPurchaseQuestEvent(upgradeItem);
			Sound.PlaySound("snd_upgrade");
			return true;
		}
		return false;
	}

	public static bool BuyConsumableWithGems(ItemInfo item)
	{
		return SettleImmediatePurchase(item, true, () => ApplyImmediateConsumablePurchase(item));
	}

	private static bool ApplyImmediateConsumablePurchase(ItemInfo item)
	{
		if (item == null)
		{
			return false;
		}
		if (ListSF.GetRoster().GetBonus() >= (ObscuredLong)(item.GemPrice))
		{
			long newBalance = ListSF.GetRoster().GetBonus() - (ObscuredLong)(item.GemPrice);
			bool flag = AddPurchasedItem(item);
			if (flag)
			{
				switch (item.SubType)
				{
				case "PerkReset":
					ListSF.GetRoster().GetPerks().ResetPerks();
					break;
				case "Currency":
					ListSF.GetRoster().AddCurrencyCount(item.CurrencyName, (ObscuredInt)(item.CurrencyValue));
					break;
				}
				ListSF.GetRoster().SetBonus(newBalance, Roster.BalanceChangeType.CHANGE_BUY_ITEM);
				ListSF.GetRoster().RequestSave(true);
				ReportPurchaseStatistics(item, StatisticsCollector.CurrencyType.Bonus, false);
				NotifyPurchaseQuestEvent(item);
			}
			return flag;
		}
		return false;
	}

	private static void ReportPurchaseStatistics(ItemInfo item, StatisticsCollector.CurrencyType currencyType, bool isImmediateDelivery)
	{
		ArgsDict args = new ArgsDict();
		args["item"] = item;
		args["type"] = currencyType;
		args["immediatelyDelivery"] = isImmediateDelivery;
		StatisticsCollector.LogEvent(StatisticsEvent.EventType.Purchase, args);
	}

	private static void NotifyPurchaseQuestEvent(ItemInfo item)
	{
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		FightIDS savedFightIds = questParameters.fightIds;
		questParameters.fightIds = FightIDS.Empty();
		questParameters.fightResult = string.Empty;
		questParameters.raidResult = string.Empty;
		questParameters.purchasedItem = item;
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE))
		{
			ListSF.GetInstance().RunQuestActions();
		}
		questParameters.fightIds = savedFightIds;
	}
}
