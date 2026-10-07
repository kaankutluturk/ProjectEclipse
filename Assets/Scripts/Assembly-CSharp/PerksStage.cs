using System.Collections.Generic;

public partial class PerksStage : global::EventDispatcher<PerksStage.PerkEventStruct>
{
	public enum PerkParentType
	{
		PERK_PARENT_NONE = 0,
		PERK_PARENT_ITEM = 1,
		PERK_PARENT_RULE = 2,
		PERK_PARENT_MODEL = 3
	}

	public class ActionPerk
	{
		public bool IsExpired;

		public Model SourceModel;

		public Model TargetModel;

		public PerkAction Action;

		public int ElapsedFrames;

		public int DurationFrames;

		public string IconPath = string.Empty;

		public string StackKey = string.Empty;

		public bool ShowExpiration;

			public int ExpirationVersion;

			// Eclipse-authored status icons use this because the recovered ApplyModEffect
			// parser omitted DE's XML StackCount handling.
			public int EclipseStackCount;

		public ItemInfo PreviousMagic;
        public Dictionary<string, int> AppliedAttributes;

		public string PerkName
		{
			get
			{
				return GetPerkName();
			}
		}

		public string ModName
		{
			get
			{
				return GetModName();
			}
		}

		public ActionPerk()
		{
		}

		public ActionPerk(ActionPerk IBODMPMJELJ)
		{
			IsExpired = IBODMPMJELJ.IsExpired;
			SourceModel = IBODMPMJELJ.SourceModel;
			TargetModel = IBODMPMJELJ.TargetModel;
			Action = IBODMPMJELJ.Action;
			ElapsedFrames = IBODMPMJELJ.ElapsedFrames;
			DurationFrames = IBODMPMJELJ.DurationFrames;
			IconPath = IBODMPMJELJ.IconPath;
			StackKey = IBODMPMJELJ.StackKey;
			ShowExpiration = IBODMPMJELJ.ShowExpiration;
				ExpirationVersion = IBODMPMJELJ.ExpirationVersion;
				EclipseStackCount = IBODMPMJELJ.EclipseStackCount;
			PreviousMagic = IBODMPMJELJ.PreviousMagic;
            AppliedAttributes = IBODMPMJELJ.AppliedAttributes == null ? null :
                new Dictionary<string, int>(IBODMPMJELJ.AppliedAttributes);
		}

		public string GetPerkName()
		{
			return Action.GetPerk().Name;
		}

		public string GetModName()
		{
			return Action.get_Name();
		}
	}

	public class PerkEventStruct
	{
		public Model Model;

		public object Data;

		public PerkEvent.PerkEventType EventType;
	}

	private Dictionary<string, object> _PerkMap = new Dictionary<string, object>();

	private List<PerkModelStruct> modelRegistrations = new List<PerkModelStruct>();

	private List<ActionPerk> expiredActions = new List<ActionPerk>();

	private static Dictionary<string, List<ActionPerk>> actionsByNamespace = new Dictionary<string, List<ActionPerk>>();

	private static Dictionary<string, int> PerkUsesLeft = new Dictionary<string, int>();

	public Dictionary<string, object> PerkMap
	{
		get
		{
			return GetPerkMap();
		}
	}

	public Dictionary<string, object> GetPerkMap()
	{
		return _PerkMap;
	}

	public void Run()
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			foreach (InfoPerk item2 in item.GetInfoPerks())
			{
				item2.Run();
			}
		}
	}

	public void SetModels(List<Model> INNLAFHKJNI)
	{
		ClearModels();
		foreach (Model item in INNLAFHKJNI)
		{
			AddModel(item);
		}
	}

	public void AddModel(Model ACENLMONNPA)
	{
		var prepared = PrepareModelRegistration(ACENLMONNPA);
		RemoveModel(ACENLMONNPA);
		modelRegistrations.Add(prepared);
	}

    internal System.Action ReplaceFormRegistration(Model expected, Model replacement)
    {
        if (expected == null || replacement == null || expected == replacement)
            throw new System.ArgumentException("Form perk registration requires distinct models.");
        int index = modelRegistrations.FindIndex(value => value.get_Model() == expected);
        if (index < 0 || modelRegistrations.Exists(value => value.get_Model() == replacement))
            throw new System.InvalidOperationException("Form perk registration identity is stale.");
        var original = modelRegistrations[index];
        var prepared = PrepareModelRegistration(replacement);
        // Existing effect containers own timers, queued work and expiry history.
        // New trigger tables come from the replacement's loadout, while effects
        // already started continue through those same containers exactly once.
        prepared.GetInfoPerks().AddRange(original.GetInfoPerks());
        foreach (var data in prepared.GetPerkDataList())
        {
            var previous = original.GetPerkDataList().Find(value => value.PerkInfo == data.PerkInfo);
            if (previous != null) data.Enabled = previous.Enabled;
        }
        modelRegistrations[index] = prepared;
        return () => modelRegistrations[index] = original;
    }

    internal System.Action TransferFormEffects(Model expected, Model replacement)
    {
        var undo = new List<System.Action>();
        var seen = new HashSet<ActionPerk>();
        System.Action restore = () =>
        {
            for (int index = undo.Count - 1; index >= 0; index--) undo[index]();
        };
        try
        {
            undo.Add(replacement.CopyFormModifiersFrom(expected));
            foreach (var registration in modelRegistrations)
                foreach (var perk in registration.GetInfoPerks())
                    foreach (var action in perk.GetActiveActions())
                    {
                        if (!seen.Add(action)) continue;
                        if (action.TargetModel == expected && action.Action is PerkActionSetAttributes)
                            undo.Add(perk.TransferAttributeEffect(action, expected, replacement));
                        else if (action.Action is ModHealthChange &&
                            (action.TargetModel == expected || action.SourceModel == expected))
                            undo.Add(perk.TransferHealthEffect(action, expected, replacement));
                        else if (action.TargetModel == expected && IsFormBodyModifier(action.Action.get_Type()))
                        {
                            var source = action.SourceModel;
                            action.TargetModel = replacement;
                            if (source == expected) action.SourceModel = replacement;
                            undo.Add(() => { action.TargetModel = expected; action.SourceModel = source; });
                        }
                        else if (action.SourceModel == expected && action.TargetModel != expected)
                        {
                            // Source-only references carry attribution, not an
                            // applied modification on the retiring body.
                            action.SourceModel = replacement;
                            undo.Add(() => action.SourceModel = expected);
                        }
                    }
            var history = new HashSet<ActionPerk>(expiredActions);
            foreach (var action in history)
            {
                // CLBPEANCNOA records expired actions here. Live aliases still
                // take the effect-specific path. Namespace-only records are not
                // proof of expiry and remain subject to the retirement gate.
                if (action == null || seen.Contains(action)) continue;
                var target = action.TargetModel;
                var source = action.SourceModel;
                if (target != expected && source != expected) continue;
                undo.Add(() => { action.TargetModel = target; action.SourceModel = source; });
                if (target == expected) action.TargetModel = replacement;
                if (source == expected) action.SourceModel = replacement;
            }
        }
        catch { restore(); throw; }
        return restore;
    }

    private static bool IsFormBodyModifier(ActionType type)
    {
        switch (type)
        {
            case ActionType.ACTION_CHANGE_IMPULSE:
            case ActionType.ACTION_CHANGE_HIT_EFFECT_SCALE:
            case ActionType.ACTION_CHANGE_ADD_DAMAGE_VALUE:
            case ActionType.ACTION_CHANGE_MODEL_COLOR:
            case ActionType.ACTION_SLOW_MODEL:
            case ActionType.ACTION_TURN_OFF_COLLISION:
            // Flags live in the existing action/name/namespace records. Retain
            // their timer and pending removal while moving the expiry target.
            case ActionType.ACTION_FLAG:
            // The applied variable dictionaries move with combat-state ownership.
            // Keep this record without evaluating or writing its value again.
            case ActionType.ACTION_VARIABLE:
            // These presentations belong to the fight/side; keep their existing
            // objects and timers. Expiry receives the replacement participant.
            case ActionType.ACTION_SHOW_ICONS:
            case ActionType.ACTION_PERK_AREA:
            case ActionType.ACTION_INVISIBILITY:
                return true;
            default: return false;
        }
    }

    internal void RequireFormReferencesTransferred(ISet<Model> retired)
    {
        var actions = new HashSet<ActionPerk>(expiredActions);
        foreach (var values in actionsByNamespace.Values) actions.UnionWith(values);
        foreach (var registration in modelRegistrations)
            foreach (var perk in registration.GetInfoPerks())
            {
                actions.UnionWith(perk.GetPendingActions());
                actions.UnionWith(perk.GetActiveActions());
            }
        foreach (var action in actions)
            if (action != null && (retired.Contains(action.TargetModel) || retired.Contains(action.SourceModel)))
                throw new System.InvalidOperationException("Form still owns an untransferred perk action: " +
                    (action.Action == null ? "unknown" : action.Action.get_Type().ToString()));
    }

    // Only pending actions are safe to retarget without transferring effects that
    // have already modified a body. Containers and timing fields stay unchanged.
    internal System.Action RebindQueuedFormActions(Model expected, Model replacement)
    {
        if (expected == null || replacement == null || expected == replacement)
            throw new System.ArgumentException("Queued perk rebinding requires distinct models.");
        var pending = new HashSet<ActionPerk>();
        var active = new HashSet<ActionPerk>(expiredActions);
        foreach (var registration in modelRegistrations)
            foreach (var perk in registration.GetInfoPerks())
            {
                pending.UnionWith(perk.GetPendingActions());
                active.UnionWith(perk.GetActiveActions());
            }
        foreach (var actions in actionsByNamespace.Values) active.UnionWith(actions);
        var restore = new List<System.Action>();
        foreach (var action in pending)
        {
            if (action == null || active.Contains(action)) continue;
            var target = action.TargetModel;
            var source = action.SourceModel;
            if (target != expected && source != expected) continue;
            restore.Add(() => { action.TargetModel = target; action.SourceModel = source; });
        }
        // Capture every reference before mutation; no action execution is involved.
        foreach (var action in pending)
        {
            if (action == null || active.Contains(action)) continue;
            if (action.TargetModel == expected) action.TargetModel = replacement;
            if (action.SourceModel == expected) action.SourceModel = replacement;
        }
        return () => { foreach (var undo in restore) undo(); };
    }

    // Build trigger tables without exposing a partially prepared registration to
    // combat dispatch. Preparation does not run perks or transfer active effects.
    internal PerkModelStruct PrepareModelRegistration(Model model)
    {
        if (model == null) throw new System.ArgumentNullException(nameof(model));
        var prepared = new PerkModelStruct();
        prepared.set_Model(model);
        foreach (var perk in model.Parameters.Perks)
            RegisterPerkTriggers(prepared, perk);
        return prepared;
    }

	public void RemoveModel(Model ACENLMONNPA)
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			if (item.get_Model() == ACENLMONNPA)
			{
				modelRegistrations.Remove(item);
				break;
			}
		}
	}

	public void ClearModels()
	{
		modelRegistrations.Clear();
	}

	public void Reset()
	{
		PerkUsesLeft.Clear();
		foreach (PerkModelStruct item in modelRegistrations)
		{
			foreach (InfoPerk item2 in item.GetInfoPerks())
			{
				bool gIBIGPCELOB = true;
				item2.ClearActions(gIBIGPCELOB);
			}
		}
	}

	public bool FireEvent(Model FAJBDBKEHJL, PerkEvent.PerkEventType LFLGCDNKNJI, bool GMFCKPBJNLC = false, PerkTrigger CPBHKJFPFJB = null)
	{
		DispatchEvent(FAJBDBKEHJL, LFLGCDNKNJI);
		object obj = ((!GetPerkMap().ContainsKey("Namespace")) ? null : GetPerkMap()["Namespace"]);
		string fILIJOFBNMA = string.Empty;
		if (obj != null)
		{
			fILIJOFBNMA = (string)obj;
		}
		foreach (PerkModelStruct item in modelRegistrations)
		{
			if (CPBHKJFPFJB != null)
			{
				if (FAJBDBKEHJL == item.get_Model())
				{
					ProcessTrigger(item, FAJBDBKEHJL, null, CPBHKJFPFJB, true);
				}
				continue;
			}
			List<PerkTrigger> list = item.GetTriggersForEvent(LFLGCDNKNJI);
			if (list == null)
			{
				continue;
			}
			PerkEvent.EventStruct pJEJIOPNBIJ = new PerkEvent.EventStruct();
			pJEJIOPNBIJ.Type = LFLGCDNKNJI;
			pJEJIOPNBIJ.Info = ((!GMFCKPBJNLC) ? null : GetPerkMap());
			pJEJIOPNBIJ.PerkOwnerModel = item.get_Model();
			pJEJIOPNBIJ.EventModel = FAJBDBKEHJL;
			pJEJIOPNBIJ.Namespace = fILIJOFBNMA;
			foreach (PerkTrigger item2 in list)
			{
				ProcessTrigger(item, FAJBDBKEHJL, pJEJIOPNBIJ, item2);
			}
		}
		if (CPBHKJFPFJB == null)
		{
			Run();
		}
		return true;
	}

	private void ProcessTrigger(PerkModelStruct MAEPLNACFKD, Model FAJBDBKEHJL, PerkEvent.EventStruct EJMEALJNNIL, PerkTrigger CPBHKJFPFJB, bool CAPNMPNNBHF = false)
	{
		PerkData mFKICNALNFB = MAEPLNACFKD.FindPerkData(CPBHKJFPFJB.GetPerk());
		if (mFKICNALNFB != null && mFKICNALNFB.Enabled)
		{
			InfoPerk bPDFFLADJMJ = FindInfoPerk(MAEPLNACFKD, CPBHKJFPFJB.GetPerk());
			List<string> nIKHAICFGNM = ((bPDFFLADJMJ == null) ? new List<string>() : bPDFFLADJMJ.GetActiveActionNames());
			CPBHKJFPFJB.GetPerk().SetOwnerModel(MAEPLNACFKD.get_Model());
			if ((EJMEALJNNIL == null || CPBHKJFPFJB.MatchesEvent(EJMEALJNNIL)) && CPBHKJFPFJB.AreConditionsMet(MAEPLNACFKD.get_Model(), nIKHAICFGNM))
			{
				ExecuteTriggerActions(MAEPLNACFKD, FAJBDBKEHJL, CPBHKJFPFJB, CAPNMPNNBHF);
			}
		}
	}

	public void Render()
	{
		ClearExpiredActions();
		foreach (PerkModelStruct item in modelRegistrations)
		{
			foreach (InfoPerk item2 in item.GetInfoPerks())
			{
				item2.Render();
			}
		}
	}

	public void CollectActiveActions(Model ACENLMONNPA, List<ActionPerk> FFFLNOBCBGL)
	{
		FFFLNOBCBGL.Clear();
		PerkModelStruct iAIBLEELGNK = null;
		InfoPerk bPDFFLADJMJ = null;
		ActionPerk oAJGINIDKJD = null;
		for (int i = 0; i < modelRegistrations.Count; i++)
		{
			iAIBLEELGNK = modelRegistrations[i];
			for (int j = 0; j < iAIBLEELGNK.GetInfoPerks().Count; j++)
			{
				bPDFFLADJMJ = iAIBLEELGNK.GetInfoPerks()[j];
				for (int k = 0; k < bPDFFLADJMJ.GetActiveActions().Count; k++)
				{
					oAJGINIDKJD = bPDFFLADJMJ.GetActiveActions()[k];
					if (oAJGINIDKJD.TargetModel == ACENLMONNPA)
					{
						FFFLNOBCBGL.Add(oAJGINIDKJD);
					}
				}
			}
		}
	}

	public void CollectExpiredActions(Model ACENLMONNPA, List<ActionPerk> FFFLNOBCBGL)
	{
		FFFLNOBCBGL.Clear();
		ActionPerk oAJGINIDKJD = null;
		for (int i = 0; i < expiredActions.Count; i++)
		{
			oAJGINIDKJD = expiredActions[i];
			if (oAJGINIDKJD.TargetModel == ACENLMONNPA)
			{
				FFFLNOBCBGL.Add(oAJGINIDKJD);
			}
		}
	}

	public InfoPerk FindInfoPerk(Model ACENLMONNPA, PerkInfoItem AEFFHJGMNFI)
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			if (ACENLMONNPA != item.get_Model())
			{
				continue;
			}
			foreach (InfoPerk item2 in item.GetInfoPerks())
			{
				if (item2.Data.PerkInfo == AEFFHJGMNFI)
				{
					return item2;
				}
			}
		}
		return null;
	}

	public InfoPerk FindInfoPerk(PerkModelStruct ACENLMONNPA, PerkInfoItem AEFFHJGMNFI)
	{
		return FindInfoPerk(ACENLMONNPA.get_Model(), AEFFHJGMNFI);
	}

	public void OnDisarm(object data)
	{
		Model.DisarmData aADFODEJPHG = (Model.DisarmData)data;
		PerkModelStruct iAIBLEELGNK = FindModelRegistration(aADFODEJPHG.Owner);
		foreach (PerkInfoItem item in aADFODEJPHG.LostPerks)
		{
			foreach (InfoPerk item2 in iAIBLEELGNK.GetInfoPerks())
			{
				if (item2.Data.PerkInfo == item)
				{
					item2.ClearActions(true);
				}
			}
			iAIBLEELGNK.SetPerkEnabled(item, false);
		}
	}

	public void EnableAllPerks()
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			item.GetPerkDataList().ForEach((PerkData DHDMNHCIPEH) =>
			{
				DHDMNHCIPEH.Enabled = true;
			});
		}
	}

	public void ResetInfoPerks()
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			item.GetInfoPerks().ForEach((InfoPerk DHDMNHCIPEH) =>
			{
				DHDMNHCIPEH.ResetExpiredMods();
			});
		}
	}

	public static void RegisterNamespaceAction(ActionPerk IBODMPMJELJ)
	{
		if (IBODMPMJELJ.Action.GetModificator())
		{
			PerkActionModificator cKCICHAIMFL = (PerkActionModificator)IBODMPMJELJ.Action;
			if (cKCICHAIMFL != null && !string.IsNullOrEmpty(cKCICHAIMFL.GetNamespace()))
			{
				if (!actionsByNamespace.ContainsKey(cKCICHAIMFL.GetNamespace()))
					actionsByNamespace.Add(cKCICHAIMFL.GetNamespace(), new List<ActionPerk>());
				List<ActionPerk> oMKIGJOLJJE = actionsByNamespace[cKCICHAIMFL.GetNamespace()];
				oMKIGJOLJJE.AddIfNotExist(IBODMPMJELJ);
			}
		}
	}

	public static void UnregisterNamespaceAction(ActionPerk IBODMPMJELJ)
	{
		if (IBODMPMJELJ.Action.GetModificator())
		{
			PerkActionModificator cKCICHAIMFL = (PerkActionModificator)IBODMPMJELJ.Action;
			if (cKCICHAIMFL != null && actionsByNamespace.ContainsKey(cKCICHAIMFL.GetNamespace()))
			{
				List<ActionPerk> list = actionsByNamespace[cKCICHAIMFL.GetNamespace()];
				list.Remove(IBODMPMJELJ);
			}
		}
	}

	public static void ClearNamespaceActions()
	{
		actionsByNamespace.Clear();
	}

	public static bool CheckModNameInNamespace(string GBHAIILPKFC, string PJPJIBOAFKF)
	{
		if (actionsByNamespace.ContainsKey(PJPJIBOAFKF))
		{
			List<ActionPerk> list = actionsByNamespace[PJPJIBOAFKF];
			foreach (ActionPerk item in list)
			{
				if (item.Action.get_Name().Equals(GBHAIILPKFC))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static ActionPerk FindNamespaceAction(string GBHAIILPKFC, string PJPJIBOAFKF)
	{
		if (!actionsByNamespace.ContainsKey(PJPJIBOAFKF))
		{
			return null;
		}
		List<ActionPerk> list = actionsByNamespace[PJPJIBOAFKF];
		foreach (ActionPerk item in list)
		{
			if (item.Action.get_Name().Equals(GBHAIILPKFC))
			{
				return item;
			}
		}
		return null;
	}

	public static List<ActionPerk> GetNamespaceActions(string PJPJIBOAFKF)
	{
		return (!actionsByNamespace.ContainsKey(PJPJIBOAFKF)) ? null : actionsByNamespace[PJPJIBOAFKF];
	}

	public static void IncrementPerkUse(string name)
	{
		if (string.IsNullOrEmpty(name))
			return;
		int used;
		PerkUsesLeft.TryGetValue(name, out used);
		PerkUsesLeft[name] = used + 1;
	}

	public static bool CanUsePerk(string name)
	{
		// Special Edition shipped this live-service usage hook stubbed to false.
		// Modern perk XML puts <PerkStart/> on normal enchantment procs, so the
		// stub disabled virtually every migrated enchantment.  There is no local
		// per-fight usage cap in this edition; cooldown/mod conditions in the XML
		// are authoritative.  Keep the counter for diagnostics/statistics only.
		return true;
	}

	public void ExecuteTriggerActions(Model FAJBDBKEHJL, PerkTrigger CPBHKJFPFJB, bool CAPNMPNNBHF = false)
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			if (FAJBDBKEHJL == item.get_Model())
			{
				ExecuteTriggerActions(item, FAJBDBKEHJL, CPBHKJFPFJB, CAPNMPNNBHF);
				break;
			}
		}
	}

	public void ExecuteTriggerActions(PerkModelStruct MAEPLNACFKD, Model FAJBDBKEHJL, PerkTrigger CPBHKJFPFJB, bool CAPNMPNNBHF = false)
	{
		InfoPerk bPDFFLADJMJ = FindInfoPerk(MAEPLNACFKD, CPBHKJFPFJB.GetPerk());
		if (bPDFFLADJMJ == null)
		{
			bPDFFLADJMJ = new InfoPerk();
			bPDFFLADJMJ.Data = new PerkData(CPBHKJFPFJB.GetPerk());
			MAEPLNACFKD.GetInfoPerks().Add(bPDFFLADJMJ);
		}
		List<ActionPerk> list = new List<ActionPerk>();
		List<PerkAction> list2 = CPBHKJFPFJB.GetActions();
		foreach (PerkAction item in list2)
		{
			ActionPerk oAJGINIDKJD = new ActionPerk();
			oAJGINIDKJD.TargetModel = item.ResolveTargetModel(MAEPLNACFKD.get_Model());
			oAJGINIDKJD.SourceModel = FAJBDBKEHJL;
			oAJGINIDKJD.Action = item;
			oAJGINIDKJD.IsExpired = false;
			oAJGINIDKJD.ElapsedFrames = 0;
			oAJGINIDKJD.DurationFrames = 0;
			if (item.GetFrames() != null)
			{
				FunctionResult dEIHAOLOPLC = item.GetFrames().Calculate();
				oAJGINIDKJD.DurationFrames = dEIHAOLOPLC.ToInt();
			}
			if (CAPNMPNNBHF)
			{
				list.Add(oAJGINIDKJD);
			}
			else
			{
				bPDFFLADJMJ.GetPendingActions().Add(oAJGINIDKJD);
			}
		}
		if (list.Count > 0)
		{
			bPDFFLADJMJ.ExecuteActions(list);
		}
	}

	public void RegisterPerkTriggers(PerkModelStruct ACENLMONNPA, PerkInfoItem AEFFHJGMNFI)
	{
		if (AEFFHJGMNFI != null)
		{
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetComboTriggers(), PerkEvent.PerkEventType.EVENT_COMBO);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetEveryFrameTriggers(), PerkEvent.PerkEventType.EVENT_EVERY_FRAME);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetHitPreCritTriggers(), PerkEvent.PerkEventType.EVENT_HIT_PRECRIT);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetHitPostCritTriggers(), PerkEvent.PerkEventType.EVENT_HIT_POSTCRIT);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetPostHitTriggers(), PerkEvent.PerkEventType.EVENT_POST_HIT);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetMagicChargedTriggers(), PerkEvent.PerkEventType.EVENT_MAGIC_CHARGED);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetRoundStageStartTriggers(), PerkEvent.PerkEventType.EVENT_ROUND_STAGE_START);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetStyleTriggers(), PerkEvent.PerkEventType.EVENT_STYLE);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetAnimationStartTriggers(), PerkEvent.PerkEventType.EVENT_ANIMATION_START);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetAnimationEndTriggers(), PerkEvent.PerkEventType.EVENT_ANIMATION_END);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetModExpiresTriggers(), PerkEvent.PerkEventType.EVENT_MOD_EXPIRES);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetAreaEnterTriggers(), PerkEvent.PerkEventType.EVENT_AREA_ENTER);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetAreaExitTriggers(), PerkEvent.PerkEventType.EVENT_AREA_EXIT);
			AEFFHJGMNFI.CollectTriggersForEvent(ACENLMONNPA.GetIntervalEndTriggers(), PerkEvent.PerkEventType.EVENT_INTERVAL_END);
			PerkData item = new PerkData(AEFFHJGMNFI);
			ACENLMONNPA.GetPerkDataList().Add(item);
		}
	}

	public void AddExpiredAction(ActionPerk DIMEFLGFIME)
	{
		expiredActions.Add(DIMEFLGFIME);
	}

	private void ClearExpiredActions()
	{
		expiredActions.Clear();
	}

	private PerkModelStruct FindModelRegistration(Model ACENLMONNPA)
	{
		return modelRegistrations.Find((PerkModelStruct DHDMNHCIPEH) => DHDMNHCIPEH.get_Model() == ACENLMONNPA);
	}

	private void DispatchEvent(Model ACENLMONNPA, PerkEvent.PerkEventType LFLGCDNKNJI)
	{
		PerkEventStruct nFFNFAAPEPF = new PerkEventStruct();
		nFFNFAAPEPF.Model = ACENLMONNPA;
		nFFNFAAPEPF.EventType = LFLGCDNKNJI;
		if (LFLGCDNKNJI == PerkEvent.PerkEventType.EVENT_MOD_EXPIRES)
		{
			nFFNFAAPEPF.Data = ((!GetPerkMap().ContainsKey("ModExpires")) ? null : GetPerkMap()["ModExpires"]);
			CallEvent((int)LFLGCDNKNJI, nFFNFAAPEPF);
		}
	}
}
