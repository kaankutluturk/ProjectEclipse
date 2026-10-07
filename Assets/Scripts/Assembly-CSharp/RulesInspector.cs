using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Nekki.SF2.Core.Fights.Controller;
using UnityEngine;

public class RulesInspector : global::EventDispatcher<object>
{
	public const float SCORE_TABLE_X = 0f;

	public const float SCORE_TABLE_Y = -220f;

	private Fight _fight;

	private List<PerkInfoItem> _playerPerks = new List<PerkInfoItem>();

	private List<PerkInfoItem> _enemyPerks = new List<PerkInfoItem>();

	private List<NoPerksRule> _playerNoPerksRules = new List<NoPerksRule>();

	private List<NoPerksRule> _enemyNoPerksRules = new List<NoPerksRule>();

	private List<InFightRule> _renderRules = new List<InFightRule>();

	private List<InFightRule> _collisionRules = new List<InFightRule>();

	private List<InFightRule> _hitRules = new List<InFightRule>();

	private List<InFightRule> _strikeRules = new List<InFightRule>();

	private List<InFightRule> _animationRules = new List<InFightRule>();

	private List<InFightRule> _physicsRules = new List<InFightRule>();

	private List<InFightRule> _crazyRules = new List<InFightRule>();

	private List<InFightRule> _timeoutRules = new List<InFightRule>();

	private List<InFightRule> _damageRules = new List<InFightRule>();

	private List<InFightRule> _comboRules = new List<InFightRule>();

	private List<InFightRule> _resistanceRules = new List<InFightRule>();

	private List<InFightRule> _inFightRules = new List<InFightRule>();

    internal System.Action PrepareModelRebind(Model expected, Model replacement)
    {
        if (expected == null || replacement == null) throw new System.ArgumentNullException();
        var assignments = new List<System.Action>();
        foreach (var rule in _inFightRules)
        {
            var assignment = rule.PrepareModelRebind(expected, replacement);
            if (assignment != null) assignments.Add(assignment);
        }
        return () => { foreach (var assign in assignments) assign(); };
    }

	private List<ItemRule> _itemRules = new List<ItemRule>();

	private List<ItemRule> _playerItemRules = new List<ItemRule>();

	private List<ItemRule> _enemyItemRules = new List<ItemRule>();

	private List<NoButtonRule> _noButtonRules = new List<NoButtonRule>();

	private List<NoAnimationRule> _noAnimationRules = new List<NoAnimationRule>();

	private List<ChangeFightRule> _changeFightRules = new List<ChangeFightRule>();

	private List<RandomRule> _randomRules = new List<RandomRule>();

	private List<AvatarRule> _avatarRules = new List<AvatarRule>();

	private List<NameRule> _nameRules = new List<NameRule>();

	private List<Rule> _rules = new List<Rule>();

	public bool RulesActive;

	public int CurrentRound;

	private int _randomRuleSeed;

	private bool _hasRandomSeed;

	public RulesInspector(Fight fight, FightList KGKDKENMAOA)
	{
		_fight = null;
		CurrentRound = 1;
		RulesActive = false;
		_hasRandomSeed = false;
		_randomRuleSeed = 0;
		Init(fight, KGKDKENMAOA);
	}

	public void Init(Fight fight, FightList KGKDKENMAOA)
	{
		_fight = fight;
		RulesActive = false;
		if (KGKDKENMAOA.GetRosterFight() != null && KGKDKENMAOA.GetRosterFight().HasRandomSeeds)
		{
			SetRandomRuleSeed(KGKDKENMAOA.GetRosterFight().GetRandomRuleSeed());
			SetHasRandomSeed(true);
		}
		List<Rule> list = KGKDKENMAOA.GetRules();
		foreach (Rule item in list)
		{
			PutRule(item);
		}
	}

	public void CheckEvent(FightEvent KOJNCHKPLLN, RuleAppliance EJPOJJKKICO, object data)
	{
		if (!RulesActive)
		{
			return;
		}
		bool flag = false;
		List<InFightRule> list = new List<InFightRule>();
		switch (KOJNCHKPLLN)
		{
		case FightEvent.RenderEvent:
			list = _renderRules;
			flag = true;
			break;
		case FightEvent.CollisionEvent:
			list = _collisionRules;
			break;
		case FightEvent.HitEvent:
			list = _hitRules;
			break;
		case FightEvent.StrikeEvent:
			list = _strikeRules;
			break;
		case FightEvent.AnimationStartEvent:
			list = _animationRules;
			break;
		case FightEvent.PhysicsStartEvent:
			list = _physicsRules;
			break;
		case FightEvent.CrazyEvent:
			list = _crazyRules;
			break;
		case FightEvent.TimeoutEvent:
			list = _timeoutRules;
			break;
		case FightEvent.DamageCheckEvent:
			list = _damageRules;
			break;
		case FightEvent.ComboEvent:
			list = _comboRules;
			break;
		case FightEvent.ResistanceCheckEvent:
			list = _resistanceRules;
			break;
		default:
			GameLog.Error("Error - RulesInspector::checkEvent - unknown event %i", KOJNCHKPLLN);
			return;
		}
		foreach (InFightRule item in list)
		{
			if (item.GetActive() && (item.GetAppliance() == EJPOJJKKICO || item.GetAppliance() == RuleAppliance.ApplianceAll || EJPOJJKKICO == RuleAppliance.ApplianceAll) && item.Compare(data))
			{
				RulePassed(item);
			}
		}
		if (flag)
		{
			CheckRulesRender();
		}
	}

	public void CheckPreDraws()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool spotlightPrepared = false;
		foreach (InFightRule item in _inFightRules)
		{
			if (!item.GetActive())
			{
				continue;
			}
			switch (item.get_Type())
			{
			case Rule.RuleType.RuleRingout:
				if (!flag)
				{
					RingOutRule iKKBOBLOPDI = (RingOutRule)item;
					_fight.CreateRingout(iKKBOBLOPDI.GetMinX(), iKKBOBLOPDI.GetMaxX(), iKKBOBLOPDI.GetSequenceSpeed(), iKKBOBLOPDI.GetSequenceName());
					flag = true;
				}
				break;
			case Rule.RuleType.RuleHotGround:
				if (!flag5)
				{
					HotGroundRule gCNCEGFIOKG = (HotGroundRule)item;
					if (gCNCEGFIOKG.HasSequence())
					{
						_fight.CreateHotGround(gCNCEGFIOKG.GetSequenceName(), gCNCEGFIOKG.GetSequenceWidth());
					}
					flag5 = true;
				}
				break;
			case Rule.RuleType.RuleNoHealthBar:
				_fight.SetHealthBarVisible(item.GetAppliance(), false);
				break;
			case Rule.RuleType.RuleDarkness:
				if (!flag2)
				{
					_fight.CreateDarkness();
					flag2 = true;
				}
				break;
			case Rule.RuleType.RuleLightInTheDarkness:
				if (!spotlightPrepared)
				{
					_fight.CreateLightInTheDarkness();
					spotlightPrepared = true;
				}
				break;
			case Rule.RuleType.RulePoints:
				if (!flag3)
				{
					PointsTableType nOPJGLHKJPG = ((PointsRule)item).GetTableType();
					int lOMKKEAMMIG = ((PointsRule)item).GetMaxPoints();
					_fight.CreatePointsTable(0f, -220f, nOPJGLHKJPG, lOMKKEAMMIG);
					flag3 = true;
				}
				break;
			case Rule.RuleType.RuleRandomArea:
				if (!flag4)
				{
					_fight.CreatePerkActivationArea(((RandomAreaRule)item).GetWidth(), ((RandomAreaRule)item).GetImagePath(), ((RandomAreaRule)item).GetIconPath());
				}
				break;
			}
		}
	}

	public void InitRules(object data)
	{
		foreach (InFightRule item in _inFightRules)
		{
			if (item.GetActive())
			{
				item.InitRule(data);
				switch (item.get_Type())
				{
				case Rule.RuleType.RuleRechargeMagicEachRound:
					_fight.RechargeMagic(item.GetAppliance());
					break;
				case Rule.RuleType.RuleTactic:
					_fight.SetBotTactic(((TacticRule)item).GetTacticName());
					break;
				case Rule.RuleType.RuleInvertJoystick:
					_fight.SetControlsInverted(true);
					break;
				}
			}
		}
		CheckDamageRules(RuleAppliance.ApplianceAll);
		CheckResistanceRules();
	}

	public void ClearRules()
	{
		_playerPerks.Clear();
		_enemyPerks.Clear();
		foreach (InFightRule item in _inFightRules)
		{
			item.Clear();
		}
	}

	public void StopRules()
	{
		foreach (InFightRule item in _inFightRules)
		{
			if (item.GetActive())
			{
				item.Stop();
				Rule.RuleType bCBLLMPAMLP = item.get_Type();
				if (bCBLLMPAMLP == Rule.RuleType.RuleDarkness)
				{
					_fight.SetDarknessAlpha(0f);
				}
				else if (bCBLLMPAMLP == Rule.RuleType.RuleLightInTheDarkness)
				{
					_fight.RemoveLightInTheDarkness();
				}
			}
		}
	}

	public void SetRulesActivity()
	{
		bool flag = false;
		foreach (InFightRule item in _inFightRules)
		{
			flag = item.AppliesToRound(CurrentRound) && item.IsPlayerLevelInRange();
			item.SetActive(flag);
		}
		foreach (NoButtonRule item2 in _noButtonRules)
		{
			flag = item2.AppliesToRound(CurrentRound) && item2.IsPlayerLevelInRange();
			item2.SetActive(flag);
		}
		foreach (NoAnimationRule item3 in _noAnimationRules)
		{
			flag = item3.AppliesToRound(CurrentRound) && item3.IsPlayerLevelInRange();
			item3.SetActive(flag);
		}
		foreach (ItemRule item4 in _itemRules)
		{
			flag = item4.AppliesToRound(CurrentRound) && item4.IsPlayerLevelInRange();
			item4.SetActive(flag);
		}
		foreach (RandomRule item5 in _randomRules)
		{
			flag = item5.AppliesToRound(CurrentRound) && item5.IsPlayerLevelInRange();
			item5.SetActive(flag);
		}
	}

	public void ApplyNoPerksRules(ModelParameters IHEFAMAFBIA, List<NoPerksRule> GOMIMEDNKHH)
	{
		if (IHEFAMAFBIA == null)
		{
			GameLog.Error("RulesInspector::applyNoPerksRules ERROR - modelParameters is NULL");
		}
		IHEFAMAFBIA.ExcludedPerkNames.Clear();
		foreach (NoPerksRule item2 in GOMIMEDNKHH)
		{
			string item = item2.GetPerkName();
			IHEFAMAFBIA.ExcludedPerkNames.Add(item);
		}
		IHEFAMAFBIA.RemovePerksByNames(IHEFAMAFBIA.Perks, IHEFAMAFBIA.ExcludedPerkNames);
	}

	public void ApplyNoAnimationRules(ModelParameters IHEFAMAFBIA)
	{
		if (IHEFAMAFBIA == null)
		{
			GameLog.Error("RulesInspector::applyNoAnimationRules ERROR - modelParameters is NULL");
		}
		IHEFAMAFBIA.ExcludedMoveNames.Clear();
		foreach (NoAnimationRule item in _noAnimationRules)
		{
			if (item.GetActive())
			{
				IHEFAMAFBIA.ExcludedMoveNames.Add(item.GetAnimationName());
			}
		}
	}

	public void ApplyAvatarAndNameRules(ModelParameters IHEFAMAFBIA)
	{
		if (IHEFAMAFBIA == null)
		{
			GameLog.Error("RulesInspector::ApplyAvatarAndNameRules ERROR - modelParameters is NULL");
		}
		foreach (AvatarRule item in _avatarRules)
		{
			IHEFAMAFBIA.Avatar = item.get_Name();
		}
		foreach (NameRule item2 in _nameRules)
		{
			IHEFAMAFBIA.FirstName = item2.get_Name();
		}
	}

	public void CheckButtonRules(GameController LPGANKOAPJL)
	{
		if (LPGANKOAPJL == null)
		{
			GameLog.Error("RulesInspector::checkButtonRules ERROR - gameController is NULL");
			return;
		}
		foreach (NoButtonRule item in _noButtonRules)
		{
			if (item.GetActive())
			{
				ApplyButtonRule(item, LPGANKOAPJL);
			}
		}
	}

	public void ApplyButtonRule(NoButtonRule HNBFMAKFJAM, GameController LPGANKOAPJL)
	{
		switch (HNBFMAKFJAM.GetButtonType())
		{
		case NoButtonRule.NoButtonType.ButtonTypePunch:
			LPGANKOAPJL.SetButtonRuleEnabled(FightCID.Punch, false);
			break;
		case NoButtonRule.NoButtonType.ButtonTypeKick:
			LPGANKOAPJL.SetButtonRuleEnabled(FightCID.Kick, false);
			break;
        case NoButtonRule.NoButtonType.ButtonTypeRanged:
            LPGANKOAPJL.SetButtonRuleEnabled(FightCID.MissileButton, false);
            break;
        case NoButtonRule.NoButtonType.ButtonTypeMagic:
            LPGANKOAPJL.SetButtonRuleEnabled(FightCID.MagicButton, false);
            break;
        case NoButtonRule.NoButtonType.ButtonTypeRaidCharge:
            LPGANKOAPJL.SetButtonRuleEnabled(FightCID.RaidChargeButton, false);
			break;
		}
	}

	public void CheckChangeFightRules(FightList KGKDKENMAOA)
	{
		if (_changeFightRules.Count <= 0)
		{
			return;
		}
		ChangeFightRule iJCOGNNJLFA = null;
		foreach (ChangeFightRule item in _changeFightRules)
		{
			if (item.GetActive())
			{
				iJCOGNNJLFA = item;
			}
		}
		if (iJCOGNNJLFA != null)
		{
			ApplyChangeFightRule(_changeFightRules[_changeFightRules.Count - 1], KGKDKENMAOA);
		}
	}

	public void ApplyChangeFightRule(ChangeFightRule HNBFMAKFJAM, FightList KGKDKENMAOA)
	{
		int num = HNBFMAKFJAM.GetRounds();
		if (num > 0)
		{
			KGKDKENMAOA.RoundsToWin = num;
		}
		int num2 = HNBFMAKFJAM.GetRoundTime();
		if (num2 > 0)
		{
			KGKDKENMAOA.RoundTime = (ObscuredInt)(num2);
		}
	}

	public void ResetRandomRules()
	{
		ClearRandomRules();
		if (CurrentRound == 1)
		{
			if (_hasRandomSeed)
			{
				NekkiMath.SetSeed(_randomRuleSeed);
			}
			else
			{
				Eclipse.Multiplayer.VersusDeterminism.ReseedRules(CurrentRound);
			}
			ResetRandomRules(RandomRule.RefreshMode.REFRESH_EACH_FIGHT);
		}
		Eclipse.Multiplayer.VersusDeterminism.ReseedRules(CurrentRound);
		ResetRandomRules(RandomRule.RefreshMode.REFRESH_EACH_ROUND);
		PutRandomRules();
	}

	public void ResetRules(int round)
	{
		ClearRules();
		CurrentRound = round;
		SetRulesActivity();
		ResetRandomRules();
		RefillPerksFromRules();
	}

	public void PrepareItemRules(List<ItemRule> JIILGONALOA)
	{
		foreach (ItemRule item in JIILGONALOA)
		{
			if (item != null && item.get_Type() == Rule.RuleType.RuleRandomAquiredItem)
			{
				RandomAquiredItemRule kJNNJGGKBCO = (RandomAquiredItemRule)item;
				kJNNJGGKBCO.RefreshItems();
			}
		}
	}

	public List<ItemRule> GetItemRules()
	{
		return _itemRules;
	}

	public List<ItemRule> GetPlayerItemRules()
	{
		return _playerItemRules;
	}

	public List<ItemRule> GetEnemyItemRules()
	{
		return _enemyItemRules;
	}

	public List<PerkInfoItem> GetPlayerPerks()
	{
		return _playerPerks;
	}

	public List<PerkInfoItem> GetEnemyPerks()
	{
		return _enemyPerks;
	}

	public List<NoPerksRule> GetPlayerNoPerks()
	{
		return _playerNoPerksRules;
	}

	public List<NoPerksRule> GetEnemyNoPerks()
	{
		return _enemyNoPerksRules;
	}

	protected void RulePassed(InFightRule HNBFMAKFJAM)
	{
		bool flag = false;
		switch (HNBFMAKFJAM.get_Type())
		{
		case Rule.RuleType.RuleRegeneration:
		{
			float num2 = ((RegenerationRule)HNBFMAKFJAM).GetRate();
			num2 /= (float)GameUtils.GetSlowMode();
			if (_fight.UpdateLife(HNBFMAKFJAM.GetAppliance(), num2))
			{
				_fight.SetEndFightRule(HNBFMAKFJAM);
			}
			break;
		}
		case Rule.RuleType.RuleLifeSteal:
		{
			float num = ((LifeStealRule)HNBFMAKFJAM).GetLastLifeStolen();
			num /= (float)GameUtils.GetSlowMode();
			if (_fight.UpdateLife(HNBFMAKFJAM.GetAppliance(), num))
			{
				_fight.SetEndFightRule(HNBFMAKFJAM);
			}
			break;
		}
		case Rule.RuleType.RuleRingout:
		case Rule.RuleType.RuleHotGround:
			if (HNBFMAKFJAM.IsDeathRule())
			{
				_fight.SetLifeToZero(HNBFMAKFJAM.GetAppliance());
			}
			_fight.SetEndFightRule(HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleLoseFall:
		case Rule.RuleType.RuleTimeoutWin:
		case Rule.RuleType.RuleWinStyle:
		case Rule.RuleType.RuleWinCombo:
		case Rule.RuleType.RuleWinShock:
			_fight.SetEndFightRule(HNBFMAKFJAM);
			break;
		case Rule.RuleType.RulePoints:
			_fight.UpdatePointsTable(((PointsRule)HNBFMAKFJAM).GetPlayerPoints(), ((PointsRule)HNBFMAKFJAM).GetOpponentPoints());
			if (((PointsRule)HNBFMAKFJAM).GetIsFinished())
			{
				_fight.SetEndFightRule(HNBFMAKFJAM);
			}
			break;
		case Rule.RuleType.RuleCrazy:
		case Rule.RuleType.RuleCombo:
			flag = true;
			break;
		}
		if (flag)
		{
			CheckDamageRules(HNBFMAKFJAM.GetAppliance());
		}
	}

	protected void CheckRulesRender()
	{
		foreach (InFightRule item in _renderRules)
		{
			if (!item.GetActive())
			{
				continue;
			}
			switch (item.get_Type())
			{
			case Rule.RuleType.RuleHotGround:
				if (((HotGroundRule)item).timerChanged)
				{
					if (_fight.preFight != null)
					{
						_fight.preFight.ViewerUpdateHotGroundTimer(((HotGroundRule)item).GetRemainingSeconds(), item.GetAppliance());
					}
					((HotGroundRule)item).timerChanged = false;
				}
				break;
			case Rule.RuleType.RuleDarkness:
				_fight.SetDarknessAlpha(((DarknessRule)item).GetAlpha());
				break;
			case Rule.RuleType.RuleLightInTheDarkness:
				var spotlight = (Eclipse.Combat.LightInTheDarknessRule)item;
				_fight.UpdateLightInTheDarkness(spotlight.GetAppliance(), spotlight.LightRadius, spotlight.LightShape);
				break;
			case Rule.RuleType.RuleRandomArea:
			{
				RandomAreaRule dFAONBFDMKA = (RandomAreaRule)item;
				_fight.UpdatePerkActivationArea(dFAONBFDMKA.GetPositionX(), dFAONBFDMKA.GetAlpha(), dFAONBFDMKA.IsAreaVisible());
				break;
			}
			}
		}
	}

	protected void CheckDamageRules(RuleAppliance EJPOJJKKICO)
	{
		if (EJPOJJKKICO == RuleAppliance.ApplianceAll)
		{
			CheckDamageRules(RuleAppliance.AppliancePlayer);
			CheckDamageRules(RuleAppliance.ApplianceOpponent);
			return;
		}
		bool flag = true;
		foreach (InFightRule item in _damageRules)
		{
			if (item.GetActive() && item.GetAppliance() == EJPOJJKKICO)
			{
				flag = flag && !((DamageRule)item).IsNoDamage();
			}
		}
		RuleAppliance eJPOJJKKICO = ((EJPOJJKKICO != RuleAppliance.AppliancePlayer) ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		_fight.GetModelByAppliance(eJPOJJKKICO).SetDamageImmune(!flag);
	}

	protected void CheckResistanceRules()
	{
		Model fGCODGKLHED = _fight.GetModelByAppliance(RuleAppliance.AppliancePlayer);
		Model fGCODGKLHED2 = _fight.GetModelByAppliance(RuleAppliance.ApplianceOpponent);
		float num = 1f;
		float num2 = 1f;
		foreach (InFightRule item in _resistanceRules)
		{
			ResistanceRule hCOHJNFLKIF = item as ResistanceRule;
			if (hCOHJNFLKIF != null)
			{
				string gOHIIMFFFJI = hCOHJNFLKIF.GetResistanceName();
				int num3 = hCOHJNFLKIF.GetResistanceValue();
				int num4 = ListSF.GetRoster().GetResistanceCount(gOHIIMFFFJI);
				if (num4 < num3)
				{
					float num5 = Mathf.Pow(2f, (float)(num3 - num4) / GameUtils.GetResistanceDoublingRange());
					float num6 = Mathf.Pow(2f, (float)(num4 - num3) / GameUtils.GetResistanceDoublingRange());
					num *= num6;
					num2 *= num5;
				}
			}
		}
		fGCODGKLHED.SetDamageMultiplier(num);
		fGCODGKLHED2.SetDamageMultiplier(num2);
	}

	protected void SetItemRules(List<ItemRule> GEEJLFGCKNJ)
	{
		_itemRules.AddRange(GEEJLFGCKNJ);
		foreach (ItemRule item in _itemRules)
		{
			_rules.Add(item);
		}
	}

	protected void SetNoButtonRules(List<NoButtonRule> PINLMLCCFPH)
	{
		_noButtonRules.AddRange(PINLMLCCFPH);
		foreach (NoButtonRule item in _noButtonRules)
		{
			_rules.Add(item);
		}
	}

	protected void SetNoAnimationRules(List<NoAnimationRule> IMAJKIFPLNM)
	{
		_noAnimationRules.AddRange(IMAJKIFPLNM);
		foreach (NoAnimationRule item in _noAnimationRules)
		{
			_rules.Add(item);
		}
	}

	protected void SetInFightRules(List<InFightRule> JIILGONALOA)
	{
		foreach (InFightRule item in JIILGONALOA)
		{
			SetInFightRule(item);
		}
	}

	protected void SetInFightRule(InFightRule HNBFMAKFJAM)
	{
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.RenderEvent))
		{
			_renderRules.Add(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.CollisionEvent))
		{
			_collisionRules.Add(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.HitEvent))
		{
			_hitRules.Add(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.AnimationStartEvent))
		{
			_animationRules.Add(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.PhysicsStartEvent))
		{
			_physicsRules.Add(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.CrazyEvent))
		{
			_crazyRules.Add(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.StrikeEvent))
		{
			_strikeRules.Add(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.TimeoutEvent))
		{
			_timeoutRules.Add(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.DamageCheckEvent))
		{
			_damageRules.Add(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.ComboEvent))
		{
			_comboRules.Add(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.ResistanceCheckEvent))
		{
			_resistanceRules.Add(HNBFMAKFJAM);
		}
		_inFightRules.AddIfNotExist(HNBFMAKFJAM);
		_rules.AddIfNotExist(HNBFMAKFJAM);
	}

	protected void DeactivateInFightRule(InFightRule HNBFMAKFJAM)
	{
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.RenderEvent))
		{
			_renderRules.Remove(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.CollisionEvent))
		{
			_collisionRules.Remove(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.HitEvent))
		{
			_hitRules.Remove(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.AnimationStartEvent))
		{
			_animationRules.Remove(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.PhysicsStartEvent))
		{
			_physicsRules.Remove(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.CrazyEvent))
		{
			_crazyRules.Remove(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.StrikeEvent))
		{
			_strikeRules.Remove(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.TimeoutEvent))
		{
			_timeoutRules.Remove(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.DamageCheckEvent))
		{
			_damageRules.Remove(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.ComboEvent))
		{
			_comboRules.Remove(HNBFMAKFJAM);
		}
		if (HNBFMAKFJAM.IsSubscribedTo(FightEvent.ResistanceCheckEvent))
		{
			_resistanceRules.Remove(HNBFMAKFJAM);
		}
	}

	protected void SetRandomRules(List<RandomRule> GOAJNDLFBDN)
	{
		_randomRules.AddRange(GOAJNDLFBDN);
	}

	protected void RemoveRule(Rule HNBFMAKFJAM)
	{
		switch (HNBFMAKFJAM.get_Type())
		{
		case Rule.RuleType.RuleComplex:
			RemoveComplexRule((ComplexRule)HNBFMAKFJAM);
			return;
		case Rule.RuleType.RuleItem:
		case Rule.RuleType.RuleEquipItem:
		case Rule.RuleType.RuleRandomAquiredItem:
			_itemRules.Remove((ItemRule)HNBFMAKFJAM);
			_playerItemRules.Remove((ItemRule)HNBFMAKFJAM);
			_enemyItemRules.Remove((ItemRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleNoButton:
			_noButtonRules.Remove((NoButtonRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleNoAnimation:
			_noAnimationRules.Remove((NoAnimationRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleChangeFight:
			_changeFightRules.Remove((ChangeFightRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleRingout:
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
		case Rule.RuleType.RuleRechargeMagicEachRound:
		case Rule.RuleType.RuleNoBulletsReplenishment:
		case Rule.RuleType.RuleInvulnerability:
		case Rule.RuleType.RuleResistance:
			RemoveInFightRule((InFightRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleDarkness:
		case Rule.RuleType.RuleLightInTheDarkness:
		case Rule.RuleType.RulePoints:
		case Rule.RuleType.RuleInvertJoystick:
		case Rule.RuleType.RuleRandomArea:
			RemoveInFightRule((InFightRule)HNBFMAKFJAM, true);
			break;
		case Rule.RuleType.RuleAvatar:
			_avatarRules.Remove((AvatarRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleName:
			_nameRules.Remove((NameRule)HNBFMAKFJAM);
			break;
		}
		_rules.Remove(HNBFMAKFJAM);
	}

	protected void RemoveComplexRule(ComplexRule FPMPFCGEBKE)
	{
		List<Rule> list = FPMPFCGEBKE.GetRules();
		foreach (Rule item in list)
		{
			RemoveRule(item);
		}
		_rules.Remove(FPMPFCGEBKE);
	}

	protected void PutPerkFromRule(PerkRule HNBFMAKFJAM)
	{
		switch (HNBFMAKFJAM.GetAppliance())
		{
		case RuleAppliance.AppliancePlayer:
			_playerPerks.Add(HNBFMAKFJAM.GetPerk());
			break;
		case RuleAppliance.ApplianceOpponent:
			_enemyPerks.Add(HNBFMAKFJAM.GetPerk());
			break;
		default:
			GameLog.Error("RulesInspector::putPerkFromRule ERROR - wrong rule appliance %i", HNBFMAKFJAM.GetAppliance());
			break;
		}
	}

	protected void PutNoPerkFromRule(NoPerksRule HNBFMAKFJAM)
	{
		switch (HNBFMAKFJAM.GetAppliance())
		{
		case RuleAppliance.AppliancePlayer:
			_playerNoPerksRules.Add(HNBFMAKFJAM);
			break;
		case RuleAppliance.ApplianceOpponent:
			_enemyNoPerksRules.Add(HNBFMAKFJAM);
			break;
		case RuleAppliance.ApplianceAll:
			_playerNoPerksRules.Add(HNBFMAKFJAM);
			_enemyNoPerksRules.Add(HNBFMAKFJAM);
			break;
		default:
			GameLog.Error("RulesInspector::putPerkFromRule ERROR - wrong rule appliance %i", HNBFMAKFJAM.GetAppliance());
			break;
		}
	}

	protected void RefillPerksFromRules()
	{
		_playerPerks.Clear();
		_enemyPerks.Clear();
		foreach (Rule item in _rules)
		{
			if (item.GetActive())
			{
				if (item.get_Type() == Rule.RuleType.RulePerk)
				{
					PutPerkFromRule((PerkRule)item);
				}
				else if (item.get_Type() == Rule.RuleType.RuleNoPerks)
				{
					PutNoPerkFromRule((NoPerksRule)item);
				}
			}
		}
	}

	protected void ClearRandomRules()
	{
		_rules.FindAll((Rule DHDMNHCIPEH) => DHDMNHCIPEH.IsRandom).ForEach((Rule DHDMNHCIPEH) =>
		{
			RemoveRule(DHDMNHCIPEH);
		});
	}

	protected void ResetRandomRules(RandomRule.RefreshMode LFLGCDNKNJI)
	{
		foreach (RandomRule item in _randomRules)
		{
			if (item.GetActive() && item.GetRefreshMode() == LFLGCDNKNJI)
			{
				item.SelectRandomRule();
			}
		}
	}

	protected void PutRandomRules()
	{
		foreach (RandomRule item in _randomRules)
		{
			if (item.GetActive())
			{
				PutRule(item.GetSelectedRule());
			}
		}
	}

	protected void PutRule(Rule HNBFMAKFJAM, bool HLEIILHFBKP = false)
	{
		switch (HNBFMAKFJAM.get_Type())
		{
		case Rule.RuleType.RuleRandom:
			_randomRules.Add((RandomRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleComplex:
			PutComplexRule((ComplexRule)HNBFMAKFJAM);
			return;
		case Rule.RuleType.RuleItem:
		case Rule.RuleType.RuleEquipItem:
		case Rule.RuleType.RuleRandomAquiredItem:
			PutItemRule((ItemRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleNoButton:
			_noButtonRules.Add((NoButtonRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleNoAnimation:
			_noAnimationRules.Add((NoAnimationRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleChangeFight:
			_changeFightRules.Add((ChangeFightRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleRingout:
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
		case Rule.RuleType.RuleRechargeMagicEachRound:
		case Rule.RuleType.RuleNoBulletsReplenishment:
		case Rule.RuleType.RulePerk:
		case Rule.RuleType.RuleNoPerks:
		case Rule.RuleType.RuleWinStyle:
		case Rule.RuleType.RuleWinCombo:
		case Rule.RuleType.RuleWinShock:
		case Rule.RuleType.RuleTactic:
		case Rule.RuleType.RuleInvulnerability:
		case Rule.RuleType.RuleResistance:
			PutInFightRule((InFightRule)HNBFMAKFJAM);
			return;
		case Rule.RuleType.RuleDarkness:
		case Rule.RuleType.RuleLightInTheDarkness:
		case Rule.RuleType.RulePoints:
			PutInFightRule((InFightRule)HNBFMAKFJAM, true);
			return;
		case Rule.RuleType.RuleDescription:
		case Rule.RuleType.RuleRatingEvaluation:
		case Rule.RuleType.RuleCurrencyCost:
		case Rule.RuleType.RuleRaidCurrencyCost:
			return;
		case Rule.RuleType.RuleInvertJoystick:
			PutInFightRule((InFightRule)HNBFMAKFJAM, true);
			break;
		case Rule.RuleType.RuleRandomArea:
			PutInFightRule((InFightRule)HNBFMAKFJAM, true);
			break;
		case Rule.RuleType.RuleAvatar:
			_avatarRules.Add((AvatarRule)HNBFMAKFJAM);
			break;
		case Rule.RuleType.RuleName:
			_nameRules.Add((NameRule)HNBFMAKFJAM);
			break;
		default:
			GameLog.Error("RulesInspector::putRule ERROR - wrong rule type %i", HNBFMAKFJAM.get_Type());
			break;
		}
		if (HNBFMAKFJAM.get_Type() != Rule.RuleType.RuleRandom)
		{
			_rules.AddIfNotExist(HNBFMAKFJAM);
		}
	}

	protected void PutComplexRule(ComplexRule HNBFMAKFJAM)
	{
		List<Rule> list = HNBFMAKFJAM.GetRules();
		foreach (Rule item in list)
		{
			PutRule(item);
		}
		_rules.AddIfNotExist(HNBFMAKFJAM);
	}

	protected void PutItemRule(ItemRule BICJICMJNMC)
	{
		switch (BICJICMJNMC.GetAppliance())
		{
		case RuleAppliance.AppliancePlayer:
			_playerItemRules.Add(BICJICMJNMC);
			break;
		case RuleAppliance.ApplianceOpponent:
			_enemyItemRules.Add(BICJICMJNMC);
			break;
		case RuleAppliance.ApplianceAll:
			_playerItemRules.Add(BICJICMJNMC);
			_enemyItemRules.Add(BICJICMJNMC);
			break;
		}
		_itemRules.Add(BICJICMJNMC);
	}

	protected void PutInFightRule(InFightRule MGEAFPEKMMC, bool CNKAMHAILKG = false)
	{
		if (!CNKAMHAILKG && MGEAFPEKMMC.GetAppliance() == RuleAppliance.ApplianceAll)
		{
			InFightRule aAJIFBJLJOA = MGEAFPEKMMC.Copy();
			InFightRule aAJIFBJLJOA2 = MGEAFPEKMMC.Copy();
			aAJIFBJLJOA.SetAppliance(RuleAppliance.AppliancePlayer);
			aAJIFBJLJOA2.SetAppliance(RuleAppliance.ApplianceOpponent);
			aAJIFBJLJOA.ParentRule = MGEAFPEKMMC;
			aAJIFBJLJOA2.ParentRule = MGEAFPEKMMC;
			SetInFightRule(aAJIFBJLJOA);
			SetInFightRule(aAJIFBJLJOA2);
		}
		else
		{
			SetInFightRule(MGEAFPEKMMC);
		}
	}

	protected void RemoveInFightRule(InFightRule HNBFMAKFJAM, bool CNKAMHAILKG = false)
	{
		if (!CNKAMHAILKG && HNBFMAKFJAM.GetAppliance() == RuleAppliance.ApplianceAll)
		{
			int num = 0;
			while (num < _inFightRules.Count)
			{
				InFightRule aAJIFBJLJOA = _inFightRules[num];
				if (aAJIFBJLJOA.ParentRule == HNBFMAKFJAM)
				{
					_inFightRules.Remove(aAJIFBJLJOA);
					DeactivateInFightRule(aAJIFBJLJOA);
				}
				else
				{
					num++;
				}
			}
			return;
		}
		for (int i = 0; i < _inFightRules.Count; i++)
		{
			InFightRule aAJIFBJLJOA2 = _inFightRules[i];
			if (HNBFMAKFJAM == aAJIFBJLJOA2)
			{
				_inFightRules.Remove(aAJIFBJLJOA2);
				DeactivateInFightRule(aAJIFBJLJOA2);
				break;
			}
		}
	}

	protected void SetRandomRuleSeed(int OKGKLCLEDFN)
	{
		_randomRuleSeed = OKGKLCLEDFN;
	}

	protected void SetHasRandomSeed(bool value)
	{
		_hasRandomSeed = value;
	}
}
