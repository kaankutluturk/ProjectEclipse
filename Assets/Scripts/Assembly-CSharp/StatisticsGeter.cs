using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using SimpleJSON;
using UnityEngine;

public static class StatisticsGeter
{
	private class FightCounts
	{
		public string Name;

		public int WinCount;

		public int LossCount;

		public int EclipseWinCount;

		public int EclipseLossCount;
	}

	public static void AddBundleId(JSONClass json, string key = "b_id")
	{
		json.Add(key, Application.identifier);
	}

	public static void AddCurrencies(JSONClass json)
	{
		JSONArray jSONArray = new JSONArray();
		List<GameCurrency> list = GameUtils.GameCurrencies.GetCurrencies();
		foreach (GameCurrency item in list)
		{
			string currencyName = item.Name;
			int num = ListSF.GetRoster().GetCurrencyCount(currencyName);
			JSONClass jSONClass = new JSONClass();
			jSONClass["name"] = currencyName;
			jSONClass["amount"] = num;
			jSONArray.Add(jSONClass);
		}
		json.Add("currencies", jSONArray);
	}

	public static void AddGemsChange(JSONClass json, ArgsDict eventArgs)
	{
		long num = ((!eventArgs.ContainsKey("changed")) ? 0 : ((long)eventArgs["changed"]));
		string text = ((!eventArgs.ContainsKey("type")) ? string.Empty : eventArgs["type"].ToString());
		bool flag = eventArgs.ContainsKey("isPaid") && (bool)eventArgs["isPaid"];
		json["type"] = text;
		json["gems_free_changed"] = ((!flag) ? num : 0);
		json["gems_paid_changed"] = ((!flag) ? 0 : num);
	}

	public static void AddPriceUsd(JSONClass json, string name)
	{
		float priceUsd = 0f;
		GeneralConfig.Prices.TryGetPriceValue(name, out priceUsd);
		json["price_USD"] = priceUsd;
	}

	public static void AddBalances(JSONClass json)
	{
		json["money"] = ListSF.GetRoster().GetMoney().ToString();
		json["gems_paid"] = (long)(ListSF.GetRoster().GetPaidBonus());
		json["gems_free"] = ListSF.GetRoster().GetBonus() - (long)(ListSF.GetRoster().GetPaidBonus());
	}

	public static void AddPurchaseInfo(JSONClass json, ArgsDict eventArgs)
	{
		ItemInfo purchasedItem = ((!eventArgs.ContainsKey("item")) ? null : (eventArgs["item"] as ItemInfo));
		if (purchasedItem == null)
		{
			return;
		}
		StatisticsCollector.CurrencyType currencyType = (eventArgs.ContainsKey("type") ? ((StatisticsCollector.CurrencyType)eventArgs["type"]) : StatisticsCollector.CurrencyType.Money);
		bool flag = eventArgs.ContainsKey("immediatelyDelivery") && (bool)eventArgs["immediatelyDelivery"];
		long freeAmount = 0L;
		long paidAmount = 0L;
		GetPurchaseCost(purchasedItem, currencyType, flag, ref freeAmount, ref paidAmount);
		long num = 0L;
		long num2 = 0L;
		long num3 = 0L;
		if (currencyType == StatisticsCollector.CurrencyType.Money)
		{
			num = freeAmount + paidAmount;
		}
		else
		{
			num2 = freeAmount;
			num3 = paidAmount;
		}
		string text = string.Empty;
		if (purchasedItem.Type == "Energy")
		{
			text = "refill_Energy";
		}
		else if (purchasedItem.Type == "Recipe")
		{
			text = "recipe";
		}
		else
		{
			long num4 = ((!flag) ? (-1) : GameUtils.GetLeftTime(purchasedItem.DeliveryTime));
			bool flag2 = num4 > -1;
			bool isUpgradePurchase = purchasedItem.IsUpgradePurchase;
			if (flag2)
			{
				text += "finish_";
			}
			text = ((!isUpgradePurchase) ? (text + "buy_item") : (text + "upgrade_item"));
		}
		json["type"] = text;
		json["item"] = purchasedItem.Name;
		json["item_type"] = purchasedItem.Type;
		json["upgrade_level"] = purchasedItem.UpgradeLevel;
		json["money_changed"] = num;
		json["gems_free_changed"] = num2;
		json["gems_paid_changed"] = num3;
		json["upgrade"] = ((!purchasedItem.IsUpgradeVariant()) ? "0" : "1");
		json["paid_item"] = purchasedItem.LegacyPaidItem;
	}

	private static void GetPurchaseCost(ItemInfo item, StatisticsCollector.CurrencyType currencyType, bool immediatelyDelivery, ref long freeAmount, ref long paidAmount)
	{
		if (currencyType == StatisticsCollector.CurrencyType.Bonus)
		{
			paidAmount = item.MissingGems;
			if (immediatelyDelivery)
			{
				freeAmount = (ObscuredLong)(item.DeliveryGemPrice) - paidAmount;
			}
			else
			{
				freeAmount = item.GetGemPrice() - paidAmount;
			}
		}
		else
		{
			paidAmount = item.MissingCoins;
			if (immediatelyDelivery)
			{
				freeAmount = (ObscuredLong)(item.DeliveryCoinPrice) - paidAmount;
			}
			else
			{
				freeAmount = item.GetCoinPrice() - paidAmount;
			}
		}
	}

	public static void AddFpsLimit(JSONClass json)
	{
		int num = GameUtils.FrameRate;
		if (GameUtils.ReduceFps)
		{
			num /= GameUtils.FpsReductionDivisor;
		}
		json["fps_limit"] = num;
	}

	public static void AddEclipseMode(JSONClass json)
	{
		json["eclipse"] = (ListSF.GetRoster().IsEclipseMode() ? 1 : 0);
	}

	public static void AddRounds(JSONClass json, ArgsDict eventArgs)
	{
		if (eventArgs.ContainsKey("completedRounds"))
		{
			json["rounds"] = (int)eventArgs["completedRounds"];
		}
	}

	public static void AddAverageFps(JSONClass json, ArgsDict eventArgs)
	{
		if (eventArgs.ContainsKey("avgFps"))
		{
			json["fps"] = (float)eventArgs["avgFps"];
		}
	}

	public static void AddFightTimeElapsed(JSONClass json, ArgsDict eventArgs)
	{
		if (eventArgs.ContainsKey("fightTimeElapsed"))
		{
			json["fight_time_elapsed"] = (float)eventArgs["fightTimeElapsed"];
		}
	}

	public static void AddReplayCount(JSONClass json, FightList fightList)
	{
		int num = 0;
		if (fightList != null)
		{
			Battle battle = fightList.Battle;
			if (battle != null)
			{
				RosterBattle rosterBattle = battle.GetRosterBattle();
				if (rosterBattle != null)
				{
					num = rosterBattle.GetReplayCount();
				}
			}
		}
		json["replay_count"] = num;
	}

	public static void AddFightResult(JSONClass json, ArgsDict eventArgs)
	{
		FightResult fightResult = ((!eventArgs.ContainsKey("fightList")) ? null : (eventArgs["fightResult"] as FightResult));
		if (fightResult != null)
		{
			json["money_reward"] = fightResult.GetMoneyReward();
			json["gems_reward"] = fightResult.GetGemsReward();
			int num = ((!eventArgs.ContainsKey("isSurrender") || !(bool)eventArgs["isSurrender"]) ? Convert.ToInt32(fightResult.IsWinner()) : (-1));
			json["fight_result"] = num;
			DetailedDamages enemyDamages = fightResult.PlayerStatistics.Damages;
			DetailedDamages playerDamages = fightResult.OpponentStatistics.Damages;
			AddDetailedDamages(json, "player_damage", playerDamages);
			AddDetailedDamages(json, "enemy_damage", enemyDamages);
			ComboStatistic playerCombo = fightResult.PlayerStatistics;
			ComboStatistic enemyCombo = fightResult.OpponentStatistics;
			AddComboStatistics(json, "player", playerCombo);
			AddComboStatistics(json, "enemy", enemyCombo);
			ModelParameters playerParameters = ((!fightResult.IsWinner()) ? fightResult.OpponentParameters : fightResult.PlayerParameters);
			if (playerParameters != null)
			{
				AddEquipment(json, playerParameters);
			}
		}
	}

	public static void AddComboStatistics(JSONClass json, string prefix, ComboStatistic comboStatistic)
	{
		if (comboStatistic != null)
		{
			json[prefix + "_perfects"] = comboStatistic.PerfectCount;
			json[prefix + "_first_strikes"] = comboStatistic.FirstStrikeCount;
			json[prefix + "_shocks"] = comboStatistic.ShockCount;
			json[prefix + "_max_combo"] = comboStatistic.MaxCombo;
			json[prefix + "_max_crazy"] = comboStatistic.GetCrazyStyleAlias();
		}
	}

	public static void AddDetailedDamages(JSONClass json, string key, DetailedDamages detailedDamages)
	{
		JSONClass jSONClass = (JSONClass)(json[key] = new JSONClass());
		if (detailedDamages == null)
		{
			return;
		}
		Dictionary<string, Dictionary<string, float>> damagesByType = detailedDamages.DamagesByType;
		foreach (KeyValuePair<string, Dictionary<string, float>> item in damagesByType)
		{
			JSONClass jSONClass2 = new JSONClass();
			foreach (KeyValuePair<string, float> item2 in item.Value)
			{
				jSONClass2[item2.Key] = item2.Value;
			}
			jSONClass[item.Key] = jSONClass2;
		}
	}

	public static void AddFightInfo(JSONClass json, ArgsDict eventArgs)
	{
		FightList fightList = ((!eventArgs.ContainsKey("fightList")) ? null : (eventArgs["fightList"] as FightList));
		if (fightList != null)
		{
			json["zone"] = fightList.FightId.GetZone();
			json["fight_name"] = fightList.FightId.GetBattle();
			json["fight_type"] = ListSF.GetInstance().GetBattleTypeName(fightList.get_Type());
			json["stage_number"] = fightList.FightId.GetFight();
			json["difficulty"] = GameUtils.CalculateFightDifficulty(fightList);
			AddReplayCount(json, fightList);
		}
	}

	public static void AddFights(JSONClass json, int level)
	{
		Dictionary<string, FightCounts> dictionary = new Dictionary<string, FightCounts>();
		List<RosterFight> list = ListSF.GetRoster().FindFightsByLevel(level);
		foreach (RosterFight item in list)
		{
			string text = item.GetBattleName();
			if (dictionary.ContainsKey(text))
			{
				FightCounts fightCounts = dictionary[text];
				fightCounts.WinCount += item.GetWinCount();
				fightCounts.LossCount += item.GetLossCount();
				fightCounts.EclipseWinCount += item.GetEclipseWinCount();
				fightCounts.EclipseLossCount += item.GetEclipseLossCount();
			}
			else
			{
				FightCounts fightCounts = new FightCounts();
				fightCounts.Name = text;
				fightCounts.WinCount = item.GetWinCount();
				fightCounts.LossCount = item.GetLossCount();
				fightCounts.EclipseWinCount = item.GetEclipseWinCount();
				fightCounts.EclipseLossCount = item.GetEclipseLossCount();
				FightCounts value = fightCounts;
				dictionary[text] = value;
			}
		}
		foreach (KeyValuePair<string, FightCounts> item2 in dictionary)
		{
			JSONClass jSONClass = new JSONClass();
			jSONClass["fight_name"] = item2.Value.Name;
			jSONClass["win_count"] = item2.Value.WinCount;
			jSONClass["loss_count"] = item2.Value.LossCount;
			jSONClass["eclipse_win_count"] = item2.Value.EclipseWinCount;
			jSONClass["eclipse_loss_count"] = item2.Value.EclipseLossCount;
			json["fights"] = jSONClass;
		}
	}

	public static void AddPerks(JSONClass json, string key = "perks")
	{
		JSONArray jSONArray = new JSONArray();
		List<RosterPerk> list = ListSF.GetRoster().GetPerks().GetPerks();
		foreach (RosterPerk item in list)
		{
			JSONClass jSONClass = new JSONClass();
			jSONClass["perk"] = item.get_Name();
			jSONClass["upgrade_level"] = item.GetUpgradeLevel();
			jSONArray.Add(jSONClass);
		}
		json[key] = jSONArray;
	}

	public static void AddPerkChoice(JSONClass json, ArgsDict eventArgs)
	{
		if (eventArgs.ContainsKey("learnedPerk"))
		{
			PerkInfoItem learnedPerk = (PerkInfoItem)eventArgs["learnedPerk"];
			json["taken"] = learnedPerk.Name;
			json["taken_upgrade_level"] = learnedPerk.UpgradeLevel;
		}
		if (eventArgs.ContainsKey("rejectedPerk"))
		{
			PerkInfoItem rejectedPerk = (PerkInfoItem)eventArgs["rejectedPerk"];
			json["rejected"] = rejectedPerk.Name;
			json["rejected_upgrade_level"] = rejectedPerk.UpgradeLevel;
		}
	}

	public static void AddEnchantments(ItemInfo item, JSONClass json, string key)
	{
		JSONArray jSONArray = null;
		List<PerkInfoItem> list = ListSF.GetItemEnchantments(item);
		if (list.Count > 0)
		{
			jSONArray = new JSONArray();
			foreach (PerkInfoItem perkInfo in list)
			{
				JSONClass jSONClass = new JSONClass();
				jSONClass["perk"] = perkInfo.Name;
				jSONClass["aspect"] = perkInfo.GetSetValue("Aspect");
				jSONArray.Add(jSONClass);
			}
		}
		if (jSONArray != null)
		{
			json[key] = jSONArray;
		}
	}

	public static void AddEquippedItem(JSONClass json, string prefix, ItemInfo item)
	{
		if (item != null)
		{
			json[prefix + "_name"] = item.Name;
			json[prefix + "_upg_level"] = item.UpgradeLevel;
			AddEnchantments(item, json, prefix + "_enchantments");
		}
	}

	public static void AddEquipment(JSONClass json, ModelParameters modelParameters)
	{
		if (modelParameters != null)
		{
			AddEquippedItem(json, "weapon", modelParameters.Weapon);
			AddEquippedItem(json, "armor", modelParameters.Armor);
			AddEquippedItem(json, "helmet", modelParameters.Helm);
			AddEquippedItem(json, "ranged", modelParameters.Ranged);
			AddEquippedItem(json, "magic", modelParameters.Magic);
		}
	}

	public static void AddRank(JSONClass json, string key = "rank")
	{
	}

	public static void AddRanks(JSONClass json, string key = "ranks")
	{
		JSONArray aItem = new JSONArray();
		json.Add(key, aItem);
	}

	public static void AddEnergy(JSONClass json, string key = "energy")
	{
	}

	public static void AddRuns(JSONClass json, string key = "runs")
	{
	}

	public static void AddSeed(JSONClass json, string key = "seed")
	{
	}

	public static void AddFloor(JSONClass json, string key = "floor")
	{
	}

	public static void AddTime(JSONClass json, string key = "time")
	{
	}

	public static void AddUnusedData(JSONClass json)
	{
	}

	public static void AddPreviousFloorTime(JSONClass json, string key = "prev_floor_time")
	{
	}

	public static void AddScene(JSONClass json, string key = "scene")
	{
	}

	public static void AddScreen(JSONClass json, string key = "screen")
	{
	}

	public static void AddLevelUpItems(JSONClass json)
	{
		List<UserItem> list = ListSF.GetRoster().GetInventory().GetItems();
		foreach (UserItem item in list)
		{
			int num = GetItemLevel(item);
			if (num == ListSF.GetRoster().GetLevel() - 1)
			{
				JSONClass jSONClass = null;
				jSONClass = ((!json.HasValue("items")) ? new JSONClass() : ((JSONClass)json["items"]));
				AddUserItem(jSONClass, item);
				json["items"] = jSONClass;
			}
		}
	}

	public static void AddUserItem(JSONClass json, UserItem item)
	{
		json["name"] = item.get_Name();
		json["upgrade_level"] = item.GetUpgradeLevel();
		json["type"] = item.GetInfo().Type;
		AddEnchantments(item.GetInfo(), json, "enchantments");
	}

	private static int GetItemLevel(UserItem item)
	{
		ItemInfo itemInfo = item.GetCurrentUpgradeItem();
		if (itemInfo == null)
		{
			itemInfo = item.GetInfo();
		}
		if (itemInfo != null)
		{
			return itemInfo.ItemLevel;
		}
		GameLog.Error("logging UserItem without ItemInfo");
		return 0;
	}
}
