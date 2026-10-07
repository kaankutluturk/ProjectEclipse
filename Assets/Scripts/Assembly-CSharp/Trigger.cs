using System.Collections.Generic;

public class Trigger
{
	public class TriggerInside
	{
		public List<EventAnimation> Events = new List<EventAnimation>();

		public List<ConditionAnimation> Conditions = new List<ConditionAnimation>();

		public List<ConditionAnimation> ExtraConditions = new List<ConditionAnimation>();

		public List<ActionAnimation> Actions = new List<ActionAnimation>();

		public EventAnimation FindEvent(EventAnimation.EventAnimationType LFLGCDNKNJI)
		{
			foreach (EventAnimation item in Events)
			{
				if (item.Type == LFLGCDNKNJI)
				{
					return item;
				}
			}
			return null;
		}
	}

	private List<string> _TemplateNames = new List<string>();

	public TriggerInside Definition;

	public string Name;

	public virtual List<string> TemplateNames
	{
		get
		{
			return GetTemplateNames();
		}
	}

	public virtual ConditionKeys KeyConditions
	{
		get
		{
			return GetKeyConditions();
		}
	}

	public virtual List<ConditionKeys> AllKeyConditions
	{
		get
		{
			return GetAllKeyConditions();
		}
	}

	public Trigger()
	{
		Definition = new TriggerInside();
	}

	public virtual void Init()
	{
	}

	public virtual void AddTemplateName(string name)
	{
	}

	public virtual bool ContainsEvent(EventAnimation p_event)
	{
		foreach (EventAnimation item in Definition.Events)
		{
			if (item.IsEqual(p_event))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool CheckConditions(ModelConditions conditions, List<ConditionAnimation> JPGMNIFICDM = null, EventAnimation DOANBADPBGH = null)
	{
		List<ConditionAnimation> list = ((JPGMNIFICDM == null) ? Definition.Conditions : JPGMNIFICDM);
		if (DOANBADPBGH != null)
		{
			conditions.CurrentEvent = DOANBADPBGH;
			DOANBADPBGH.Conditions = conditions;
		}
		foreach (ConditionAnimation item in list)
		{
			if (!item.IsEqual(conditions))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool CheckConditions(Model ACENLMONNPA, List<ConditionAnimation> JPGMNIFICDM = null, EventAnimation DOANBADPBGH = null)
	{
		List<ConditionAnimation> list = ((JPGMNIFICDM == null) ? Definition.Conditions : JPGMNIFICDM);
		foreach (ConditionAnimation item in list)
		{
			ModelType.ModelTargetType kEIDBIOIFGA = item.GetTargetModelType();
			Model fGCODGKLHED = item.ResolveTargetModel(ACENLMONNPA, kEIDBIOIFGA);
			if (fGCODGKLHED == null)
			{
				return false;
			}
			ModelConditions dGJJDPIAEAO = fGCODGKLHED.GetConditions();
			if (DOANBADPBGH != null)
			{
				dGJJDPIAEAO.CurrentEvent = DOANBADPBGH;
				DOANBADPBGH.Conditions = dGJJDPIAEAO;
			}
			item.ApplyTargetModelType(ModelType.ModelTargetType.MODEL_THIS);
			bool flag = false;
			if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					flag = eLFKOGJJNMN.EvaluateWithModel(ACENLMONNPA.GetConditions(), ACENLMONNPA, DOANBADPBGH);
				}
			}
			else
			{
				flag = item.IsEqual(fGCODGKLHED.GetConditions());
			}
			if (!flag)
			{
				item.SetTargetModelType(kEIDBIOIFGA);
				return false;
			}
			item.SetTargetModelType(kEIDBIOIFGA);
		}
		return true;
	}

	public virtual List<string> GetTemplateNames()
	{
		return _TemplateNames;
	}

	public virtual ConditionKeys GetKeyConditions()
	{
		return FindFirstKeyConditions(Definition.Conditions);
	}

	public virtual List<ConditionKeys> GetAllKeyConditions()
	{
		List<ConditionKeys> list = new List<ConditionKeys>();
		CollectKeyConditions(Definition.Conditions, list);
		return list;
	}

	public static ConditionKeys AsConditionKeys(ConditionAnimation IOFGGOCEIAM)
	{
		if (IOFGGOCEIAM.Type == ConditionAnimation.ConditionType.KEYS)
		{
			return IOFGGOCEIAM as ConditionKeys;
		}
		return null;
	}

	public virtual void PreloadEffects(List<string> MNDEJPFJODO = null)
	{
		string text = "Textures/Effects/Magic/";
		foreach (ActionAnimation item in Definition.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.EFFECT)
			{
				ActionEffect jFJGGMEJDPG = (ActionEffect)item;
				string oNNKJLOGHGH = text + jFJGGMEJDPG.GetSequence();
				LocationSpriteCache.LoadAtlasSprites(oNNKJLOGHGH);
			}
		}
	}

	public virtual void PreloadSounds()
	{
		foreach (ActionAnimation item in Definition.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.SOUND)
			{
				ActionSound nMLKJLJHCIA = (ActionSound)item;
				Sound.LoadSound(nMLKJLJHCIA.get_Name());
			}
		}
	}

	public virtual void MergeDefinition(TriggerInside KECIIKEIJBH)
	{
		if (Definition != null)
		{
			AddEvents(KECIIKEIJBH.Events);
			AddConditions(KECIIKEIJBH.Conditions);
			AddActions(KECIIKEIJBH.Actions);
			AddExtraConditions(KECIIKEIJBH.ExtraConditions);
		}
	}

	public virtual void UpdateForObject(ModelObject OECPEDPMKCD, bool EKBOGDKIHIH, bool PHADJMAONJG, ModelObject MJCGOJBGFIE)
	{
		ModelNode aECCPADGGPG = null;
		UpdateConditions(Definition.Conditions, OECPEDPMKCD, EKBOGDKIHIH, PHADJMAONJG, MJCGOJBGFIE, aECCPADGGPG);
		foreach (ActionAnimation item in Definition.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.EFFECT)
			{
				ActionEffect jFJGGMEJDPG = (ActionEffect)item;
				jFJGGMEJDPG.UpdateNodes(OECPEDPMKCD, EKBOGDKIHIH, null, PHADJMAONJG, MJCGOJBGFIE);
			}
		}
	}

	public void ResetConditions(List<ConditionAnimation> AIDMEPEKEOL)
	{
		foreach (ConditionAnimation item in AIDMEPEKEOL)
		{
			if (item.Type == ConditionAnimation.ConditionType.DISTANCE)
			{
				ConditionDistance jNPIBKBDJAN = item as ConditionDistance;
				if (jNPIBKBDJAN != null)
				{
					jNPIBKBDJAN.ResetNodes();
				}
				else
				{
					GameLog.Error("conditionDistance is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.DIRECTION)
			{
				ConditionDirection cFCGJLJBOKI = item as ConditionDirection;
				if (cFCGJLJBOKI != null)
				{
					cFCGJLJBOKI.ResetNodes();
				}
				else
				{
					GameLog.Error("conditionDistance is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					List<ConditionAnimation> aIDMEPEKEOL = eLFKOGJJNMN.GetConditions();
					ResetConditions(aIDMEPEKEOL);
				}
				else
				{
					GameLog.Error("conditions is null");
				}
			}
		}
	}

	public virtual void ResetState()
	{
		ResetConditions(Definition.Conditions);
		foreach (ActionAnimation item in Definition.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.EFFECT)
			{
				ActionEffect jFJGGMEJDPG = (ActionEffect)item;
				jFJGGMEJDPG.ResetNodes();
			}
		}
	}

	public virtual bool MatchesName(string name)
	{
		return Name == name || HasTemplateName(name);
	}

	public virtual bool HasTemplateName(string IJBOAGICOON)
	{
		foreach (string item in _TemplateNames)
		{
			if (item == IJBOAGICOON)
			{
				return true;
			}
		}
		return false;
	}

	protected static ConditionKeys FindFirstKeyConditions(List<ConditionAnimation> conditions)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					List<ConditionAnimation> kDOGKKGDOBK = eLFKOGJJNMN.GetConditions();
					ConditionKeys bHDEBDIHDFM = FindFirstKeyConditions(kDOGKKGDOBK);
					if (bHDEBDIHDFM != null)
					{
						return bHDEBDIHDFM;
					}
				}
				else
				{
					GameLog.Error("conditionList is null");
				}
			}
			else
			{
				ConditionKeys bHDEBDIHDFM2 = InfoAnimation.AsKeysCondition(item);
				if (bHDEBDIHDFM2 != null)
				{
					return bHDEBDIHDFM2;
				}
			}
		}
		return null;
	}

	protected static void CollectKeyConditions(List<ConditionAnimation> conditions, List<ConditionKeys> GKHEPKGMEFI)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					List<ConditionAnimation> kDOGKKGDOBK = eLFKOGJJNMN.GetConditions();
					CollectKeyConditions(kDOGKKGDOBK, GKHEPKGMEFI);
				}
				else
				{
					GameLog.Error("conditionList is null");
				}
			}
			else
			{
				ConditionKeys bHDEBDIHDFM = InfoAnimation.AsKeysCondition(item);
				if (bHDEBDIHDFM != null)
				{
					GKHEPKGMEFI.Add(bHDEBDIHDFM);
				}
			}
		}
	}

	protected virtual void UpdateConditions(List<ConditionAnimation> conditions, ModelObject OECPEDPMKCD, bool EKBOGDKIHIH, bool PHADJMAONJG, ModelObject MJCGOJBGFIE, ModelNode AECCPADGGPG)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item == null)
			{
				continue;
			}
			if (item.Type == ConditionAnimation.ConditionType.DISTANCE)
			{
				ConditionDistance jNPIBKBDJAN = ((item == null) ? null : (item as ConditionDistance));
				if (jNPIBKBDJAN != null)
				{
					jNPIBKBDJAN.UpdateNodes(OECPEDPMKCD, EKBOGDKIHIH, AECCPADGGPG, PHADJMAONJG, MJCGOJBGFIE);
				}
				else
				{
					GameLog.Error("subcondition is null");
				}
			}
			if (item.Type == ConditionAnimation.ConditionType.DIRECTION)
			{
				ConditionDirection cFCGJLJBOKI = item as ConditionDirection;
				if (cFCGJLJBOKI != null)
				{
					cFCGJLJBOKI.UpdateNodes(OECPEDPMKCD, EKBOGDKIHIH, AECCPADGGPG, PHADJMAONJG, MJCGOJBGFIE);
				}
				else
				{
					GameLog.Error("subcondition is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					List<ConditionAnimation> kDOGKKGDOBK = eLFKOGJJNMN.GetConditions();
					UpdateConditions(kDOGKKGDOBK, OECPEDPMKCD, EKBOGDKIHIH, PHADJMAONJG, MJCGOJBGFIE, AECCPADGGPG);
				}
				else
				{
					GameLog.Error("subconditions is null");
				}
			}
		}
	}

	protected virtual void AddEvents(List<EventAnimation> value)
	{
		Definition.Events.AddRange(value);
	}

	protected virtual void AddConditions(List<ConditionAnimation> value)
	{
		Definition.Conditions.AddRange(value);
	}

	protected virtual void AddExtraConditions(List<ConditionAnimation> value)
	{
		Definition.ExtraConditions.AddRange(value);
	}

	protected virtual void AddActions(List<ActionAnimation> value)
	{
		Definition.Actions.AddRange(value);
	}
}
