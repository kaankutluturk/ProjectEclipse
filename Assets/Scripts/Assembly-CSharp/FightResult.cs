using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class FightResult
{
	public class ItemGrant
	{
		public ItemInfo Item;

		public RewardItem RewardSource;

		public bool IsDrop;
	}

	public class MoneyGrant
	{
		public MoneyStruct Money;

		public bool IsDrop;
	}

	public class CurrencyGrant
	{
		public CurrencyStruct Currency;

		public bool IsDrop;
	}

	public class ResistanceGrant
	{
		public ResistanceStruct Resistance;

		public bool IsDrop;
	}

	public class ResultPrizeStruct
	{
		public long Money;

		public long Bonus;

		public uint exp;

		public List<ItemGrant> Items = new List<ItemGrant>();

		public List<CurrencyGrant> Currencies = new List<CurrencyGrant>();

		public List<ResistanceGrant> Resistances = new List<ResistanceGrant>();

		public RewardLottery Lottery;

		public void Clear()
		{
			Money = 0L;
			Bonus = 0L;
			exp = 0u;
			Items.Clear();
			Currencies.Clear();
			Resistances.Clear();
			Lottery = null;
		}

		public void AddReward(Rewardable reward)
		{
			if (reward != null)
			{
				switch (reward.Kind)
				{
				case Rewardable.RewardKind.REWARD_ITEM:
				{
					RewardItem rewardItem = (RewardItem)reward;
					AddReward(rewardItem);
					break;
				}
				case Rewardable.RewardKind.REWARD_MONEY:
				{
					RewardMoney rewardMoney = (RewardMoney)reward;
					AddReward(rewardMoney);
					break;
				}
				case Rewardable.RewardKind.REWARD_CURRENCY:
				{
					RewardCurrency rewardCurrency = (RewardCurrency)reward;
					AddReward(rewardCurrency);
					break;
				}
				case Rewardable.RewardKind.REWARD_RESISTANCE:
				{
					RewardResistance rewardResistance = (RewardResistance)reward;
					AddReward(rewardResistance);
					break;
				}
				case Rewardable.RewardKind.REWARD_LOTTERY:
				{
					RewardLottery rewardLottery = (RewardLottery)reward;
					AddReward(rewardLottery);
					break;
				}
				}
			}
		}

		public void AddReward(RewardMoney reward)
		{
			Money += reward.GetValue();
		}

		public void AddReward(RewardCurrency reward)
		{
			if (reward == null)
			{
				return;
			}
			GameCurrency currency = GameUtils.GameCurrencies.GetCurrencyByName(reward.Name);
			if (currency == null)
			{
				return;
			}
			foreach (CurrencyGrant item in Currencies)
			{
				if (item.Currency.Currency == currency)
				{
					int num = (ObscuredInt)(item.Currency.Count);
					num += reward.RollAmount();
					item.Currency.Count = (ObscuredInt)(num);
					return;
				}
			}
			int num2 = reward.RollAmount();
			if (num2 > 0)
			{
				CurrencyStruct currencyStruct = new CurrencyStruct(currency, num2);
				CurrencyGrant grant = new CurrencyGrant();
				grant.Currency = currencyStruct;
				grant.IsDrop = reward.IsDrop;
				Currencies.Add(grant);
			}
		}

		public void AddReward(RewardResistance reward)
		{
			if (reward == null)
			{
				return;
			}
			GameResistance resistance = GameUtils.GameResistances.GetResistanceByName(reward.Name);
			if (resistance == null)
			{
				return;
			}
			foreach (ResistanceGrant item in Resistances)
			{
				if (item.Resistance.resistance == resistance)
				{
					int num = (ObscuredInt)(item.Resistance.Count);
					num += reward.Value;
					item.Resistance.Count = (ObscuredInt)(num);
					return;
				}
			}
			int amount = reward.Value;
			if (amount > 0)
			{
				ResistanceStruct resistanceStruct = new ResistanceStruct(resistance, amount);
				ResistanceGrant grant = new ResistanceGrant();
				grant.Resistance = resistanceStruct;
				grant.IsDrop = reward.IsDrop;
				Resistances.Add(grant);
			}
		}

		public void AddReward(RewardLottery reward)
		{
			if (reward != null)
			{
				if (Lottery == null)
				{
					Lottery = reward.CloneForRewardComposition();
				}
				else
				{
					Lottery.slots.AddRange(reward.slots);
				}
			}
		}

		public void AddReward(RewardItem reward)
		{
			if (reward == null)
			{
				return;
			}
			UserItem ownedItem = ListSF.GetRoster().GetInventory().FindItem(reward.Name);
			Eclipse.Modding.DefinitionId rewardId;
			bool repeatableModConsumable = Eclipse.Modding.DefinitionId.TryParse(reward.Name, out rewardId) &&
				rewardId.Namespace.Value != "core" && rewardId.Category == "items" &&
				ListSF.GetItems().GetItemByName(reward.Name)?.Type == "Consumable";
			if (ownedItem != null && !repeatableModConsumable)
			{
				return;
			}
			ItemInfo itemInfo = ListSF.GetItems().GetItemByName(reward.Name);
			if (itemInfo == null)
			{
				return;
			}
			bool configuredRewardGrant = reward.HasEclipseGrantConfiguration;
			if (configuredRewardGrant)
			{
				int playerLevelSnapshot = ListSF.GetRoster().Level;
				RewardItem configuredReward;
				string configurationError;
				if (!Eclipse.Modding.ModRuntime.TryConfigureRewardGrant(reward, playerLevelSnapshot,
					out configuredReward, out configurationError))
				{
					UnityEngine.Debug.LogWarning("[ModReward] Skipping configured item reward '" + reward.Name +
						"': " + configurationError);
					return;
				}
				reward = configuredReward;
			}
			int requestedLevel = reward.EvaluateLevel();
			int num = requestedLevel <= 0 ? ListSF.GetRoster().GetLevel() : requestedLevel;
			ItemInfo dJKEECEOCJB2 = null;
			if (itemInfo.ItemLevel == num)
			{
				dJKEECEOCJB2 = itemInfo;
			}
			else
			{
				ItemInfo dJKEECEOCJB3 = itemInfo.GetUpdateItemByLevel(num, false);
				if (dJKEECEOCJB3 == null && configuredRewardGrant)
				{
					UnityEngine.Debug.LogWarning("[ModReward] Skipping configured item reward '" + reward.Name +
						"': exact level " + num + " is unavailable.");
					return;
				}
				dJKEECEOCJB2 = ((dJKEECEOCJB3 == null) ? itemInfo : dJKEECEOCJB3);
			}
			if (!string.IsNullOrEmpty(reward.UpgradeLevelExpression))
			{
				int upgradeLevel = reward.EvaluateUpgradeLevel();
				if (upgradeLevel < 0)
					throw new System.InvalidOperationException("Negative reward upgrade level: " + reward.Name);
				// The native quest grant uses this encoded-level lookup, not an ordinal.
				dJKEECEOCJB2 = itemInfo.GetUpgradeItemAtOrAboveUpgradeLevel(upgradeLevel);
				if (dJKEECEOCJB2 == null)
					throw new System.InvalidOperationException("Reward upgrade level is unavailable: " + reward.Name + " / " + upgradeLevel);
			}
			else if (reward.UpgradeNumber != 0)
			{
				List<UpgradeData> list = dJKEECEOCJB2.GetUpgrades(true, dJKEECEOCJB2.ItemLevel);
				uint count = (uint)list.Count;
				if (count != 0)
				{
					uint num2 = reward.UpgradeNumber;
					if (count - 1 < num2)
					{
						num2 = count - 1;
					}
					dJKEECEOCJB2 = itemInfo.CreateUpgradedItem(list[(int)num2]);
				}
			}
			ItemGrant grant = new ItemGrant();
			grant.Item = dJKEECEOCJB2;
			grant.RewardSource = reward;
			grant.IsDrop = reward.IsDrop;
			Items.Add(grant);
		}

		public List<ItemInfo> GetItems(bool dropsOnly = false)
		{
			List<ItemInfo> list = new List<ItemInfo>();
			foreach (ItemGrant item in Items)
			{
				if (!dropsOnly || item.IsDrop)
				{
					list.Add(item.Item);
				}
			}
			return list;
		}

		public List<CurrencyStruct> GetCurrencies(bool dropsOnly = false)
		{
			List<CurrencyStruct> list = new List<CurrencyStruct>();
			foreach (CurrencyGrant item in Currencies)
			{
				if (!dropsOnly || item.IsDrop)
				{
					list.Add(item.Currency);
				}
			}
			return list;
		}

		public List<ResistanceStruct> GetResistances(bool dropsOnly = false)
		{
			List<ResistanceStruct> list = new List<ResistanceStruct>();
			foreach (ResistanceGrant item in Resistances)
			{
				if (!dropsOnly || item.IsDrop)
				{
					list.Add(item.Resistance);
				}
			}
			return list;
		}

		public RewardLottery GetLottery()
		{
			return Lottery;
		}
	}

	public GameOverTypes GameOverType = GameOverTypes.GAME_OVER_NONE;

	public ResultPrizeStruct Prize = new ResultPrizeStruct();

	public BattleType FightType;

	public int FightValue;

	public FightIDS FightId;

	public float ExpReward;

	public ComboStatistic PlayerStatistics;

	public ComboStatistic OpponentStatistics;

	public FightStatistics Statistics;

	public int PlayerRoundsWon;

	public int OpponentRoundsWon;

	public int TotalRounds;

	public List<ItemInfo> DroppedItems = new List<ItemInfo>();

	public ModelParameters PlayerParameters;

	public ModelParameters OpponentParameters;

	public FightList FightDefinition;

	public void RecordPrizeStatistics(long baseBonus, long baseGold, float experience, float perfectFactor, float firstStrikeFactor, float headShotFactor, float comboFactor, float shockFactor, List<float> styleFactors)
	{
		if (PlayerStatistics != null)
		{
			PlayerStatistics.AddPrizes(baseBonus, baseGold, (long)experience, perfectFactor, firstStrikeFactor, headShotFactor, comboFactor, shockFactor, styleFactors);
		}
	}

	public long GetMoneyReward()
	{
		return Prize.Money;
	}

	public long GetGemsReward()
	{
		return Prize.Bonus;
	}

	public float GetExpReward()
	{
		return Prize.exp;
	}

	public bool IsWinner()
	{
		return GameOverType == GameOverTypes.GAME_OVER_WIN;
	}

	public bool IsRaidRoundTimeout()
	{
		return GameOverType == GameOverTypes.GAME_OVER_RAID_ROUND_TIMEOUT;
	}

	public void CalculateRewards(RewardStruct rewardStruct, ComboStatistic playerStatistics, ComboStatistic opponentStatistics, FightList fightList)
	{
		if (rewardStruct == null)
		{
			this.PlayerStatistics = playerStatistics;
			this.OpponentStatistics = opponentStatistics;
			return;
		}
		Prize.Clear();
		Reward modeReward = ((!ListSF.GetRoster().IsEclipseMode()) ? rewardStruct.NormalModeReward : rewardStruct.EclipseModeReward);
		bool noPrizeBase = true;
		Roster roster = ListSF.GetRoster();
		if ((ObscuredFloat)(rewardStruct.CommonReward.GetPrizeForLevel(roster.GetLevel()).prizeBase) > 0f || (modeReward != null && (ObscuredFloat)(modeReward.GetPrizeForLevel(roster.GetLevel()).prizeBase) > 0f) || fightList.PrizeBase > 0f)
		{
			noPrizeBase = false;
		}
		ApplyReward(rewardStruct.CommonReward, fightList.PrizeBase, playerStatistics, opponentStatistics, noPrizeBase);
		ApplyReward(modeReward, fightList.PrizeBase, playerStatistics, opponentStatistics, noPrizeBase);
	}

	public void ApplyReward(Reward reward, float prizeBase, ComboStatistic playerStatistics, ComboStatistic opponentStatistics, bool noPrizeBase)
	{
		if (reward == null)
		{
			return;
		}
		Roster roster = ListSF.GetRoster();
		RewardPrize prize = reward.GetPrizeForLevel(roster.GetLevel());
		float num = (ObscuredUInt)((reward == null) ? (ObscuredUInt)(0u) : prize.exp);
		ExpReward += num;
		PlayerStatistics = playerStatistics;
		OpponentStatistics = opponentStatistics;
		long num2 = (ObscuredLong)((reward == null) ? (ObscuredLong)(0L) : prize.money);
		float bonus = (ObscuredLong)((reward == null) ? (ObscuredLong)(0L) : prize.bonus);
		float num3 = 0f;
		num3 = ((reward != null && (ObscuredFloat)(prize.prizeBase) > 0f) ? (float)(ObscuredFloat)(prize.prizeBase) : ((prizeBase >= 0f) ? prizeBase : ((!noPrizeBase) ? 0f : Mathf.Ceil((float)num2 * GameUtils.RewardsPrizeSettings.DefaultPrizeBaseFactor))));
		RecordPrizeStatistics((long)num3, num2, bonus, GameUtils.RewardsPrizeSettings.PerfectFactor, GameUtils.RewardsPrizeSettings.FirstStrikeFactor, GameUtils.RewardsPrizeSettings.HeadShotFactor, GameUtils.RewardsPrizeSettings.ComboCountFactor, GameUtils.RewardsPrizeSettings.ShockFactor, GameUtils.RewardsPrizeSettings.Styles);
		Prize.Money = GetComboMoneyReward();
		Prize.Bonus = GetComboBonusReward();
		Prize.exp += (ObscuredUInt)(prize.exp);
		foreach (RewardMoney item in prize.moneyRewards)
		{
			Prize.AddReward(item);
		}
		foreach (RewardCurrency item2 in prize.currencyRewards)
		{
			Prize.AddReward(item2);
		}
		foreach (RewardResistance item3 in prize.resistanceRewards)
		{
			Prize.AddReward(item3);
		}
		if (prize.lottery != null)
		{
			Prize.AddReward(prize.lottery);
		}
		foreach (RewardItem item4 in prize.items)
		{
			Prize.AddReward(item4);
		}
		foreach (RewardChoice item5 in prize.choices)
		{
			Prize.AddReward(item5.ChooseRandomReward());
		}
	}

	private long GetComboBonusReward()
	{
		if (PlayerStatistics != null)
		{
			return PlayerStatistics.Prize.TotalExperience;
		}
		return 0L;
	}

	private long GetComboMoneyReward()
	{
		if (PlayerStatistics != null)
		{
			return PlayerStatistics.Prize.TotalGold;
		}
		return 0L;
	}
}
