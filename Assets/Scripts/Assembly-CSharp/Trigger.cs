using System.Collections.Generic;

public class Trigger
{
	public class TriggerInside
	{
		public List<EventAnimation> Events = new List<EventAnimation>();

		public List<ConditionAnimation> Conditions = new List<ConditionAnimation>();

		public List<ConditionAnimation> ExtraConditions = new List<ConditionAnimation>();

		public List<ActionAnimation> Actions = new List<ActionAnimation>();

		public EventAnimation FindEvent(EventAnimation.EventAnimationType eventType)
		{
			foreach (EventAnimation item in Events)
			{
				if (item.Type == eventType)
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

	public virtual bool CheckConditions(ModelConditions conditions, List<ConditionAnimation> conditionsToCheck = null, EventAnimation eventAnimation = null)
	{
		List<ConditionAnimation> list = ((conditionsToCheck == null) ? Definition.Conditions : conditionsToCheck);
		if (eventAnimation != null)
		{
			conditions.CurrentEvent = eventAnimation;
			eventAnimation.Conditions = conditions;
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

	public virtual bool CheckConditions(Model model, List<ConditionAnimation> conditionsToCheck = null, EventAnimation eventAnimation = null)
	{
		List<ConditionAnimation> list = ((conditionsToCheck == null) ? Definition.Conditions : conditionsToCheck);
		foreach (ConditionAnimation item in list)
		{
			ModelType.ModelTargetType targetType = item.GetTargetModelType();
			Model targetModel = item.ResolveTargetModel(model, targetType);
			if (targetModel == null)
			{
				return false;
			}
			ModelConditions targetConditions = targetModel.GetConditions();
			if (eventAnimation != null)
			{
				targetConditions.CurrentEvent = eventAnimation;
				eventAnimation.Conditions = targetConditions;
			}
			item.ApplyTargetModelType(ModelType.ModelTargetType.MODEL_THIS);
			bool flag = false;
			if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList conditionList = item as ConditionList;
				if (conditionList != null)
				{
					flag = conditionList.EvaluateWithModel(model.GetConditions(), model, eventAnimation);
				}
			}
			else
			{
				flag = item.IsEqual(targetModel.GetConditions());
			}
			if (!flag)
			{
				item.SetTargetModelType(targetType);
				return false;
			}
			item.SetTargetModelType(targetType);
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

	public static ConditionKeys AsConditionKeys(ConditionAnimation condition)
	{
		if (condition.Type == ConditionAnimation.ConditionType.KEYS)
		{
			return condition as ConditionKeys;
		}
		return null;
	}

	public virtual void PreloadEffects(List<string> effectNames = null)
	{
		string text = "Textures/Effects/Magic/";
		foreach (ActionAnimation item in Definition.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.EFFECT)
			{
				ActionEffect effect = (ActionEffect)item;
				string atlasName = text + effect.GetSequence();
				LocationSpriteCache.LoadAtlasSprites(atlasName);
			}
		}
	}

	public virtual void PreloadSounds()
	{
		foreach (ActionAnimation item in Definition.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.SOUND)
			{
				ActionSound soundAction = (ActionSound)item;
				Sound.LoadSound(soundAction.get_Name());
			}
		}
	}

	public virtual void MergeDefinition(TriggerInside sourceDefinition)
	{
		if (Definition != null)
		{
			AddEvents(sourceDefinition.Events);
			AddConditions(sourceDefinition.Conditions);
			AddActions(sourceDefinition.Actions);
			AddExtraConditions(sourceDefinition.ExtraConditions);
		}
	}

	public virtual void UpdateForObject(ModelObject modelObject, bool isPlayer, bool isChildObject, ModelObject childOwner)
	{
		ModelNode modelNode = null;
		UpdateConditions(Definition.Conditions, modelObject, isPlayer, isChildObject, childOwner, modelNode);
		foreach (ActionAnimation item in Definition.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.EFFECT)
			{
				ActionEffect effect = (ActionEffect)item;
				effect.UpdateNodes(modelObject, isPlayer, null, isChildObject, childOwner);
			}
		}
	}

	public void ResetConditions(List<ConditionAnimation> conditions)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.DISTANCE)
			{
				ConditionDistance distanceCondition = item as ConditionDistance;
				if (distanceCondition != null)
				{
					distanceCondition.ResetNodes();
				}
				else
				{
					GameLog.Error("conditionDistance is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.DIRECTION)
			{
				ConditionDirection directionCondition = item as ConditionDirection;
				if (directionCondition != null)
				{
					directionCondition.ResetNodes();
				}
				else
				{
					GameLog.Error("conditionDistance is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList conditionList = item as ConditionList;
				if (conditionList != null)
				{
					List<ConditionAnimation> nestedConditions = conditionList.GetConditions();
					ResetConditions(nestedConditions);
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
				ActionEffect effect = (ActionEffect)item;
				effect.ResetNodes();
			}
		}
	}

	public virtual bool MatchesName(string name)
	{
		return Name == name || HasTemplateName(name);
	}

	public virtual bool HasTemplateName(string templateName)
	{
		foreach (string item in _TemplateNames)
		{
			if (item == templateName)
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
				ConditionList conditionList = item as ConditionList;
				if (conditionList != null)
				{
					List<ConditionAnimation> nestedConditions = conditionList.GetConditions();
					ConditionKeys keysCondition = FindFirstKeyConditions(nestedConditions);
					if (keysCondition != null)
					{
						return keysCondition;
					}
				}
				else
				{
					GameLog.Error("conditionList is null");
				}
			}
			else
			{
				ConditionKeys keysCondition = InfoAnimation.AsKeysCondition(item);
				if (keysCondition != null)
				{
					return keysCondition;
				}
			}
		}
		return null;
	}

	protected static void CollectKeyConditions(List<ConditionAnimation> conditions, List<ConditionKeys> keyConditions)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList conditionList = item as ConditionList;
				if (conditionList != null)
				{
					List<ConditionAnimation> nestedConditions = conditionList.GetConditions();
					CollectKeyConditions(nestedConditions, keyConditions);
				}
				else
				{
					GameLog.Error("conditionList is null");
				}
			}
			else
			{
				ConditionKeys keysCondition = InfoAnimation.AsKeysCondition(item);
				if (keysCondition != null)
				{
					keyConditions.Add(keysCondition);
				}
			}
		}
	}

	protected virtual void UpdateConditions(List<ConditionAnimation> conditions, ModelObject modelObject, bool isPlayer, bool isChildObject, ModelObject childOwner, ModelNode modelNode)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item == null)
			{
				continue;
			}
			if (item.Type == ConditionAnimation.ConditionType.DISTANCE)
			{
				ConditionDistance distanceCondition = ((item == null) ? null : (item as ConditionDistance));
				if (distanceCondition != null)
				{
					distanceCondition.UpdateNodes(modelObject, isPlayer, modelNode, isChildObject, childOwner);
				}
				else
				{
					GameLog.Error("subcondition is null");
				}
			}
			if (item.Type == ConditionAnimation.ConditionType.DIRECTION)
			{
				ConditionDirection directionCondition = item as ConditionDirection;
				if (directionCondition != null)
				{
					directionCondition.UpdateNodes(modelObject, isPlayer, modelNode, isChildObject, childOwner);
				}
				else
				{
					GameLog.Error("subcondition is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList conditionList = item as ConditionList;
				if (conditionList != null)
				{
					List<ConditionAnimation> nestedConditions = conditionList.GetConditions();
					UpdateConditions(nestedConditions, modelObject, isPlayer, isChildObject, childOwner, modelNode);
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
