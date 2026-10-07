using System.Collections.Generic;
using System.Text;
using CodeStage.AntiCheat.ObscuredTypes;

public partial class InfoPerk
{
	public enum InfoPerkEvent
	{
		EVENT_MOD_EXPIRES = 0
	}

	public PerkData Data;

	private List<PerksStage.ActionPerk> pendingActions = new List<PerksStage.ActionPerk>();

	private List<PerksStage.ActionPerk> activeActions = new List<PerksStage.ActionPerk>();

	private List<string> activeActionNames = new List<string>();

	private List<string> expiredModNames = new List<string>();

	public List<PerksStage.ActionPerk> PendingActions
	{
		get
		{
			return GetPendingActions();
		}
	}

	public List<PerksStage.ActionPerk> ActiveActions
	{
		get
		{
			return GetActiveActions();
		}
	}

	public List<string> ActiveActionNames
	{
		get
		{
			return GetActiveActionNames();
		}
	}

	public List<string> ExpiredModNames
	{
		get
		{
			return GetExpiredModNames();
		}
	}

	public bool IsOwnerPlayer
	{
		get
		{
			return GetIsOwnerPlayer();
		}
	}

	public List<PerksStage.ActionPerk> GetPendingActions()
	{
		return pendingActions;
	}

	public List<PerksStage.ActionPerk> GetActiveActions()
	{
		return activeActions;
	}

	public List<string> GetActiveActionNames()
	{
		return activeActionNames;
	}

	public List<string> GetExpiredModNames()
	{
		return expiredModNames;
	}

	public bool GetIsOwnerPlayer()
	{
		if (Data != null && Data.PerkInfo != null && Data.PerkInfo.GetOwnerModel() != null)
		{
			return Data.PerkInfo.GetOwnerModel().IsPlayerModel();
		}
		return false;
	}

	public void Render()
	{
		int num = 0;
		int count = activeActions.Count;
		while (num < activeActions.Count)
		{
			count = activeActions.Count;
			PerksStage.ActionPerk actionPerk = activeActions[num];
			ApplyHealthChangeTick(actionPerk);
			if (actionPerk.DurationFrames > 0)
			{
				if (actionPerk.ElapsedFrames >= actionPerk.DurationFrames || actionPerk.IsExpired)
				{
					ExpireAction(actionPerk);
				}
				actionPerk.ElapsedFrames++;
			}
			if (count == activeActions.Count)
			{
				num++;
			}
		}
	}

	private void ApplyHealthChangeTick(PerksStage.ActionPerk actionPerk)
	{
		if (actionPerk.Action.get_Type() == ActionType.ACTION_MOD_HEALTH_CHANGE)
		{
			ApplyHealthChange(actionPerk);
		}
	}

	public void Run()
	{
		if (pendingActions.Count > 0)
		{
			ExecuteActions(pendingActions);
			pendingActions.Clear();
		}
	}

	private void LogModEvent(PerksStage.ActionPerk actionPerk, bool isStart)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("PERK ----- ");
		stringBuilder.Append((!isStart) ? "ModDestruction " : "ModStart ");
		stringBuilder.Append("PerkName: {0} ModName: {1} ModXML: {2}");
		if (LogRules.GetInstance().GetLogPerks())
		{
			GameLog.Info(stringBuilder.ToString(), actionPerk.Action.GetTrigger().GetPerk().Name, actionPerk.Action.get_Name(), actionPerk.Action.GetElementName());
		}
	}

	public void ExecuteActions(List<PerksStage.ActionPerk> perkActions)
	{
		foreach (PerksStage.ActionPerk item in perkActions)
		{
			PerksStage.ActionPerk actionPerk = ((!item.Action.GetModificator()) ? item : new PerksStage.ActionPerk(item));
			switch (item.Action.get_Type())
			{
			case ActionType.ACTION_SHOW_ICONS:
				ApplyShowIcon(actionPerk, false);
				break;
			case ActionType.ACTION_MOD_HEALTH_CHANGE:
				ApplyHealthChangeStart(actionPerk, false);
				break;
			case ActionType.ACTION_SET_ATTRIBUTES:
				ApplySetAttributes(actionPerk, false);
				break;
			case ActionType.ACTION_INVISIBILITY:
				ApplyInvisibility(actionPerk, false);
				break;
			case ActionType.ACTION_DISABLE_INTERVAL:
				ApplyDisableInterval(actionPerk);
				break;
			case ActionType.ACTION_SET_HIT:
				ApplySetHit(actionPerk);
				break;
			case ActionType.ACTION_LIFE_STEAL:
				ApplyLifeSteal(actionPerk);
				break;
			case ActionType.ACTION_ADD_BULLETS:
				ApplyAddBullets(actionPerk);
				break;
			case ActionType.ACTION_ADD_MAGIC:
				ApplyAddMagicCharge(actionPerk);
				break;
			case ActionType.ACTION_SET_MOD_FRAMES:
				ApplySetModFrames(actionPerk);
				break;
			case ActionType.ACTION_MOD_EFFECT:
				ApplySetModEffect(actionPerk);
				break;
			case ActionType.ACTION_PROVOKE:
				ApplyProvoke(actionPerk);
				break;
			case ActionType.ACTION_SET_TACTICS:
				ApplySetTactics(actionPerk);
				break;
			case ActionType.ACTION_CLEAR_ACTION:
				ApplyClearAction(actionPerk);
				break;
			case ActionType.ACTION_VARIABLE:
				ApplyVariable(actionPerk);
				break;
			case ActionType.ACTION_SET_VARIABLE:
				ApplySetVariable(actionPerk);
				break;
			case ActionType.ACTION_SET_COOLDOWN:
				ApplySetCooldown(actionPerk);
				break;
			case ActionType.ACTION_CHANGE_IMPULSE:
				ApplyChangeImpulse(actionPerk, false);
				break;
			case ActionType.ACTION_CHANGE_HIT_EFFECT_SCALE:
				ApplyChangeHitEffectScale(actionPerk, false);
				break;
			case ActionType.ACTION_CHANGE_ADD_DAMAGE_VALUE:
				ApplyChangeAdditionalDamage(actionPerk, false);
				break;
			case ActionType.ACTION_CHANGE_MODEL_COLOR:
				ApplyChangeModelColor(actionPerk, false);
				break;
			case ActionType.ACTION_SLOW_MODEL:
				ApplySlowModel(actionPerk, false);
				break;
			case ActionType.ACTION_TURN_OFF_COLLISION:
				ApplyTurnOffCollision(actionPerk, false);
				break;
			case ActionType.ACTION_SWITCH:
				ApplySwitch(actionPerk);
				break;
			case ActionType.ACTION_MARK_PERK_USED:
				PerksStage.IncrementPerkUse(Data.PerkInfo.Name);
				break;
			case ActionType.ACTION_PERK_AREA:
				ApplyPerkArea(actionPerk, false);
				break;
			case ActionType.ACTION_MOVE_MODEL:
				ApplyMoveModel(actionPerk);
				break;
			case ActionType.ACTION_SET_MOVES_VARIABLE:
				ApplySetMovesVariable(actionPerk);
				break;
			case ActionType.ACTION_STEAL_MAGIC:
				ApplyStealMagic(actionPerk, false);
				break;
			}
			if (item.Action.GetModificator())
			{
				activeActions.Add(actionPerk);
				activeActionNames.Add(actionPerk.Action.get_Name());
				PerkActionModificator modificator = (PerkActionModificator)actionPerk.Action;
				if (modificator.GetNamespace() != string.Empty)
				{
					PerksStage.RegisterNamespaceAction(actionPerk);
				}
			}
			LogModEvent(item, true);
		}
		perkActions.Clear();
		ClearActions();
	}

	private void ApplyShowIcon(PerksStage.ActionPerk actionPerk, bool isRemoval)
	{
		if (!isRemoval)
		{
			MarkPerkUsed();
		}
		PerkActionShowIcon showIconAction = (PerkActionShowIcon)actionPerk.Action;
		string image = (showIconAction.GetImage() != string.Empty)
			? showIconAction.GetImage()
			: Data.PerkInfo.ImageName;
		actionPerk.IconPath = ResolveIconPath(image);
		actionPerk.ShowExpiration = showIconAction.GetShowExpiration();
		actionPerk.ExpirationVersion = showIconAction.GetExpirationVer();
		actionPerk.TargetModel.NotifyPerkAction(actionPerk, isRemoval);
	}

	private static string ResolveIconPath(string image)
	{
		// Vanilla images are relative to UI/Skills. Mod API images may already be
		// fully-qualified asset IDs, which must not receive the legacy prefix.
		return (image != null && image.IndexOf(':') > 0)
			? image
			: string.Format("{0}{1}", SF2Paths.GetSkillsUiPath(), image ?? string.Empty);
	}

	private void ApplyHealthChangeStart(PerksStage.ActionPerk actionPerk, bool isRemoval)
	{
	}

	private void ApplySetAttributes(PerksStage.ActionPerk actionPerk, bool isRemoval)
	{
		int num = ((!isRemoval) ? 1 : (-1));
		PerkActionSetAttributes setAttributesAction = (PerkActionSetAttributes)actionPerk.Action;
        var applied = isRemoval ? actionPerk.AppliedAttributes : null;
        if (applied == null)
        {
            // Resolve every expression before mutation, and retain the normalized
            // deltas so expiry does not reevaluate a changed combat context.
            applied = new Dictionary<string, int>();
            foreach (var item in setAttributesAction.GetAttributes())
            {
                var attributes = new Attributes();
                attributes.Set(item.Key, item.Value.Calculate().ToInt());
                int amount = 0;
                attributes.Get(item.Key, ref amount);
                applied.Add(item.Key, amount);
            }
        }
        if (!isRemoval) actionPerk.AppliedAttributes = applied;
		foreach (var item in applied)
		{
			string key = item.Key;
			int attributeDelta = item.Value;
			int currentValue = 0;
			actionPerk.TargetModel.Parameters.FinalAttributes.Get(key, ref currentValue, false, true);
			actionPerk.TargetModel.Parameters.FinalAttributes.Set(key, currentValue + attributeDelta * num, true);
			if (key == "DamageFactor" && !isRemoval && GetIsOwnerPlayer())
			{
				Model.StrikeResult strikeResult = actionPerk.SourceModel.LastStrike;
				strikeResult.AddProcedPerk(Data.PerkInfo.Id);
			}
		}
	}

    internal System.Action TransferHealthEffect(PerksStage.ActionPerk action, Model expected, Model replacement)
    {
        if (action == null || expected == null || replacement == null || expected == replacement)
            throw new System.ArgumentException("Health effect transfer requires distinct models.");
        if (!activeActions.Contains(action) || !(action.Action is ModHealthChange) ||
            (action.TargetModel != expected && action.SourceModel != expected))
            throw new System.InvalidOperationException("The active health effect does not refer to this form.");
        var target = action.TargetModel;
        var source = action.SourceModel;
        if (target == expected) action.TargetModel = replacement;
        if (source == expected) action.SourceModel = replacement;
        return () => { action.TargetModel = target; action.SourceModel = source; };
    }

    // The form coordinator retains the returned rollback until all registrations
    // commit. This moves one active attribute effect without restarting its timer.
    internal System.Action TransferAttributeEffect(PerksStage.ActionPerk action, Model expected, Model replacement)
    {
        if (action == null || expected == null || replacement == null || expected == replacement)
            throw new System.ArgumentException("Attribute transfer requires an action and distinct models.");
        if (!activeActions.Contains(action) || !(action.Action is PerkActionSetAttributes) ||
            action.TargetModel != expected || action.AppliedAttributes == null)
            throw new System.InvalidOperationException("The active attribute effect has no matching applied state.");
        var oldAttributes = expected.Parameters.FinalAttributes;
        var newAttributes = replacement.Parameters.FinalAttributes;
        if (ReferenceEquals(oldAttributes, newAttributes))
            throw new System.InvalidOperationException("Form parameters must own separate attributes.");
        var beforeOld = new Attributes(oldAttributes);
        var beforeNew = new Attributes(newAttributes);
        var afterOld = new Attributes(oldAttributes);
        var afterNew = new Attributes(newAttributes);
        foreach (var delta in action.AppliedAttributes)
        {
            int oldValue = 0, newValue = 0;
            afterOld.Get(delta.Key, ref oldValue, false, true);
            afterNew.Get(delta.Key, ref newValue, false, true);
            afterOld.Set(delta.Key, checked(oldValue - delta.Value), true);
            afterNew.Set(delta.Key, checked(newValue + delta.Value), true);
        }
        var source = action.SourceModel;
        oldAttributes.Clear(); oldAttributes.AddRange(afterOld);
        newAttributes.Clear(); newAttributes.AddRange(afterNew);
        action.TargetModel = replacement;
        if (source == expected) action.SourceModel = replacement;
        return () =>
        {
            oldAttributes.Clear(); oldAttributes.AddRange(beforeOld);
            newAttributes.Clear(); newAttributes.AddRange(beforeNew);
            action.TargetModel = expected; action.SourceModel = source;
        };
    }

	private void ApplyChangeImpulse(PerksStage.ActionPerk actionPerk, bool isRemoval)
	{
		PerkActionChangeImpulse changeImpulseAction = (PerkActionChangeImpulse)actionPerk.Action;
		float multiplierX = changeImpulseAction.GetMultiplierX();
		float multiplierY = changeImpulseAction.GetMultiplierY();
		float multiplierZ = changeImpulseAction.GetMultiplierZ();
		if (isRemoval)
		{
			actionPerk.TargetModel.ResetImpulseFactor();
		}
		else
		{
			actionPerk.TargetModel.SetImpulseFactor(multiplierX, multiplierY, multiplierZ);
		}
	}

	private void ApplyChangeHitEffectScale(PerksStage.ActionPerk actionPerk, bool isRemoval)
	{
		PerkActionChangeHitEffectScale hitEffectScaleAction = (PerkActionChangeHitEffectScale)actionPerk.Action;
		float hitEffectScale = hitEffectScaleAction.GetHitEffectScale();
		if (isRemoval)
		{
			actionPerk.TargetModel.ResetHitEffectScale();
		}
		else
		{
			actionPerk.TargetModel.set_HitEffectScale(hitEffectScale);
		}
	}

	private void ApplyChangeAdditionalDamage(PerksStage.ActionPerk actionPerk, bool isRemoval)
	{
		PerkActionChangeAdditionalDamageValue additionalDamageAction = (PerkActionChangeAdditionalDamageValue)actionPerk.Action;
		float additionalDamage = additionalDamageAction.GetAdditionalDamageValue();
		if (isRemoval)
		{
			actionPerk.TargetModel.SetAdditionalDamageToOne();
			return;
		}
		actionPerk.TargetModel.set_AdditionalDamageValue(additionalDamage);
		if (GetIsOwnerPlayer())
		{
			Model.StrikeResult strikeResult = actionPerk.SourceModel.LastStrike;
			strikeResult.AddProcedPerk(Data.PerkInfo.Id);
		}
	}

	private void ApplyChangeModelColor(PerksStage.ActionPerk action, bool remove)
	{
		PerkActionChangeModelColor color = (PerkActionChangeModelColor)action.Action;
		if (remove) action.TargetModel.ClearPerkColor();
		else action.TargetModel.SetPerkColor(color.Color);
	}

	private void ApplySlowModel(PerksStage.ActionPerk action, bool remove)
	{
		PerkActionSlowModel slow = (PerkActionSlowModel)action.Action;
		action.TargetModel.SetPerkSlowFactor(remove ? 1 : slow.Speed);
	}

	private void ApplyTurnOffCollision(PerksStage.ActionPerk action, bool remove)
	{
		action.TargetModel.SetPerkCollisionDisabled(!remove);
	}

	private void ApplySwitch(PerksStage.ActionPerk action)
	{
		PerkActionSwitch switchAction = (PerkActionSwitch)action.Action;
		List<PerksStage.ActionPerk> selected = new List<PerksStage.ActionPerk>();
		foreach (PerkAction nested in switchAction.SelectActions())
		{
			PerksStage.ActionPerk nestedAction = new PerksStage.ActionPerk();
			nestedAction.Action = nested;
			nestedAction.SourceModel = action.SourceModel;
			nestedAction.TargetModel = nested.ResolveTargetModel(nested.GetPerk().GetOwnerModel());
			if (nestedAction.TargetModel != null)
				selected.Add(nestedAction);
		}
		if (selected.Count != 0)
			ExecuteActions(selected);
	}

	private void ApplyPerkArea(PerksStage.ActionPerk action, bool remove)
	{
		Fight fight = Fight.GetCurrentFight();
		if (fight == null)
			return;
		if (remove)
		{
			fight.RemovePerkActivationArea();
			return;
		}
		PerkActionArea area = (PerkActionArea)action.Action;
		float x = area.PositionX.Calculate().ToFloat();
		fight.CreatePerkActivationArea(area.Width, area.FileName, area.get_Name());
		fight.UpdatePerkActivationArea(x, area.ShiftY, true);
	}

	private void ApplyMoveModel(PerksStage.ActionPerk action)
	{
		PerkActionMoveModel move = (PerkActionMoveModel)action.Action;
		float offset = UnityEngine.Mathf.Abs(move.OffsetX.Calculate().ToFloat());
		Model target = action.TargetModel;
		if (target == null)
			return;
		Model enemy = target.GetCombatTarget();
		if (enemy == null)
			return;
		Vector3f position = new Vector3f(target.GetPosition());
		float direction = enemy.GetPosition().GetX() >= position.GetX() ? 1f : -1f;
		position.SetX(position.GetX() + direction * offset);
		target.SetModelPosition(position);
	}

	private void ApplySetMovesVariable(PerksStage.ActionPerk action)
	{
		PerkActionSetMovesVariable variable = (PerkActionSetMovesVariable)action.Action;
		FunctionResult result = variable.Value.Calculate();
		float number;
		if (float.TryParse(result.Value, out number))
			action.TargetModel.GetConditions().PerkVariables[variable.get_Name()] = number;
		else
			action.TargetModel.GetConditions().PerkStringVariables[variable.get_Name()] = result.Value;
	}

	private void ApplyStealMagic(PerksStage.ActionPerk action, bool remove)
	{
		if (remove)
		{
			if (action.PreviousMagic != null)
				action.TargetModel.SwapPerkItem(action.PreviousMagic);
			return;
		}
		PerkActionStealMagic steal = (PerkActionStealMagic)action.Action;
		string magicName = steal.MagicName.Calculate().Value;
		ItemInfo magic = ListSF.GetItems().GetItemByName(magicName);
		if (magic != null)
		{
			action.PreviousMagic = action.TargetModel.Parameters.Magic;
			action.TargetModel.SwapPerkItem(magic);
		}
	}

	private void ApplyInvisibility(PerksStage.ActionPerk actionPerk, bool isRemoval)
	{
		Fight fight = Fight.GetCurrentFight();
		if (fight != null)
		{
			fight.SetModelVisible(actionPerk.TargetModel, isRemoval);
		}
	}

	private void ApplyDisableInterval(PerksStage.ActionPerk actionPerk)
	{
		PerkActionDisableInterval disableIntervalAction = (PerkActionDisableInterval)actionPerk.Action;
		if (disableIntervalAction.GetIntervalType() != string.Empty)
		{
			IntervalAnimation.IntervalType intervalType = IntervalAnimation.ParseIntervalType(disableIntervalAction.GetIntervalType());
			actionPerk.TargetModel.RemoveInterval(intervalType);
		}
		else if (disableIntervalAction.GetIntervalName() != string.Empty)
		{
			actionPerk.TargetModel.RemoveInterval(disableIntervalAction.GetIntervalName());
		}
	}

	private void ApplyLifeSteal(PerksStage.ActionPerk actionPerk)
	{
		Fight fight = Fight.GetCurrentFight();
		if (fight != null)
		{
			PerkActionLifesteal lifestealAction = (PerkActionLifesteal)actionPerk.Action;
			Model.StrikeResult strikeResult = actionPerk.SourceModel.LastStrike;
			float num = (ObscuredFloat)(actionPerk.TargetModel.Parameters.GetCurrentLife());
			float healAmount = lifestealAction.GetDamagePart() * strikeResult.FinalDamage * (actionPerk.TargetModel.GetCombatTarget().GetPowerMultiplier() / strikeResult.Victim.GetPowerMultiplier());
			fight.UpdateLife(actionPerk.TargetModel, healAmount);
		}
	}

	private void ApplySetHit(PerksStage.ActionPerk actionPerk)
	{
		PerkActionSetHit setHitAction = (PerkActionSetHit)actionPerk.Action;
		Model sourceModel = actionPerk.SourceModel;
		Model.StrikeResult strikeResult = sourceModel.LastStrike;
		bool flag = true;
		if (setHitAction.GetCritical() > -1)
		{
			strikeResult.IsCritical = setHitAction.GetCritical() > 0;
			if (!strikeResult.IsCritical)
			{
				flag = false;
			}
		}
		if (setHitAction.GetShock() > -1)
		{
			strikeResult.IsShock = setHitAction.GetShock() > 0;
		}
		if (setHitAction.GetDisarm() > -1)
		{
			strikeResult.IsDisarm = setHitAction.GetDisarm() > 0;
		}
		if (setHitAction.GetBlock() > -1)
		{
			strikeResult.IsBlocked = setHitAction.GetBlock() > 0;
		}
		if (setHitAction.GetDamage() != null)
		{
			FunctionResult damageResult = setHitAction.GetDamage().Calculate();
			strikeResult.RawDamage = damageResult.ToFloat();
			strikeResult.FinalDamage = damageResult.ToFloat();
		}
		if (GetIsOwnerPlayer() && flag)
		{
			strikeResult.AddProcedPerk(Data.PerkInfo.Id);
		}
	}

	private void ApplySetTactics(PerksStage.ActionPerk actionPerk)
	{
		PerkActionSetTactics setTacticsAction = (PerkActionSetTactics)actionPerk.Action;
		actionPerk.TargetModel.ChangeAiTactic(setTacticsAction.GetTactics());
	}

	private void ApplyAddBullets(PerksStage.ActionPerk actionPerk)
	{
		PerkActionAddBullets addBulletsAction = (PerkActionAddBullets)actionPerk.Action;
		FunctionResult bulletsResult = addBulletsAction.GetValue().Calculate();
		int bulletCount = bulletsResult.ToInt();
		if (addBulletsAction.GetBulletType() == "MagicBullet")
		{
			actionPerk.TargetModel.AddMagicCharges(bulletCount);
			actionPerk.TargetModel.UpdateMagicButton();
		}
	}

	private void ApplyAddMagicCharge(PerksStage.ActionPerk actionPerk)
	{
		PerkActionAddMagicCharge addMagicChargeAction = (PerkActionAddMagicCharge)actionPerk.Action;
		FunctionResult chargeResult = addMagicChargeAction.GetValue().Calculate();
		float chargeFraction = chargeResult.ToFloat();
		actionPerk.TargetModel.AddMagicChargeFraction(chargeFraction);
		actionPerk.TargetModel.UpdateMagicButton();
	}

	private void ApplySetModFrames(PerksStage.ActionPerk actionPerk)
	{
		MarkPerkUsed();
		PerkActionSetModFrames setModFramesAction = (PerkActionSetModFrames)actionPerk.Action;
		setModFramesAction.GetModFrames().Calculate();
		FunctionResult framesResult = setModFramesAction.GetModFrames().Calculate();
		int durationFrames = framesResult.ToInt();
		foreach (PerksStage.ActionPerk item in activeActions)
		{
			if (setModFramesAction.GetModName() == item.Action.get_Name())
			{
				item.DurationFrames = durationFrames;
				item.ElapsedFrames = 0;
			}
		}
		if (setModFramesAction.GetNamespace() == null || !(setModFramesAction.GetNamespace() != string.Empty))
		{
			return;
		}
		List<PerksStage.ActionPerk> list = PerksStage.GetNamespaceActions(setModFramesAction.GetNamespace());
		if (list == null)
		{
			return;
		}
		foreach (PerksStage.ActionPerk item2 in list)
		{
			if (setModFramesAction.GetModName().Equals(item2.Action.get_Name()))
			{
				item2.DurationFrames = durationFrames;
				item2.ElapsedFrames = 0;
			}
		}
	}

	private void ApplySetModEffect(PerksStage.ActionPerk actionPerk)
	{
		PerkActionSetModEffect setModEffectAction = (PerkActionSetModEffect)actionPerk.Action;
		string text = setModEffectAction.GetNamespace();
		if (text != null && text != string.Empty)
		{
			PerksStage.ActionPerk replacedAction = PerksStage.FindNamespaceAction(setModEffectAction.GetModName(), text);
			if (replacedAction != null)
			{
				replacedAction.TargetModel.NotifyPerkActionReplaced(replacedAction, actionPerk);
			}
			return;
		}
		foreach (PerksStage.ActionPerk item in activeActions)
		{
			if (setModEffectAction.GetModName() == item.Action.get_Name())
			{
				item.TargetModel.NotifyPerkActionReplaced(item, actionPerk);
			}
		}
	}

	private void ApplyProvoke(PerksStage.ActionPerk actionPerk)
	{
		Fight fight = Fight.GetCurrentFight();
		if (fight == null)
		{
			return;
		}
		PerkActionProvoke provokeAction = (PerkActionProvoke)actionPerk.Action;
		PerkInfoItem triggerPerk = provokeAction.GetTrigger().GetPerk();
		InfoPerk provokedInfoPerk = fight.GetPerksStage().FindInfoPerk(actionPerk.TargetModel, triggerPerk);
		if (provokedInfoPerk != null)
		{
			List<string> list = provokedInfoPerk.GetActiveActionNames();
		}
		List<PerkTrigger> list2 = new List<PerkTrigger>();
		foreach (PerkTrigger item in triggerPerk.GetTriggers())
		{
			if (item.get_Name() == provokeAction.GetProvokeTrigger() && item.AreConditionsMet(actionPerk.TargetModel, GetActiveActionNames()))
			{
				list2.Add(item);
			}
		}
		foreach (PerkTrigger item2 in list2)
		{
			fight.GetPerksStage().ExecuteTriggerActions(actionPerk.TargetModel, item2, true);
		}
	}

	private void ApplyClearAction(PerksStage.ActionPerk actionPerk)
	{
		PerkActionClearAction clearAction = (PerkActionClearAction)actionPerk.Action;
		foreach (PerksStage.ActionPerk item in activeActions)
		{
			if (clearAction.GetNameAction() == string.Empty || clearAction.GetNameAction() == item.Action.get_Name())
			{
				item.IsExpired = true;
			}
		}
		if (clearAction.GetNamespace() == null || !(clearAction.GetNamespace() != string.Empty))
		{
			return;
		}
		List<PerksStage.ActionPerk> list = PerksStage.GetNamespaceActions(clearAction.GetNamespace());
		if (list == null)
		{
			return;
		}
		foreach (PerksStage.ActionPerk item2 in list)
		{
			if (clearAction.GetNameAction() == null || clearAction.GetNameAction().Equals(string.Empty) || clearAction.GetNameAction().Equals(item2.Action.get_Name()))
			{
				item2.IsExpired = true;
			}
		}
	}

	private void ApplyHealthChange(PerksStage.ActionPerk actionPerk)
	{
		Fight fight = Fight.GetCurrentFight();
		if (fight != null)
		{
			ModHealthChange healthChange = (ModHealthChange)actionPerk.Action;
			fight.UpdateLife(actionPerk.TargetModel, healthChange.GetPerFrameValue());
		}
	}

	private void MarkPerkUsed()
	{
		if (GetIsOwnerPlayer())
		{
			Fight fight = Fight.GetCurrentFight();
			if (fight != null)
			{
				PerksStage.IncrementPerkUse(Data.PerkInfo.Name);
			}
			else
			{
				GameLog.Error("Error: No fight on perk start");
			}
		}
	}

	private void ExpireAction(PerksStage.ActionPerk actionPerk)
	{
		actionPerk.DurationFrames = 0;
		if (actionPerk.Action.GetFrames() != null)
		{
			FunctionResult framesResult = actionPerk.Action.GetFrames().Calculate();
			actionPerk.DurationFrames = framesResult.ToInt();
		}
		switch (actionPerk.Action.get_Type())
		{
		case ActionType.ACTION_SHOW_ICONS:
			ApplyShowIcon(actionPerk, true);
			break;
		case ActionType.ACTION_SET_ATTRIBUTES:
			ApplySetAttributes(actionPerk, true);
			break;
		case ActionType.ACTION_CHANGE_IMPULSE:
			ApplyChangeImpulse(actionPerk, true);
			break;
		case ActionType.ACTION_CHANGE_HIT_EFFECT_SCALE:
			ApplyChangeHitEffectScale(actionPerk, true);
			break;
		case ActionType.ACTION_CHANGE_ADD_DAMAGE_VALUE:
			ApplyChangeAdditionalDamage(actionPerk, true);
			break;
		case ActionType.ACTION_CHANGE_MODEL_COLOR:
			ApplyChangeModelColor(actionPerk, true);
			break;
		case ActionType.ACTION_SLOW_MODEL:
			ApplySlowModel(actionPerk, true);
			break;
		case ActionType.ACTION_TURN_OFF_COLLISION:
			ApplyTurnOffCollision(actionPerk, true);
			break;
		case ActionType.ACTION_PERK_AREA:
			ApplyPerkArea(actionPerk, true);
			break;
		case ActionType.ACTION_STEAL_MAGIC:
			ApplyStealMagic(actionPerk, true);
			break;
		case ActionType.ACTION_INVISIBILITY:
			ApplyInvisibility(actionPerk, true);
			break;
		}
		LogModEvent(actionPerk, false);
		bool flag = actionPerk.Action.GetModificator();
		string value = actionPerk.Action.get_Name();
		Model targetModel = actionPerk.TargetModel;
		RemoveActiveAction(actionPerk);
		if (!flag)
		{
			return;
		}
		Fight fight = Fight.GetCurrentFight();
		if (fight != null)
		{
			PerkActionModificator modificator = (PerkActionModificator)actionPerk.Action;
			if (modificator.GetNamespace() != null && modificator.GetNamespace() != string.Empty)
			{
				PerksStage.UnregisterNamespaceAction(actionPerk);
			}
			string text = (string)fight.GetPerksStage().GetPerkMap()["ModExpires"];
			if (text != null)
			{
				GetExpiredModNames().Add(text);
			}
			fight.GetPerksStage().GetPerkMap()["ModExpires"] = value;
			fight.GetPerksStage().GetPerkMap()["Namespace"] = modificator.GetNamespace();
			fight.GetPerksStage().GetPerkMap()["ParentPerk"] = modificator.GetPerk();
			fight.GetPerksStage().FireEvent(targetModel, PerkEvent.PerkEventType.EVENT_MOD_EXPIRES, true);
			fight.GetPerksStage().AddExpiredAction(actionPerk);
		}
	}

	private void RemoveActiveAction(PerksStage.ActionPerk removedAction)
	{
		foreach (PerksStage.ActionPerk item in activeActions)
		{
			if (removedAction == item)
			{
				activeActions.Remove(item);
				activeActionNames.Remove(item.Action.get_Name());
				break;
			}
		}
	}

	private void ApplyVariable(PerksStage.ActionPerk actionPerk)
	{
		PerkActionVariable variableAction = (PerkActionVariable)actionPerk.Action;
		string key = variableAction.get_Name();
		FunctionResult valueResult = variableAction.GetValue().Calculate();
		float value;
		if (float.TryParse(valueResult.Value, out value))
		{
			actionPerk.TargetModel.GetConditions().PerkVariables[key] = value;
			actionPerk.TargetModel.GetConditions().PerkStringVariables.Remove(key);
		}
		else
		{
			actionPerk.TargetModel.GetConditions().PerkStringVariables[key] =
				valueResult.Value ?? string.Empty;
		}
	}

	private void ApplySetVariable(PerksStage.ActionPerk actionPerk)
	{
		PerkActionSetVariable setVariableAction = (PerkActionSetVariable)actionPerk.Action;
		string text = setVariableAction.get_Name();
		FunctionResult valueResult = setVariableAction.GetValue().Calculate();
		float num = valueResult.ToFloat();
		if (setVariableAction.GetHasMinValue())
		{
			FunctionResult minResult = setVariableAction.GetMinValue().Calculate();
			float num2 = minResult.ToFloat();
			if (num < num2)
			{
				num = num2;
			}
		}
		if (setVariableAction.GetHasMaxValue())
		{
			FunctionResult maxResult = setVariableAction.GetMaxValue().Calculate();
			float num3 = maxResult.ToFloat();
			if (num > num3)
			{
				num = num3;
			}
		}
		actionPerk.TargetModel.GetConditions().PerkVariables[text] = num;
		if (SystemProperties.IsDebug())
		{
			GameLog.Info("SetVariable {0} = {1}", text, num);
		}
	}

	private void ApplySetCooldown(PerksStage.ActionPerk actionPerk)
	{
		PerkActionSetCooldown cooldownAction = (PerkActionSetCooldown)actionPerk.Action;
		int num = cooldownAction.GetCooldownFrames();
		string buttonName = cooldownAction.GetButtonName();
		FightCID buttonId = (FightCID)MovesMaps.GetMappedIndex(MovesMaps.MapType.KEY_TYPE, buttonName);
		actionPerk.TargetModel.ResetButtonCooldown(buttonId, 0);
		actionPerk.TargetModel.StartButtonCooldown(buttonId, num);
		if (SystemProperties.IsDebug())
		{
			GameLog.Info("SetCooldown button = {0}, frames = {1}", cooldownAction.GetButtonName(), num);
		}
	}

	public void ClearActions(bool forceExpire = false)
	{
		if (activeActions.Count <= 0)
		{
			return;
		}
		int num = 0;
		int count = activeActions.Count;
		while (num < activeActions.Count)
		{
			count = activeActions.Count;
			PerksStage.ActionPerk actionPerk = activeActions[num];
			if (forceExpire || actionPerk.IsExpired)
			{
				ExpireAction(actionPerk);
			}
			if (count == activeActions.Count)
			{
				num++;
			}
		}
	}

	public void ResetExpiredMods()
	{
		expiredModNames.Clear();
		Fight fight = Fight.GetCurrentFight();
		if (fight != null)
		{
			fight.GetPerksStage().GetPerkMap()["ModExpires"] = null;
		}
	}
}
