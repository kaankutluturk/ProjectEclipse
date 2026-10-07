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
			PerksStage.ActionPerk oAJGINIDKJD = activeActions[num];
			ApplyHealthChangeTick(oAJGINIDKJD);
			if (oAJGINIDKJD.DurationFrames > 0)
			{
				if (oAJGINIDKJD.ElapsedFrames >= oAJGINIDKJD.DurationFrames || oAJGINIDKJD.IsExpired)
				{
					ExpireAction(oAJGINIDKJD);
				}
				oAJGINIDKJD.ElapsedFrames++;
			}
			if (count == activeActions.Count)
			{
				num++;
			}
		}
	}

	private void ApplyHealthChangeTick(PerksStage.ActionPerk IBODMPMJELJ)
	{
		if (IBODMPMJELJ.Action.get_Type() == ActionType.ACTION_MOD_HEALTH_CHANGE)
		{
			ApplyHealthChange(IBODMPMJELJ);
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

	private void LogModEvent(PerksStage.ActionPerk IBODMPMJELJ, bool PENNHKHFEOM)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("PERK ----- ");
		stringBuilder.Append((!PENNHKHFEOM) ? "ModDestruction " : "ModStart ");
		stringBuilder.Append("PerkName: {0} ModName: {1} ModXML: {2}");
		if (LogRules.GetInstance().GetLogPerks())
		{
			GameLog.Info(stringBuilder.ToString(), IBODMPMJELJ.Action.GetTrigger().GetPerk().Name, IBODMPMJELJ.Action.get_Name(), IBODMPMJELJ.Action.GetElementName());
		}
	}

	public void ExecuteActions(List<PerksStage.ActionPerk> AFENHJFICNN)
	{
		foreach (PerksStage.ActionPerk item in AFENHJFICNN)
		{
			PerksStage.ActionPerk oAJGINIDKJD = ((!item.Action.GetModificator()) ? item : new PerksStage.ActionPerk(item));
			switch (item.Action.get_Type())
			{
			case ActionType.ACTION_SHOW_ICONS:
				ApplyShowIcon(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_MOD_HEALTH_CHANGE:
				ApplyHealthChangeStart(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_SET_ATTRIBUTES:
				ApplySetAttributes(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_INVISIBILITY:
				ApplyInvisibility(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_DISABLE_INTERVAL:
				ApplyDisableInterval(oAJGINIDKJD);
				break;
			case ActionType.ACTION_SET_HIT:
				ApplySetHit(oAJGINIDKJD);
				break;
			case ActionType.ACTION_LIFE_STEAL:
				ApplyLifeSteal(oAJGINIDKJD);
				break;
			case ActionType.ACTION_ADD_BULLETS:
				ApplyAddBullets(oAJGINIDKJD);
				break;
			case ActionType.ACTION_ADD_MAGIC:
				ApplyAddMagicCharge(oAJGINIDKJD);
				break;
			case ActionType.ACTION_SET_MOD_FRAMES:
				ApplySetModFrames(oAJGINIDKJD);
				break;
			case ActionType.ACTION_MOD_EFFECT:
				ApplySetModEffect(oAJGINIDKJD);
				break;
			case ActionType.ACTION_PROVOKE:
				ApplyProvoke(oAJGINIDKJD);
				break;
			case ActionType.ACTION_SET_TACTICS:
				ApplySetTactics(oAJGINIDKJD);
				break;
			case ActionType.ACTION_CLEAR_ACTION:
				ApplyClearAction(oAJGINIDKJD);
				break;
			case ActionType.ACTION_VARIABLE:
				ApplyVariable(oAJGINIDKJD);
				break;
			case ActionType.ACTION_SET_VARIABLE:
				ApplySetVariable(oAJGINIDKJD);
				break;
			case ActionType.ACTION_SET_COOLDOWN:
				ApplySetCooldown(oAJGINIDKJD);
				break;
			case ActionType.ACTION_CHANGE_IMPULSE:
				ApplyChangeImpulse(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_CHANGE_HIT_EFFECT_SCALE:
				ApplyChangeHitEffectScale(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_CHANGE_ADD_DAMAGE_VALUE:
				ApplyChangeAdditionalDamage(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_CHANGE_MODEL_COLOR:
				ApplyChangeModelColor(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_SLOW_MODEL:
				ApplySlowModel(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_TURN_OFF_COLLISION:
				ApplyTurnOffCollision(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_SWITCH:
				ApplySwitch(oAJGINIDKJD);
				break;
			case ActionType.ACTION_MARK_PERK_USED:
				PerksStage.IncrementPerkUse(Data.PerkInfo.Name);
				break;
			case ActionType.ACTION_PERK_AREA:
				ApplyPerkArea(oAJGINIDKJD, false);
				break;
			case ActionType.ACTION_MOVE_MODEL:
				ApplyMoveModel(oAJGINIDKJD);
				break;
			case ActionType.ACTION_SET_MOVES_VARIABLE:
				ApplySetMovesVariable(oAJGINIDKJD);
				break;
			case ActionType.ACTION_STEAL_MAGIC:
				ApplyStealMagic(oAJGINIDKJD, false);
				break;
			}
			if (item.Action.GetModificator())
			{
				activeActions.Add(oAJGINIDKJD);
				activeActionNames.Add(oAJGINIDKJD.Action.get_Name());
				PerkActionModificator cKCICHAIMFL = (PerkActionModificator)oAJGINIDKJD.Action;
				if (cKCICHAIMFL.GetNamespace() != string.Empty)
				{
					PerksStage.RegisterNamespaceAction(oAJGINIDKJD);
				}
			}
			LogModEvent(item, true);
		}
		AFENHJFICNN.Clear();
		ClearActions();
	}

	private void ApplyShowIcon(PerksStage.ActionPerk IBODMPMJELJ, bool CCBEDPIHKAD)
	{
		if (!CCBEDPIHKAD)
		{
			MarkPerkUsed();
		}
		PerkActionShowIcon fMJDHMBCMKL = (PerkActionShowIcon)IBODMPMJELJ.Action;
		string image = (fMJDHMBCMKL.GetImage() != string.Empty)
			? fMJDHMBCMKL.GetImage()
			: Data.PerkInfo.ImageName;
		IBODMPMJELJ.IconPath = ResolveIconPath(image);
		IBODMPMJELJ.ShowExpiration = fMJDHMBCMKL.GetShowExpiration();
		IBODMPMJELJ.ExpirationVersion = fMJDHMBCMKL.GetExpirationVer();
		IBODMPMJELJ.TargetModel.NotifyPerkAction(IBODMPMJELJ, CCBEDPIHKAD);
	}

	private static string ResolveIconPath(string image)
	{
		// Vanilla images are relative to UI/Skills. Mod API images may already be
		// fully-qualified asset IDs, which must not receive the legacy prefix.
		return (image != null && image.IndexOf(':') > 0)
			? image
			: string.Format("{0}{1}", SF2Paths.GetSkillsUiPath(), image ?? string.Empty);
	}

	private void ApplyHealthChangeStart(PerksStage.ActionPerk IBODMPMJELJ, bool CCBEDPIHKAD)
	{
	}

	private void ApplySetAttributes(PerksStage.ActionPerk IBODMPMJELJ, bool CCBEDPIHKAD)
	{
		int num = ((!CCBEDPIHKAD) ? 1 : (-1));
		PerkActionSetAttributes aHFKENAALLF = (PerkActionSetAttributes)IBODMPMJELJ.Action;
        var applied = CCBEDPIHKAD ? IBODMPMJELJ.AppliedAttributes : null;
        if (applied == null)
        {
            // Resolve every expression before mutation, and retain the normalized
            // deltas so expiry does not reevaluate a changed combat context.
            applied = new Dictionary<string, int>();
            foreach (var item in aHFKENAALLF.GetAttributes())
            {
                var attributes = new Attributes();
                attributes.Set(item.Key, item.Value.Calculate().ToInt());
                int amount = 0;
                attributes.Get(item.Key, ref amount);
                applied.Add(item.Key, amount);
            }
        }
        if (!CCBEDPIHKAD) IBODMPMJELJ.AppliedAttributes = applied;
		foreach (var item in applied)
		{
			string key = item.Key;
			int OEMALIFPGPO = item.Value;
			int OEMALIFPGPO2 = 0;
			IBODMPMJELJ.TargetModel.Parameters.FinalAttributes.Get(key, ref OEMALIFPGPO2, false, true);
			IBODMPMJELJ.TargetModel.Parameters.FinalAttributes.Set(key, OEMALIFPGPO2 + OEMALIFPGPO * num, true);
			if (key == "DamageFactor" && !CCBEDPIHKAD && GetIsOwnerPlayer())
			{
				Model.StrikeResult gHHCDAFIKJE = IBODMPMJELJ.SourceModel.LastStrike;
				gHHCDAFIKJE.AddProcedPerk(Data.PerkInfo.Id);
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

	private void ApplyChangeImpulse(PerksStage.ActionPerk IBODMPMJELJ, bool CCBEDPIHKAD)
	{
		PerkActionChangeImpulse nKPJIECMIJB = (PerkActionChangeImpulse)IBODMPMJELJ.Action;
		float dHDMNHCIPEH = nKPJIECMIJB.GetMultiplierX();
		float bGEEALIPKCC = nKPJIECMIJB.GetMultiplierY();
		float lKPCKJOLJDO = nKPJIECMIJB.GetMultiplierZ();
		if (CCBEDPIHKAD)
		{
			IBODMPMJELJ.TargetModel.ResetImpulseFactor();
		}
		else
		{
			IBODMPMJELJ.TargetModel.SetImpulseFactor(dHDMNHCIPEH, bGEEALIPKCC, lKPCKJOLJDO);
		}
	}

	private void ApplyChangeHitEffectScale(PerksStage.ActionPerk IBODMPMJELJ, bool CCBEDPIHKAD)
	{
		PerkActionChangeHitEffectScale aCEHLJCDLKB = (PerkActionChangeHitEffectScale)IBODMPMJELJ.Action;
		float bAINMLLIKOL = aCEHLJCDLKB.GetHitEffectScale();
		if (CCBEDPIHKAD)
		{
			IBODMPMJELJ.TargetModel.ResetHitEffectScale();
		}
		else
		{
			IBODMPMJELJ.TargetModel.set_HitEffectScale(bAINMLLIKOL);
		}
	}

	private void ApplyChangeAdditionalDamage(PerksStage.ActionPerk IBODMPMJELJ, bool CCBEDPIHKAD)
	{
		PerkActionChangeAdditionalDamageValue dMPBHHGACBP = (PerkActionChangeAdditionalDamageValue)IBODMPMJELJ.Action;
		float bAINMLLIKOL = dMPBHHGACBP.GetAdditionalDamageValue();
		if (CCBEDPIHKAD)
		{
			IBODMPMJELJ.TargetModel.SetAdditionalDamageToOne();
			return;
		}
		IBODMPMJELJ.TargetModel.set_AdditionalDamageValue(bAINMLLIKOL);
		if (GetIsOwnerPlayer())
		{
			Model.StrikeResult gHHCDAFIKJE = IBODMPMJELJ.SourceModel.LastStrike;
			gHHCDAFIKJE.AddProcedPerk(Data.PerkInfo.Id);
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

	private void ApplyInvisibility(PerksStage.ActionPerk IBODMPMJELJ, bool CCBEDPIHKAD)
	{
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		if (gDBOMJODDEA != null)
		{
			gDBOMJODDEA.SetModelVisible(IBODMPMJELJ.TargetModel, CCBEDPIHKAD);
		}
	}

	private void ApplyDisableInterval(PerksStage.ActionPerk IBODMPMJELJ)
	{
		PerkActionDisableInterval dDALHNPFHAO = (PerkActionDisableInterval)IBODMPMJELJ.Action;
		if (dDALHNPFHAO.GetIntervalType() != string.Empty)
		{
			IntervalAnimation.IntervalType lFLGCDNKNJI = IntervalAnimation.ParseIntervalType(dDALHNPFHAO.GetIntervalType());
			IBODMPMJELJ.TargetModel.RemoveInterval(lFLGCDNKNJI);
		}
		else if (dDALHNPFHAO.GetIntervalName() != string.Empty)
		{
			IBODMPMJELJ.TargetModel.RemoveInterval(dDALHNPFHAO.GetIntervalName());
		}
	}

	private void ApplyLifeSteal(PerksStage.ActionPerk IBODMPMJELJ)
	{
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		if (gDBOMJODDEA != null)
		{
			PerkActionLifesteal gGFKBGKDALP = (PerkActionLifesteal)IBODMPMJELJ.Action;
			Model.StrikeResult gHHCDAFIKJE = IBODMPMJELJ.SourceModel.LastStrike;
			float num = (ObscuredFloat)(IBODMPMJELJ.TargetModel.Parameters.GetCurrentLife());
			float aACBFABMADJ = gGFKBGKDALP.GetDamagePart() * gHHCDAFIKJE.FinalDamage * (IBODMPMJELJ.TargetModel.GetCombatTarget().GetPowerMultiplier() / gHHCDAFIKJE.Victim.GetPowerMultiplier());
			gDBOMJODDEA.UpdateLife(IBODMPMJELJ.TargetModel, aACBFABMADJ);
		}
	}

	private void ApplySetHit(PerksStage.ActionPerk IBODMPMJELJ)
	{
		PerkActionSetHit jLKGFCFBJGE = (PerkActionSetHit)IBODMPMJELJ.Action;
		Model bIKLKJMNGKP = IBODMPMJELJ.SourceModel;
		Model.StrikeResult gHHCDAFIKJE = bIKLKJMNGKP.LastStrike;
		bool flag = true;
		if (jLKGFCFBJGE.GetCritical() > -1)
		{
			gHHCDAFIKJE.IsCritical = jLKGFCFBJGE.GetCritical() > 0;
			if (!gHHCDAFIKJE.IsCritical)
			{
				flag = false;
			}
		}
		if (jLKGFCFBJGE.GetShock() > -1)
		{
			gHHCDAFIKJE.IsShock = jLKGFCFBJGE.GetShock() > 0;
		}
		if (jLKGFCFBJGE.GetDisarm() > -1)
		{
			gHHCDAFIKJE.IsDisarm = jLKGFCFBJGE.GetDisarm() > 0;
		}
		if (jLKGFCFBJGE.GetBlock() > -1)
		{
			gHHCDAFIKJE.IsBlocked = jLKGFCFBJGE.GetBlock() > 0;
		}
		if (jLKGFCFBJGE.GetDamage() != null)
		{
			FunctionResult dEIHAOLOPLC = jLKGFCFBJGE.GetDamage().Calculate();
			gHHCDAFIKJE.RawDamage = dEIHAOLOPLC.ToFloat();
			gHHCDAFIKJE.FinalDamage = dEIHAOLOPLC.ToFloat();
		}
		if (GetIsOwnerPlayer() && flag)
		{
			gHHCDAFIKJE.AddProcedPerk(Data.PerkInfo.Id);
		}
	}

	private void ApplySetTactics(PerksStage.ActionPerk IBODMPMJELJ)
	{
		PerkActionSetTactics fBDAHEODOGP = (PerkActionSetTactics)IBODMPMJELJ.Action;
		IBODMPMJELJ.TargetModel.ChangeAiTactic(fBDAHEODOGP.GetTactics());
	}

	private void ApplyAddBullets(PerksStage.ActionPerk IBODMPMJELJ)
	{
		PerkActionAddBullets cLBEGGLEHMB = (PerkActionAddBullets)IBODMPMJELJ.Action;
		FunctionResult dEIHAOLOPLC = cLBEGGLEHMB.GetValue().Calculate();
		int fOIPKLDNGDL = dEIHAOLOPLC.ToInt();
		if (cLBEGGLEHMB.GetBulletType() == "MagicBullet")
		{
			IBODMPMJELJ.TargetModel.AddMagicCharges(fOIPKLDNGDL);
			IBODMPMJELJ.TargetModel.UpdateMagicButton();
		}
	}

	private void ApplyAddMagicCharge(PerksStage.ActionPerk IBODMPMJELJ)
	{
		PerkActionAddMagicCharge aMOILMJLADC = (PerkActionAddMagicCharge)IBODMPMJELJ.Action;
		FunctionResult dEIHAOLOPLC = aMOILMJLADC.GetValue().Calculate();
		float fOIPKLDNGDL = dEIHAOLOPLC.ToFloat();
		IBODMPMJELJ.TargetModel.AddMagicChargeFraction(fOIPKLDNGDL);
		IBODMPMJELJ.TargetModel.UpdateMagicButton();
	}

	private void ApplySetModFrames(PerksStage.ActionPerk IBODMPMJELJ)
	{
		MarkPerkUsed();
		PerkActionSetModFrames iGDDHFCDELM = (PerkActionSetModFrames)IBODMPMJELJ.Action;
		iGDDHFCDELM.GetModFrames().Calculate();
		FunctionResult dEIHAOLOPLC = iGDDHFCDELM.GetModFrames().Calculate();
		int fLNLMIHEDCI = dEIHAOLOPLC.ToInt();
		foreach (PerksStage.ActionPerk item in activeActions)
		{
			if (iGDDHFCDELM.GetModName() == item.Action.get_Name())
			{
				item.DurationFrames = fLNLMIHEDCI;
				item.ElapsedFrames = 0;
			}
		}
		if (iGDDHFCDELM.GetNamespace() == null || !(iGDDHFCDELM.GetNamespace() != string.Empty))
		{
			return;
		}
		List<PerksStage.ActionPerk> list = PerksStage.GetNamespaceActions(iGDDHFCDELM.GetNamespace());
		if (list == null)
		{
			return;
		}
		foreach (PerksStage.ActionPerk item2 in list)
		{
			if (iGDDHFCDELM.GetModName().Equals(item2.Action.get_Name()))
			{
				item2.DurationFrames = fLNLMIHEDCI;
				item2.ElapsedFrames = 0;
			}
		}
	}

	private void ApplySetModEffect(PerksStage.ActionPerk IBODMPMJELJ)
	{
		PerkActionSetModEffect fBLKPCHKAHM = (PerkActionSetModEffect)IBODMPMJELJ.Action;
		string text = fBLKPCHKAHM.GetNamespace();
		if (text != null && text != string.Empty)
		{
			PerksStage.ActionPerk oAJGINIDKJD = PerksStage.FindNamespaceAction(fBLKPCHKAHM.GetModName(), text);
			if (oAJGINIDKJD != null)
			{
				oAJGINIDKJD.TargetModel.NotifyPerkActionReplaced(oAJGINIDKJD, IBODMPMJELJ);
			}
			return;
		}
		foreach (PerksStage.ActionPerk item in activeActions)
		{
			if (fBLKPCHKAHM.GetModName() == item.Action.get_Name())
			{
				item.TargetModel.NotifyPerkActionReplaced(item, IBODMPMJELJ);
			}
		}
	}

	private void ApplyProvoke(PerksStage.ActionPerk IBODMPMJELJ)
	{
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		if (gDBOMJODDEA == null)
		{
			return;
		}
		PerkActionProvoke bLAIFJHNJIO = (PerkActionProvoke)IBODMPMJELJ.Action;
		PerkInfoItem aCONCDFDNJH = bLAIFJHNJIO.GetTrigger().GetPerk();
		InfoPerk bPDFFLADJMJ = gDBOMJODDEA.GetPerksStage().FindInfoPerk(IBODMPMJELJ.TargetModel, aCONCDFDNJH);
		if (bPDFFLADJMJ != null)
		{
			List<string> list = bPDFFLADJMJ.GetActiveActionNames();
		}
		List<PerkTrigger> list2 = new List<PerkTrigger>();
		foreach (PerkTrigger item in aCONCDFDNJH.GetTriggers())
		{
			if (item.get_Name() == bLAIFJHNJIO.GetProvokeTrigger() && item.AreConditionsMet(IBODMPMJELJ.TargetModel, GetActiveActionNames()))
			{
				list2.Add(item);
			}
		}
		foreach (PerkTrigger item2 in list2)
		{
			gDBOMJODDEA.GetPerksStage().ExecuteTriggerActions(IBODMPMJELJ.TargetModel, item2, true);
		}
	}

	private void ApplyClearAction(PerksStage.ActionPerk IBODMPMJELJ)
	{
		PerkActionClearAction hHMDDFCJDEO = (PerkActionClearAction)IBODMPMJELJ.Action;
		foreach (PerksStage.ActionPerk item in activeActions)
		{
			if (hHMDDFCJDEO.GetNameAction() == string.Empty || hHMDDFCJDEO.GetNameAction() == item.Action.get_Name())
			{
				item.IsExpired = true;
			}
		}
		if (hHMDDFCJDEO.GetNamespace() == null || !(hHMDDFCJDEO.GetNamespace() != string.Empty))
		{
			return;
		}
		List<PerksStage.ActionPerk> list = PerksStage.GetNamespaceActions(hHMDDFCJDEO.GetNamespace());
		if (list == null)
		{
			return;
		}
		foreach (PerksStage.ActionPerk item2 in list)
		{
			if (hHMDDFCJDEO.GetNameAction() == null || hHMDDFCJDEO.GetNameAction().Equals(string.Empty) || hHMDDFCJDEO.GetNameAction().Equals(item2.Action.get_Name()))
			{
				item2.IsExpired = true;
			}
		}
	}

	private void ApplyHealthChange(PerksStage.ActionPerk IBODMPMJELJ)
	{
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		if (gDBOMJODDEA != null)
		{
			ModHealthChange eFIMNMBMCIJ = (ModHealthChange)IBODMPMJELJ.Action;
			gDBOMJODDEA.UpdateLife(IBODMPMJELJ.TargetModel, eFIMNMBMCIJ.GetPerFrameValue());
		}
	}

	private void MarkPerkUsed()
	{
		if (GetIsOwnerPlayer())
		{
			Fight gDBOMJODDEA = Fight.GetCurrentFight();
			if (gDBOMJODDEA != null)
			{
				PerksStage.IncrementPerkUse(Data.PerkInfo.Name);
			}
			else
			{
				GameLog.Error("Error: No fight on perk start");
			}
		}
	}

	private void ExpireAction(PerksStage.ActionPerk IBODMPMJELJ)
	{
		IBODMPMJELJ.DurationFrames = 0;
		if (IBODMPMJELJ.Action.GetFrames() != null)
		{
			FunctionResult dEIHAOLOPLC = IBODMPMJELJ.Action.GetFrames().Calculate();
			IBODMPMJELJ.DurationFrames = dEIHAOLOPLC.ToInt();
		}
		switch (IBODMPMJELJ.Action.get_Type())
		{
		case ActionType.ACTION_SHOW_ICONS:
			ApplyShowIcon(IBODMPMJELJ, true);
			break;
		case ActionType.ACTION_SET_ATTRIBUTES:
			ApplySetAttributes(IBODMPMJELJ, true);
			break;
		case ActionType.ACTION_CHANGE_IMPULSE:
			ApplyChangeImpulse(IBODMPMJELJ, true);
			break;
		case ActionType.ACTION_CHANGE_HIT_EFFECT_SCALE:
			ApplyChangeHitEffectScale(IBODMPMJELJ, true);
			break;
		case ActionType.ACTION_CHANGE_ADD_DAMAGE_VALUE:
			ApplyChangeAdditionalDamage(IBODMPMJELJ, true);
			break;
		case ActionType.ACTION_CHANGE_MODEL_COLOR:
			ApplyChangeModelColor(IBODMPMJELJ, true);
			break;
		case ActionType.ACTION_SLOW_MODEL:
			ApplySlowModel(IBODMPMJELJ, true);
			break;
		case ActionType.ACTION_TURN_OFF_COLLISION:
			ApplyTurnOffCollision(IBODMPMJELJ, true);
			break;
		case ActionType.ACTION_PERK_AREA:
			ApplyPerkArea(IBODMPMJELJ, true);
			break;
		case ActionType.ACTION_STEAL_MAGIC:
			ApplyStealMagic(IBODMPMJELJ, true);
			break;
		case ActionType.ACTION_INVISIBILITY:
			ApplyInvisibility(IBODMPMJELJ, true);
			break;
		}
		LogModEvent(IBODMPMJELJ, false);
		bool flag = IBODMPMJELJ.Action.GetModificator();
		string value = IBODMPMJELJ.Action.get_Name();
		Model kJDFJPBIGJC = IBODMPMJELJ.TargetModel;
		RemoveActiveAction(IBODMPMJELJ);
		if (!flag)
		{
			return;
		}
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		if (gDBOMJODDEA != null)
		{
			PerkActionModificator cKCICHAIMFL = (PerkActionModificator)IBODMPMJELJ.Action;
			if (cKCICHAIMFL.GetNamespace() != null && cKCICHAIMFL.GetNamespace() != string.Empty)
			{
				PerksStage.UnregisterNamespaceAction(IBODMPMJELJ);
			}
			string text = (string)gDBOMJODDEA.GetPerksStage().GetPerkMap()["ModExpires"];
			if (text != null)
			{
				GetExpiredModNames().Add(text);
			}
			gDBOMJODDEA.GetPerksStage().GetPerkMap()["ModExpires"] = value;
			gDBOMJODDEA.GetPerksStage().GetPerkMap()["Namespace"] = cKCICHAIMFL.GetNamespace();
			gDBOMJODDEA.GetPerksStage().GetPerkMap()["ParentPerk"] = cKCICHAIMFL.GetPerk();
			gDBOMJODDEA.GetPerksStage().FireEvent(kJDFJPBIGJC, PerkEvent.PerkEventType.EVENT_MOD_EXPIRES, true);
			gDBOMJODDEA.GetPerksStage().AddExpiredAction(IBODMPMJELJ);
		}
	}

	private void RemoveActiveAction(PerksStage.ActionPerk DIMEFLGFIME)
	{
		foreach (PerksStage.ActionPerk item in activeActions)
		{
			if (DIMEFLGFIME == item)
			{
				activeActions.Remove(item);
				activeActionNames.Remove(item.Action.get_Name());
				break;
			}
		}
	}

	private void ApplyVariable(PerksStage.ActionPerk IBODMPMJELJ)
	{
		PerkActionVariable nMCKMGOCCBO = (PerkActionVariable)IBODMPMJELJ.Action;
		string key = nMCKMGOCCBO.get_Name();
		FunctionResult dEIHAOLOPLC = nMCKMGOCCBO.GetValue().Calculate();
		float value;
		if (float.TryParse(dEIHAOLOPLC.Value, out value))
		{
			IBODMPMJELJ.TargetModel.GetConditions().PerkVariables[key] = value;
			IBODMPMJELJ.TargetModel.GetConditions().PerkStringVariables.Remove(key);
		}
		else
		{
			IBODMPMJELJ.TargetModel.GetConditions().PerkStringVariables[key] =
				dEIHAOLOPLC.Value ?? string.Empty;
		}
	}

	private void ApplySetVariable(PerksStage.ActionPerk IBODMPMJELJ)
	{
		PerkActionSetVariable lBGDPLDCKFJ = (PerkActionSetVariable)IBODMPMJELJ.Action;
		string text = lBGDPLDCKFJ.get_Name();
		FunctionResult dEIHAOLOPLC = lBGDPLDCKFJ.GetValue().Calculate();
		float num = dEIHAOLOPLC.ToFloat();
		if (lBGDPLDCKFJ.GetHasMinValue())
		{
			FunctionResult dEIHAOLOPLC2 = lBGDPLDCKFJ.GetMinValue().Calculate();
			float num2 = dEIHAOLOPLC2.ToFloat();
			if (num < num2)
			{
				num = num2;
			}
		}
		if (lBGDPLDCKFJ.GetHasMaxValue())
		{
			FunctionResult dEIHAOLOPLC3 = lBGDPLDCKFJ.GetMaxValue().Calculate();
			float num3 = dEIHAOLOPLC3.ToFloat();
			if (num > num3)
			{
				num = num3;
			}
		}
		IBODMPMJELJ.TargetModel.GetConditions().PerkVariables[text] = num;
		if (SystemProperties.IsDebug())
		{
			GameLog.Info("SetVariable {0} = {1}", text, num);
		}
	}

	private void ApplySetCooldown(PerksStage.ActionPerk IBODMPMJELJ)
	{
		PerkActionSetCooldown bHPLOIHAPFP = (PerkActionSetCooldown)IBODMPMJELJ.Action;
		int num = bHPLOIHAPFP.GetCooldownFrames();
		string bAINMLLIKOL = bHPLOIHAPFP.GetButtonName();
		FightCID dDNBGEJJGMG = (FightCID)MovesMaps.GetMappedIndex(MovesMaps.MapType.KEY_TYPE, bAINMLLIKOL);
		IBODMPMJELJ.TargetModel.ResetButtonCooldown(dDNBGEJJGMG, 0);
		IBODMPMJELJ.TargetModel.StartButtonCooldown(dDNBGEJJGMG, num);
		if (SystemProperties.IsDebug())
		{
			GameLog.Info("SetCooldown button = {0}, frames = {1}", bHPLOIHAPFP.GetButtonName(), num);
		}
	}

	public void ClearActions(bool GIBIGPCELOB = false)
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
			PerksStage.ActionPerk oAJGINIDKJD = activeActions[num];
			if (GIBIGPCELOB || oAJGINIDKJD.IsExpired)
			{
				ExpireAction(oAJGINIDKJD);
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
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		if (gDBOMJODDEA != null)
		{
			gDBOMJODDEA.GetPerksStage().GetPerkMap()["ModExpires"] = null;
		}
	}
}
