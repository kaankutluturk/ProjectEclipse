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

	public RulesInspector(Fight fight, FightList fightList)
	{
		_fight = null;
		CurrentRound = 1;
		RulesActive = false;
		_hasRandomSeed = false;
		_randomRuleSeed = 0;
		Init(fight, fightList);
	}

	public void Init(Fight fight, FightList fightList)
	{
		_fight = fight;
		RulesActive = false;
		if (fightList.GetRosterFight() != null && fightList.GetRosterFight().HasRandomSeeds)
		{
			SetRandomRuleSeed(fightList.GetRosterFight().GetRandomRuleSeed());
			SetHasRandomSeed(true);
		}
		List<Rule> list = fightList.GetRules();
		foreach (Rule item in list)
		{
			PutRule(item);
		}
	}

	public void CheckEvent(FightEvent fightEvent, RuleAppliance appliance, object data)
	{
		if (!RulesActive)
		{
			return;
		}
		bool flag = false;
		List<InFightRule> list = new List<InFightRule>();
		switch (fightEvent)
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
			GameLog.Error("Error - RulesInspector::checkEvent - unknown event %i", fightEvent);
			return;
		}
		foreach (InFightRule item in list)
		{
			if (item.GetActive() && (item.GetAppliance() == appliance || item.GetAppliance() == RuleAppliance.ApplianceAll || appliance == RuleAppliance.ApplianceAll) && item.Compare(data))
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
					RingOutRule ringOutRule = (RingOutRule)item;
					_fight.CreateRingout(ringOutRule.GetMinX(), ringOutRule.GetMaxX(), ringOutRule.GetSequenceSpeed(), ringOutRule.GetSequenceName());
					flag = true;
				}
				break;
			case Rule.RuleType.RuleHotGround:
				if (!flag5)
				{
					HotGroundRule hotGroundRule = (HotGroundRule)item;
					if (hotGroundRule.HasSequence())
					{
						_fight.CreateHotGround(hotGroundRule.GetSequenceName(), hotGroundRule.GetSequenceWidth());
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
					PointsTableType tableType = ((PointsRule)item).GetTableType();
					int maxPoints = ((PointsRule)item).GetMaxPoints();
					_fight.CreatePointsTable(0f, -220f, tableType, maxPoints);
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
				Rule.RuleType ruleType = item.get_Type();
				if (ruleType == Rule.RuleType.RuleDarkness)
				{
					_fight.SetDarknessAlpha(0f);
				}
				else if (ruleType == Rule.RuleType.RuleLightInTheDarkness)
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

	public void ApplyNoPerksRules(ModelParameters modelParameters, List<NoPerksRule> noPerksRules)
	{
		if (modelParameters == null)
		{
			GameLog.Error("RulesInspector::applyNoPerksRules ERROR - modelParameters is NULL");
		}
		modelParameters.ExcludedPerkNames.Clear();
		foreach (NoPerksRule item2 in noPerksRules)
		{
			string item = item2.GetPerkName();
			modelParameters.ExcludedPerkNames.Add(item);
		}
		modelParameters.RemovePerksByNames(modelParameters.Perks, modelParameters.ExcludedPerkNames);
	}

	public void ApplyNoAnimationRules(ModelParameters modelParameters)
	{
		if (modelParameters == null)
		{
			GameLog.Error("RulesInspector::applyNoAnimationRules ERROR - modelParameters is NULL");
		}
		modelParameters.ExcludedMoveNames.Clear();
		foreach (NoAnimationRule item in _noAnimationRules)
		{
			if (item.GetActive())
			{
				modelParameters.ExcludedMoveNames.Add(item.GetAnimationName());
			}
		}
	}

	public void ApplyAvatarAndNameRules(ModelParameters modelParameters)
	{
		if (modelParameters == null)
		{
			GameLog.Error("RulesInspector::ApplyAvatarAndNameRules ERROR - modelParameters is NULL");
		}
		foreach (AvatarRule item in _avatarRules)
		{
			modelParameters.Avatar = item.get_Name();
		}
		foreach (NameRule item2 in _nameRules)
		{
			modelParameters.FirstName = item2.get_Name();
		}
	}

	public void CheckButtonRules(GameController gameController)
	{
		if (gameController == null)
		{
			GameLog.Error("RulesInspector::checkButtonRules ERROR - gameController is NULL");
			return;
		}
		foreach (NoButtonRule item in _noButtonRules)
		{
			if (item.GetActive())
			{
				ApplyButtonRule(item, gameController);
			}
		}
	}

	public void ApplyButtonRule(NoButtonRule noButtonRule, GameController gameController)
	{
		switch (noButtonRule.GetButtonType())
		{
		case NoButtonRule.NoButtonType.ButtonTypePunch:
			gameController.SetButtonRuleEnabled(FightCID.Punch, false);
			break;
		case NoButtonRule.NoButtonType.ButtonTypeKick:
			gameController.SetButtonRuleEnabled(FightCID.Kick, false);
			break;
        case NoButtonRule.NoButtonType.ButtonTypeRanged:
            gameController.SetButtonRuleEnabled(FightCID.MissileButton, false);
            break;
        case NoButtonRule.NoButtonType.ButtonTypeMagic:
            gameController.SetButtonRuleEnabled(FightCID.MagicButton, false);
            break;
        case NoButtonRule.NoButtonType.ButtonTypeRaidCharge:
            gameController.SetButtonRuleEnabled(FightCID.RaidChargeButton, false);
			break;
		}
	}

	public void CheckChangeFightRules(FightList fightList)
	{
		if (_changeFightRules.Count <= 0)
		{
			return;
		}
		ChangeFightRule changeFightRule = null;
		foreach (ChangeFightRule item in _changeFightRules)
		{
			if (item.GetActive())
			{
				changeFightRule = item;
			}
		}
		if (changeFightRule != null)
		{
			ApplyChangeFightRule(_changeFightRules[_changeFightRules.Count - 1], fightList);
		}
	}

	public void ApplyChangeFightRule(ChangeFightRule changeFightRule, FightList fightList)
	{
		int num = changeFightRule.GetRounds();
		if (num > 0)
		{
			fightList.RoundsToWin = num;
		}
		int num2 = changeFightRule.GetRoundTime();
		if (num2 > 0)
		{
			fightList.RoundTime = (ObscuredInt)(num2);
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

	public void PrepareItemRules(List<ItemRule> itemRules)
	{
		foreach (ItemRule item in itemRules)
		{
			if (item != null && item.get_Type() == Rule.RuleType.RuleRandomAquiredItem)
			{
				RandomAquiredItemRule randomItemRule = (RandomAquiredItemRule)item;
				randomItemRule.RefreshItems();
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

	protected void RulePassed(InFightRule rule)
	{
		bool flag = false;
		switch (rule.get_Type())
		{
		case Rule.RuleType.RuleRegeneration:
		{
			float num2 = ((RegenerationRule)rule).GetRate();
			num2 /= (float)GameUtils.GetSlowMode();
			if (_fight.UpdateLife(rule.GetAppliance(), num2))
			{
				_fight.SetEndFightRule(rule);
			}
			break;
		}
		case Rule.RuleType.RuleLifeSteal:
		{
			float num = ((LifeStealRule)rule).GetLastLifeStolen();
			num /= (float)GameUtils.GetSlowMode();
			if (_fight.UpdateLife(rule.GetAppliance(), num))
			{
				_fight.SetEndFightRule(rule);
			}
			break;
		}
		case Rule.RuleType.RuleRingout:
		case Rule.RuleType.RuleHotGround:
			if (rule.IsDeathRule())
			{
				_fight.SetLifeToZero(rule.GetAppliance());
			}
			_fight.SetEndFightRule(rule);
			break;
		case Rule.RuleType.RuleLoseFall:
		case Rule.RuleType.RuleTimeoutWin:
		case Rule.RuleType.RuleWinStyle:
		case Rule.RuleType.RuleWinCombo:
		case Rule.RuleType.RuleWinShock:
			_fight.SetEndFightRule(rule);
			break;
		case Rule.RuleType.RulePoints:
			_fight.UpdatePointsTable(((PointsRule)rule).GetPlayerPoints(), ((PointsRule)rule).GetOpponentPoints());
			if (((PointsRule)rule).GetIsFinished())
			{
				_fight.SetEndFightRule(rule);
			}
			break;
		case Rule.RuleType.RuleCrazy:
		case Rule.RuleType.RuleCombo:
			flag = true;
			break;
		}
		if (flag)
		{
			CheckDamageRules(rule.GetAppliance());
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
				RandomAreaRule randomAreaRule = (RandomAreaRule)item;
				_fight.UpdatePerkActivationArea(randomAreaRule.GetPositionX(), randomAreaRule.GetAlpha(), randomAreaRule.IsAreaVisible());
				break;
			}
			}
		}
	}

	protected void CheckDamageRules(RuleAppliance appliance)
	{
		if (appliance == RuleAppliance.ApplianceAll)
		{
			CheckDamageRules(RuleAppliance.AppliancePlayer);
			CheckDamageRules(RuleAppliance.ApplianceOpponent);
			return;
		}
		bool flag = true;
		foreach (InFightRule item in _damageRules)
		{
			if (item.GetActive() && item.GetAppliance() == appliance)
			{
				flag = flag && !((DamageRule)item).IsNoDamage();
			}
		}
		RuleAppliance oppositeAppliance = ((appliance != RuleAppliance.AppliancePlayer) ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		_fight.GetModelByAppliance(oppositeAppliance).SetDamageImmune(!flag);
	}

	protected void CheckResistanceRules()
	{
		Model playerModel = _fight.GetModelByAppliance(RuleAppliance.AppliancePlayer);
		Model fGCODGKLHED2 = _fight.GetModelByAppliance(RuleAppliance.ApplianceOpponent);
		float num = 1f;
		float num2 = 1f;
		foreach (InFightRule item in _resistanceRules)
		{
			ResistanceRule resistanceRule = item as ResistanceRule;
			if (resistanceRule != null)
			{
				string resistanceName = resistanceRule.GetResistanceName();
				int num3 = resistanceRule.GetResistanceValue();
				int num4 = ListSF.GetRoster().GetResistanceCount(resistanceName);
				if (num4 < num3)
				{
					float num5 = Mathf.Pow(2f, (float)(num3 - num4) / GameUtils.GetResistanceDoublingRange());
					float num6 = Mathf.Pow(2f, (float)(num4 - num3) / GameUtils.GetResistanceDoublingRange());
					num *= num6;
					num2 *= num5;
				}
			}
		}
		playerModel.SetDamageMultiplier(num);
		fGCODGKLHED2.SetDamageMultiplier(num2);
	}

	protected void SetItemRules(List<ItemRule> itemRules)
	{
		_itemRules.AddRange(itemRules);
		foreach (ItemRule item in _itemRules)
		{
			_rules.Add(item);
		}
	}

	protected void SetNoButtonRules(List<NoButtonRule> noButtonRules)
	{
		_noButtonRules.AddRange(noButtonRules);
		foreach (NoButtonRule item in _noButtonRules)
		{
			_rules.Add(item);
		}
	}

	protected void SetNoAnimationRules(List<NoAnimationRule> noAnimationRules)
	{
		_noAnimationRules.AddRange(noAnimationRules);
		foreach (NoAnimationRule item in _noAnimationRules)
		{
			_rules.Add(item);
		}
	}

	protected void SetInFightRules(List<InFightRule> inFightRules)
	{
		foreach (InFightRule item in inFightRules)
		{
			SetInFightRule(item);
		}
	}

	protected void SetInFightRule(InFightRule rule)
	{
		if (rule.IsSubscribedTo(FightEvent.RenderEvent))
		{
			_renderRules.Add(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.CollisionEvent))
		{
			_collisionRules.Add(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.HitEvent))
		{
			_hitRules.Add(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.AnimationStartEvent))
		{
			_animationRules.Add(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.PhysicsStartEvent))
		{
			_physicsRules.Add(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.CrazyEvent))
		{
			_crazyRules.Add(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.StrikeEvent))
		{
			_strikeRules.Add(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.TimeoutEvent))
		{
			_timeoutRules.Add(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.DamageCheckEvent))
		{
			_damageRules.Add(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.ComboEvent))
		{
			_comboRules.Add(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.ResistanceCheckEvent))
		{
			_resistanceRules.Add(rule);
		}
		_inFightRules.AddIfNotExist(rule);
		_rules.AddIfNotExist(rule);
	}

	protected void DeactivateInFightRule(InFightRule rule)
	{
		if (rule.IsSubscribedTo(FightEvent.RenderEvent))
		{
			_renderRules.Remove(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.CollisionEvent))
		{
			_collisionRules.Remove(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.HitEvent))
		{
			_hitRules.Remove(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.AnimationStartEvent))
		{
			_animationRules.Remove(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.PhysicsStartEvent))
		{
			_physicsRules.Remove(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.CrazyEvent))
		{
			_crazyRules.Remove(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.StrikeEvent))
		{
			_strikeRules.Remove(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.TimeoutEvent))
		{
			_timeoutRules.Remove(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.DamageCheckEvent))
		{
			_damageRules.Remove(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.ComboEvent))
		{
			_comboRules.Remove(rule);
		}
		if (rule.IsSubscribedTo(FightEvent.ResistanceCheckEvent))
		{
			_resistanceRules.Remove(rule);
		}
	}

	protected void SetRandomRules(List<RandomRule> randomRules)
	{
		_randomRules.AddRange(randomRules);
	}

	protected void RemoveRule(Rule rule)
	{
		switch (rule.get_Type())
		{
		case Rule.RuleType.RuleComplex:
			RemoveComplexRule((ComplexRule)rule);
			return;
		case Rule.RuleType.RuleItem:
		case Rule.RuleType.RuleEquipItem:
		case Rule.RuleType.RuleRandomAquiredItem:
			_itemRules.Remove((ItemRule)rule);
			_playerItemRules.Remove((ItemRule)rule);
			_enemyItemRules.Remove((ItemRule)rule);
			break;
		case Rule.RuleType.RuleNoButton:
			_noButtonRules.Remove((NoButtonRule)rule);
			break;
		case Rule.RuleType.RuleNoAnimation:
			_noAnimationRules.Remove((NoAnimationRule)rule);
			break;
		case Rule.RuleType.RuleChangeFight:
			_changeFightRules.Remove((ChangeFightRule)rule);
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
			RemoveInFightRule((InFightRule)rule);
			break;
		case Rule.RuleType.RuleDarkness:
		case Rule.RuleType.RuleLightInTheDarkness:
		case Rule.RuleType.RulePoints:
		case Rule.RuleType.RuleInvertJoystick:
		case Rule.RuleType.RuleRandomArea:
			RemoveInFightRule((InFightRule)rule, true);
			break;
		case Rule.RuleType.RuleAvatar:
			_avatarRules.Remove((AvatarRule)rule);
			break;
		case Rule.RuleType.RuleName:
			_nameRules.Remove((NameRule)rule);
			break;
		}
		_rules.Remove(rule);
	}

	protected void RemoveComplexRule(ComplexRule complexRule)
	{
		List<Rule> list = complexRule.GetRules();
		foreach (Rule item in list)
		{
			RemoveRule(item);
		}
		_rules.Remove(complexRule);
	}

	protected void PutPerkFromRule(PerkRule perkRule)
	{
		switch (perkRule.GetAppliance())
		{
		case RuleAppliance.AppliancePlayer:
			_playerPerks.Add(perkRule.GetPerk());
			break;
		case RuleAppliance.ApplianceOpponent:
			_enemyPerks.Add(perkRule.GetPerk());
			break;
		default:
			GameLog.Error("RulesInspector::putPerkFromRule ERROR - wrong rule appliance %i", perkRule.GetAppliance());
			break;
		}
	}

	protected void PutNoPerkFromRule(NoPerksRule noPerksRule)
	{
		switch (noPerksRule.GetAppliance())
		{
		case RuleAppliance.AppliancePlayer:
			_playerNoPerksRules.Add(noPerksRule);
			break;
		case RuleAppliance.ApplianceOpponent:
			_enemyNoPerksRules.Add(noPerksRule);
			break;
		case RuleAppliance.ApplianceAll:
			_playerNoPerksRules.Add(noPerksRule);
			_enemyNoPerksRules.Add(noPerksRule);
			break;
		default:
			GameLog.Error("RulesInspector::putPerkFromRule ERROR - wrong rule appliance %i", noPerksRule.GetAppliance());
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
		_rules.FindAll((Rule rule) => rule.IsRandom).ForEach((Rule rule) =>
		{
			RemoveRule(rule);
		});
	}

	protected void ResetRandomRules(RandomRule.RefreshMode refreshMode)
	{
		foreach (RandomRule item in _randomRules)
		{
			if (item.GetActive() && item.GetRefreshMode() == refreshMode)
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

	protected void PutRule(Rule rule, bool isCopy = false)
	{
		switch (rule.get_Type())
		{
		case Rule.RuleType.RuleRandom:
			_randomRules.Add((RandomRule)rule);
			break;
		case Rule.RuleType.RuleComplex:
			PutComplexRule((ComplexRule)rule);
			return;
		case Rule.RuleType.RuleItem:
		case Rule.RuleType.RuleEquipItem:
		case Rule.RuleType.RuleRandomAquiredItem:
			PutItemRule((ItemRule)rule);
			break;
		case Rule.RuleType.RuleNoButton:
			_noButtonRules.Add((NoButtonRule)rule);
			break;
		case Rule.RuleType.RuleNoAnimation:
			_noAnimationRules.Add((NoAnimationRule)rule);
			break;
		case Rule.RuleType.RuleChangeFight:
			_changeFightRules.Add((ChangeFightRule)rule);
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
			PutInFightRule((InFightRule)rule);
			return;
		case Rule.RuleType.RuleDarkness:
		case Rule.RuleType.RuleLightInTheDarkness:
		case Rule.RuleType.RulePoints:
			PutInFightRule((InFightRule)rule, true);
			return;
		case Rule.RuleType.RuleDescription:
		case Rule.RuleType.RuleRatingEvaluation:
		case Rule.RuleType.RuleCurrencyCost:
		case Rule.RuleType.RuleRaidCurrencyCost:
			return;
		case Rule.RuleType.RuleInvertJoystick:
			PutInFightRule((InFightRule)rule, true);
			break;
		case Rule.RuleType.RuleRandomArea:
			PutInFightRule((InFightRule)rule, true);
			break;
		case Rule.RuleType.RuleAvatar:
			_avatarRules.Add((AvatarRule)rule);
			break;
		case Rule.RuleType.RuleName:
			_nameRules.Add((NameRule)rule);
			break;
		default:
			GameLog.Error("RulesInspector::putRule ERROR - wrong rule type %i", rule.get_Type());
			break;
		}
		if (rule.get_Type() != Rule.RuleType.RuleRandom)
		{
			_rules.AddIfNotExist(rule);
		}
	}

	protected void PutComplexRule(ComplexRule complexRule)
	{
		List<Rule> list = complexRule.GetRules();
		foreach (Rule item in list)
		{
			PutRule(item);
		}
		_rules.AddIfNotExist(complexRule);
	}

	protected void PutItemRule(ItemRule itemRule)
	{
		switch (itemRule.GetAppliance())
		{
		case RuleAppliance.AppliancePlayer:
			_playerItemRules.Add(itemRule);
			break;
		case RuleAppliance.ApplianceOpponent:
			_enemyItemRules.Add(itemRule);
			break;
		case RuleAppliance.ApplianceAll:
			_playerItemRules.Add(itemRule);
			_enemyItemRules.Add(itemRule);
			break;
		}
		_itemRules.Add(itemRule);
	}

	protected void PutInFightRule(InFightRule rule, bool isCopy = false)
	{
		if (!isCopy && rule.GetAppliance() == RuleAppliance.ApplianceAll)
		{
			InFightRule playerRule = rule.Copy();
			InFightRule aAJIFBJLJOA2 = rule.Copy();
			playerRule.SetAppliance(RuleAppliance.AppliancePlayer);
			aAJIFBJLJOA2.SetAppliance(RuleAppliance.ApplianceOpponent);
			playerRule.ParentRule = rule;
			aAJIFBJLJOA2.ParentRule = rule;
			SetInFightRule(playerRule);
			SetInFightRule(aAJIFBJLJOA2);
		}
		else
		{
			SetInFightRule(rule);
		}
	}

	protected void RemoveInFightRule(InFightRule rule, bool isCopy = false)
	{
		if (!isCopy && rule.GetAppliance() == RuleAppliance.ApplianceAll)
		{
			int num = 0;
			while (num < _inFightRules.Count)
			{
				InFightRule inFightRule = _inFightRules[num];
				if (inFightRule.ParentRule == rule)
				{
					_inFightRules.Remove(inFightRule);
					DeactivateInFightRule(inFightRule);
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
			if (rule == aAJIFBJLJOA2)
			{
				_inFightRules.Remove(aAJIFBJLJOA2);
				DeactivateInFightRule(aAJIFBJLJOA2);
				break;
			}
		}
	}

	protected void SetRandomRuleSeed(int seed)
	{
		_randomRuleSeed = seed;
	}

	protected void SetHasRandomSeed(bool value)
	{
		_hasRandomSeed = value;
	}
}
