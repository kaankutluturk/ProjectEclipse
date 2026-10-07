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

		public void AddReward(Rewardable POHFOGPKMMK)
		{
			if (POHFOGPKMMK != null)
			{
				switch (POHFOGPKMMK.Kind)
				{
				case Rewardable.RewardKind.REWARD_ITEM:
				{
					RewardItem jJBPBGKBEED = (RewardItem)POHFOGPKMMK;
					AddReward(jJBPBGKBEED);
					break;
				}
				case Rewardable.RewardKind.REWARD_MONEY:
				{
					RewardMoney mNEDNJMBHMF = (RewardMoney)POHFOGPKMMK;
					AddReward(mNEDNJMBHMF);
					break;
				}
				case Rewardable.RewardKind.REWARD_CURRENCY:
				{
					RewardCurrency oIPIAAJCEOO = (RewardCurrency)POHFOGPKMMK;
					AddReward(oIPIAAJCEOO);
					break;
				}
				case Rewardable.RewardKind.REWARD_RESISTANCE:
				{
					RewardResistance gBKBCEGJNLA = (RewardResistance)POHFOGPKMMK;
					AddReward(gBKBCEGJNLA);
					break;
				}
				case Rewardable.RewardKind.REWARD_LOTTERY:
				{
					RewardLottery mIPHAMDMKJB = (RewardLottery)POHFOGPKMMK;
					AddReward(mIPHAMDMKJB);
					break;
				}
				}
			}
		}

		public void AddReward(RewardMoney MNEDNJMBHMF)
		{
			Money += MNEDNJMBHMF.GetValue();
		}

		public void AddReward(RewardCurrency OIPIAAJCEOO)
		{
			if (OIPIAAJCEOO == null)
			{
				return;
			}
			GameCurrency cJJOFMHLFFM = GameUtils.GameCurrencies.GetCurrencyByName(OIPIAAJCEOO.Name);
			if (cJJOFMHLFFM == null)
			{
				return;
			}
			foreach (CurrencyGrant item in Currencies)
			{
				if (item.Currency.Currency == cJJOFMHLFFM)
				{
					int num = (ObscuredInt)(item.Currency.Count);
					num += OIPIAAJCEOO.RollAmount();
					item.Currency.Count = (ObscuredInt)(num);
					return;
				}
			}
			int num2 = OIPIAAJCEOO.RollAmount();
			if (num2 > 0)
			{
				CurrencyStruct nAKKNKPJNHB = new CurrencyStruct(cJJOFMHLFFM, num2);
				CurrencyGrant nFBOLAJJIAD = new CurrencyGrant();
				nFBOLAJJIAD.Currency = nAKKNKPJNHB;
				nFBOLAJJIAD.IsDrop = OIPIAAJCEOO.IsDrop;
				Currencies.Add(nFBOLAJJIAD);
			}
		}

		public void AddReward(RewardResistance GBKBCEGJNLA)
		{
			if (GBKBCEGJNLA == null)
			{
				return;
			}
			GameResistance oOJJEOFENBJ = GameUtils.GameResistances.GetResistanceByName(GBKBCEGJNLA.Name);
			if (oOJJEOFENBJ == null)
			{
				return;
			}
			foreach (ResistanceGrant item in Resistances)
			{
				if (item.Resistance.resistance == oOJJEOFENBJ)
				{
					int num = (ObscuredInt)(item.Resistance.Count);
					num += GBKBCEGJNLA.Value;
					item.Resistance.Count = (ObscuredInt)(num);
					return;
				}
			}
			int iOHAOMLJECE = GBKBCEGJNLA.Value;
			if (iOHAOMLJECE > 0)
			{
				ResistanceStruct jIDLBLPFAAE = new ResistanceStruct(oOJJEOFENBJ, iOHAOMLJECE);
				ResistanceGrant oLJIFHLGHNM = new ResistanceGrant();
				oLJIFHLGHNM.Resistance = jIDLBLPFAAE;
				oLJIFHLGHNM.IsDrop = GBKBCEGJNLA.IsDrop;
				Resistances.Add(oLJIFHLGHNM);
			}
		}

		public void AddReward(RewardLottery MIPHAMDMKJB)
		{
			if (MIPHAMDMKJB != null)
			{
				if (Lottery == null)
				{
					Lottery = MIPHAMDMKJB.CloneForRewardComposition();
				}
				else
				{
					Lottery.slots.AddRange(MIPHAMDMKJB.slots);
				}
			}
		}

		public void AddReward(RewardItem JJBPBGKBEED)
		{
			if (JJBPBGKBEED == null)
			{
				return;
			}
			UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(JJBPBGKBEED.Name);
			Eclipse.Modding.DefinitionId rewardId;
			bool repeatableModConsumable = Eclipse.Modding.DefinitionId.TryParse(JJBPBGKBEED.Name, out rewardId) &&
				rewardId.Namespace.Value != "core" && rewardId.Category == "items" &&
				ListSF.GetItems().GetItemByName(JJBPBGKBEED.Name)?.Type == "Consumable";
			if (dKCHDHMLKHN != null && !repeatableModConsumable)
			{
				return;
			}
			ItemInfo dJKEECEOCJB = ListSF.GetItems().GetItemByName(JJBPBGKBEED.Name);
			if (dJKEECEOCJB == null)
			{
				return;
			}
			bool configuredRewardGrant = JJBPBGKBEED.HasEclipseGrantConfiguration;
			if (configuredRewardGrant)
			{
				int playerLevelSnapshot = ListSF.GetRoster().Level;
				RewardItem configuredReward;
				string configurationError;
				if (!Eclipse.Modding.ModRuntime.TryConfigureRewardGrant(JJBPBGKBEED, playerLevelSnapshot,
					out configuredReward, out configurationError))
				{
					UnityEngine.Debug.LogWarning("[ModReward] Skipping configured item reward '" + JJBPBGKBEED.Name +
						"': " + configurationError);
					return;
				}
				JJBPBGKBEED = configuredReward;
			}
			int requestedLevel = JJBPBGKBEED.EvaluateLevel();
			int num = requestedLevel <= 0 ? ListSF.GetRoster().GetLevel() : requestedLevel;
			ItemInfo dJKEECEOCJB2 = null;
			if (dJKEECEOCJB.ItemLevel == num)
			{
				dJKEECEOCJB2 = dJKEECEOCJB;
			}
			else
			{
				ItemInfo dJKEECEOCJB3 = dJKEECEOCJB.GetUpdateItemByLevel(num, false);
				if (dJKEECEOCJB3 == null && configuredRewardGrant)
				{
					UnityEngine.Debug.LogWarning("[ModReward] Skipping configured item reward '" + JJBPBGKBEED.Name +
						"': exact level " + num + " is unavailable.");
					return;
				}
				dJKEECEOCJB2 = ((dJKEECEOCJB3 == null) ? dJKEECEOCJB : dJKEECEOCJB3);
			}
			if (!string.IsNullOrEmpty(JJBPBGKBEED.UpgradeLevelExpression))
			{
				int upgradeLevel = JJBPBGKBEED.EvaluateUpgradeLevel();
				if (upgradeLevel < 0)
					throw new System.InvalidOperationException("Negative reward upgrade level: " + JJBPBGKBEED.Name);
				// The native quest grant uses this encoded-level lookup, not an ordinal.
				dJKEECEOCJB2 = dJKEECEOCJB.GetUpgradeItemAtOrAboveUpgradeLevel(upgradeLevel);
				if (dJKEECEOCJB2 == null)
					throw new System.InvalidOperationException("Reward upgrade level is unavailable: " + JJBPBGKBEED.Name + " / " + upgradeLevel);
			}
			else if (JJBPBGKBEED.UpgradeNumber != 0)
			{
				List<UpgradeData> list = dJKEECEOCJB2.GetUpgrades(true, dJKEECEOCJB2.ItemLevel);
				uint count = (uint)list.Count;
				if (count != 0)
				{
					uint num2 = JJBPBGKBEED.UpgradeNumber;
					if (count - 1 < num2)
					{
						num2 = count - 1;
					}
					dJKEECEOCJB2 = dJKEECEOCJB.CreateUpgradedItem(list[(int)num2]);
				}
			}
			ItemGrant lJFFIBFBGID = new ItemGrant();
			lJFFIBFBGID.Item = dJKEECEOCJB2;
			lJFFIBFBGID.RewardSource = JJBPBGKBEED;
			lJFFIBFBGID.IsDrop = JJBPBGKBEED.IsDrop;
			Items.Add(lJFFIBFBGID);
		}

		public List<ItemInfo> GetItems(bool NLDNIHHPEFI = false)
		{
			List<ItemInfo> list = new List<ItemInfo>();
			foreach (ItemGrant item in Items)
			{
				if (!NLDNIHHPEFI || item.IsDrop)
				{
					list.Add(item.Item);
				}
			}
			return list;
		}

		public List<CurrencyStruct> GetCurrencies(bool NLDNIHHPEFI = false)
		{
			List<CurrencyStruct> list = new List<CurrencyStruct>();
			foreach (CurrencyGrant item in Currencies)
			{
				if (!NLDNIHHPEFI || item.IsDrop)
				{
					list.Add(item.Currency);
				}
			}
			return list;
		}

		public List<ResistanceStruct> GetResistances(bool NLDNIHHPEFI = false)
		{
			List<ResistanceStruct> list = new List<ResistanceStruct>();
			foreach (ResistanceGrant item in Resistances)
			{
				if (!NLDNIHHPEFI || item.IsDrop)
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

	public void RecordPrizeStatistics(long BLOOFMGLMHP, long GICNLBOICGP, float KNDKJANLIDI, float BHGNKHIKGOG, float FKHKEHICPAH, float IFCOPPPDOCD, float LMKJOMKPOAM, float OJIPBDBMLLO, List<float> JGANMCPMMLN)
	{
		if (PlayerStatistics != null)
		{
			PlayerStatistics.AddPrizes(BLOOFMGLMHP, GICNLBOICGP, (long)KNDKJANLIDI, BHGNKHIKGOG, FKHKEHICPAH, IFCOPPPDOCD, LMKJOMKPOAM, OJIPBDBMLLO, JGANMCPMMLN);
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

	public void CalculateRewards(RewardStruct LGDIIADDFLH, ComboStatistic AIOMDIAFHGB, ComboStatistic MOJHPBGGNAH, FightList KGKDKENMAOA)
	{
		if (LGDIIADDFLH == null)
		{
			this.PlayerStatistics = AIOMDIAFHGB;
			this.OpponentStatistics = MOJHPBGGNAH;
			return;
		}
		Prize.Clear();
		Reward lOELDGJGPIF = ((!ListSF.GetRoster().IsEclipseMode()) ? LGDIIADDFLH.NormalModeReward : LGDIIADDFLH.EclipseModeReward);
		bool bLBDMKNOJEJ = true;
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if ((ObscuredFloat)(LGDIIADDFLH.CommonReward.GetPrizeForLevel(nKGLHEGIKKP.GetLevel()).prizeBase) > 0f || (lOELDGJGPIF != null && (ObscuredFloat)(lOELDGJGPIF.GetPrizeForLevel(nKGLHEGIKKP.GetLevel()).prizeBase) > 0f) || KGKDKENMAOA.PrizeBase > 0f)
		{
			bLBDMKNOJEJ = false;
		}
		ApplyReward(LGDIIADDFLH.CommonReward, KGKDKENMAOA.PrizeBase, AIOMDIAFHGB, MOJHPBGGNAH, bLBDMKNOJEJ);
		ApplyReward(lOELDGJGPIF, KGKDKENMAOA.PrizeBase, AIOMDIAFHGB, MOJHPBGGNAH, bLBDMKNOJEJ);
	}

	public void ApplyReward(Reward POHFOGPKMMK, float prizeBase, ComboStatistic ODOJIOOGLJM, ComboStatistic IHNEOCGCCJO, bool BLBDMKNOJEJ)
	{
		if (POHFOGPKMMK == null)
		{
			return;
		}
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		RewardPrize cMHHEHILIIH = POHFOGPKMMK.GetPrizeForLevel(nKGLHEGIKKP.GetLevel());
		float num = (ObscuredUInt)((POHFOGPKMMK == null) ? (ObscuredUInt)(0u) : cMHHEHILIIH.exp);
		ExpReward += num;
		PlayerStatistics = ODOJIOOGLJM;
		OpponentStatistics = IHNEOCGCCJO;
		long num2 = (ObscuredLong)((POHFOGPKMMK == null) ? (ObscuredLong)(0L) : cMHHEHILIIH.money);
		float kNDKJANLIDI = (ObscuredLong)((POHFOGPKMMK == null) ? (ObscuredLong)(0L) : cMHHEHILIIH.bonus);
		float num3 = 0f;
		num3 = ((POHFOGPKMMK != null && (ObscuredFloat)(cMHHEHILIIH.prizeBase) > 0f) ? (float)(ObscuredFloat)(cMHHEHILIIH.prizeBase) : ((prizeBase >= 0f) ? prizeBase : ((!BLBDMKNOJEJ) ? 0f : Mathf.Ceil((float)num2 * GameUtils.RewardsPrizeSettings.DefaultPrizeBaseFactor))));
		RecordPrizeStatistics((long)num3, num2, kNDKJANLIDI, GameUtils.RewardsPrizeSettings.PerfectFactor, GameUtils.RewardsPrizeSettings.FirstStrikeFactor, GameUtils.RewardsPrizeSettings.HeadShotFactor, GameUtils.RewardsPrizeSettings.ComboCountFactor, GameUtils.RewardsPrizeSettings.ShockFactor, GameUtils.RewardsPrizeSettings.Styles);
		Prize.Money = GetComboMoneyReward();
		Prize.Bonus = GetComboBonusReward();
		Prize.exp += (ObscuredUInt)(cMHHEHILIIH.exp);
		foreach (RewardMoney item in cMHHEHILIIH.moneyRewards)
		{
			Prize.AddReward(item);
		}
		foreach (RewardCurrency item2 in cMHHEHILIIH.currencyRewards)
		{
			Prize.AddReward(item2);
		}
		foreach (RewardResistance item3 in cMHHEHILIIH.resistanceRewards)
		{
			Prize.AddReward(item3);
		}
		if (cMHHEHILIIH.lottery != null)
		{
			Prize.AddReward(cMHHEHILIIH.lottery);
		}
		foreach (RewardItem item4 in cMHHEHILIIH.items)
		{
			Prize.AddReward(item4);
		}
		foreach (RewardChoice item5 in cMHHEHILIIH.choices)
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
