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
		UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(item);
		if (!ListSF.CanIncrementItemCount(dKCHDHMLKHN == null ? 0 : dKCHDHMLKHN.GetCount(), 1)) return false;
		if (dKCHDHMLKHN == null)
		{
			XmlNode fMBDAPOMFGN = ListSF.GetRoster().GetItemsNode();
			UserItem dKCHDHMLKHN2 = new UserItem(fMBDAPOMFGN, item.Name, false, 1, -1, -1L);
			dKCHDHMLKHN2.SetInfo(item);
			dKCHDHMLKHN2.SetIsUpgrade(false);
			dKCHDHMLKHN2.ApplyDefaultEnchantments();
			ListSF.GetRoster().GetInventory().AddItem(dKCHDHMLKHN2);
			dKCHDHMLKHN2.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
			Sound.PlaySound("snd_buy");
			return true;
		}
		dKCHDHMLKHN.SetCount(dKCHDHMLKHN.GetCount() + 1);
		return true;
	}

	private static bool AddItemWithDelivery(ItemInfo item)
	{
		UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(item);
		if (dKCHDHMLKHN == null)
		{
			long aFHNFJLOGIC = GlobalTimer.get_LocalTimeUTC() + item.DeliveryTime;
			XmlNode fMBDAPOMFGN = ListSF.GetRoster().GetItemsNode();
			UserItem dKCHDHMLKHN2 = new UserItem(fMBDAPOMFGN, item.Name, false, 0, -1, aFHNFJLOGIC);
			dKCHDHMLKHN2.SetInfo(item);
			dKCHDHMLKHN2.SetIsUpgrade(false);
			dKCHDHMLKHN2.ApplyDefaultEnchantments();
			ListSF.GetRoster().GetInventory().AddItem(dKCHDHMLKHN2);
			dKCHDHMLKHN2.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
			Sound.PlaySound("snd_upgrade");
			return true;
		}
		return false;
	}

	private static bool ApplyUpgrade(ItemInfo item, UserItem NDMCFNGEPOA)
	{
		if (NDMCFNGEPOA != null)
		{
			NDMCFNGEPOA.SetIsUpgrade(true);
			NDMCFNGEPOA.SetUpgradeLevel(item.UpgradeLevel);
			NDMCFNGEPOA.RefreshUpgradeState(ListSF.GetRoster().GetLevel());
			Sound.PlaySound("snd_upgrade");
			return true;
		}
		return false;
	}

	private static bool ApplyUpgradeWithDelivery(ItemInfo item, UserItem NDMCFNGEPOA)
	{
		if (NDMCFNGEPOA != null)
		{
			long bAINMLLIKOL = GlobalTimer.get_LocalTimeUTC() + item.DeliveryTime;
			NDMCFNGEPOA.set_DeliveryTime(bAINMLLIKOL);
			NDMCFNGEPOA.SetDeliveryUpgradeLevel(item.UpgradeLevel);
			NDMCFNGEPOA.SetIsUpgrade(true);
			NDMCFNGEPOA.ApplyDefaultEnchantments();
			ListSF.GetRoster().GetInventory().AddItem(NDMCFNGEPOA, true);
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
			long bAINMLLIKOL = ListSF.GetRoster().GetMoney() - (ObscuredLong)(item.CoinPrice);
			bool flag = false;
			// Desktop/offline builds have no reliable server-backed delivery clock.
			// Complete coin purchases immediately so an order cannot strand the item.
			flag = AddPurchasedItem(item);
			if (flag)
			{
				ListSF.GetRoster().SetMoney(bAINMLLIKOL);
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
			long bAINMLLIKOL = ListSF.GetRoster().GetBonus() - (ObscuredLong)(item.GemPrice);
			bool flag = AddPurchasedItem(item);
			if (flag)
			{
				ListSF.GetRoster().SetBonus(bAINMLLIKOL, Roster.BalanceChangeType.CHANGE_BUY_ITEM);
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
		UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(item);
		if (dKCHDHMLKHN == null)
		{
			return false;
		}
		ItemInfo dJKEECEOCJB = dKCHDHMLKHN.GetNextUpgradeItem();
		if (dJKEECEOCJB == null)
		{
			return false;
		}
		if (ListSF.GetRoster().GetMoney() >= (ObscuredLong)(dJKEECEOCJB.CoinPrice))
		{
			long bAINMLLIKOL = ListSF.GetRoster().GetMoney() - (ObscuredLong)(dJKEECEOCJB.CoinPrice);
			bool flag = false;
			// Shop upgrades are immediate in the offline runtime. This also avoids
			// entering the legacy delivery branch without reporting success.
			flag = ApplyUpgrade(dJKEECEOCJB, dKCHDHMLKHN);
			if (flag)
			{
				ListSF.GetRoster().SetMoney(bAINMLLIKOL);
				ListSF.GetRoster().RequestSave(true);
				ReportPurchaseStatistics(dJKEECEOCJB, StatisticsCollector.CurrencyType.Money, false);
				NotifyPurchaseQuestEvent(dJKEECEOCJB);
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
		UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(item);
		if (dKCHDHMLKHN == null)
		{
			return false;
		}
		ItemInfo dJKEECEOCJB = dKCHDHMLKHN.GetNextUpgradeItem();
		if (dJKEECEOCJB == null)
		{
			return false;
		}
		if (ListSF.GetRoster().GetBonus() >= (ObscuredLong)(dJKEECEOCJB.GemPrice))
		{
			long bAINMLLIKOL = ListSF.GetRoster().GetBonus() - (ObscuredLong)(dJKEECEOCJB.GemPrice);
			bool flag = ApplyUpgrade(dJKEECEOCJB, dKCHDHMLKHN);
			if (flag)
			{
				ListSF.GetRoster().SetBonus(bAINMLLIKOL, Roster.BalanceChangeType.CHANGE_BUY_ITEM);
				ListSF.GetRoster().RequestSave(true);
				ReportPurchaseStatistics(dJKEECEOCJB, StatisticsCollector.CurrencyType.Bonus, false);
				NotifyPurchaseQuestEvent(dJKEECEOCJB);
			}
			return flag;
		}
		return false;
	}

	public static bool BuyImmediatelyDelivery(string OHCGEEEKEJH)
	{
		ItemInfo mBIJKDIEFIF = ListSF.GetItems().GetItemByName(OHCGEEEKEJH);
		return BuyImmediatelyDelivery(mBIJKDIEFIF);
	}

	public static bool BuyImmediatelyDelivery(ItemInfo item)
	{
		if (item == null)
		{
			return false;
		}
		UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(item);
		if (dKCHDHMLKHN == null)
		{
			return false;
		}
		ItemInfo dJKEECEOCJB = dKCHDHMLKHN.GetNextUpgradeItem();
		if (dJKEECEOCJB == null)
		{
			return false;
		}
		bool flag = ListSF.GetRoster().GetBonus() >= (ObscuredLong)(dJKEECEOCJB.DeliveryGemPrice);
		bool flag2 = dKCHDHMLKHN.GetDeliveryTimestamp() > GlobalTimer.get_LocalTimeUTC();
		if (flag && flag2)
		{
			long bAINMLLIKOL = ListSF.GetRoster().GetBonus() - (ObscuredLong)(dJKEECEOCJB.DeliveryGemPrice);
			ListSF.GetRoster().GetInventory().CompleteDelivery(dKCHDHMLKHN);
			ListSF.GetRoster().SetBonus(bAINMLLIKOL, Roster.BalanceChangeType.CHANGE_BUY_DELIVERY);
			ListSF.GetRoster().RequestSave(true);
			ReportPurchaseStatistics(dJKEECEOCJB, StatisticsCollector.CurrencyType.Bonus, true);
			NotifyPurchaseQuestEvent(dJKEECEOCJB);
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
			long bAINMLLIKOL = ListSF.GetRoster().GetBonus() - (ObscuredLong)(item.GemPrice);
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
				ListSF.GetRoster().SetBonus(bAINMLLIKOL, Roster.BalanceChangeType.CHANGE_BUY_ITEM);
				ListSF.GetRoster().RequestSave(true);
				ReportPurchaseStatistics(item, StatisticsCollector.CurrencyType.Bonus, false);
				NotifyPurchaseQuestEvent(item);
			}
			return flag;
		}
		return false;
	}

	private static void ReportPurchaseStatistics(ItemInfo item, StatisticsCollector.CurrencyType LFLGCDNKNJI, bool MNGGLFFHDJG)
	{
		ArgsDict kEMMIFBFDPK = new ArgsDict();
		kEMMIFBFDPK["item"] = item;
		kEMMIFBFDPK["type"] = LFLGCDNKNJI;
		kEMMIFBFDPK["immediatelyDelivery"] = MNGGLFFHDJG;
		StatisticsCollector.LogEvent(StatisticsEvent.EventType.Purchase, kEMMIFBFDPK);
	}

	private static void NotifyPurchaseQuestEvent(ItemInfo item)
	{
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		FightIDS jLGLBLDPAAF = hHKLFIIBIFF.fightIds;
		hHKLFIIBIFF.fightIds = FightIDS.Empty();
		hHKLFIIBIFF.fightResult = string.Empty;
		hHKLFIIBIFF.raidResult = string.Empty;
		hHKLFIIBIFF.purchasedItem = item;
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE))
		{
			ListSF.GetInstance().RunQuestActions();
		}
		hHKLFIIBIFF.fightIds = jLGLBLDPAAF;
	}
}
