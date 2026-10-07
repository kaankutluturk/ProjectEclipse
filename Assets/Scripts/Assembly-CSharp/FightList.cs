using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class FightList
{
    // Transient encounter observation identity; never part of the roster save.
    internal Eclipse.Modding.ModStoryEncounter EclipseStoryEncounter;
	// best guess for name
	public FightIDS FightId = new FightIDS();

	protected BattleType _type;

	protected RosterFight rosterFight;

	protected int powerRequired;

	protected string _description = string.Empty;

	// best guess for name
	public string Location = string.Empty;

	// best guess for name
	public Battle Battle;

	public string Name = string.Empty;

	public int Index = -1;

	public int ReplayCount;

	public long RepeatTime;

	// best guess for name
	public bool TrackFightProgress;

	private bool isInFight;

	private bool randomRulesResetPending;

	// best guess for name
	public ConditionStatus Status;

	public bool IsLocked;

	// best guess for name
	public int RoundsToWin;

	public ObscuredInt RoundTime;

	public int EffectiveRoundTime => _type == BattleType.FightNone ? (int)RoundTime :
		Eclipse.Modding.ModPolicies.BattleSeconds((int)RoundTime,
			_type == BattleType.FightRaid || (Battle != null &&
				(Eclipse.Modding.ModPolicies.TryUnderworldBattle(Battle.get_Name(), out _) ||
				 Eclipse.Modding.ModPolicies.TryRaidBattle(Battle.get_Name(), out _))));

	// best guess for name
	public int RewardIndex;

	public ObscuredLong PrizeMoney;

	public ObscuredLong PrizeBonus;

	public ObscuredUInt PrizeExp;

	public string AltImage = string.Empty;

	public float PrizeBase;

	public float EvaluatedRating;

	// best guess for name
	public float HealthRecovery;

	// best guess for name
	public string Music = string.Empty;

	private List<ModelParameters> opponents = new List<ModelParameters>();

	private List<ConditionFight> _conditions = new List<ConditionFight>();

	private List<RewardStruct> rewards = new List<RewardStruct>();

	private List<ItemRule> _itemRules = new List<ItemRule>();

	private List<RandomRule> _randomRules = new List<RandomRule>();

	private DescriptionRule descriptionRule;

	private List<Rule> _rules = new List<Rule>();

	private List<Rule> eclipseRules = new List<Rule>();

	private List<Rule> temporaryRules = new List<Rule>();

	private List<Rule> allRules = new List<Rule>();

	public ushort RewardDigits;

	public ushort PrizeBaseDigits;

	public string RewardImage = string.Empty;

	public RosterFight RosterFightInfo
	{
		get
		{
			return GetRosterFight();
		}
		set
		{
			SetRosterFight(value);
		}
	}

	public int RequiredPower
	{
		get
		{
			return GetPowerRequired();
		}
		set
		{
			set_PowerRequired(value);
		}
	}

	public string FightDescription
	{
		get
		{
			return GetDescription();
		}
		set
		{
			set_Description(value);
		}
	}

	public bool InFight
	{
		get
		{
			return GetIsInFight();
		}
		set
		{
			set_IsInFight(value);
		}
	}

	public List<ModelParameters> Opponents
	{
		get
		{
			return GetOpponents();
		}
	}

	public List<ConditionFight> Conditions
	{
		get
		{
			return GetConditions();
		}
	}

	public List<RewardStruct> Rewards
	{
		get
		{
			return GetRewards();
		}
	}

	public List<ItemRule> ItemRules
	{
		get
		{
			return GetItemRules();
		}
	}

	public List<RandomRule> RandomRules
	{
		get
		{
			return GetRandomRules();
		}
	}

	public BattleType get_Type()
	{
		return _type;
	}

	public void set_Type(BattleType value)
	{
		_type = value;
	}

	public RosterFight GetRosterFight()
	{
		return rosterFight;
	}

	// best guess for name
	public void SetRosterFight(RosterFight value)
	{
		rosterFight = value;
		rosterFight.LinkedFightList = this;
	}

	public int GetPowerRequired()
	{
		return powerRequired;
	}

	public void set_PowerRequired(int value)
	{
		powerRequired = value;
	}

	public string GetDescription()
	{
		// Map previews request descriptions before fight initialization populates
        // the selected rule. Resolve the active rule for the preview as well.
        DescriptionRule description = descriptionRule ?? FindDescriptionRule();
        return description == null ? _description : description.GetDescriptionAlias();
	}

	public void set_Description(string value)
	{
		_description = value;
	}

	public bool GetIsInFight()
	{
		return isInFight;
	}

	public void set_IsInFight(bool value)
	{
		isInFight = value;
	}

	public List<ModelParameters> GetOpponents()
	{
		return opponents;
	}

	public List<ConditionFight> GetConditions()
	{
		return _conditions;
	}

	public List<RewardStruct> GetRewards()
	{
		return rewards;
	}

	public List<ItemRule> GetItemRules()
	{
		return _itemRules;
	}

	public List<RandomRule> GetRandomRules()
	{
		return _randomRules;
	}

	public void RandomizeObscuredVars()
	{
		RoundTime.RandomizeCryptoKey();
		PrizeMoney.RandomizeCryptoKey();
		PrizeBonus.RandomizeCryptoKey();
		PrizeExp.RandomizeCryptoKey();
		GetRewards().ForEach((RewardStruct DHDMNHCIPEH) =>
		{
			DHDMNHCIPEH.RandomizeObscuredVars();
		});
		GetOpponents().ForEach((ModelParameters DHDMNHCIPEH) =>
		{
			DHDMNHCIPEH.RandomizeObscuredVars();
		});
	}

	public long GetTimeLeft()
	{
		long num = RepeatTime - rosterFight.GetElapsedSinceCompletion();
		if (num < 0)
		{
			num = 0L;
		}
		return num;
	}

	public string GetRuleItemName(string LMNNBBKHMEI)
	{
		foreach (ItemRule item in _itemRules)
		{
			if (item.IsPlayerLevelInRange() && !item.GetIsEquipRule())
			{
				UserItem dKCHDHMLKHN = item.get_Item();
				if (dKCHDHMLKHN.GetInfo().Type == LMNNBBKHMEI)
				{
					return dKCHDHMLKHN.get_Name();
				}
			}
		}
		return string.Empty;
	}

	public int GetRuleItemLevel(string LMNNBBKHMEI)
	{
		foreach (ItemRule item in _itemRules)
		{
			if (item.IsPlayerLevelInRange() && !item.GetIsEquipRule())
			{
				UserItem dKCHDHMLKHN = item.get_Item();
				if (dKCHDHMLKHN.GetInfo().Type == LMNNBBKHMEI)
				{
					return dKCHDHMLKHN.GetUpgradeLevel();
				}
			}
		}
		return 0;
	}

	public bool HasCurrencyCost()
	{
		List<CurrencyCostRule> list = GetCurrencyCostRules();
		foreach (CurrencyCostRule item in list)
		{
			if (item.GetCurrencyName() != string.Empty && item.GetCurrencyValue() > 0)
			{
				return true;
			}
		}
		return false;
	}

	public int GetCurrencyCost(string currencyName)
	{
		int num = 0;
		List<CurrencyCostRule> list = GetCurrencyCostRules();
		foreach (CurrencyCostRule item in list)
		{
			if (item.GetCurrencyName() == currencyName)
			{
				num += item.GetCurrencyValue();
			}
		}
		return num;
	}

	public void EquipRuleItems(ModelParameters IHEFAMAFBIA, bool FFBFPLODJME, int round = 0)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		List<ItemRule> list = GetItemRules();
		foreach (ItemRule item in list)
		{
			if (round > 0 && !item.AppliesToRound(round) && !item.IsPlayerLevelInRange())
			{
				continue;
			}
			UserItem dKCHDHMLKHN = item.get_Item();
			ItemInfo dJKEECEOCJB = dKCHDHMLKHN.GetInfo();
			string text = dKCHDHMLKHN.get_Name();
			if (text == string.Empty)
			{
				GameLog.Error("name for item is empty");
			}
			UserItem dKCHDHMLKHN2 = ListSF.GetRoster().GetInventory().FindItem(text);
			dJKEECEOCJB = null;
			if (dKCHDHMLKHN2 == null)
			{
				dJKEECEOCJB = ListSF.GetItems().GetItemByName(text);
				if (dJKEECEOCJB == null)
				{
					GameLog.Error(" Model::equipRulesItems - item not found \"%s\"", text);
					continue;
				}
			}
			else
			{
				dJKEECEOCJB = dKCHDHMLKHN2.GetInfo();
			}
			if ((dJKEECEOCJB == null || !nKGLHEGIKKP.GetInventory().HasItem(dJKEECEOCJB)) && !item.GetIsEquipRule())
			{
				dJKEECEOCJB = null;
			}
			if (dJKEECEOCJB != null && (!FFBFPLODJME || !item.GetNoAttributeChange()))
			{
				ItemInfo dJKEECEOCJB2 = dJKEECEOCJB.Clone();
				dJKEECEOCJB2.IgnoreInventoryEnchantments = true;
				IHEFAMAFBIA.SetItemByType(dJKEECEOCJB.Type, dJKEECEOCJB2);
			}
		}
	}

	public RewardStruct GetRewardAt(int index)
	{
		if (GetRewards().Count > index)
		{
			return GetRewards()[index];
		}
		GameLog.Write("FightList::getReward - wrong index: " + index + " from " + GetRewards().Count + " (we need this error?)");
		return null;
	}

	public bool IsReplayAvailable()
	{
		return rosterFight == null || rosterFight.IsRepeatAvailable(RepeatTime);
	}

	public void AddReward(RewardStruct LGDIIADDFLH)
	{
		rewards.Add(LGDIIADDFLH);
	}

	public List<Rule> GetRules()
	{
		return (!ListSF.GetRoster().IsEclipseMode()) ? _rules : eclipseRules;
	}

	public bool MeetsPlayerItemRequirements(ModelParameters parameters)
	{
		if (_type != BattleType.FightChallenge)
		{
			return true;
		}
		foreach (Rule rule in GetRules())
		{
			ItemRule itemRule = rule as ItemRule;
			if (itemRule == null || !itemRule.IsEntryRequirement() || !rule.IsPlayerLevelInRange())
			{
				continue;
			}
			RuleAppliance appliance = itemRule.GetAppliance();
			if ((appliance == RuleAppliance.AppliancePlayer || appliance == RuleAppliance.ApplianceAll) &&
				!itemRule.IsSatisfiedBy(parameters))
			{
				return false;
			}
		}
		return true;
	}

	public void CopyRulesToEclipseRules()
	{
		eclipseRules = _rules;
	}

	public void SetTime(long time)
	{
		RosterFight pIGKOIFBOME = GetRosterFight();
		if (pIGKOIFBOME != null)
		{
			if (_type == BattleType.FightPeriodic)
			{
				pIGKOIFBOME.UpdateElapsedSinceRandomize(time);
				TriggerDuelUnlockedQuests();
			}
			pIGKOIFBOME.UpdateElapsedSinceCompletion(time);
		}
	}

	public int GetTotalOpponentRounds()
	{
		int num = 0;
		foreach (ModelParameters item in opponents)
		{
			num += item.OpponentCount;
		}
		return num;
	}

	public float CalculateDifficulty(ModelParameters ACENLMONNPA, ModelParameters HFGPAELCNMF)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		RatingEvaluationRule hIFCIAHLLAE = GetRatingEvaluationRule();
		if (hIFCIAHLLAE != null)
		{
			num = hIFCIAHLLAE.GetPlayerRating();
			num2 = hIFCIAHLLAE.GetEnemyRating();
			num3 = hIFCIAHLLAE.GetRatingCorrection();
		}
		EquippedItemsStruct hELFDCAIJNE = new EquippedItemsStruct();
		EquippedItemsStruct hELFDCAIJNE2 = new EquippedItemsStruct();
		ACENLMONNPA.CopyEquippedItemsTo(hELFDCAIJNE);
		HFGPAELCNMF.CopyEquippedItemsTo(hELFDCAIJNE2);
		ApplyRuleItems(ACENLMONNPA);
		ApplyRuleItems(HFGPAELCNMF);
		List<global::Pair<string, float>> list = GetPlayerAttributeModifiers();
		List<global::Pair<string, float>> list2 = GetOpponentAttributeModifiers();
		if (num == 0f)
		{
			num = HFGPAELCNMF.GetPlayerRating();
		}
		if (num < 0f)
		{
			num = ACENLMONNPA.CalculateDamageRating(HFGPAELCNMF, list);
		}
		if (num2 == 0f)
		{
			num2 = HFGPAELCNMF.GetEnemyRating();
		}
		if (num2 < 0f)
		{
			num2 = HFGPAELCNMF.CalculateDamageRating(ACENLMONNPA, list2);
		}
		float num4 = GameUtils.GetDamageFactorBase();
		string kGBGENDIMBC = GameUtils.GetDamageFactorAttribute();
		int OEMALIFPGPO = 0;
		int OEMALIFPGPO2 = 0;
		ModelParameters kIKOGDEPGHB = HFGPAELCNMF.Clone();
		ModelParameters kIKOGDEPGHB2 = ACENLMONNPA.Clone();
		kIKOGDEPGHB.AddAttributeShifts(list2);
		kIKOGDEPGHB2.AddAttributeShifts(list);
		kIKOGDEPGHB.FinalAttributes.Get(kGBGENDIMBC, ref OEMALIFPGPO);
		kIKOGDEPGHB2.FinalAttributes.Get(kGBGENDIMBC, ref OEMALIFPGPO2);
		float num5 = 1f;
		float num6 = 1f;
		List<Rule> list3 = GetRules();
		List<InFightRule> list4 = new List<InFightRule>();
		foreach (Rule item in list3)
		{
			if (item.get_Type() == Rule.RuleType.RuleResistance)
			{
				InFightRule aAJIFBJLJOA = item as InFightRule;
				if (aAJIFBJLJOA != null)
				{
					list4.Add(aAJIFBJLJOA);
				}
			}
		}
		foreach (InFightRule item2 in list4)
		{
			ResistanceRule hCOHJNFLKIF = item2 as ResistanceRule;
			if (hCOHJNFLKIF != null)
			{
				string gOHIIMFFFJI = hCOHJNFLKIF.GetResistanceName();
				int num7 = hCOHJNFLKIF.GetResistanceValue();
				int num8 = ListSF.GetRoster().GetResistanceCount(gOHIIMFFFJI);
				if (num8 < num7)
				{
					float num9 = Mathf.Pow(2f, (float)(num7 - num8) / GameUtils.GetResistanceDoublingRange());
					float num10 = Mathf.Pow(2f, (float)(num8 - num7) / GameUtils.GetResistanceDoublingRange());
					num6 *= num9;
					num5 *= num10;
				}
			}
		}
		float num11 = num2 / num * Mathf.Pow(2f, (float)(OEMALIFPGPO - OEMALIFPGPO2) * num4) * num6 / num5;
		float num12 = (float)HFGPAELCNMF.RatingCorrection + num3;
		num11 *= Mathf.Pow(2f, 2f * num12 / GameUtils.DamageDoublingRange);
		ACENLMONNPA.SetEquippedItemsFrom(hELFDCAIJNE);
		HFGPAELCNMF.SetEquippedItemsFrom(hELFDCAIJNE2);
		return num11;
	}

	public float CalculateDifficultyVsLastOpponent(ModelParameters ACENLMONNPA, List<ModelParameters> IDAAONBIBJM)
	{
		float result = 0f;
		int count = IDAAONBIBJM.Count;
		if (0 < count)
		{
			ModelParameters hFGPAELCNMF = IDAAONBIBJM[count - 1];
			result = CalculateDifficulty(ACENLMONNPA, hFGPAELCNMF);
		}
		else
		{
			GameLog.Error("enemy less than 1");
		}
		return result;
	}

	public bool HasMultipleOpponentsAndRounds()
	{
		return GetOpponents().Count > 1 && RoundsToWin > 1;
	}

	public void PutRule(Rule HNBFMAKFJAM)
	{
		switch (HNBFMAKFJAM.get_Type())
		{
		case Rule.RuleType.RuleItem:
		case Rule.RuleType.RuleEquipItem:
		case Rule.RuleType.RuleRandomAquiredItem:
			_itemRules.AddIfNotExist((ItemRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleRandom:
			_randomRules.AddIfNotExist((RandomRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleComplex:
			CollectNestedRandomRules((ComplexRule)HNBFMAKFJAM);
			break;
		default:
			GameLog.Error("FightList::putRule ERROR - wrong rule type %i. Rule not added to fight.", HNBFMAKFJAM.get_Type());
			return;
		case Rule.RuleType.RuleNoButton:
		case Rule.RuleType.RuleNoAnimation:
		case Rule.RuleType.RuleRingout:
		case Rule.RuleType.RuleDarkness:
		case Rule.RuleType.RuleLightInTheDarkness:
		case Rule.RuleType.RuleHotGround:
		case Rule.RuleType.RuleLoseFall:
		case Rule.RuleType.RuleRegeneration:
		case Rule.RuleType.RuleAttributes:
		case Rule.RuleType.RuleDamageFactor:
		case Rule.RuleType.RuleRemoveInterval:
		case Rule.RuleType.RuleCrazy:
		case Rule.RuleType.RuleLifeSteal:
		case Rule.RuleType.RuleNoHealthBar:
		case Rule.RuleType.RuleCombo:
		case Rule.RuleType.RuleTimeoutWin:
		case Rule.RuleType.RulePoints:
		case Rule.RuleType.RuleRechargeMagicEachRound:
		case Rule.RuleType.RuleNoBulletsReplenishment:
		case Rule.RuleType.RuleDescription:
		case Rule.RuleType.RulePerk:
		case Rule.RuleType.RuleNoPerks:
		case Rule.RuleType.RuleWinStyle:
		case Rule.RuleType.RuleWinCombo:
		case Rule.RuleType.RuleWinShock:
		case Rule.RuleType.RuleChangeFight:
		case Rule.RuleType.RuleTactic:
		case Rule.RuleType.RuleInvertJoystick:
		case Rule.RuleType.RuleRandomArea:
		case Rule.RuleType.RuleRatingEvaluation:
		case Rule.RuleType.RuleInvulnerability:
		case Rule.RuleType.RuleCurrencyCost:
		case Rule.RuleType.RuleResistance:
		case Rule.RuleType.RuleRaidCurrencyCost:
		case Rule.RuleType.RuleAvatar:
		case Rule.RuleType.RuleName:
			break;
		}
		switch (HNBFMAKFJAM.ModeFilter)
		{
		case Rule.RuleModeFilter.MODE_ECLIPSE:
			eclipseRules.AddIfNotExist(HNBFMAKFJAM);
			break;
		case Rule.RuleModeFilter.MODE_NORMAL:
			_rules.AddIfNotExist(HNBFMAKFJAM);
			break;
		default:
			eclipseRules.AddIfNotExist(HNBFMAKFJAM);
			_rules.AddIfNotExist(HNBFMAKFJAM);
			break;
		}
		allRules.AddIfNotExist(HNBFMAKFJAM);
	}

	// best guess for name
	public void AddOpponent(ModelParameters AIIALIFJJMB)
	{
		opponents.AddIfNotExist(AIIALIFJJMB);
	}

	public void PutTemporaryRule(Rule HNBFMAKFJAM)
	{
		temporaryRules.AddIfNotExist(HNBFMAKFJAM);
		PutRule(HNBFMAKFJAM);
	}

	public void TriggerDuelUnlockedQuests()
	{
		if (rosterFight.RerandomizeIfElapsed(RepeatTime) && !IsReplayAvailable() && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_DUEL_UNLOCKED))
		{
			ListSF.GetInstance().RunQuestActions();
		}
	}

	public void ResetRandomRules()
	{
		if (_type != BattleType.FightPeriodic || _randomRules.Count == 0)
		{
			return;
		}
		if (isInFight)
		{
			randomRulesResetPending = true;
			return;
		}
		NekkiMath.SetSeed(rosterFight.GetRandomRuleSeed());
		foreach (RandomRule item in _randomRules)
		{
			if (item.GetRefreshMode() == RandomRule.RefreshMode.REFRESH_EACH_FIGHT)
			{
				item.SelectRandomRule();
			}
		}
		NekkiMath.SetSeed();
		RefreshDescriptionRule();
		randomRulesResetPending = false;
	}

	public void ApplyPendingRandomReset()
	{
		if (randomRulesResetPending)
		{
			ResetRandomRules();
		}
	}

	public void UpdateLevel(int PPGFCLBFLEK)
	{
		int count = rewards.Count;
		if (count > 0)
		{
			RewardStruct fDFKLPHBAHJ = rewards[count - 1];
			RewardPrize cMHHEHILIIH = fDFKLPHBAHJ.GetPrizeForLevel(PPGFCLBFLEK);
			PrizeExp = cMHHEHILIIH.exp;
			PrizeMoney = cMHHEHILIIH.money;
			PrizeBonus = cMHHEHILIIH.bonus;
		}
		else
		{
			PrizeExp = (ObscuredUInt)(0u);
			PrizeMoney = (ObscuredLong)(0L);
			PrizeBonus = (ObscuredLong)(0L);
		}
	}

	public void RemoveTemporaryRules()
	{
		foreach (Rule item in temporaryRules)
		{
			allRules.Remove(item);
			switch (item.get_Type())
			{
			case Rule.RuleType.RuleItem:
			case Rule.RuleType.RuleEquipItem:
			case Rule.RuleType.RuleRandomAquiredItem:
				_itemRules.Remove((ItemRule)item);
				break;
			case Rule.RuleType.RuleRandom:
				_randomRules.Remove((RandomRule)item);
				break;
			}
			_rules.Remove(item);
			eclipseRules.Remove(item);
		}
		temporaryRules.Clear();
	}

	public void RefreshDescriptionRule()
	{
		descriptionRule = FindDescriptionRule();
		NotifyRulesChanged();
	}

	public virtual List<CurrencyCostRule> GetCurrencyCostRules()
	{
		List<CurrencyCostRule> list = new List<CurrencyCostRule>();
		List<Rule> list2 = GetRules();
		foreach (Rule item2 in list2)
		{
			if (item2.get_Type() == Rule.RuleType.RuleCurrencyCost)
			{
				CurrencyCostRule item = (CurrencyCostRule)item2;
				list.Add(item);
			}
		}
		return list;
	}

	private DescriptionRule FindDescriptionRule(Rule HNBFMAKFJAM)
	{
		switch (HNBFMAKFJAM.get_Type())
		{
		case Rule.RuleType.RuleDescription:
			return (DescriptionRule)HNBFMAKFJAM;
		case Rule.RuleType.RuleRandom:
		{
			// A map preview can run before the fight rolls its random rule; nothing is picked yet.
			Rule selected = ((RandomRule)HNBFMAKFJAM).GetSelectedRule();
			return selected == null ? null : FindDescriptionRule(selected);
		}
		case Rule.RuleType.RuleComplex:
		{
			DescriptionRule result = null;
			List<Rule> list = ((ComplexRule)HNBFMAKFJAM).GetRules();
			{
				foreach (Rule item in list)
				{
					DescriptionRule gNBDNDOBLDO = FindDescriptionRule(item);
					if (gNBDNDOBLDO != null)
					{
						result = gNBDNDOBLDO;
					}
				}
				return result;
			}
		}
		default:
			return null;
		}
	}

	private DescriptionRule FindDescriptionRule()
	{
		DescriptionRule result = null;
		List<Rule> list = GetRules();
		foreach (Rule item in list)
		{
			if (item.IsPlayerLevelInRange())
			{
				DescriptionRule gNBDNDOBLDO = FindDescriptionRule(item);
				if (gNBDNDOBLDO != null)
				{
					result = gNBDNDOBLDO;
				}
			}
		}
		return result;
	}

	private void CollectNestedRandomRules(ComplexRule FPMPFCGEBKE)
	{
		foreach (Rule item in FPMPFCGEBKE.GetRules())
		{
			if (item.get_Type() == Rule.RuleType.RuleRandom)
			{
				_randomRules.AddIfNotExist((RandomRule)item);
			}
			if (item.get_Type() == Rule.RuleType.RuleComplex)
			{
				CollectNestedRandomRules((ComplexRule)item);
			}
		}
	}

	private void NotifyRulesChanged()
	{
		ListSF.GetInstance().OnFightSelected(this);
	}

	private RatingEvaluationRule GetRatingEvaluationRule()
	{
		List<Rule> list = GetRules();
		foreach (Rule item in list)
		{
			if (item.get_Type() == Rule.RuleType.RuleRatingEvaluation)
			{
				return (RatingEvaluationRule)item;
			}
		}
		return null;
	}

	private List<global::Pair<string, float>> GetPlayerAttributeModifiers()
	{
		return GetAttributeModifiers(RuleAppliance.AppliancePlayer);
	}

	private List<global::Pair<string, float>> GetOpponentAttributeModifiers()
	{
		return GetAttributeModifiers(RuleAppliance.ApplianceOpponent);
	}

	private List<global::Pair<string, float>> GetAttributeModifiers(RuleAppliance IGFNCCEHFEK)
	{
		List<global::Pair<string, float>> list = new List<global::Pair<string, float>>();
		List<Rule> list2 = GetRules();
		foreach (Rule item in list2)
		{
			if (item.get_Type() != Rule.RuleType.RuleAttributes)
			{
				continue;
			}
			AttributesRule bGIGBBHDIDB = (AttributesRule)item;
			Dictionary<string, float> dictionary = bGIGBBHDIDB.GetAttributeValues();
			foreach (KeyValuePair<string, float> item2 in dictionary)
			{
				if (bGIGBBHDIDB.GetAppliance() == IGFNCCEHFEK || bGIGBBHDIDB.GetAppliance() == RuleAppliance.ApplianceAll)
				{
					if (!item2.Key.Contains("Defense"))
					{
						list.Add(new global::Pair<string, float>(item2.Key, item2.Value));
					}
				}
				else if (item2.Key.Contains("Defense"))
				{
					list.Add(new global::Pair<string, float>(item2.Key, item2.Value));
				}
			}
		}
		return list;
	}

	private void ApplyRuleItems(ModelParameters IHEFAMAFBIA)
	{
		RuleAppliance iGFNCCEHFEK = (IHEFAMAFBIA.IsPlayer ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		List<ItemRule> list = GetItemRulesFor(iGFNCCEHFEK);
		foreach (RandomRule item in _randomRules)
		{
			Rule gKAJMMNJBGA = item.GetSelectedRule();
			if (gKAJMMNJBGA != null)
			{
				CollectItemRules(gKAJMMNJBGA, list);
			}
		}
		IHEFAMAFBIA.SetItemsFromRules(list, false);
	}

	private List<ItemRule> GetItemRulesFor(RuleAppliance IGFNCCEHFEK)
	{
		List<ItemRule> list = new List<ItemRule>();
		foreach (ItemRule item in _itemRules)
		{
			if (item.GetAppliance() == IGFNCCEHFEK || item.GetAppliance() == RuleAppliance.ApplianceAll)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private void CollectItemRules(Rule HNBFMAKFJAM, List<ItemRule> OEMALIFPGPO)
	{
		switch (HNBFMAKFJAM.get_Type())
		{
		case Rule.RuleType.RuleRandom:
		{
			RandomRule lEAKKGFJBLL = (RandomRule)HNBFMAKFJAM;
			CollectItemRules(lEAKKGFJBLL.GetSelectedRule(), OEMALIFPGPO);
			break;
		}
		case Rule.RuleType.RuleComplex:
		{
			ComplexRule cDFLHDCCMMN = (ComplexRule)HNBFMAKFJAM;
			{
				foreach (Rule item in cDFLHDCCMMN.GetRules())
				{
					CollectItemRules(item, OEMALIFPGPO);
				}
				break;
			}
		}
		case Rule.RuleType.RuleItem:
		case Rule.RuleType.RuleEquipItem:
		case Rule.RuleType.RuleRandomAquiredItem:
			OEMALIFPGPO.Add((ItemRule)HNBFMAKFJAM);
			break;
		}
	}
}
