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

	public static void AddBundleId(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "b_id")
	{
		MEEAKLDGLDF.Add(BFADPFOIPLL, Application.identifier);
	}

	public static void AddCurrencies(JSONClass MEEAKLDGLDF)
	{
		JSONArray jSONArray = new JSONArray();
		List<GameCurrency> list = GameUtils.GameCurrencies.GetCurrencies();
		foreach (GameCurrency item in list)
		{
			string mENAJEAJJBE = item.Name;
			int num = ListSF.GetRoster().GetCurrencyCount(mENAJEAJJBE);
			JSONClass jSONClass = new JSONClass();
			jSONClass["name"] = mENAJEAJJBE;
			jSONClass["amount"] = num;
			jSONArray.Add(jSONClass);
		}
		MEEAKLDGLDF.Add("currencies", jSONArray);
	}

	public static void AddGemsChange(JSONClass MEEAKLDGLDF, ArgsDict PCJAKPJMKGN)
	{
		long num = ((!PCJAKPJMKGN.ContainsKey("changed")) ? 0 : ((long)PCJAKPJMKGN["changed"]));
		string text = ((!PCJAKPJMKGN.ContainsKey("type")) ? string.Empty : PCJAKPJMKGN["type"].ToString());
		bool flag = PCJAKPJMKGN.ContainsKey("isPaid") && (bool)PCJAKPJMKGN["isPaid"];
		MEEAKLDGLDF["type"] = text;
		MEEAKLDGLDF["gems_free_changed"] = ((!flag) ? num : 0);
		MEEAKLDGLDF["gems_paid_changed"] = ((!flag) ? 0 : num);
	}

	public static void AddPriceUsd(JSONClass MEEAKLDGLDF, string name)
	{
		float HCHKFOJEEBK = 0f;
		GeneralConfig.Prices.TryGetPriceValue(name, out HCHKFOJEEBK);
		MEEAKLDGLDF["price_USD"] = HCHKFOJEEBK;
	}

	public static void AddBalances(JSONClass MEEAKLDGLDF)
	{
		MEEAKLDGLDF["money"] = ListSF.GetRoster().GetMoney().ToString();
		MEEAKLDGLDF["gems_paid"] = (long)(ListSF.GetRoster().GetPaidBonus());
		MEEAKLDGLDF["gems_free"] = ListSF.GetRoster().GetBonus() - (long)(ListSF.GetRoster().GetPaidBonus());
	}

	public static void AddPurchaseInfo(JSONClass MEEAKLDGLDF, ArgsDict PCJAKPJMKGN)
	{
		ItemInfo dJKEECEOCJB = ((!PCJAKPJMKGN.ContainsKey("item")) ? null : (PCJAKPJMKGN["item"] as ItemInfo));
		if (dJKEECEOCJB == null)
		{
			return;
		}
		StatisticsCollector.CurrencyType cNCDMFJLMFH = (PCJAKPJMKGN.ContainsKey("type") ? ((StatisticsCollector.CurrencyType)PCJAKPJMKGN["type"]) : StatisticsCollector.CurrencyType.Money);
		bool flag = PCJAKPJMKGN.ContainsKey("immediatelyDelivery") && (bool)PCJAKPJMKGN["immediatelyDelivery"];
		long OMALFAGNPEE = 0L;
		long DBMJEEHOABD = 0L;
		GetPurchaseCost(dJKEECEOCJB, cNCDMFJLMFH, flag, ref OMALFAGNPEE, ref DBMJEEHOABD);
		long num = 0L;
		long num2 = 0L;
		long num3 = 0L;
		if (cNCDMFJLMFH == StatisticsCollector.CurrencyType.Money)
		{
			num = OMALFAGNPEE + DBMJEEHOABD;
		}
		else
		{
			num2 = OMALFAGNPEE;
			num3 = DBMJEEHOABD;
		}
		string text = string.Empty;
		if (dJKEECEOCJB.Type == "Energy")
		{
			text = "refill_Energy";
		}
		else if (dJKEECEOCJB.Type == "Recipe")
		{
			text = "recipe";
		}
		else
		{
			long num4 = ((!flag) ? (-1) : GameUtils.GetLeftTime(dJKEECEOCJB.DeliveryTime));
			bool flag2 = num4 > -1;
			bool aCOIHHPOBDH = dJKEECEOCJB.IsUpgradePurchase;
			if (flag2)
			{
				text += "finish_";
			}
			text = ((!aCOIHHPOBDH) ? (text + "buy_item") : (text + "upgrade_item"));
		}
		MEEAKLDGLDF["type"] = text;
		MEEAKLDGLDF["item"] = dJKEECEOCJB.Name;
		MEEAKLDGLDF["item_type"] = dJKEECEOCJB.Type;
		MEEAKLDGLDF["upgrade_level"] = dJKEECEOCJB.UpgradeLevel;
		MEEAKLDGLDF["money_changed"] = num;
		MEEAKLDGLDF["gems_free_changed"] = num2;
		MEEAKLDGLDF["gems_paid_changed"] = num3;
		MEEAKLDGLDF["upgrade"] = ((!dJKEECEOCJB.IsUpgradeVariant()) ? "0" : "1");
		MEEAKLDGLDF["paid_item"] = dJKEECEOCJB.LegacyPaidItem;
	}

	private static void GetPurchaseCost(ItemInfo item, StatisticsCollector.CurrencyType LFLGCDNKNJI, bool CNIOCCCBDBJ, ref long OMALFAGNPEE, ref long DBMJEEHOABD)
	{
		if (LFLGCDNKNJI == StatisticsCollector.CurrencyType.Bonus)
		{
			DBMJEEHOABD = item.MissingGems;
			if (CNIOCCCBDBJ)
			{
				OMALFAGNPEE = (ObscuredLong)(item.DeliveryGemPrice) - DBMJEEHOABD;
			}
			else
			{
				OMALFAGNPEE = item.GetGemPrice() - DBMJEEHOABD;
			}
		}
		else
		{
			DBMJEEHOABD = item.MissingCoins;
			if (CNIOCCCBDBJ)
			{
				OMALFAGNPEE = (ObscuredLong)(item.DeliveryCoinPrice) - DBMJEEHOABD;
			}
			else
			{
				OMALFAGNPEE = item.GetCoinPrice() - DBMJEEHOABD;
			}
		}
	}

	public static void AddFpsLimit(JSONClass MEEAKLDGLDF)
	{
		int num = GameUtils.FrameRate;
		if (GameUtils.ReduceFps)
		{
			num /= GameUtils.FpsReductionDivisor;
		}
		MEEAKLDGLDF["fps_limit"] = num;
	}

	public static void AddEclipseMode(JSONClass MEEAKLDGLDF)
	{
		MEEAKLDGLDF["eclipse"] = (ListSF.GetRoster().IsEclipseMode() ? 1 : 0);
	}

	public static void AddRounds(JSONClass MEEAKLDGLDF, ArgsDict PCJAKPJMKGN)
	{
		if (PCJAKPJMKGN.ContainsKey("completedRounds"))
		{
			MEEAKLDGLDF["rounds"] = (int)PCJAKPJMKGN["completedRounds"];
		}
	}

	public static void AddAverageFps(JSONClass MEEAKLDGLDF, ArgsDict PCJAKPJMKGN)
	{
		if (PCJAKPJMKGN.ContainsKey("avgFps"))
		{
			MEEAKLDGLDF["fps"] = (float)PCJAKPJMKGN["avgFps"];
		}
	}

	public static void AddFightTimeElapsed(JSONClass MEEAKLDGLDF, ArgsDict PCJAKPJMKGN)
	{
		if (PCJAKPJMKGN.ContainsKey("fightTimeElapsed"))
		{
			MEEAKLDGLDF["fight_time_elapsed"] = (float)PCJAKPJMKGN["fightTimeElapsed"];
		}
	}

	public static void AddReplayCount(JSONClass MEEAKLDGLDF, FightList KGKDKENMAOA)
	{
		int num = 0;
		if (KGKDKENMAOA != null)
		{
			Battle cNAOMDMIGLJ = KGKDKENMAOA.Battle;
			if (cNAOMDMIGLJ != null)
			{
				RosterBattle dDNLCGOPAGC = cNAOMDMIGLJ.GetRosterBattle();
				if (dDNLCGOPAGC != null)
				{
					num = dDNLCGOPAGC.GetReplayCount();
				}
			}
		}
		MEEAKLDGLDF["replay_count"] = num;
	}

	public static void AddFightResult(JSONClass MEEAKLDGLDF, ArgsDict PCJAKPJMKGN)
	{
		FightResult nHIDAJFLHJN = ((!PCJAKPJMKGN.ContainsKey("fightList")) ? null : (PCJAKPJMKGN["fightResult"] as FightResult));
		if (nHIDAJFLHJN != null)
		{
			MEEAKLDGLDF["money_reward"] = nHIDAJFLHJN.GetMoneyReward();
			MEEAKLDGLDF["gems_reward"] = nHIDAJFLHJN.GetGemsReward();
			int num = ((!PCJAKPJMKGN.ContainsKey("isSurrender") || !(bool)PCJAKPJMKGN["isSurrender"]) ? Convert.ToInt32(nHIDAJFLHJN.IsWinner()) : (-1));
			MEEAKLDGLDF["fight_result"] = num;
			DetailedDamages mNDEOFOHLHI = nHIDAJFLHJN.PlayerStatistics.Damages;
			DetailedDamages mNDEOFOHLHI2 = nHIDAJFLHJN.OpponentStatistics.Damages;
			AddDetailedDamages(MEEAKLDGLDF, "player_damage", mNDEOFOHLHI2);
			AddDetailedDamages(MEEAKLDGLDF, "enemy_damage", mNDEOFOHLHI);
			ComboStatistic aIOMDIAFHGB = nHIDAJFLHJN.PlayerStatistics;
			ComboStatistic mOJHPBGGNAH = nHIDAJFLHJN.OpponentStatistics;
			AddComboStatistics(MEEAKLDGLDF, "player", aIOMDIAFHGB);
			AddComboStatistics(MEEAKLDGLDF, "enemy", mOJHPBGGNAH);
			ModelParameters kIKOGDEPGHB = ((!nHIDAJFLHJN.IsWinner()) ? nHIDAJFLHJN.OpponentParameters : nHIDAJFLHJN.PlayerParameters);
			if (kIKOGDEPGHB != null)
			{
				AddEquipment(MEEAKLDGLDF, kIKOGDEPGHB);
			}
		}
	}

	public static void AddComboStatistics(JSONClass MEEAKLDGLDF, string JMOHMLIGHHD, ComboStatistic AIOMDIAFHGB)
	{
		if (AIOMDIAFHGB != null)
		{
			MEEAKLDGLDF[JMOHMLIGHHD + "_perfects"] = AIOMDIAFHGB.PerfectCount;
			MEEAKLDGLDF[JMOHMLIGHHD + "_first_strikes"] = AIOMDIAFHGB.FirstStrikeCount;
			MEEAKLDGLDF[JMOHMLIGHHD + "_shocks"] = AIOMDIAFHGB.ShockCount;
			MEEAKLDGLDF[JMOHMLIGHHD + "_max_combo"] = AIOMDIAFHGB.MaxCombo;
			MEEAKLDGLDF[JMOHMLIGHHD + "_max_crazy"] = AIOMDIAFHGB.GetCrazyStyleAlias();
		}
	}

	public static void AddDetailedDamages(JSONClass MEEAKLDGLDF, string IMGCANJHPND, DetailedDamages KNKLGEAIKGE)
	{
		JSONClass jSONClass = (JSONClass)(MEEAKLDGLDF[IMGCANJHPND] = new JSONClass());
		if (KNKLGEAIKGE == null)
		{
			return;
		}
		Dictionary<string, Dictionary<string, float>> bEOLFOFKIAG = KNKLGEAIKGE.DamagesByType;
		foreach (KeyValuePair<string, Dictionary<string, float>> item in bEOLFOFKIAG)
		{
			JSONClass jSONClass2 = new JSONClass();
			foreach (KeyValuePair<string, float> item2 in item.Value)
			{
				jSONClass2[item2.Key] = item2.Value;
			}
			jSONClass[item.Key] = jSONClass2;
		}
	}

	public static void AddFightInfo(JSONClass MEEAKLDGLDF, ArgsDict PCJAKPJMKGN)
	{
		FightList jDIPBIHBGPF = ((!PCJAKPJMKGN.ContainsKey("fightList")) ? null : (PCJAKPJMKGN["fightList"] as FightList));
		if (jDIPBIHBGPF != null)
		{
			MEEAKLDGLDF["zone"] = jDIPBIHBGPF.FightId.GetZone();
			MEEAKLDGLDF["fight_name"] = jDIPBIHBGPF.FightId.GetBattle();
			MEEAKLDGLDF["fight_type"] = ListSF.GetInstance().GetBattleTypeName(jDIPBIHBGPF.get_Type());
			MEEAKLDGLDF["stage_number"] = jDIPBIHBGPF.FightId.GetFight();
			MEEAKLDGLDF["difficulty"] = GameUtils.CalculateFightDifficulty(jDIPBIHBGPF);
			AddReplayCount(MEEAKLDGLDF, jDIPBIHBGPF);
		}
	}

	public static void AddFights(JSONClass MEEAKLDGLDF, int GNLOCMLBNHF)
	{
		Dictionary<string, FightCounts> dictionary = new Dictionary<string, FightCounts>();
		List<RosterFight> list = ListSF.GetRoster().FindFightsByLevel(GNLOCMLBNHF);
		foreach (RosterFight item in list)
		{
			string text = item.GetBattleName();
			if (dictionary.ContainsKey(text))
			{
				FightCounts iECDAEPLNEP = dictionary[text];
				iECDAEPLNEP.WinCount += item.GetWinCount();
				iECDAEPLNEP.LossCount += item.GetLossCount();
				iECDAEPLNEP.EclipseWinCount += item.GetEclipseWinCount();
				iECDAEPLNEP.EclipseLossCount += item.GetEclipseLossCount();
			}
			else
			{
				FightCounts iECDAEPLNEP2 = new FightCounts();
				iECDAEPLNEP2.Name = text;
				iECDAEPLNEP2.WinCount = item.GetWinCount();
				iECDAEPLNEP2.LossCount = item.GetLossCount();
				iECDAEPLNEP2.EclipseWinCount = item.GetEclipseWinCount();
				iECDAEPLNEP2.EclipseLossCount = item.GetEclipseLossCount();
				FightCounts value = iECDAEPLNEP2;
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
			MEEAKLDGLDF["fights"] = jSONClass;
		}
	}

	public static void AddPerks(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "perks")
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
		MEEAKLDGLDF[BFADPFOIPLL] = jSONArray;
	}

	public static void AddPerkChoice(JSONClass MEEAKLDGLDF, ArgsDict PCJAKPJMKGN)
	{
		if (PCJAKPJMKGN.ContainsKey("learnedPerk"))
		{
			PerkInfoItem aCONCDFDNJH = (PerkInfoItem)PCJAKPJMKGN["learnedPerk"];
			MEEAKLDGLDF["taken"] = aCONCDFDNJH.Name;
			MEEAKLDGLDF["taken_upgrade_level"] = aCONCDFDNJH.UpgradeLevel;
		}
		if (PCJAKPJMKGN.ContainsKey("rejectedPerk"))
		{
			PerkInfoItem aCONCDFDNJH2 = (PerkInfoItem)PCJAKPJMKGN["rejectedPerk"];
			MEEAKLDGLDF["rejected"] = aCONCDFDNJH2.Name;
			MEEAKLDGLDF["rejected_upgrade_level"] = aCONCDFDNJH2.UpgradeLevel;
		}
	}

	public static void AddEnchantments(ItemInfo item, JSONClass MEEAKLDGLDF, string IMGCANJHPND)
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
			MEEAKLDGLDF[IMGCANJHPND] = jSONArray;
		}
	}

	public static void AddEquippedItem(JSONClass MEEAKLDGLDF, string JMOHMLIGHHD, ItemInfo item)
	{
		if (item != null)
		{
			MEEAKLDGLDF[JMOHMLIGHHD + "_name"] = item.Name;
			MEEAKLDGLDF[JMOHMLIGHHD + "_upg_level"] = item.UpgradeLevel;
			AddEnchantments(item, MEEAKLDGLDF, JMOHMLIGHHD + "_enchantments");
		}
	}

	public static void AddEquipment(JSONClass MEEAKLDGLDF, ModelParameters JCICKLIMBEF)
	{
		if (JCICKLIMBEF != null)
		{
			AddEquippedItem(MEEAKLDGLDF, "weapon", JCICKLIMBEF.Weapon);
			AddEquippedItem(MEEAKLDGLDF, "armor", JCICKLIMBEF.Armor);
			AddEquippedItem(MEEAKLDGLDF, "helmet", JCICKLIMBEF.Helm);
			AddEquippedItem(MEEAKLDGLDF, "ranged", JCICKLIMBEF.Ranged);
			AddEquippedItem(MEEAKLDGLDF, "magic", JCICKLIMBEF.Magic);
		}
	}

	public static void AddRank(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "rank")
	{
	}

	public static void AddRanks(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "ranks")
	{
		JSONArray aItem = new JSONArray();
		MEEAKLDGLDF.Add(BFADPFOIPLL, aItem);
	}

	public static void AddEnergy(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "energy")
	{
	}

	public static void AddRuns(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "runs")
	{
	}

	public static void AddSeed(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "seed")
	{
	}

	public static void AddFloor(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "floor")
	{
	}

	public static void AddTime(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "time")
	{
	}

	public static void AddUnusedData(JSONClass MEEAKLDGLDF)
	{
	}

	public static void AddPreviousFloorTime(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "prev_floor_time")
	{
	}

	public static void AddScene(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "scene")
	{
	}

	public static void AddScreen(JSONClass MEEAKLDGLDF, string BFADPFOIPLL = "screen")
	{
	}

	public static void AddLevelUpItems(JSONClass MEEAKLDGLDF)
	{
		List<UserItem> list = ListSF.GetRoster().GetInventory().GetItems();
		foreach (UserItem item in list)
		{
			int num = GetItemLevel(item);
			if (num == ListSF.GetRoster().GetLevel() - 1)
			{
				JSONClass jSONClass = null;
				jSONClass = ((!MEEAKLDGLDF.HasValue("items")) ? new JSONClass() : ((JSONClass)MEEAKLDGLDF["items"]));
				AddUserItem(jSONClass, item);
				MEEAKLDGLDF["items"] = jSONClass;
			}
		}
	}

	public static void AddUserItem(JSONClass MEEAKLDGLDF, UserItem item)
	{
		MEEAKLDGLDF["name"] = item.get_Name();
		MEEAKLDGLDF["upgrade_level"] = item.GetUpgradeLevel();
		MEEAKLDGLDF["type"] = item.GetInfo().Type;
		AddEnchantments(item.GetInfo(), MEEAKLDGLDF, "enchantments");
	}

	private static int GetItemLevel(UserItem item)
	{
		ItemInfo dJKEECEOCJB = item.GetCurrentUpgradeItem();
		if (dJKEECEOCJB == null)
		{
			dJKEECEOCJB = item.GetInfo();
		}
		if (dJKEECEOCJB != null)
		{
			return dJKEECEOCJB.ItemLevel;
		}
		GameLog.Error("logging UserItem without ItemInfo");
		return 0;
	}
}
