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

		public ActionPerk(ActionPerk source)
		{
			IsExpired = source.IsExpired;
			SourceModel = source.SourceModel;
			TargetModel = source.TargetModel;
			Action = source.Action;
			ElapsedFrames = source.ElapsedFrames;
			DurationFrames = source.DurationFrames;
			IconPath = source.IconPath;
			StackKey = source.StackKey;
			ShowExpiration = source.ShowExpiration;
				ExpirationVersion = source.ExpirationVersion;
				EclipseStackCount = source.EclipseStackCount;
			PreviousMagic = source.PreviousMagic;
            AppliedAttributes = source.AppliedAttributes == null ? null :
                new Dictionary<string, int>(source.AppliedAttributes);
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

	public void SetModels(List<Model> models)
	{
		ClearModels();
		foreach (Model item in models)
		{
			AddModel(item);
		}
	}

	public void AddModel(Model model)
	{
		var prepared = PrepareModelRegistration(model);
		RemoveModel(model);
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

	public void RemoveModel(Model model)
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			if (item.get_Model() == model)
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
				bool clearAll = true;
				item2.ClearActions(clearAll);
			}
		}
	}

	public bool FireEvent(Model model, PerkEvent.PerkEventType eventType, bool includePerkMap = false, PerkTrigger trigger = null)
	{
		DispatchEvent(model, eventType);
		object obj = ((!GetPerkMap().ContainsKey("Namespace")) ? null : GetPerkMap()["Namespace"]);
		string eventNamespace = string.Empty;
		if (obj != null)
		{
			eventNamespace = (string)obj;
		}
		foreach (PerkModelStruct item in modelRegistrations)
		{
			if (trigger != null)
			{
				if (model == item.get_Model())
				{
					ProcessTrigger(item, model, null, trigger, true);
				}
				continue;
			}
			List<PerkTrigger> list = item.GetTriggersForEvent(eventType);
			if (list == null)
			{
				continue;
			}
			PerkEvent.EventStruct eventData = new PerkEvent.EventStruct();
			eventData.Type = eventType;
			eventData.Info = ((!includePerkMap) ? null : GetPerkMap());
			eventData.PerkOwnerModel = item.get_Model();
			eventData.EventModel = model;
			eventData.Namespace = eventNamespace;
			foreach (PerkTrigger item2 in list)
			{
				ProcessTrigger(item, model, eventData, item2);
			}
		}
		if (trigger == null)
		{
			Run();
		}
		return true;
	}

	private void ProcessTrigger(PerkModelStruct modelRegistration, Model model, PerkEvent.EventStruct eventData, PerkTrigger trigger, bool forceTrigger = false)
	{
		PerkData perkData = modelRegistration.FindPerkData(trigger.GetPerk());
		if (perkData != null && perkData.Enabled)
		{
			InfoPerk infoPerk = FindInfoPerk(modelRegistration, trigger.GetPerk());
			List<string> activeActionNames = ((infoPerk == null) ? new List<string>() : infoPerk.GetActiveActionNames());
			trigger.GetPerk().SetOwnerModel(modelRegistration.get_Model());
			if ((eventData == null || trigger.MatchesEvent(eventData)) && trigger.AreConditionsMet(modelRegistration.get_Model(), activeActionNames))
			{
				ExecuteTriggerActions(modelRegistration, model, trigger, forceTrigger);
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

	public void CollectActiveActions(Model model, List<ActionPerk> outActions)
	{
		outActions.Clear();
		PerkModelStruct modelRegistration = null;
		InfoPerk infoPerk = null;
		ActionPerk actionPerk = null;
		for (int i = 0; i < modelRegistrations.Count; i++)
		{
			modelRegistration = modelRegistrations[i];
			for (int j = 0; j < modelRegistration.GetInfoPerks().Count; j++)
			{
				infoPerk = modelRegistration.GetInfoPerks()[j];
				for (int k = 0; k < infoPerk.GetActiveActions().Count; k++)
				{
					actionPerk = infoPerk.GetActiveActions()[k];
					if (actionPerk.TargetModel == model)
					{
						outActions.Add(actionPerk);
					}
				}
			}
		}
	}

	public void CollectExpiredActions(Model model, List<ActionPerk> outActions)
	{
		outActions.Clear();
		ActionPerk actionPerk = null;
		for (int i = 0; i < expiredActions.Count; i++)
		{
			actionPerk = expiredActions[i];
			if (actionPerk.TargetModel == model)
			{
				outActions.Add(actionPerk);
			}
		}
	}

	public InfoPerk FindInfoPerk(Model model, PerkInfoItem perkInfo)
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			if (model != item.get_Model())
			{
				continue;
			}
			foreach (InfoPerk item2 in item.GetInfoPerks())
			{
				if (item2.Data.PerkInfo == perkInfo)
				{
					return item2;
				}
			}
		}
		return null;
	}

	public InfoPerk FindInfoPerk(PerkModelStruct modelRegistration, PerkInfoItem perkInfo)
	{
		return FindInfoPerk(modelRegistration.get_Model(), perkInfo);
	}

	public void OnDisarm(object data)
	{
		Model.DisarmData disarmData = (Model.DisarmData)data;
		PerkModelStruct modelRegistration = FindModelRegistration(disarmData.Owner);
		foreach (PerkInfoItem item in disarmData.LostPerks)
		{
			foreach (InfoPerk item2 in modelRegistration.GetInfoPerks())
			{
				if (item2.Data.PerkInfo == item)
				{
					item2.ClearActions(true);
				}
			}
			modelRegistration.SetPerkEnabled(item, false);
		}
	}

	public void EnableAllPerks()
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			item.GetPerkDataList().ForEach((PerkData perkData) =>
			{
				perkData.Enabled = true;
			});
		}
	}

	public void ResetInfoPerks()
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			item.GetInfoPerks().ForEach((InfoPerk infoPerk) =>
			{
				infoPerk.ResetExpiredMods();
			});
		}
	}

	public static void RegisterNamespaceAction(ActionPerk actionPerk)
	{
		if (actionPerk.Action.GetModificator())
		{
			PerkActionModificator modificator = (PerkActionModificator)actionPerk.Action;
			if (modificator != null && !string.IsNullOrEmpty(modificator.GetNamespace()))
			{
				if (!actionsByNamespace.ContainsKey(modificator.GetNamespace()))
					actionsByNamespace.Add(modificator.GetNamespace(), new List<ActionPerk>());
				List<ActionPerk> namespaceActions = actionsByNamespace[modificator.GetNamespace()];
				namespaceActions.AddIfNotExist(actionPerk);
			}
		}
	}

	public static void UnregisterNamespaceAction(ActionPerk actionPerk)
	{
		if (actionPerk.Action.GetModificator())
		{
			PerkActionModificator modificator = (PerkActionModificator)actionPerk.Action;
			if (modificator != null && actionsByNamespace.ContainsKey(modificator.GetNamespace()))
			{
				List<ActionPerk> list = actionsByNamespace[modificator.GetNamespace()];
				list.Remove(actionPerk);
			}
		}
	}

	public static void ClearNamespaceActions()
	{
		actionsByNamespace.Clear();
	}

	public static bool CheckModNameInNamespace(string modName, string namespaceName)
	{
		if (actionsByNamespace.ContainsKey(namespaceName))
		{
			List<ActionPerk> list = actionsByNamespace[namespaceName];
			foreach (ActionPerk item in list)
			{
				if (item.Action.get_Name().Equals(modName))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static ActionPerk FindNamespaceAction(string modName, string namespaceName)
	{
		if (!actionsByNamespace.ContainsKey(namespaceName))
		{
			return null;
		}
		List<ActionPerk> list = actionsByNamespace[namespaceName];
		foreach (ActionPerk item in list)
		{
			if (item.Action.get_Name().Equals(modName))
			{
				return item;
			}
		}
		return null;
	}

	public static List<ActionPerk> GetNamespaceActions(string namespaceName)
	{
		return (!actionsByNamespace.ContainsKey(namespaceName)) ? null : actionsByNamespace[namespaceName];
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

	public void ExecuteTriggerActions(Model model, PerkTrigger trigger, bool forceTrigger = false)
	{
		foreach (PerkModelStruct item in modelRegistrations)
		{
			if (model == item.get_Model())
			{
				ExecuteTriggerActions(item, model, trigger, forceTrigger);
				break;
			}
		}
	}

	public void ExecuteTriggerActions(PerkModelStruct modelRegistration, Model model, PerkTrigger trigger, bool forceTrigger = false)
	{
		InfoPerk infoPerk = FindInfoPerk(modelRegistration, trigger.GetPerk());
		if (infoPerk == null)
		{
			infoPerk = new InfoPerk();
			infoPerk.Data = new PerkData(trigger.GetPerk());
			modelRegistration.GetInfoPerks().Add(infoPerk);
		}
		List<ActionPerk> list = new List<ActionPerk>();
		List<PerkAction> list2 = trigger.GetActions();
		foreach (PerkAction item in list2)
		{
			ActionPerk actionPerk = new ActionPerk();
			actionPerk.TargetModel = item.ResolveTargetModel(modelRegistration.get_Model());
			actionPerk.SourceModel = model;
			actionPerk.Action = item;
			actionPerk.IsExpired = false;
			actionPerk.ElapsedFrames = 0;
			actionPerk.DurationFrames = 0;
			if (item.GetFrames() != null)
			{
				FunctionResult durationResult = item.GetFrames().Calculate();
				actionPerk.DurationFrames = durationResult.ToInt();
			}
			if (forceTrigger)
			{
				list.Add(actionPerk);
			}
			else
			{
				infoPerk.GetPendingActions().Add(actionPerk);
			}
		}
		if (list.Count > 0)
		{
			infoPerk.ExecuteActions(list);
		}
	}

	public void RegisterPerkTriggers(PerkModelStruct modelRegistration, PerkInfoItem perkInfo)
	{
		if (perkInfo != null)
		{
			perkInfo.CollectTriggersForEvent(modelRegistration.GetComboTriggers(), PerkEvent.PerkEventType.EVENT_COMBO);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetEveryFrameTriggers(), PerkEvent.PerkEventType.EVENT_EVERY_FRAME);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetHitPreCritTriggers(), PerkEvent.PerkEventType.EVENT_HIT_PRECRIT);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetHitPostCritTriggers(), PerkEvent.PerkEventType.EVENT_HIT_POSTCRIT);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetPostHitTriggers(), PerkEvent.PerkEventType.EVENT_POST_HIT);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetMagicChargedTriggers(), PerkEvent.PerkEventType.EVENT_MAGIC_CHARGED);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetRoundStageStartTriggers(), PerkEvent.PerkEventType.EVENT_ROUND_STAGE_START);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetStyleTriggers(), PerkEvent.PerkEventType.EVENT_STYLE);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetAnimationStartTriggers(), PerkEvent.PerkEventType.EVENT_ANIMATION_START);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetAnimationEndTriggers(), PerkEvent.PerkEventType.EVENT_ANIMATION_END);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetModExpiresTriggers(), PerkEvent.PerkEventType.EVENT_MOD_EXPIRES);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetAreaEnterTriggers(), PerkEvent.PerkEventType.EVENT_AREA_ENTER);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetAreaExitTriggers(), PerkEvent.PerkEventType.EVENT_AREA_EXIT);
			perkInfo.CollectTriggersForEvent(modelRegistration.GetIntervalEndTriggers(), PerkEvent.PerkEventType.EVENT_INTERVAL_END);
			PerkData item = new PerkData(perkInfo);
			modelRegistration.GetPerkDataList().Add(item);
		}
	}

	public void AddExpiredAction(ActionPerk actionPerk)
	{
		expiredActions.Add(actionPerk);
	}

	private void ClearExpiredActions()
	{
		expiredActions.Clear();
	}

	private PerkModelStruct FindModelRegistration(Model model)
	{
		return modelRegistrations.Find((PerkModelStruct registration) => registration.get_Model() == model);
	}

	private void DispatchEvent(Model model, PerkEvent.PerkEventType eventType)
	{
		PerkEventStruct eventData = new PerkEventStruct();
		eventData.Model = model;
		eventData.EventType = eventType;
		if (eventType == PerkEvent.PerkEventType.EVENT_MOD_EXPIRES)
		{
			eventData.Data = ((!GetPerkMap().ContainsKey("ModExpires")) ? null : GetPerkMap()["ModExpires"]);
			CallEvent((int)eventType, eventData);
		}
	}
}
