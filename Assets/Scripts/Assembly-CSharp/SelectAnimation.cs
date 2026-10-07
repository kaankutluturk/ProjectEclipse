using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class SelectAnimation
{
	private class TriggerStruct
	{
		public Trigger SourceTrigger;

		public Model OwnerModel;

		public TriggerStruct(Trigger sourceTrigger, Model ownerModel)
		{
			SourceTrigger = sourceTrigger;
			OwnerModel = ownerModel;
		}
	}

	public class SelectInfo
	{
		public InfoAnimation Animation;

		public int FacingSign;

		public bool IsHit;

		public bool IsRandom;

		public int Index;

		public EventAnimation.EventAnimationType EventType;

		public EventAnimation MatchedEvent;

		public SelectInfo()
		{
			Animation = null;
			FacingSign = 0;
			IsHit = false;
			IsRandom = false;
			Index = 0;
			MatchedEvent = null;
			EventType = EventAnimation.EventAnimationType.EVENT_NONE;
		}
	}

	private int _FrameRound;

	private List<Model> _Models = new List<Model>();

	private List<Model> _NewlyCreatedModels = new List<Model>();

	private List<ModelConditions> _ModelsConditions = new List<ModelConditions>();

	private List<EventModelDelayed> _PendingEvents = new List<EventModelDelayed>();

	private List<EventModelDelayed> _PendingIntervalEndEvents = new List<EventModelDelayed>();

	private List<TriggerStruct> _PendingTriggers = new List<TriggerStruct>();

	private List<List<SelectInfo>> _Selections = new List<List<SelectInfo>>();

	private List<List<SelectInfo>> _NewModelSelections = new List<List<SelectInfo>>();

	private List<Model> _ExplicitBirthModels = new List<Model>();

	public List<Model> ModelsToRegister
	{
		set
		{
			set_Models(value);
		}
	}

	public SelectAnimation()
	{
		_FrameRound = 0;
	}

	public void set_Models(List<Model> value)
	{
		_Models.Clear();
		_Models.AddRange(value);
	}

	public void AddModel(Model model)
	{
		int count = _Models.Count;
		int num = _Models.AddIfNotExist(model);
		if (num == count)
		{
			model.AddEventListener(2, OnAnimationStart);
			model.AddEventListener(3, OnAnimationEnd);
			model.AddEventListener(0, OnIntervalStart);
			model.AddEventListener(1, OnIntervalEnd);
			model.AddEventListener(4, OnEveryFrame);
			model.AddEventListener(6, OnModelCreate);
			model.AddEventListener(10, OnKeyPress);
			model.AddEventListener(11, OnKeyRelease);
			model.UpdateAnimationParameters(_Models);
		}
	}

	public void ClearModelsAndEvents()
	{
		_Models.Clear();
		_PendingEvents.Clear();
		_PendingIntervalEndEvents.Clear();
		_ExplicitBirthModels.Clear();
	}

    internal bool ReplaceModel(Model expected, Model replacement)
    {
        if (expected == null || replacement == null || _Models.Contains(replacement)) return false;
        int index = _Models.IndexOf(expected);
        if (index < 0) return false;
        var nextModels = new List<Model>(_Models);
        nextModels[index] = replacement;
        var nextConditions = new List<ModelConditions>();
        foreach (var model in nextModels) nextConditions.Add(model.GetConditions());
        // Native animation definitions cache node bindings by side. Preparation
        // here is NOT read-only; restore the live side's bindings if it fails.
        try { replacement.UpdateAnimationParameters(nextModels); }
        catch
        {
            expected.UpdateAnimationParameters(_Models);
            throw;
        }
        replacement.AddEventListener(2, OnAnimationStart);
        replacement.AddEventListener(3, OnAnimationEnd);
        replacement.AddEventListener(0, OnIntervalStart);
        replacement.AddEventListener(1, OnIntervalEnd);
        replacement.AddEventListener(4, OnEveryFrame);
        replacement.AddEventListener(6, OnModelCreate);
        replacement.AddEventListener(10, OnKeyPress);
        replacement.AddEventListener(11, OnKeyRelease);
        RemoveModel(expected);
        _Models.Insert(index, replacement);
        _ModelsConditions = nextConditions;
        return true;
    }

    internal void PrepareFormAnimation(Model model)
    {
        int index = _Models.IndexOf(model);
        if (model == null || index < 0 || model.GetCurrentAnimation() != null || model.HasPendingAnimation())
            throw new System.InvalidOperationException("Form animation requires an unstarted registered body.");
        UpdateConditions();
        var selections = new List<List<SelectInfo>>();
        foreach (var participant in _Models) selections.Add(new List<SelectInfo>());
        // A main fighter enters the ongoing round through the same eligible
        // idle/transition moves used after animation end. Birth belongs to helper
        // models. Select only this body, without dispatching triggers or a round event.
        var entry = new EventModelDelayed
        {
            Type = EventAnimation.EventAnimationType.EVENT_ANIMATION_END,
            Owner = model,
            Target = model.GetCombatTarget()
        };
        CheckAnimations(entry, model, model.AnimationEvents.GetListForEvent(entry.Type), index, selections);
        if (selections[index].Count == 0)
            throw new System.InvalidOperationException("Form has no eligible animation for entering the current round.");
        // Native priority/transition selection only schedules the owned body's
        // animation. Its first-frame actions run in Model.Render after commit.
        PlayAnimation(model, selections[index]);
        if (!model.HasPendingAnimation())
            throw new System.InvalidOperationException("Form entry did not schedule an animation.");
    }

    // Only for a synchronous form exchange between simulation steps. The event
    // records themselves are not mutated by ReplaceModel, so retain their identity.
    internal System.Action CapturePendingEvents()
    {
        var events = _PendingEvents.ToArray();
        var intervalEnds = _PendingIntervalEndEvents.ToArray();
        var births = _ExplicitBirthModels.ToArray();
        var created = _NewlyCreatedModels.ToArray();
        var triggers = _PendingTriggers.ToArray();
        return () =>
        {
            _PendingEvents.Clear(); _PendingEvents.AddRange(events);
            _PendingIntervalEndEvents.Clear(); _PendingIntervalEndEvents.AddRange(intervalEnds);
            _ExplicitBirthModels.Clear(); _ExplicitBirthModels.AddRange(births);
            _NewlyCreatedModels.Clear(); _NewlyCreatedModels.AddRange(created);
            _PendingTriggers.Clear(); _PendingTriggers.AddRange(triggers);
        };
    }

	public void RemoveModel(Model targetModel)
	{
		if (targetModel == null) return;
		targetModel.RemoveEventListener(2, OnAnimationStart);
		targetModel.RemoveEventListener(3, OnAnimationEnd);
		targetModel.RemoveEventListener(0, OnIntervalStart);
		targetModel.RemoveEventListener(1, OnIntervalEnd);
		targetModel.RemoveEventListener(4, OnEveryFrame);
		targetModel.RemoveEventListener(6, OnModelCreate);
		targetModel.RemoveEventListener(10, OnKeyPress);
		targetModel.RemoveEventListener(11, OnKeyRelease);
		RemoveEventsForModel(targetModel, _PendingEvents);
		RemoveEventsForModel(targetModel, _PendingIntervalEndEvents);
		_ExplicitBirthModels.RemoveAll(model => model == targetModel);
		_NewlyCreatedModels.RemoveAll(model => model == targetModel);
		_PendingTriggers.RemoveAll(trigger => trigger.OwnerModel == targetModel);
		for (int num = _Models.Count - 1; num >= 0; num--)
		{
			if (_Models[num] == targetModel)
			{
				_Models.RemoveAt(num);
				if (num < _ModelsConditions.Count) _ModelsConditions.RemoveAt(num);
			}
		}
	}

	public void CheckEvent(EventAnimation.EventAnimationType eventType, Model.EventModel eventModel, bool isRandom = false)
	{
		EventModelDelayed delayedEvent = new EventModelDelayed();
		delayedEvent.Type = eventType;
		delayedEvent.Data = eventModel.Data;
		delayedEvent.Owner = ((eventType != EventAnimation.EventAnimationType.EVENT_STRIKE) ? eventModel.sourceModel : eventModel.Opponent);
		delayedEvent.Target = delayedEvent.Owner.EventData.Opponent;
		delayedEvent.IsRandom = isRandom;
		if (eventType == EventAnimation.EventAnimationType.EVENT_INTERVAL_END)
		{
			_PendingIntervalEndEvents.Add(delayedEvent);
		}
		else
		{
			_PendingEvents.Add(delayedEvent);
		}
	}

	public void OnRandomKeyPress(Model.EventModel eventModel)
	{
		CheckEvent(EventAnimation.EventAnimationType.EVENT_KEY_PRESSED, eventModel, true);
		foreach (Model item in _Models)
		{
			item.FrameInRound = _FrameRound;
		}
	}

	public void UpdateConditions()
	{
		int count = _Models.Count;
		_ModelsConditions.Clear();
		_ModelsConditions.Capacity = count;
		for (int i = 0; i < count; i++)
		{
			_ModelsConditions.Add(_Models[i].GetConditions());
			UpdateConditions(_ModelsConditions[i], _Models[i]);
		}
	}

	public void Render()
	{
		UpdateConditions();
		RenderEvent();
	}

	public void Reset()
	{
		_PendingIntervalEndEvents.Clear();
		_PendingEvents.Clear();
		_ExplicitBirthModels.Clear();
	}

	public void OnAnimationStart(object data)
	{
		CheckEvent(EventAnimation.EventAnimationType.EVENT_ANIMATION_START, (Model.EventModel)data);
	}

	public void OnAnimationEnd(object data)
	{
		CheckEvent(EventAnimation.EventAnimationType.EVENT_ANIMATION_END, (Model.EventModel)data);
	}

	public void OnIntervalStart(object data)
	{
		CheckEvent(EventAnimation.EventAnimationType.EVENT_INTERVAL_START, (Model.EventModel)data);
	}

	public void OnIntervalEnd(object data)
	{
		CheckEvent(EventAnimation.EventAnimationType.EVENT_INTERVAL_END, (Model.EventModel)data);
	}

	public void OnEveryFrame(object data)
	{
		CheckEvent(EventAnimation.EventAnimationType.EVENT_EVERY_FRAME, (Model.EventModel)data);
	}

	public void OnModelCreate(object data)
	{
		Model newModel = (Model)data;
		AddModel(newModel);
		_NewlyCreatedModels.Add(newModel);
		if (newModel.HasExplicitBirthAnimation())
		{
			_ExplicitBirthModels.Add(newModel);
		}
		else
		{
			CheckEvent(EventAnimation.EventAnimationType.EVENT_BIRTH, newModel.EventData);
		}
	}

	public void OnKeyPress(object data)
	{
		CheckEvent(EventAnimation.EventAnimationType.EVENT_KEY_PRESSED, (Model.EventModel)data);
	}

	public void OnKeyRelease(object data)
	{
		CheckEvent(EventAnimation.EventAnimationType.EVENT_KEY_RELEASED, (Model.EventModel)data);
	}

	private void RenderEvent()
	{
		_NewlyCreatedModels.Clear();
		CheckEventsForModels(_Models, _Selections);
		// A scheduled PlayAnimation can emit start/end/interval events while these
		// actions run. Drain only this batch and retain newly queued events for the
		// next selection pass instead of mutating an enumerator or discarding them.
		var actionEvents = _PendingEvents.ToArray();
		_PendingEvents.Clear();
		foreach (EventModelDelayed item in actionEvents)
		{
			if (item.Owner != null)
			{
				item.Owner.GetAnimationModule().TriggerActionsForEvent(item.Type);
			}
		}
		foreach (TriggerStruct item3 in _PendingTriggers)
		{
			Model ownerModel = item3.OwnerModel;
			Trigger sourceTrigger = item3.SourceTrigger;
			if (ownerModel != null && sourceTrigger != null)
			{
				ownerModel.RunActions(sourceTrigger.Definition.Actions);
			}
		}
		// Model-create listeners (camera, effects and renderer) have all completed
		// by this point, so an XML-requested start animation can safely emit its
		// first-frame actions. Fall back to legacy Birth selection if the named
		// move is unavailable.
		foreach (Model explicitBirthModel in _ExplicitBirthModels)
		{
			if (!explicitBirthModel.TryPlayExplicitBirthAnimation())
			{
				CheckEvent(EventAnimation.EventAnimationType.EVENT_BIRTH, explicitBirthModel.EventData);
			}
		}
		_ExplicitBirthModels.Clear();
		if (_NewlyCreatedModels.Count > 0)
		{
			CheckEventsForModels(_NewlyCreatedModels, _NewModelSelections);
		}
		ClearTriggers();
		ChooseAnimations(_Models, _Selections);
		if (_NewlyCreatedModels.Count > 0)
		{
			ChooseAnimations(_NewlyCreatedModels, _NewModelSelections);
		}
		if (0 < _PendingIntervalEndEvents.Count)
		{
			_PendingEvents.AddRange(_PendingIntervalEndEvents);
			_PendingIntervalEndEvents.Clear();
		}
	}

	private static bool ContainsAnimation(InfoAnimation animation, List<SelectInfo> selectInfos)
	{
		foreach (SelectInfo item in selectInfos)
		{
			if (animation == item.Animation)
			{
				return true;
			}
		}
		return false;
	}

	private SelectInfo SelectAnimationWithWeights(Model model, List<SelectInfo> selectInfos)
	{
		int count = selectInfos.Count;
		if (0 < count)
		{
			List<InfoAnimation> list = new List<InfoAnimation>();
			foreach (SelectInfo item in selectInfos)
			{
				list.Add(item.Animation);
			}
			int num = model.GetAi().SelectAnimationWithWeights(list);
			if (-1 < num && num < count)
			{
				return selectInfos[num];
			}
		}
		return null;
	}

	private void PlayAnimation(Model model, List<SelectInfo> selectInfos, bool ignoreRandom = false)
	{
		List<SelectInfo> list = new List<SelectInfo>();
		int num = int.MinValue;
		SelectInfo chosenInfo = null;
		List<SelectInfo> list2 = new List<SelectInfo>();
		SelectInfo nKDNDLNDFJH2 = null;
		for (int i = 0; i < selectInfos.Count; i++)
		{
			nKDNDLNDFJH2 = selectInfos[i];
			if (!ignoreRandom && nKDNDLNDFJH2.IsRandom)
			{
				list.Add(nKDNDLNDFJH2);
			}
			int priority = nKDNDLNDFJH2.Animation.Priority;
			if (priority >= num)
			{
				if (priority > num)
				{
					num = priority;
					list2.Clear();
				}
				list2.Add(nKDNDLNDFJH2);
			}
		}
		int index = Eclipse.Multiplayer.VersusDeterminism.Range(0, list2.Count);
		chosenInfo = list2[index];
		if (list.Count > 0)
		{
			if (chosenInfo != null)
			{
				list.Add(chosenInfo);
			}
			PlayAnimationRandom(model, list);
		}
		else if (chosenInfo != null)
		{
			if (!chosenInfo.Animation.HasPhysics)
			{
				SetTransitions(model, _ModelsConditions[chosenInfo.Index], chosenInfo.Animation, chosenInfo.FacingSign);
			}
			else
			{
				model.SetDelayedStrike(chosenInfo.Animation, chosenInfo.IsHit);
			}
			model.LastAnimationType = chosenInfo.Animation.Type;
			model.LastEventType = chosenInfo.EventType;
            if (chosenInfo.IsHit && model.Parameters.RemainingHealthBars == 0)
            {
                // The round-end path runs later in this same simulation step.
                // Commit the selected hit reaction before it changes the stage.
                if (!model.RenderStrikeDelay()) model.RenderAnimationDelay();
            }
		}
	}

	private void PlayAnimationRandom(Model model, List<SelectInfo> selectInfos)
	{
		int num = selectInfos.Count;
		if (0 < num)
		{
			int num2 = 0;
			for (int i = 0; i < selectInfos.Count; i++)
			{
				InfoAnimation.CapabilityTable priorityConflicts = selectInfos[i].Animation.PriorityConflicts;
				bool flag = true;
				for (int j = 0; j < selectInfos.Count; j++)
				{
					if (!priorityConflicts.IsThePriority(selectInfos[j].Animation))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					selectInfos[num2] = selectInfos[i];
					num2++;
				}
			}
			num = selectInfos.Count;
			if (num2 < num)
			{
				selectInfos.Resize(num2);
				num = num2;
			}
		}
		if (0 < num)
		{
			int num3 = 0;
			for (int l = 0; l < selectInfos.Count; l++)
			{
				SelectInfo item3 = selectInfos[l];
				if (item3.Animation.MoveData.TacticsConditions.Count != 0)
				{
					model.GetConditions().CandidateMoveNames = item3.Animation.GetTemplateNames();
					model.GetConditions().AnimationSign = item3.Animation.GetDirection(model.GetConditions(), model.GetAnimationModule().GetSign());
					model.GetConditions().PivotPairSelector = (int)item3.Animation.MoveData.AlignData.PivotSideKind;
					if (item3.Animation.AreConditionsMet(model.GetConditions(), item3.Animation.MoveData.TacticsConditions, item3.MatchedEvent))
					{
						selectInfos[num3] = item3;
						num3++;
					}
				}
				else
				{
					selectInfos[num3] = item3;
					num3++;
				}
			}
			num = selectInfos.Count;
			if (num3 < num)
			{
				selectInfos.Resize(num3);
				num = num3;
			}
		}
		SelectInfo selectedInfo = SelectAnimationWithWeights(model, selectInfos);
		if (selectedInfo == null)
		{
			return;
		}
		List<SelectInfo> list = new List<SelectInfo>();
		ConditionKeys firstKeysCondition = selectedInfo.Animation.GetFirstKeysCondition();
		for (int m = 0; m < selectInfos.Count; m++)
		{
			SelectInfo item4 = selectInfos[m];
			ConditionKeys bHDEBDIHDFM2 = item4.Animation.GetFirstKeysCondition();
			if (firstKeysCondition != null && bHDEBDIHDFM2 != null)
			{
				if (bHDEBDIHDFM2.IsEqual(firstKeysCondition.RequiredKeys, true))
				{
					list.Add(item4);
				}
			}
			else
			{
				list.Add(item4);
			}
		}
		PlayAnimation(model, list, true);
	}

	private static bool IsEventMatch(EventAnimation eventAnimation, ModelConditions modelConditions, EventModelDelayed delayedEvent)
	{
		bool result = false;
		if (delayedEvent.Type == eventAnimation.Type)
		{
			switch (delayedEvent.Type)
			{
			case EventAnimation.EventAnimationType.EVENT_ROUND_STAGE:
				result = IsRoundStageMatch(eventAnimation, (StageType.Stage)delayedEvent.Data);
				result = ((!eventAnimation.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_KEY_PRESSED:
				result = IsKeyPressedMatch(eventAnimation);
				result = ((!eventAnimation.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_KEY_RELEASED:
				result = IsKeyReleasedMatch(eventAnimation);
				result = ((!eventAnimation.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_ANIMATION_START:
				result = IsAnimationStartMatch(eventAnimation, modelConditions, delayedEvent);
				break;
			case EventAnimation.EventAnimationType.EVENT_ANIMATION_END:
				result = IsAnimationEndMatch(eventAnimation, modelConditions, delayedEvent);
				break;
			case EventAnimation.EventAnimationType.EVENT_INTERVAL_START:
				result = IsIntervalStartMatch(eventAnimation, (IntervalAnimation)delayedEvent.Data);
				result = ((!eventAnimation.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_INTERVAL_END:
				result = IsIntervalEndMatch(eventAnimation, (IntervalAnimation)delayedEvent.Data);
				result = ((!eventAnimation.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_HIT:
				result = IsHit(eventAnimation, delayedEvent, delayedEvent.Owner.ReceivedCritical, delayedEvent.Owner.IsInShock());
				result = ((!eventAnimation.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_STRIKE:
				result = IsHit(eventAnimation, delayedEvent, delayedEvent.Owner.ReceivedCritical);
				result = ((!eventAnimation.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_EVERY_FRAME:
				result = IsEveryFrameMatch(eventAnimation);
				result = ((!eventAnimation.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_BIRTH:
				result = IsBirthMatch(eventAnimation);
				result = ((!eventAnimation.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_MOD_EXPIRES:
			{
				string modName = (string)delayedEvent.Data;
				result = IsModExpiresMatch(eventAnimation, modName);
				result = ((!eventAnimation.IsNot) ? result : (!result));
				break;
			}
			}
		}
		return result;
	}

	private static bool IsRoundStageMatch(EventAnimation eventAnimation, StageType.Stage stage)
	{
		EventRoundStage roundStageEvent = (EventRoundStage)eventAnimation;
		return roundStageEvent.GetStage() == stage;
	}

	private static bool IsKeyPressedMatch(EventAnimation eventAnimation)
	{
		return true;
	}

	private static bool IsKeyReleasedMatch(EventAnimation eventAnimation)
	{
		return true;
	}

	private static bool IsAnimationStartMatch(EventAnimation eventAnimation, ModelConditions conditions, EventModelDelayed delayedEvent)
	{
		if (string.IsNullOrEmpty(eventAnimation.AnimationName))
		{
			return true;
		}
		bool flag = false;
		string animationName = eventAnimation.AnimationName;
		if (animationName != string.Empty)
		{
			List<string> list = null;
			switch (eventAnimation.TargetModel)
			{
			case ModelType.ModelTargetType.MODEL_THIS:
				list = conditions.SelfAnimationNames;
				break;
			case ModelType.ModelTargetType.MODEL_OTHER:
				if (delayedEvent.Owner.GetParentModel() != null)
				{
					return false;
				}
				list = conditions.OtherAnimationNames;
				break;
			case ModelType.ModelTargetType.MODEL_PARENT:
				list = conditions.ParentAnimationNames;
				break;
			case ModelType.ModelTargetType.MODEL_CHILD:
				list = conditions.ChildAnimationNames;
				break;
			case ModelType.ModelTargetType.MODEL_BOTH:
				list = conditions.SelfAnimationNames;
				break;
			}
			List<string> list2 = null;
			if (animationName == "$Move")
			{
				list2 = conditions.CandidateMoveNames;
				int count = list.Count;
				if (count != 1)
				{
					if (count > 1)
					{
						for (int num = count - 1; num >= 1; num--)
						{
							list.RemoveAt(num);
						}
					}
					else
					{
						for (int i = count; i < 1; i++)
						{
							list.Add(string.Empty);
						}
					}
				}
			}
			else
			{
				list2 = new List<string>();
				list2.Add(animationName);
			}
			flag = IsNames(list, list2);
			return (!eventAnimation.IsNot) ? flag : (!flag);
		}
		return true;
	}

	private static bool IsNames(List<string> requiredNames, List<string> candidateNames)
	{
		string text = null;
		string text2 = null;
		int i = 0;
		for (int count = requiredNames.Count; i < count; i++)
		{
			text = requiredNames[i];
			int j = 0;
			for (int count2 = candidateNames.Count; j < count2; j++)
			{
				text2 = candidateNames[j];
				if (text == text2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsAnimationEndMatch(EventAnimation eventAnimation, ModelConditions conditions, EventModelDelayed delayedEvent)
	{
		return IsAnimationStartMatch(eventAnimation, conditions, delayedEvent);
	}

	private static bool IsIntervalStartMatch(EventAnimation eventAnimation, IntervalAnimation intervalAnimation)
	{
		IntervalAnimation.IntervalType intervalType = IntervalAnimation.IntervalType.INTERVAL_NONE;
		if (eventAnimation.HitType == "Attack")
		{
			intervalType = IntervalAnimation.IntervalType.INTERVAL_ATTACK;
		}
		else if (eventAnimation.HitType == "Block")
		{
			intervalType = IntervalAnimation.IntervalType.INTERVAL_BLOCK;
		}
		else if (eventAnimation.HitType == "Invulnerable")
		{
			intervalType = IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE;
		}
		if ((intervalType == IntervalAnimation.IntervalType.INTERVAL_NONE || intervalType == intervalAnimation.Type) && (eventAnimation.AnimationName == string.Empty || eventAnimation.AnimationName == intervalAnimation.Name))
		{
			return true;
		}
		return false;
	}

	private static bool IsIntervalEndMatch(EventAnimation eventAnimation, IntervalAnimation intervalAnimation)
	{
		return IsIntervalStartMatch(eventAnimation, intervalAnimation);
	}

	private static bool IsHit(EventAnimation eventAnimation, EventModelDelayed delayedEvent, bool isCritical = false, bool isBlocked = false)
	{
		if (string.IsNullOrEmpty(eventAnimation.HitType) || (eventAnimation.HitType == "Critical" && isCritical) || (eventAnimation.HitType == "Shock" && isBlocked))
		{
			IntervalAnimation intervalData = (IntervalAnimation)delayedEvent.Data;
			IntervalAttack intervalAttack = intervalData as IntervalAttack;
			return string.IsNullOrEmpty(eventAnimation.AnimationName) || intervalAttack == null || eventAnimation.AnimationName == intervalAttack.GetReactionName(delayedEvent.Target.GetReactionFrame());
		}
		return false;
	}

	private static bool IsEveryFrameMatch(EventAnimation eventAnimation)
	{
		return true;
	}

	private static bool IsBirthMatch(EventAnimation eventAnimation)
	{
		return true;
	}

	private static bool IsModExpiresMatch(EventAnimation eventAnimation, string name)
	{
		EventModExpires modExpiresEvent = (EventModExpires)eventAnimation;
		return modExpiresEvent.GetModName() == name;
	}

	private void UpdateConditions(ModelConditions conditions, Model model)
	{
		Fight fight = Fight.GetCurrentFight();
		Model targetModel = model.GetCombatTarget();
		Model fGCODGKLHED2 = model.GetModelByType(ModelType.ModelTargetType.MODEL_PARENT);
		Model fGCODGKLHED3 = model.GetModelByType(ModelType.ModelTargetType.MODEL_CHILD);
		// A strike can delete a projectile and create its child in the same frame.
		// The child may retain a parent reference after the parent's runtime rig is cleared.
		if (targetModel != null && targetModel.GetPhysicsModule() == null) targetModel = null;
		if (fGCODGKLHED2 != null && fGCODGKLHED2.GetPhysicsModule() == null) fGCODGKLHED2 = null;
		if (fGCODGKLHED3 != null && fGCODGKLHED3.GetPhysicsModule() == null) fGCODGKLHED3 = null;
		if (fight != null)
		{
			fight.GetPerksStage().CollectActiveActions(model, conditions.SelfActionPerks);
			fight.GetPerksStage().CollectExpiredActions(model, conditions.SelfExpiredPerks);
			if (targetModel != null)
			{
				fight.GetPerksStage().CollectActiveActions(targetModel, conditions.OtherActionPerks);
				fight.GetPerksStage().CollectActiveActions(targetModel, conditions.OtherActionPerksSecondary);
			}
			else
			{
				conditions.OtherActionPerks.Clear();
				conditions.OtherActionPerksSecondary.Clear();
			}
		}
		conditions.SelfPerks = model.Parameters.Perks;
		conditions.OtherPerks = ((targetModel == null) ? null : targetModel.Parameters.Perks);
		conditions.StrikeResult = model.LastStrike;
		conditions.HasOther = ((targetModel != null) ? true : false);
		conditions.SelfSign = model.GetFacingSign();
		conditions.OtherSign = ((targetModel == null) ? 1 : targetModel.GetFacingSign());
		conditions.ParentSign = ((fGCODGKLHED2 == null) ? 1 : fGCODGKLHED2.GetFacingSign());
		conditions.ChildSign = ((fGCODGKLHED3 == null) ? 1 : fGCODGKLHED3.GetFacingSign());
		conditions.PressedKeys = model.GetKeyDataBySign(conditions.SelfSign);
		conditions.Intervals = model.GetIntervals();
		conditions.OtherIntervals = ((targetModel == null) ? null : targetModel.GetIntervals());
		conditions.ParentIntervals = ((fGCODGKLHED2 == null) ? null : fGCODGKLHED2.GetIntervals());
		conditions.IsPlayer = model.IsPlayerModel();
		conditions.IsWeapon = model.IsWeapon();
		conditions.RoundStage = model.RoundStage;
		conditions.SelfIsPhysics = model.GetPhysicsModule().IsPhysics();
		conditions.OtherIsPhysics = targetModel != null && targetModel.GetPhysicsModule().IsPhysics();
		conditions.ParentIsPhysics = fGCODGKLHED2 != null && fGCODGKLHED2.GetPhysicsModule().IsPhysics();
		conditions.CurrentFrame = model.GetPhysicsModule().GetFrame();
		conditions.RoundEnded = model.Parameters.RoundEnded;
		conditions.IsWinner = model.Parameters.IsWinner;
		conditions.EndRoundType = model.Parameters.EndRoundType;
		conditions.IsKeyCheckEnabled = model.IsChildModel;
		conditions.ImpulseX = (int)model.LastStrike.Impulse.GetX();
		conditions.CurrentHealth = (ObscuredFloat)(model.Parameters.GetCurrentLife());
		conditions.MaxHealth = model.Parameters.MaxLife;
		conditions.NoRangedFlag = model.GetNoRangedFlag();
		conditions.MagicCharges = model.GetMagicCharges();
		conditions.RaidCharges = model.GetRaidBullets();
		conditions.SelfNode = model.GetAnimationModule().GetPlayingNode();
		conditions.OtherNode = ((targetModel == null) ? null : targetModel.GetAnimationModule().GetPlayingNode());
		conditions.ParentNode = ((fGCODGKLHED2 == null) ? null : fGCODGKLHED2.GetAnimationModule().GetPlayingNode());
		conditions.ChildNode = ((fGCODGKLHED3 == null) ? null : fGCODGKLHED3.GetAnimationModule().GetPlayingNode());
		UpdateModelPositionInfo(ref conditions.SelfAnimationNames, conditions.SelfPositions, model);
		UpdateModelPositionInfo(ref conditions.ParentAnimationNames, conditions.ParentPositions, fGCODGKLHED2);
		UpdateModelPositionInfo(ref conditions.OtherAnimationNames, conditions.OtherPositions, targetModel);
		UpdateModelPositionInfo(ref conditions.ChildAnimationNames, conditions.ChildPositions, fGCODGKLHED3);
	}

	private static void SetTransitions(Model model, ModelConditions conditions, InfoAnimation animation, int facingSign)
	{
		bool isFrameShift = false;
		int frameShift = -1;
		List<TransitionAnimation> transitions = animation.MoveData.Transitions;
		foreach (TransitionAnimation item in transitions)
		{
			if (item.AreConditionsMet(conditions))
			{
				if (item.IsFrameShift)
				{
					isFrameShift = true;
				}
				frameShift = item.FrameShift;
				break;
			}
		}
		model.PlayAnimationDelay(animation, facingSign, isFrameShift, frameShift);
	}

	private static bool IsModelTypeMatch(Model otherModel, Model sourceModel, ModelType.ModelTargetType targetType)
	{
		return targetType == ModelType.ModelTargetType.MODEL_BOTH || (targetType == ModelType.ModelTargetType.MODEL_THIS && sourceModel == otherModel) || (targetType == ModelType.ModelTargetType.MODEL_OTHER && sourceModel != otherModel) || (targetType == ModelType.ModelTargetType.MODEL_PARENT && otherModel == sourceModel.GetParentModel()) || (targetType == ModelType.ModelTargetType.MODEL_CHILD && otherModel == sourceModel.GetModelByType(ModelType.ModelTargetType.MODEL_CHILD));
	}

	private static void UpdateModelPositionInfo(ref List<string> templateNames, ModelConditions.ModelPositions positions, Model model)
	{
		if (model == null)
		{
			return;
		}
		if (!model.IsInPhysics())
		{
			InfoAnimation currentAnimation = model.GetCurrentAnimation();
			if (currentAnimation != null)
			{
				templateNames = currentAnimation.GetTemplateNames();
			}
		}
		else
		{
			List<string> list = model.GetPhysicsNames();
			if (list.Count > 0)
			{
				templateNames = list;
			}
		}
		positions.Body = model.GetBodyObject();
		positions.LeftWall.x = model.GetLeftWallX();
		positions.RightWall.x = model.GetRightWallX();
	}

	private void CheckEventsForModels(List<Model> models, List<List<SelectInfo>> selectInfosPerModel)
	{
		selectInfosPerModel.Clear();
		selectInfosPerModel.Capacity = models.Count;
		for (int i = 0; i < models.Count; i++)
		{
			selectInfosPerModel.Add(new List<SelectInfo>());
		}
		EventModelDelayed pendingEvent = null;
		Model currentModel = null;
		for (int j = 0; j < _PendingEvents.Count; j++)
		{
			pendingEvent = _PendingEvents[j];
			for (int k = 0; k < models.Count; k++)
			{
				currentModel = models[k];
				currentModel.LastEventType = EventAnimation.EventAnimationType.EVENT_NONE;
				currentModel.LastAnimationType = InfoAnimation.AnimationKind.AnimationNone;
				List<InfoAnimation> animations = currentModel.AnimationEvents.GetListForEvent(pendingEvent.Type);
				CheckAnimations(pendingEvent, currentModel, animations, k, selectInfosPerModel);
				List<Trigger> triggers = currentModel.TriggerEvents.GetListForEvent(pendingEvent.Type);
				CheckTriggers(pendingEvent, currentModel, triggers, k);
			}
		}
	}

	private void CheckAnimations(EventModelDelayed delayedEvent, Model model, List<InfoAnimation> animations, int index, List<List<SelectInfo>> selectInfosPerModel)
	{
		InfoAnimation animation = null;
		EventAnimation animationEvent = null;
		string helperWeaponSubtype = string.Empty;
		bool hasSubtypeLockedBirthMove = false;
		if (delayedEvent.Type == EventAnimation.EventAnimationType.EVENT_BIRTH && model is WeaponModel)
		{
			ItemInfo helperWeapon = model.Parameters.GetItemByType("Weapon");
			if (helperWeapon != null)
			{
				helperWeaponSubtype = helperWeapon.SubType;
				for (int candidateIndex = 0; candidateIndex < animations.Count; candidateIndex++)
				{
					if (animations[candidateIndex].IsItemRequired("Weapon", helperWeaponSubtype))
					{
						hasSubtypeLockedBirthMove = true;
						break;
					}
				}
			}
		}
		for (int i = 0; i < animations.Count; i++)
		{
			animation = animations[i];
			// Old CreatePlayer entries do not always provide StartAnimation.  In
			// that case choose only birth moves whose XML lock explicitly names the
			// copied helper Weapon subtype.  This avoids equal-priority generic
			// projectile moves being selected at random.
			if (hasSubtypeLockedBirthMove && !animation.IsItemRequired("Weapon", helperWeaponSubtype))
			{
				continue;
			}
			for (int j = 0; j < animation.MoveData.Events.Count; j++)
			{
				animationEvent = animation.MoveData.Events[j];
				if (ContainsAnimation(animation, selectInfosPerModel[index]) || !IsModelTypeMatch(delayedEvent.Owner, model, animationEvent.TargetModel) || !IsEventMatch(animationEvent, _ModelsConditions[index], delayedEvent))
				{
					continue;
				}
				_ModelsConditions[index].CandidateMoveNames = animation.GetTemplateNames();
				_ModelsConditions[index].AnimationSign = ((model.GetCombatTarget() == null) ? 1 : animation.GetDirection(_ModelsConditions[index], model.GetAnimationModule().GetSign()));
				_ModelsConditions[index].PivotPairSelector = (int)animation.MoveData.AlignData.PivotSideKind;
				if (delayedEvent.Type == EventAnimation.EventAnimationType.EVENT_KEY_PRESSED && delayedEvent.IsRandom)
				{
					_ModelsConditions[index].IsKeyCheckEnabled = false;
				}
				if (animationEvent.Type == EventAnimation.EventAnimationType.EVENT_HIT)
				{
					if (delayedEvent.Owner.ReceivedCritical)
					{
						animationEvent.HitType = "Critical";
						if (delayedEvent.Owner.IsInShock())
						{
							animationEvent.HitType += "|Shock";
						}
					}
					else if (delayedEvent.Owner.IsInShock())
					{
						animationEvent.HitType = "Shock";
					}
					else if (delayedEvent.Target.LastStrike.IsBlocked)
					{
						animationEvent.HitType = "Block";
					}
				}
				if (animation.AreConditionsMet(model, null, animationEvent))
				{
					if (delayedEvent.IsRandom && animation.Type == InfoAnimation.AnimationKind.AnimationAttack)
					{
						Model targetModel = model.GetCombatTarget();
						if (model.IsAiControlled() && model.Parameters.BeginnerCheat && targetModel != null)
						{
							float num = (ObscuredFloat)(targetModel.Parameters.GetCurrentLife());
							float maxLife = targetModel.Parameters.MaxLife;
							float num2 = num / maxLife;
							if (num2 <= GameUtils.RandomTactics.BeginnerCheat)
							{
								continue;
							}
						}
					}
					List<SelectInfo> list = selectInfosPerModel[index];
					SelectInfo newSelectInfo = new SelectInfo();
					newSelectInfo.Animation = animation;
					newSelectInfo.FacingSign = _ModelsConditions[index].AnimationSign;
					newSelectInfo.IsHit = delayedEvent.Type == EventAnimation.EventAnimationType.EVENT_HIT;
					newSelectInfo.EventType = delayedEvent.Type;
					newSelectInfo.IsRandom = delayedEvent.IsRandom;
					newSelectInfo.Index = index;
					newSelectInfo.MatchedEvent = animationEvent;
					list.Add(newSelectInfo);
				}
				if (animationEvent.Type == EventAnimation.EventAnimationType.EVENT_HIT)
				{
					animationEvent.HitType = string.Empty;
				}
				if (delayedEvent.Type == EventAnimation.EventAnimationType.EVENT_KEY_PRESSED && delayedEvent.IsRandom)
				{
					_ModelsConditions[index].IsKeyCheckEnabled = true;
				}
			}
		}
	}

	private void CheckTriggers(EventModelDelayed delayedEvent, Model model, List<Trigger> triggers, int index)
	{
		Trigger trigger = null;
		EventAnimation triggerEvent = null;
		for (int i = 0; i < triggers.Count; i++)
		{
			trigger = triggers[i];
			for (int j = 0; j < trigger.Definition.Events.Count; j++)
			{
				triggerEvent = trigger.Definition.Events[j];
				if (!IsModelTypeMatch(delayedEvent.Owner, model, triggerEvent.TargetModel) || !IsEventMatch(triggerEvent, _ModelsConditions[index], delayedEvent))
				{
					continue;
				}
				if (delayedEvent.Type == EventAnimation.EventAnimationType.EVENT_KEY_PRESSED && delayedEvent.IsRandom)
				{
					_ModelsConditions[index].IsKeyCheckEnabled = false;
				}
				if (triggerEvent.Type == EventAnimation.EventAnimationType.EVENT_HIT)
				{
					if (delayedEvent.Owner.ReceivedCritical)
					{
						if (delayedEvent.Owner.IsInShock())
						{
							triggerEvent.HitType = "Critical|Shock";
						}
						else
						{
							triggerEvent.HitType = "Critical";
						}
					}
					else if (delayedEvent.Owner.IsInShock())
					{
						triggerEvent.HitType = "Shock";
					}
					else if (delayedEvent.Owner.LastStrike.IsBlocked)
					{
						triggerEvent.HitType = "Block";
					}
				}
				if (trigger.CheckConditions(model, null, triggerEvent))
				{
					trigger.CheckConditions(model, null, triggerEvent);
					QueueTrigger(trigger, model);
				}
				if (triggerEvent.Type == EventAnimation.EventAnimationType.EVENT_HIT)
				{
					triggerEvent.HitType = string.Empty;
				}
				if (delayedEvent.Type == EventAnimation.EventAnimationType.EVENT_KEY_PRESSED && delayedEvent.IsRandom)
				{
					_ModelsConditions[index].IsKeyCheckEnabled = true;
				}
			}
		}
	}

	private void ChooseAnimations(List<Model> models, List<List<SelectInfo>> selectInfosPerModel)
	{
		int num = 0;
		foreach (List<SelectInfo> item in selectInfosPerModel)
		{
			if (0 < item.Count)
			{
				PlayAnimation(models[num], item);
			}
			num++;
		}
	}

	private void QueueTrigger(Trigger trigger, Model model)
	{
		TriggerStruct item = new TriggerStruct(trigger, model);
		_PendingTriggers.Add(item);
	}

	private void ClearTriggers()
	{
		foreach (TriggerStruct item in _PendingTriggers)
		{
		}
		_PendingTriggers.Clear();
	}

	private void RemoveEventsForModel(Model model, List<EventModelDelayed> delayedEvents)
	{
		for (int num = delayedEvents.Count - 1; num >= 0; num--)
		{
			if (delayedEvents[num].Owner == model || delayedEvents[num].Target == model)
			{
				delayedEvents.RemoveAt(num);
			}
		}
	}
}
