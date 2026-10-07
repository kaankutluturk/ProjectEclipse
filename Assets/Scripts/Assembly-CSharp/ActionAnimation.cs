using System.Collections.Generic;
using System.Xml;

public class ActionAnimation
{
	public enum ActionType
	{
		CREATE_MODEL = 0,
		DELETE = 1,
		SOUND = 2,
		STOP_SOUND = 3,
		RANDOM_SOUND = 4,
		EFFECT = 5,
		STOP_EFFECT = 6,
		STOP_FOLLOW_EFFECT = 7,
		ADD_BULLETS = 8,
		SHAKE_SCREEN = 9,
		HIT_EFFECT = 10,
		ZOOM_EFFECT = 11,
		SET_COOLDOWN = 12,
		SET_END_STAGE = 13,
		PLAY_ANIMATION = 14,
		MOD_FLAG = 15,
		CAMERA_WEIGHT = 16,
		ENABLE_BOSS_ABILITY = 17
	}

	public enum StartTrigger
	{
		START_FRAME = 0,
		START_EVENT = 1
	}

	public class ActionStartParameters
	{
		public StartTrigger Trigger;

		public int Frame;

		public EventAnimation.EventAnimationType TriggerEvent;
	}

	private ActionType _actionType;

	private ActionStartParameters _startParameters = new ActionStartParameters();

	// best guess for name
	public int? ScheduledFrame => _startParameters.Trigger == StartTrigger.START_FRAME ? (int?)_startParameters.Frame : null;

	public void SetScheduledFrame(int frame)
	{
		if (!ScheduledFrame.HasValue) throw new System.InvalidOperationException("Event-driven actions have no scheduled frame.");
		_startParameters.Frame = frame;
	}


	private Model _Model;

	private ModelType.ModelTargetType _targetPlayer;

	// Recent move data can select between several effects/sounds on the same
	// frame by attaching a Conditions block to the individual action.  The
	// original decompilation only parsed conditions belonging to a Move, which
	// caused every variant (acid + frost clouds, all three auras, etc.) to run.
	private readonly List<ConditionAnimation> _Conditions = new List<ConditionAnimation>();

	public Model OwnerModel
	{
		get
		{
			return get_Model();
		}
		set
		{
			set_Model(value);
		}
	}

	public ModelType.ModelTargetType TargetPlayer
	{
		get
		{
			return GetTargetPlayer();
		}
	}

	public ActionAnimation(ActionType actionType)
	{
		_actionType = actionType;
		_Model = null;
	}

	public ActionType get_Type()
	{
		return _actionType;
	}

	public Model get_Model()
	{
		return _Model;
	}

	public void set_Model(Model value)
	{
		_Model = value;
	}

	public ModelType.ModelTargetType GetTargetPlayer()
	{
		return _targetPlayer;
	}

	public bool NeedStart(int frame)
	{
		return _startParameters.Trigger == StartTrigger.START_FRAME && _startParameters.Frame == frame;
	}

	public bool NeedStart(EventAnimation.EventAnimationType eventType)
	{
		return _startParameters.Trigger == StartTrigger.START_EVENT && _startParameters.TriggerEvent == eventType;
	}

	public bool CanVisit(Model model)
	{
		if (model == null)
			return false;
		ModelConditions modelConditions = model.GetConditions();
		if (modelConditions == null)
			return false;
		foreach (ConditionAnimation condition in _Conditions)
		{
			bool matches;
			if (condition.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList list = condition as ConditionList;
				matches = list != null && list.EvaluateWithModel(modelConditions, model, null);
			}
			else
			{
				Model target = condition.ResolveTargetModel(model, condition.GetTargetModelType());
				ModelConditions targetConditions = (target != null) ? target.GetConditions() : null;
				matches = targetConditions != null && condition.IsEqual(targetConditions);
			}
			if (!matches)
				return false;
		}
		return true;
	}

	public int GetConditionCount()
	{
		return _Conditions.Count;
	}

	public virtual void Visit(Model model)
	{
		model.StartAction(this);
	}

	protected virtual void Parse(XmlNode node)
	{
		XmlAttribute xmlAttribute = node.Attributes["Frame"];
		if (xmlAttribute != null)
		{
			_startParameters.Trigger = StartTrigger.START_FRAME;
			_startParameters.Frame = xmlAttribute.ParseInt();
		}
		else
		{
			_startParameters.Trigger = StartTrigger.START_EVENT;
			string eventName = node.Attributes["Event"].GetStringOrDefault(string.Empty);
			_startParameters.TriggerEvent = EventAnimation.GetTypeByName(eventName);
		}
		_targetPlayer = ModelType.ParseTargetType(node.Attributes["Player"].GetStringOrDefault("Me"));
		XmlNode conditions = node["Conditions"];
		if (conditions != null)
			ConditionsParser.ParseInside(_Conditions, conditions);
	}
}
