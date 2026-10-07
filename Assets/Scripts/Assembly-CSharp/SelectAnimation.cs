using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class SelectAnimation
{
	private class TriggerStruct
	{
		public Trigger SourceTrigger;

		public Model OwnerModel;

		public TriggerStruct(Trigger CPBHKJFPFJB, Model ACENLMONNPA)
		{
			SourceTrigger = CPBHKJFPFJB;
			OwnerModel = ACENLMONNPA;
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

	public void AddModel(Model ACENLMONNPA)
	{
		int count = _Models.Count;
		int num = _Models.AddIfNotExist(ACENLMONNPA);
		if (num == count)
		{
			ACENLMONNPA.AddEventListener(2, OnAnimationStart);
			ACENLMONNPA.AddEventListener(3, OnAnimationEnd);
			ACENLMONNPA.AddEventListener(0, OnIntervalStart);
			ACENLMONNPA.AddEventListener(1, OnIntervalEnd);
			ACENLMONNPA.AddEventListener(4, OnEveryFrame);
			ACENLMONNPA.AddEventListener(6, OnModelCreate);
			ACENLMONNPA.AddEventListener(10, OnKeyPress);
			ACENLMONNPA.AddEventListener(11, OnKeyRelease);
			ACENLMONNPA.UpdateAnimationParameters(_Models);
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

	public void RemoveModel(Model ACENLMONNPA)
	{
		if (ACENLMONNPA == null) return;
		ACENLMONNPA.RemoveEventListener(2, OnAnimationStart);
		ACENLMONNPA.RemoveEventListener(3, OnAnimationEnd);
		ACENLMONNPA.RemoveEventListener(0, OnIntervalStart);
		ACENLMONNPA.RemoveEventListener(1, OnIntervalEnd);
		ACENLMONNPA.RemoveEventListener(4, OnEveryFrame);
		ACENLMONNPA.RemoveEventListener(6, OnModelCreate);
		ACENLMONNPA.RemoveEventListener(10, OnKeyPress);
		ACENLMONNPA.RemoveEventListener(11, OnKeyRelease);
		RemoveEventsForModel(ACENLMONNPA, _PendingEvents);
		RemoveEventsForModel(ACENLMONNPA, _PendingIntervalEndEvents);
		_ExplicitBirthModels.RemoveAll(model => model == ACENLMONNPA);
		_NewlyCreatedModels.RemoveAll(model => model == ACENLMONNPA);
		_PendingTriggers.RemoveAll(trigger => trigger.OwnerModel == ACENLMONNPA);
		for (int num = _Models.Count - 1; num >= 0; num--)
		{
			if (_Models[num] == ACENLMONNPA)
			{
				_Models.RemoveAt(num);
				if (num < _ModelsConditions.Count) _ModelsConditions.RemoveAt(num);
			}
		}
	}

	public void CheckEvent(EventAnimation.EventAnimationType LFLGCDNKNJI, Model.EventModel EGHPHELLOGO, bool HLEIILHFBKP = false)
	{
		EventModelDelayed gBEJMGCOCOJ = new EventModelDelayed();
		gBEJMGCOCOJ.Type = LFLGCDNKNJI;
		gBEJMGCOCOJ.Data = EGHPHELLOGO.Data;
		gBEJMGCOCOJ.Owner = ((LFLGCDNKNJI != EventAnimation.EventAnimationType.EVENT_STRIKE) ? EGHPHELLOGO.KJDFJPBIGJC : EGHPHELLOGO.Opponent);
		gBEJMGCOCOJ.Target = gBEJMGCOCOJ.Owner.EventData.Opponent;
		gBEJMGCOCOJ.IsRandom = HLEIILHFBKP;
		if (LFLGCDNKNJI == EventAnimation.EventAnimationType.EVENT_INTERVAL_END)
		{
			_PendingIntervalEndEvents.Add(gBEJMGCOCOJ);
		}
		else
		{
			_PendingEvents.Add(gBEJMGCOCOJ);
		}
	}

	public void OnRandomKeyPress(Model.EventModel EGHPHELLOGO)
	{
		CheckEvent(EventAnimation.EventAnimationType.EVENT_KEY_PRESSED, EGHPHELLOGO, true);
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
		Model fGCODGKLHED = (Model)data;
		AddModel(fGCODGKLHED);
		_NewlyCreatedModels.Add(fGCODGKLHED);
		if (fGCODGKLHED.HasExplicitBirthAnimation())
		{
			_ExplicitBirthModels.Add(fGCODGKLHED);
		}
		else
		{
			CheckEvent(EventAnimation.EventAnimationType.EVENT_BIRTH, fGCODGKLHED.EventData);
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
			Model kJDFJPBIGJC = item3.OwnerModel;
			Trigger fEDHCBGNJIM = item3.SourceTrigger;
			if (kJDFJPBIGJC != null && fEDHCBGNJIM != null)
			{
				kJDFJPBIGJC.RunActions(fEDHCBGNJIM.Definition.Actions);
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

	private static bool ContainsAnimation(InfoAnimation DBOLBEOCEME, List<SelectInfo> MAHEJFLCCHP)
	{
		foreach (SelectInfo item in MAHEJFLCCHP)
		{
			if (DBOLBEOCEME == item.Animation)
			{
				return true;
			}
		}
		return false;
	}

	private SelectInfo SelectAnimationWithWeights(Model ACENLMONNPA, List<SelectInfo> GBKDAGPNJLB)
	{
		int count = GBKDAGPNJLB.Count;
		if (0 < count)
		{
			List<InfoAnimation> list = new List<InfoAnimation>();
			foreach (SelectInfo item in GBKDAGPNJLB)
			{
				list.Add(item.Animation);
			}
			int num = ACENLMONNPA.GetAi().SelectAnimationWithWeights(list);
			if (-1 < num && num < count)
			{
				return GBKDAGPNJLB[num];
			}
		}
		return null;
	}

	private void PlayAnimation(Model ACENLMONNPA, List<SelectInfo> MAHEJFLCCHP, bool HLEIILHFBKP = false)
	{
		List<SelectInfo> list = new List<SelectInfo>();
		int num = int.MinValue;
		SelectInfo nKDNDLNDFJH = null;
		List<SelectInfo> list2 = new List<SelectInfo>();
		SelectInfo nKDNDLNDFJH2 = null;
		for (int i = 0; i < MAHEJFLCCHP.Count; i++)
		{
			nKDNDLNDFJH2 = MAHEJFLCCHP[i];
			if (!HLEIILHFBKP && nKDNDLNDFJH2.IsRandom)
			{
				list.Add(nKDNDLNDFJH2);
			}
			int eBMPEMKCDGP = nKDNDLNDFJH2.Animation.Priority;
			if (eBMPEMKCDGP >= num)
			{
				if (eBMPEMKCDGP > num)
				{
					num = eBMPEMKCDGP;
					list2.Clear();
				}
				list2.Add(nKDNDLNDFJH2);
			}
		}
		int index = Eclipse.Multiplayer.VersusDeterminism.Range(0, list2.Count);
		nKDNDLNDFJH = list2[index];
		if (list.Count > 0)
		{
			if (nKDNDLNDFJH != null)
			{
				list.Add(nKDNDLNDFJH);
			}
			PlayAnimationRandom(ACENLMONNPA, list);
		}
		else if (nKDNDLNDFJH != null)
		{
			if (!nKDNDLNDFJH.Animation.HasPhysics)
			{
				SetTransitions(ACENLMONNPA, _ModelsConditions[nKDNDLNDFJH.Index], nKDNDLNDFJH.Animation, nKDNDLNDFJH.FacingSign);
			}
			else
			{
				ACENLMONNPA.SetDelayedStrike(nKDNDLNDFJH.Animation, nKDNDLNDFJH.IsHit);
			}
			ACENLMONNPA.LastAnimationType = nKDNDLNDFJH.Animation.Type;
			ACENLMONNPA.LastEventType = nKDNDLNDFJH.EventType;
            if (nKDNDLNDFJH.IsHit && ACENLMONNPA.Parameters.RemainingHealthBars == 0)
            {
                // The round-end path runs later in this same simulation step.
                // Commit the selected hit reaction before it changes the stage.
                if (!ACENLMONNPA.RenderStrikeDelay()) ACENLMONNPA.RenderAnimationDelay();
            }
		}
	}

	private void PlayAnimationRandom(Model ACENLMONNPA, List<SelectInfo> MAHEJFLCCHP)
	{
		int num = MAHEJFLCCHP.Count;
		if (0 < num)
		{
			int num2 = 0;
			for (int i = 0; i < MAHEJFLCCHP.Count; i++)
			{
				InfoAnimation.CapabilityTable iCANLHJKKNE = MAHEJFLCCHP[i].Animation.PriorityConflicts;
				bool flag = true;
				for (int j = 0; j < MAHEJFLCCHP.Count; j++)
				{
					if (!iCANLHJKKNE.IsThePriority(MAHEJFLCCHP[j].Animation))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					MAHEJFLCCHP[num2] = MAHEJFLCCHP[i];
					num2++;
				}
			}
			num = MAHEJFLCCHP.Count;
			if (num2 < num)
			{
				MAHEJFLCCHP.Resize(num2);
				num = num2;
			}
		}
		if (0 < num)
		{
			int num3 = 0;
			for (int l = 0; l < MAHEJFLCCHP.Count; l++)
			{
				SelectInfo item3 = MAHEJFLCCHP[l];
				if (item3.Animation.MoveData.TacticsConditions.Count != 0)
				{
					ACENLMONNPA.GetConditions().CandidateMoveNames = item3.Animation.GetTemplateNames();
					ACENLMONNPA.GetConditions().AnimationSign = item3.Animation.GetDirection(ACENLMONNPA.GetConditions(), ACENLMONNPA.GetAnimationModule().GetSign());
					ACENLMONNPA.GetConditions().PivotPairSelector = (int)item3.Animation.MoveData.AlignData.PivotSideKind;
					if (item3.Animation.AreConditionsMet(ACENLMONNPA.GetConditions(), item3.Animation.MoveData.TacticsConditions, item3.MatchedEvent))
					{
						MAHEJFLCCHP[num3] = item3;
						num3++;
					}
				}
				else
				{
					MAHEJFLCCHP[num3] = item3;
					num3++;
				}
			}
			num = MAHEJFLCCHP.Count;
			if (num3 < num)
			{
				MAHEJFLCCHP.Resize(num3);
				num = num3;
			}
		}
		SelectInfo nKDNDLNDFJH = SelectAnimationWithWeights(ACENLMONNPA, MAHEJFLCCHP);
		if (nKDNDLNDFJH == null)
		{
			return;
		}
		List<SelectInfo> list = new List<SelectInfo>();
		ConditionKeys bHDEBDIHDFM = nKDNDLNDFJH.Animation.GetFirstKeysCondition();
		for (int m = 0; m < MAHEJFLCCHP.Count; m++)
		{
			SelectInfo item4 = MAHEJFLCCHP[m];
			ConditionKeys bHDEBDIHDFM2 = item4.Animation.GetFirstKeysCondition();
			if (bHDEBDIHDFM != null && bHDEBDIHDFM2 != null)
			{
				if (bHDEBDIHDFM2.IsEqual(bHDEBDIHDFM.RequiredKeys, true))
				{
					list.Add(item4);
				}
			}
			else
			{
				list.Add(item4);
			}
		}
		PlayAnimation(ACENLMONNPA, list, true);
	}

	private static bool IsEventMatch(EventAnimation DOANBADPBGH, ModelConditions BCGJLLNBHJG, EventModelDelayed PEADINOKLKN)
	{
		bool result = false;
		if (PEADINOKLKN.Type == DOANBADPBGH.Type)
		{
			switch (PEADINOKLKN.Type)
			{
			case EventAnimation.EventAnimationType.EVENT_ROUND_STAGE:
				result = IsRoundStageMatch(DOANBADPBGH, (StageType.Stage)PEADINOKLKN.Data);
				result = ((!DOANBADPBGH.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_KEY_PRESSED:
				result = IsKeyPressedMatch(DOANBADPBGH);
				result = ((!DOANBADPBGH.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_KEY_RELEASED:
				result = IsKeyReleasedMatch(DOANBADPBGH);
				result = ((!DOANBADPBGH.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_ANIMATION_START:
				result = IsAnimationStartMatch(DOANBADPBGH, BCGJLLNBHJG, PEADINOKLKN);
				break;
			case EventAnimation.EventAnimationType.EVENT_ANIMATION_END:
				result = IsAnimationEndMatch(DOANBADPBGH, BCGJLLNBHJG, PEADINOKLKN);
				break;
			case EventAnimation.EventAnimationType.EVENT_INTERVAL_START:
				result = IsIntervalStartMatch(DOANBADPBGH, (IntervalAnimation)PEADINOKLKN.Data);
				result = ((!DOANBADPBGH.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_INTERVAL_END:
				result = IsIntervalEndMatch(DOANBADPBGH, (IntervalAnimation)PEADINOKLKN.Data);
				result = ((!DOANBADPBGH.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_HIT:
				result = IsHit(DOANBADPBGH, PEADINOKLKN, PEADINOKLKN.Owner.ReceivedCritical, PEADINOKLKN.Owner.IsInShock());
				result = ((!DOANBADPBGH.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_STRIKE:
				result = IsHit(DOANBADPBGH, PEADINOKLKN, PEADINOKLKN.Owner.ReceivedCritical);
				result = ((!DOANBADPBGH.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_EVERY_FRAME:
				result = IsEveryFrameMatch(DOANBADPBGH);
				result = ((!DOANBADPBGH.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_BIRTH:
				result = IsBirthMatch(DOANBADPBGH);
				result = ((!DOANBADPBGH.IsNot) ? result : (!result));
				break;
			case EventAnimation.EventAnimationType.EVENT_MOD_EXPIRES:
			{
				string gOHIIMFFFJI = (string)PEADINOKLKN.Data;
				result = IsModExpiresMatch(DOANBADPBGH, gOHIIMFFFJI);
				result = ((!DOANBADPBGH.IsNot) ? result : (!result));
				break;
			}
			}
		}
		return result;
	}

	private static bool IsRoundStageMatch(EventAnimation FOPOKALJIIJ, StageType.Stage LFLGCDNKNJI)
	{
		EventRoundStage gBIJAGPBADA = (EventRoundStage)FOPOKALJIIJ;
		return gBIJAGPBADA.GetStage() == LFLGCDNKNJI;
	}

	private static bool IsKeyPressedMatch(EventAnimation FOPOKALJIIJ)
	{
		return true;
	}

	private static bool IsKeyReleasedMatch(EventAnimation FOPOKALJIIJ)
	{
		return true;
	}

	private static bool IsAnimationStartMatch(EventAnimation FOPOKALJIIJ, ModelConditions conditions, EventModelDelayed PEADINOKLKN)
	{
		if (string.IsNullOrEmpty(FOPOKALJIIJ.AnimationName))
		{
			return true;
		}
		bool flag = false;
		string lJICHLHMBFA = FOPOKALJIIJ.AnimationName;
		if (lJICHLHMBFA != string.Empty)
		{
			List<string> list = null;
			switch (FOPOKALJIIJ.TargetModel)
			{
			case ModelType.ModelTargetType.MODEL_THIS:
				list = conditions.SelfAnimationNames;
				break;
			case ModelType.ModelTargetType.MODEL_OTHER:
				if (PEADINOKLKN.Owner.GetParentModel() != null)
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
			if (lJICHLHMBFA == "$Move")
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
				list2.Add(lJICHLHMBFA);
			}
			flag = IsNames(list, list2);
			return (!FOPOKALJIIJ.IsNot) ? flag : (!flag);
		}
		return true;
	}

	private static bool IsNames(List<string> NIKHAICFGNM, List<string> MGNOPLPBOHC)
	{
		string text = null;
		string text2 = null;
		int i = 0;
		for (int count = NIKHAICFGNM.Count; i < count; i++)
		{
			text = NIKHAICFGNM[i];
			int j = 0;
			for (int count2 = MGNOPLPBOHC.Count; j < count2; j++)
			{
				text2 = MGNOPLPBOHC[j];
				if (text == text2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsAnimationEndMatch(EventAnimation FOPOKALJIIJ, ModelConditions conditions, EventModelDelayed PEADINOKLKN)
	{
		return IsAnimationStartMatch(FOPOKALJIIJ, conditions, PEADINOKLKN);
	}

	private static bool IsIntervalStartMatch(EventAnimation FOPOKALJIIJ, IntervalAnimation CHCGJBLDPML)
	{
		IntervalAnimation.IntervalType nGAJJDIEDGF = IntervalAnimation.IntervalType.INTERVAL_NONE;
		if (FOPOKALJIIJ.HitType == "Attack")
		{
			nGAJJDIEDGF = IntervalAnimation.IntervalType.INTERVAL_ATTACK;
		}
		else if (FOPOKALJIIJ.HitType == "Block")
		{
			nGAJJDIEDGF = IntervalAnimation.IntervalType.INTERVAL_BLOCK;
		}
		else if (FOPOKALJIIJ.HitType == "Invulnerable")
		{
			nGAJJDIEDGF = IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE;
		}
		if ((nGAJJDIEDGF == IntervalAnimation.IntervalType.INTERVAL_NONE || nGAJJDIEDGF == CHCGJBLDPML.Type) && (FOPOKALJIIJ.AnimationName == string.Empty || FOPOKALJIIJ.AnimationName == CHCGJBLDPML.Name))
		{
			return true;
		}
		return false;
	}

	private static bool IsIntervalEndMatch(EventAnimation FOPOKALJIIJ, IntervalAnimation CHCGJBLDPML)
	{
		return IsIntervalStartMatch(FOPOKALJIIJ, CHCGJBLDPML);
	}

	private static bool IsHit(EventAnimation FOPOKALJIIJ, EventModelDelayed PEADINOKLKN, bool OOGIBOBMGJA = false, bool EPKEEMFHHFM = false)
	{
		if (string.IsNullOrEmpty(FOPOKALJIIJ.HitType) || (FOPOKALJIIJ.HitType == "Critical" && OOGIBOBMGJA) || (FOPOKALJIIJ.HitType == "Shock" && EPKEEMFHHFM))
		{
			IntervalAnimation mNOIEOBBCMI = (IntervalAnimation)PEADINOKLKN.Data;
			IntervalAttack hFIIPNLCIEE = mNOIEOBBCMI as IntervalAttack;
			return string.IsNullOrEmpty(FOPOKALJIIJ.AnimationName) || hFIIPNLCIEE == null || FOPOKALJIIJ.AnimationName == hFIIPNLCIEE.GetReactionName(PEADINOKLKN.Target.GetReactionFrame());
		}
		return false;
	}

	private static bool IsEveryFrameMatch(EventAnimation FOPOKALJIIJ)
	{
		return true;
	}

	private static bool IsBirthMatch(EventAnimation FOPOKALJIIJ)
	{
		return true;
	}

	private static bool IsModExpiresMatch(EventAnimation FOPOKALJIIJ, string name)
	{
		EventModExpires bEKAAGNGPFP = (EventModExpires)FOPOKALJIIJ;
		return bEKAAGNGPFP.GetModName() == name;
	}

	private void UpdateConditions(ModelConditions conditions, Model ACENLMONNPA)
	{
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		Model fGCODGKLHED = ACENLMONNPA.GetCombatTarget();
		Model fGCODGKLHED2 = ACENLMONNPA.GetModelByType(ModelType.ModelTargetType.MODEL_PARENT);
		Model fGCODGKLHED3 = ACENLMONNPA.GetModelByType(ModelType.ModelTargetType.MODEL_CHILD);
		// A strike can delete a projectile and create its child in the same frame.
		// The child may retain a parent reference after the parent's runtime rig is cleared.
		if (fGCODGKLHED != null && fGCODGKLHED.GetPhysicsModule() == null) fGCODGKLHED = null;
		if (fGCODGKLHED2 != null && fGCODGKLHED2.GetPhysicsModule() == null) fGCODGKLHED2 = null;
		if (fGCODGKLHED3 != null && fGCODGKLHED3.GetPhysicsModule() == null) fGCODGKLHED3 = null;
		if (gDBOMJODDEA != null)
		{
			gDBOMJODDEA.GetPerksStage().CollectActiveActions(ACENLMONNPA, conditions.SelfActionPerks);
			gDBOMJODDEA.GetPerksStage().CollectExpiredActions(ACENLMONNPA, conditions.SelfExpiredPerks);
			if (fGCODGKLHED != null)
			{
				gDBOMJODDEA.GetPerksStage().CollectActiveActions(fGCODGKLHED, conditions.OtherActionPerks);
				gDBOMJODDEA.GetPerksStage().CollectActiveActions(fGCODGKLHED, conditions.OtherActionPerksSecondary);
			}
			else
			{
				conditions.OtherActionPerks.Clear();
				conditions.OtherActionPerksSecondary.Clear();
			}
		}
		conditions.SelfPerks = ACENLMONNPA.Parameters.Perks;
		conditions.OtherPerks = ((fGCODGKLHED == null) ? null : fGCODGKLHED.Parameters.Perks);
		conditions.StrikeResult = ACENLMONNPA.LastStrike;
		conditions.HasOther = ((fGCODGKLHED != null) ? true : false);
		conditions.SelfSign = ACENLMONNPA.GetFacingSign();
		conditions.OtherSign = ((fGCODGKLHED == null) ? 1 : fGCODGKLHED.GetFacingSign());
		conditions.ParentSign = ((fGCODGKLHED2 == null) ? 1 : fGCODGKLHED2.GetFacingSign());
		conditions.ChildSign = ((fGCODGKLHED3 == null) ? 1 : fGCODGKLHED3.GetFacingSign());
		conditions.PressedKeys = ACENLMONNPA.GetKeyDataBySign(conditions.SelfSign);
		conditions.Intervals = ACENLMONNPA.GetIntervals();
		conditions.OtherIntervals = ((fGCODGKLHED == null) ? null : fGCODGKLHED.GetIntervals());
		conditions.ParentIntervals = ((fGCODGKLHED2 == null) ? null : fGCODGKLHED2.GetIntervals());
		conditions.IsPlayer = ACENLMONNPA.IsPlayerModel();
		conditions.IsWeapon = ACENLMONNPA.IsWeapon();
		conditions.RoundStage = ACENLMONNPA.RoundStage;
		conditions.SelfIsPhysics = ACENLMONNPA.GetPhysicsModule().IsPhysics();
		conditions.OtherIsPhysics = fGCODGKLHED != null && fGCODGKLHED.GetPhysicsModule().IsPhysics();
		conditions.ParentIsPhysics = fGCODGKLHED2 != null && fGCODGKLHED2.GetPhysicsModule().IsPhysics();
		conditions.CurrentFrame = ACENLMONNPA.GetPhysicsModule().GetFrame();
		conditions.RoundEnded = ACENLMONNPA.Parameters.RoundEnded;
		conditions.IsWinner = ACENLMONNPA.Parameters.IsWinner;
		conditions.EndRoundType = ACENLMONNPA.Parameters.EndRoundType;
		conditions.IsKeyCheckEnabled = ACENLMONNPA.IsChildModel;
		conditions.ImpulseX = (int)ACENLMONNPA.LastStrike.Impulse.GetX();
		conditions.CurrentHealth = (ObscuredFloat)(ACENLMONNPA.Parameters.GetCurrentLife());
		conditions.MaxHealth = ACENLMONNPA.Parameters.MaxLife;
		conditions.NoRangedFlag = ACENLMONNPA.GetNoRangedFlag();
		conditions.MagicCharges = ACENLMONNPA.GetMagicCharges();
		conditions.RaidCharges = ACENLMONNPA.GetRaidBullets();
		conditions.SelfNode = ACENLMONNPA.GetAnimationModule().GetPlayingNode();
		conditions.OtherNode = ((fGCODGKLHED == null) ? null : fGCODGKLHED.GetAnimationModule().GetPlayingNode());
		conditions.ParentNode = ((fGCODGKLHED2 == null) ? null : fGCODGKLHED2.GetAnimationModule().GetPlayingNode());
		conditions.ChildNode = ((fGCODGKLHED3 == null) ? null : fGCODGKLHED3.GetAnimationModule().GetPlayingNode());
		UpdateModelPositionInfo(ref conditions.SelfAnimationNames, conditions.SelfPositions, ACENLMONNPA);
		UpdateModelPositionInfo(ref conditions.ParentAnimationNames, conditions.ParentPositions, fGCODGKLHED2);
		UpdateModelPositionInfo(ref conditions.OtherAnimationNames, conditions.OtherPositions, fGCODGKLHED);
		UpdateModelPositionInfo(ref conditions.ChildAnimationNames, conditions.ChildPositions, fGCODGKLHED3);
	}

	private static void SetTransitions(Model ACENLMONNPA, ModelConditions conditions, InfoAnimation DBOLBEOCEME, int AOJJBKLCHJO)
	{
		bool hHJGACBCGBP = false;
		int bADKABIKMBD = -1;
		List<TransitionAnimation> eLFBPNOBDKC = DBOLBEOCEME.MoveData.Transitions;
		foreach (TransitionAnimation item in eLFBPNOBDKC)
		{
			if (item.AreConditionsMet(conditions))
			{
				if (item.IsFrameShift)
				{
					hHJGACBCGBP = true;
				}
				bADKABIKMBD = item.FrameShift;
				break;
			}
		}
		ACENLMONNPA.PlayAnimationDelay(DBOLBEOCEME, AOJJBKLCHJO, hHJGACBCGBP, bADKABIKMBD);
	}

	private static bool IsModelTypeMatch(Model CEDPFKAOGHN, Model DBPIIMHNKNN, ModelType.ModelTargetType LFLGCDNKNJI)
	{
		return LFLGCDNKNJI == ModelType.ModelTargetType.MODEL_BOTH || (LFLGCDNKNJI == ModelType.ModelTargetType.MODEL_THIS && DBPIIMHNKNN == CEDPFKAOGHN) || (LFLGCDNKNJI == ModelType.ModelTargetType.MODEL_OTHER && DBPIIMHNKNN != CEDPFKAOGHN) || (LFLGCDNKNJI == ModelType.ModelTargetType.MODEL_PARENT && CEDPFKAOGHN == DBPIIMHNKNN.GetParentModel()) || (LFLGCDNKNJI == ModelType.ModelTargetType.MODEL_CHILD && CEDPFKAOGHN == DBPIIMHNKNN.GetModelByType(ModelType.ModelTargetType.MODEL_CHILD));
	}

	private static void UpdateModelPositionInfo(ref List<string> IPFMIJKPABH, ModelConditions.ModelPositions LJKGOKDLAKL, Model ACENLMONNPA)
	{
		if (ACENLMONNPA == null)
		{
			return;
		}
		if (!ACENLMONNPA.IsInPhysics())
		{
			InfoAnimation pJAHIOELGGD = ACENLMONNPA.GetCurrentAnimation();
			if (pJAHIOELGGD != null)
			{
				IPFMIJKPABH = pJAHIOELGGD.GetTemplateNames();
			}
		}
		else
		{
			List<string> list = ACENLMONNPA.GetPhysicsNames();
			if (list.Count > 0)
			{
				IPFMIJKPABH = list;
			}
		}
		LJKGOKDLAKL.Body = ACENLMONNPA.GetBodyObject();
		LJKGOKDLAKL.LeftWall.x = ACENLMONNPA.GetLeftWallX();
		LJKGOKDLAKL.RightWall.x = ACENLMONNPA.GetRightWallX();
	}

	private void CheckEventsForModels(List<Model> INNLAFHKJNI, List<List<SelectInfo>> GLEOPGKNDAO)
	{
		GLEOPGKNDAO.Clear();
		GLEOPGKNDAO.Capacity = INNLAFHKJNI.Count;
		for (int i = 0; i < INNLAFHKJNI.Count; i++)
		{
			GLEOPGKNDAO.Add(new List<SelectInfo>());
		}
		EventModelDelayed gBEJMGCOCOJ = null;
		Model fGCODGKLHED = null;
		for (int j = 0; j < _PendingEvents.Count; j++)
		{
			gBEJMGCOCOJ = _PendingEvents[j];
			for (int k = 0; k < INNLAFHKJNI.Count; k++)
			{
				fGCODGKLHED = INNLAFHKJNI[k];
				fGCODGKLHED.LastEventType = EventAnimation.EventAnimationType.EVENT_NONE;
				fGCODGKLHED.LastAnimationType = InfoAnimation.AnimationKind.AnimationNone;
				List<InfoAnimation> mAHEJFLCCHP = fGCODGKLHED.AnimationEvents.GetListForEvent(gBEJMGCOCOJ.Type);
				CheckAnimations(gBEJMGCOCOJ, fGCODGKLHED, mAHEJFLCCHP, k, GLEOPGKNDAO);
				List<Trigger> cMHFKBKKKOK = fGCODGKLHED.TriggerEvents.GetListForEvent(gBEJMGCOCOJ.Type);
				CheckTriggers(gBEJMGCOCOJ, fGCODGKLHED, cMHFKBKKKOK, k);
			}
		}
	}

	private void CheckAnimations(EventModelDelayed PEADINOKLKN, Model ACENLMONNPA, List<InfoAnimation> MAHEJFLCCHP, int index, List<List<SelectInfo>> GLEOPGKNDAO)
	{
		InfoAnimation pJAHIOELGGD = null;
		EventAnimation nFCCFMOMPHG = null;
		string helperWeaponSubtype = string.Empty;
		bool hasSubtypeLockedBirthMove = false;
		if (PEADINOKLKN.Type == EventAnimation.EventAnimationType.EVENT_BIRTH && ACENLMONNPA is WeaponModel)
		{
			ItemInfo helperWeapon = ACENLMONNPA.Parameters.GetItemByType("Weapon");
			if (helperWeapon != null)
			{
				helperWeaponSubtype = helperWeapon.SubType;
				for (int candidateIndex = 0; candidateIndex < MAHEJFLCCHP.Count; candidateIndex++)
				{
					if (MAHEJFLCCHP[candidateIndex].IsItemRequired("Weapon", helperWeaponSubtype))
					{
						hasSubtypeLockedBirthMove = true;
						break;
					}
				}
			}
		}
		for (int i = 0; i < MAHEJFLCCHP.Count; i++)
		{
			pJAHIOELGGD = MAHEJFLCCHP[i];
			// Old CreatePlayer entries do not always provide StartAnimation.  In
			// that case choose only birth moves whose XML lock explicitly names the
			// copied helper Weapon subtype.  This avoids equal-priority generic
			// projectile moves being selected at random.
			if (hasSubtypeLockedBirthMove && !pJAHIOELGGD.IsItemRequired("Weapon", helperWeaponSubtype))
			{
				continue;
			}
			for (int j = 0; j < pJAHIOELGGD.MoveData.Events.Count; j++)
			{
				nFCCFMOMPHG = pJAHIOELGGD.MoveData.Events[j];
				if (ContainsAnimation(pJAHIOELGGD, GLEOPGKNDAO[index]) || !IsModelTypeMatch(PEADINOKLKN.Owner, ACENLMONNPA, nFCCFMOMPHG.TargetModel) || !IsEventMatch(nFCCFMOMPHG, _ModelsConditions[index], PEADINOKLKN))
				{
					continue;
				}
				_ModelsConditions[index].CandidateMoveNames = pJAHIOELGGD.GetTemplateNames();
				_ModelsConditions[index].AnimationSign = ((ACENLMONNPA.GetCombatTarget() == null) ? 1 : pJAHIOELGGD.GetDirection(_ModelsConditions[index], ACENLMONNPA.GetAnimationModule().GetSign()));
				_ModelsConditions[index].PivotPairSelector = (int)pJAHIOELGGD.MoveData.AlignData.PivotSideKind;
				if (PEADINOKLKN.Type == EventAnimation.EventAnimationType.EVENT_KEY_PRESSED && PEADINOKLKN.IsRandom)
				{
					_ModelsConditions[index].IsKeyCheckEnabled = false;
				}
				if (nFCCFMOMPHG.Type == EventAnimation.EventAnimationType.EVENT_HIT)
				{
					if (PEADINOKLKN.Owner.ReceivedCritical)
					{
						nFCCFMOMPHG.HitType = "Critical";
						if (PEADINOKLKN.Owner.IsInShock())
						{
							nFCCFMOMPHG.HitType += "|Shock";
						}
					}
					else if (PEADINOKLKN.Owner.IsInShock())
					{
						nFCCFMOMPHG.HitType = "Shock";
					}
					else if (PEADINOKLKN.Target.LastStrike.IsBlocked)
					{
						nFCCFMOMPHG.HitType = "Block";
					}
				}
				if (pJAHIOELGGD.AreConditionsMet(ACENLMONNPA, null, nFCCFMOMPHG))
				{
					if (PEADINOKLKN.IsRandom && pJAHIOELGGD.Type == InfoAnimation.AnimationKind.AnimationAttack)
					{
						Model fGCODGKLHED = ACENLMONNPA.GetCombatTarget();
						if (ACENLMONNPA.IsAiControlled() && ACENLMONNPA.Parameters.BeginnerCheat && fGCODGKLHED != null)
						{
							float num = (ObscuredFloat)(fGCODGKLHED.Parameters.GetCurrentLife());
							float cIDCNCDFONA = fGCODGKLHED.Parameters.MaxLife;
							float num2 = num / cIDCNCDFONA;
							if (num2 <= GameUtils.RandomTactics.BeginnerCheat)
							{
								continue;
							}
						}
					}
					List<SelectInfo> list = GLEOPGKNDAO[index];
					SelectInfo nKDNDLNDFJH = new SelectInfo();
					nKDNDLNDFJH.Animation = pJAHIOELGGD;
					nKDNDLNDFJH.FacingSign = _ModelsConditions[index].AnimationSign;
					nKDNDLNDFJH.IsHit = PEADINOKLKN.Type == EventAnimation.EventAnimationType.EVENT_HIT;
					nKDNDLNDFJH.EventType = PEADINOKLKN.Type;
					nKDNDLNDFJH.IsRandom = PEADINOKLKN.IsRandom;
					nKDNDLNDFJH.Index = index;
					nKDNDLNDFJH.MatchedEvent = nFCCFMOMPHG;
					list.Add(nKDNDLNDFJH);
				}
				if (nFCCFMOMPHG.Type == EventAnimation.EventAnimationType.EVENT_HIT)
				{
					nFCCFMOMPHG.HitType = string.Empty;
				}
				if (PEADINOKLKN.Type == EventAnimation.EventAnimationType.EVENT_KEY_PRESSED && PEADINOKLKN.IsRandom)
				{
					_ModelsConditions[index].IsKeyCheckEnabled = true;
				}
			}
		}
	}

	private void CheckTriggers(EventModelDelayed PEADINOKLKN, Model ACENLMONNPA, List<Trigger> CMHFKBKKKOK, int index)
	{
		Trigger cPFMGFAFAFB = null;
		EventAnimation nFCCFMOMPHG = null;
		for (int i = 0; i < CMHFKBKKKOK.Count; i++)
		{
			cPFMGFAFAFB = CMHFKBKKKOK[i];
			for (int j = 0; j < cPFMGFAFAFB.Definition.Events.Count; j++)
			{
				nFCCFMOMPHG = cPFMGFAFAFB.Definition.Events[j];
				if (!IsModelTypeMatch(PEADINOKLKN.Owner, ACENLMONNPA, nFCCFMOMPHG.TargetModel) || !IsEventMatch(nFCCFMOMPHG, _ModelsConditions[index], PEADINOKLKN))
				{
					continue;
				}
				if (PEADINOKLKN.Type == EventAnimation.EventAnimationType.EVENT_KEY_PRESSED && PEADINOKLKN.IsRandom)
				{
					_ModelsConditions[index].IsKeyCheckEnabled = false;
				}
				if (nFCCFMOMPHG.Type == EventAnimation.EventAnimationType.EVENT_HIT)
				{
					if (PEADINOKLKN.Owner.ReceivedCritical)
					{
						if (PEADINOKLKN.Owner.IsInShock())
						{
							nFCCFMOMPHG.HitType = "Critical|Shock";
						}
						else
						{
							nFCCFMOMPHG.HitType = "Critical";
						}
					}
					else if (PEADINOKLKN.Owner.IsInShock())
					{
						nFCCFMOMPHG.HitType = "Shock";
					}
					else if (PEADINOKLKN.Owner.LastStrike.IsBlocked)
					{
						nFCCFMOMPHG.HitType = "Block";
					}
				}
				if (cPFMGFAFAFB.CheckConditions(ACENLMONNPA, null, nFCCFMOMPHG))
				{
					cPFMGFAFAFB.CheckConditions(ACENLMONNPA, null, nFCCFMOMPHG);
					QueueTrigger(cPFMGFAFAFB, ACENLMONNPA);
				}
				if (nFCCFMOMPHG.Type == EventAnimation.EventAnimationType.EVENT_HIT)
				{
					nFCCFMOMPHG.HitType = string.Empty;
				}
				if (PEADINOKLKN.Type == EventAnimation.EventAnimationType.EVENT_KEY_PRESSED && PEADINOKLKN.IsRandom)
				{
					_ModelsConditions[index].IsKeyCheckEnabled = true;
				}
			}
		}
	}

	private void ChooseAnimations(List<Model> INNLAFHKJNI, List<List<SelectInfo>> GLEOPGKNDAO)
	{
		int num = 0;
		foreach (List<SelectInfo> item in GLEOPGKNDAO)
		{
			if (0 < item.Count)
			{
				PlayAnimation(INNLAFHKJNI[num], item);
			}
			num++;
		}
	}

	private void QueueTrigger(Trigger CPBHKJFPFJB, Model ACENLMONNPA)
	{
		TriggerStruct item = new TriggerStruct(CPBHKJFPFJB, ACENLMONNPA);
		_PendingTriggers.Add(item);
	}

	private void ClearTriggers()
	{
		foreach (TriggerStruct item in _PendingTriggers)
		{
		}
		_PendingTriggers.Clear();
	}

	private void RemoveEventsForModel(Model ACENLMONNPA, List<EventModelDelayed> CDIELLOLINA)
	{
		for (int num = CDIELLOLINA.Count - 1; num >= 0; num--)
		{
			if (CDIELLOLINA[num].Owner == ACENLMONNPA || CDIELLOLINA[num].Target == ACENLMONNPA)
			{
				CDIELLOLINA.RemoveAt(num);
			}
		}
	}
}
