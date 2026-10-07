using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class PerkAction : PerkObject
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _name;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _elementName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _namespace;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ActionType _type;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool _modificator;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private PerkTrigger _trigger;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FunctionExtension _frames;

	public string ElementName
	{
		get
		{
			return GetElementName();
		}
		protected set
		{
			SetElementName(value);
		}
	}

	public bool IsModificatorAction
	{
		get
		{
			return GetModificator();
		}
		protected set
		{
			set_Modificator(value);
		}
	}

	public PerkTrigger ActionTrigger
	{
		get
		{
			return GetTrigger();
		}
		protected set
		{
			SetTrigger(value);
		}
	}

	public FunctionExtension FramesFunction
	{
		get
		{
			return GetFrames();
		}
		protected set
		{
			set_Frames(value);
		}
	}

	public PerkAction()
	{
	}

	public PerkAction(PerkAction source)
		: base(source)
	{
		set_Name(source.get_Name());
		SetElementName(source.GetElementName());
		set_Namespace(source.GetNamespace());
		set_Type(source.get_Type());
		set_Modificator(source.GetModificator());
		SetTrigger(source.GetTrigger());
		set_Frames(source.GetFrames());
	}

	public string get_Name()
	{
		return _name;
	}

	protected void set_Name(string value)
	{
		_name = value;
	}

	public string GetElementName()
	{
		return _elementName;
	}

	protected void SetElementName(string value)
	{
		_elementName = value;
	}

	public string GetNamespace()
	{
		return _namespace;
	}

	protected void set_Namespace(string value)
	{
		_namespace = value;
	}

	public ActionType get_Type()
	{
		return _type;
	}

	protected void set_Type(ActionType value)
	{
		_type = value;
	}

	public bool GetModificator()
	{
		return _modificator;
	}

	protected void set_Modificator(bool value)
	{
		_modificator = value;
	}

	public PerkTrigger GetTrigger()
	{
		return _trigger;
	}

	protected void SetTrigger(PerkTrigger value)
	{
		_trigger = value;
	}

	public FunctionExtension GetFrames()
	{
		return _frames;
	}

	protected void set_Frames(FunctionExtension value)
	{
		_frames = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		SetElementName(node.Name);
		set_Name(node.Attributes["Name"].GetStringOrDefault(string.Empty));
		string text = node.Attributes["Frames"].GetStringOrDefault(string.Empty);
		if (text != null && !text.Equals(string.Empty))
		{
			set_Frames(new FunctionExtension());
			GetFrames().Parse(text);
			GetFrames().SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
			GetFrames().SetVariableCallback(GetPerk().OnFunctionPreCallback);
			GetFrames().set_Target(this);
		}
		set_Namespace(node.Attributes["Namespace"].GetStringOrDefault(string.Empty));
	}

	public Model ResolveTargetModel(Model sourceModel)
	{
		if (TargetPlayer == PlayerType.PLAYER_ME)
		{
			return sourceModel;
		}
		if (TargetPlayer == PlayerType.PLAYER_ENEMY)
		{
			return sourceModel.GetCombatTarget();
		}
		return null;
	}

	public static List<PerkAction> Create(XmlNode node, PerkInfoItem perk, PerkTrigger trigger)
	{
		List<PerkAction> list = new List<PerkAction>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkAction action = null;
			switch (childNode.Name)
			{
			case "ModIcon":
				action = new PerkActionShowIcon();
				break;
			case "ModAttributes":
				action = new PerkActionSetAttributes();
				break;
			case "ModFlag":
				action = new PerkActionFlag();
				break;
			case "ClearMods":
				action = new PerkActionClearAction();
				break;
			case "DisableInterval":
				action = new PerkActionDisableInterval();
				break;
			case "SetHit":
				action = new PerkActionSetHit();
				break;
			case "AddBullets":
				action = new PerkActionAddBullets();
				break;
			case "AddMagicCharge":
				action = new PerkActionAddMagicCharge();
				break;
			case "SetModFrames":
				action = new PerkActionSetModFrames();
				break;
			case "ApplyModEffect":
				action = new PerkActionSetModEffect();
				break;
			case "ModHealthChange":
				action = new ModHealthChange();
				break;
			case "Provoke":
				action = new PerkActionProvoke();
				break;
			case "SetTactic":
				action = new PerkActionSetTactics();
				break;
			case "Lifesteal":
				action = new PerkActionLifesteal();
				break;
			case "ModInvisibility":
				action = new ModInvisibility();
				break;
			case "ModVariable":
				action = new PerkActionVariable();
				break;
			case "SetVariable":
			case "SetRangeVariable":
				action = new PerkActionSetVariable();
				break;
			case "SetModVariable":
				action = new PerkActionVariable();
				break;
			case "SetCooldown":
				action = new PerkActionSetCooldown();
				break;
			case "ChangeImpulse":
				action = new PerkActionChangeImpulse();
				break;
			case "ChangeHitEffectScale":
				action = new PerkActionChangeHitEffectScale();
				break;
			case "ChangeAdditionalDamageValue":
				action = new PerkActionChangeAdditionalDamageValue();
				break;
			case "ChangeModelColor":
				action = new PerkActionChangeModelColor();
				break;
			case "SlowModel":
				action = new PerkActionSlowModel();
				break;
			case "TurnOffCollision":
				action = new PerkActionTurnOffCollision();
				break;
			case "Switch":
				action = new PerkActionSwitch();
				break;
			case "MarkPerkAsUsed":
				action = new PerkActionMarkUsed();
				break;
			case "PerkArea":
				action = new PerkActionArea();
				break;
			case "MoveModel":
				action = new PerkActionMoveModel();
				break;
			case "SetMovesVariable":
				action = new PerkActionSetMovesVariable();
				break;
			case "StealMagicMod":
				action = new PerkActionStealMagic();
				break;
			}
			if (action != null)
			{
				action.SetPerk(perk);
				action.SetTrigger(trigger);
				action.Parse(childNode);
				list.Add(action);
			}
		}
		return list;
	}

	public static PerkAction Clone(PerkAction source, PerkInfoItem perk, PerkTrigger trigger)
	{
		PerkAction action = null;
		switch (source.get_Type())
		{
		case ActionType.ACTION_SHOW_ICONS:
			action = new PerkActionShowIcon((PerkActionShowIcon)source);
			break;
		case ActionType.ACTION_SET_ATTRIBUTES:
			action = new PerkActionSetAttributes((PerkActionSetAttributes)source);
			break;
		case ActionType.ACTION_FLAG:
			action = new PerkActionFlag((PerkActionFlag)source);
			break;
		case ActionType.ACTION_CLEAR_ACTION:
			action = new PerkActionClearAction((PerkActionClearAction)source);
			break;
		case ActionType.ACTION_DISABLE_INTERVAL:
			action = new PerkActionDisableInterval((PerkActionDisableInterval)source);
			break;
		case ActionType.ACTION_SET_HIT:
			action = new PerkActionSetHit((PerkActionSetHit)source);
			break;
		case ActionType.ACTION_ADD_BULLETS:
			action = new PerkActionAddBullets((PerkActionAddBullets)source);
			break;
		case ActionType.ACTION_ADD_MAGIC:
			action = new PerkActionAddMagicCharge((PerkActionAddMagicCharge)source);
			break;
		case ActionType.ACTION_SET_MOD_FRAMES:
			action = new PerkActionSetModFrames((PerkActionSetModFrames)source);
			break;
		case ActionType.ACTION_MOD_EFFECT:
			action = new PerkActionSetModEffect((PerkActionSetModEffect)source);
			break;
		case ActionType.ACTION_MOD_HEALTH_CHANGE:
			action = new ModHealthChange((ModHealthChange)source);
			break;
		case ActionType.ACTION_PROVOKE:
			action = new PerkActionProvoke((PerkActionProvoke)source);
			break;
		case ActionType.ACTION_SET_TACTICS:
			action = new PerkActionSetTactics((PerkActionSetTactics)source);
			break;
		case ActionType.ACTION_LIFE_STEAL:
			action = new PerkActionLifesteal((PerkActionLifesteal)source);
			break;
		case ActionType.ACTION_INVISIBILITY:
			action = new ModInvisibility((ModInvisibility)source);
			break;
		case ActionType.ACTION_VARIABLE:
			action = new PerkActionVariable((PerkActionVariable)source);
			break;
		case ActionType.ACTION_SET_VARIABLE:
			action = new PerkActionSetVariable((PerkActionSetVariable)source);
			break;
		case ActionType.ACTION_SET_COOLDOWN:
			action = new PerkActionSetCooldown((PerkActionSetCooldown)source);
			break;
		case ActionType.ACTION_CHANGE_IMPULSE:
			action = new PerkActionChangeImpulse((PerkActionChangeImpulse)source);
			break;
		case ActionType.ACTION_CHANGE_HIT_EFFECT_SCALE:
			action = new PerkActionChangeHitEffectScale((PerkActionChangeHitEffectScale)source);
			break;
		case ActionType.ACTION_CHANGE_ADD_DAMAGE_VALUE:
			action = new PerkActionChangeAdditionalDamageValue((PerkActionChangeAdditionalDamageValue)source);
			break;
		case ActionType.ACTION_CHANGE_MODEL_COLOR:
			action = new PerkActionChangeModelColor((PerkActionChangeModelColor)source);
			break;
		case ActionType.ACTION_SLOW_MODEL:
			action = new PerkActionSlowModel((PerkActionSlowModel)source);
			break;
		case ActionType.ACTION_TURN_OFF_COLLISION:
			action = new PerkActionTurnOffCollision((PerkActionTurnOffCollision)source);
			break;
		case ActionType.ACTION_SWITCH:
			action = new PerkActionSwitch((PerkActionSwitch)source);
			break;
		case ActionType.ACTION_MARK_PERK_USED:
			action = new PerkActionMarkUsed((PerkActionMarkUsed)source);
			break;
		case ActionType.ACTION_PERK_AREA:
			action = new PerkActionArea((PerkActionArea)source);
			break;
		case ActionType.ACTION_MOVE_MODEL:
			action = new PerkActionMoveModel((PerkActionMoveModel)source);
			break;
		case ActionType.ACTION_SET_MOVES_VARIABLE:
			action = new PerkActionSetMovesVariable((PerkActionSetMovesVariable)source);
			break;
		case ActionType.ACTION_STEAL_MAGIC:
			action = new PerkActionStealMagic((PerkActionStealMagic)source);
			break;
		default:
			GameLog.Error("PerkAction.Clone PerkAction type is ActionType.ACTION_NONE");
			break;
		}
		if (action != null)
		{
			action.SetPerk(perk);
			action.SetTrigger(trigger);
		}
		return action;
	}
}

public class PerkActionChangeModelColor : PerkActionModificator
{
	public UnityEngine.Color Color = UnityEngine.Color.white;

	public PerkActionChangeModelColor() { }
	public PerkActionChangeModelColor(PerkActionChangeModelColor source) : base(source) { Color = source.Color; }

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_CHANGE_MODEL_COLOR);
		string value = node.Attributes["Color"].GetStringOrDefault("#FFFFFFFF").TrimStart('#');
		uint packed;
		if (uint.TryParse(value, System.Globalization.NumberStyles.HexNumber,
			System.Globalization.CultureInfo.InvariantCulture, out packed))
		{
			if (value.Length <= 6)
				packed = (packed << 8) | 255u;
			Color = new UnityEngine.Color32((byte)(packed >> 24), (byte)(packed >> 16),
				(byte)(packed >> 8), (byte)packed);
		}
	}
}

public class PerkActionSlowModel : PerkActionModificator
{
	public int Speed = 1;
	public bool IsRulePerk;

	public PerkActionSlowModel() { }
	public PerkActionSlowModel(PerkActionSlowModel source) : base(source)
	{
		Speed = source.Speed;
		IsRulePerk = source.IsRulePerk;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_SLOW_MODEL);
		Speed = System.Math.Max(1, node.Attributes["Speed"].ParseInt(1));
		IsRulePerk = node.Attributes["IsRulePerk"].ParseBool();
	}
}

public class PerkActionTurnOffCollision : PerkActionModificator
{
	public PerkActionTurnOffCollision() { }
	public PerkActionTurnOffCollision(PerkActionTurnOffCollision source) : base(source) { }
	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_TURN_OFF_COLLISION);
	}
}

public class PerkActionMarkUsed : PerkAction
{
	public PerkActionMarkUsed() { }
	public PerkActionMarkUsed(PerkActionMarkUsed source) : base(source) { }
	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_MARK_PERK_USED);
	}
}

public class PerkActionSwitch : PerkAction
{
	public class Branch
	{
		public FunctionExtension Value;
		public List<PerkAction> Actions;
	}

	public FunctionExtension Value;
	public readonly List<Branch> Cases = new List<Branch>();
	public List<PerkAction> DefaultActions = new List<PerkAction>();

	public PerkActionSwitch() { }
	public PerkActionSwitch(PerkActionSwitch source) : base(source)
	{
		Value = source.Value;
		Cases.AddRange(source.Cases);
		DefaultActions.AddRange(source.DefaultActions);
	}

	private FunctionExtension ParseFunction(string expression)
	{
		FunctionExtension function = new FunctionExtension();
		function.Parse(expression);
		function.SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
		function.SetVariableCallback(GetPerk().OnFunctionPreCallback);
		function.set_Target(this);
		return function;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_SWITCH);
		Value = ParseFunction(node.Attributes["Value"].GetStringOrDefault("0"));
		foreach (XmlNode branchNode in node.ChildNodes)
		{
			if (branchNode.Name == "Case")
			{
				Branch branch = new Branch();
				branch.Value = ParseFunction(branchNode.Attributes["Value"].GetStringOrDefault("0"));
				branch.Actions = Create(branchNode, GetPerk(), GetTrigger());
				Cases.Add(branch);
			}
			else if (branchNode.Name == "Default")
			{
				DefaultActions = Create(branchNode, GetPerk(), GetTrigger());
			}
		}
	}

	public List<PerkAction> SelectActions()
	{
		string actual = Value.Calculate().Value;
		foreach (Branch branch in Cases)
		{
			string expected = branch.Value.Calculate().Value;
			float actualNumber;
			float expectedNumber;
			if ((float.TryParse(actual, out actualNumber) && float.TryParse(expected, out expectedNumber) &&
				System.Math.Abs(actualNumber - expectedNumber) < 0.0001f) || actual == expected)
				return branch.Actions;
		}
		return DefaultActions;
	}
}

public class PerkActionArea : PerkActionModificator
{
	public float Width;
	public string FileName = string.Empty;
	public float ShiftY;
	public FunctionExtension PositionX;

	public PerkActionArea() { }
	public PerkActionArea(PerkActionArea source) : base(source)
	{
		Width = source.Width; FileName = source.FileName; ShiftY = source.ShiftY; PositionX = source.PositionX;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_PERK_AREA);
		Width = node.Attributes["Width"].ParseFloat(400f);
		FileName = node.Attributes["FileName"].GetStringOrDefault(string.Empty);
		ShiftY = node.Attributes["ShiftY"].ParseFloat();
		PositionX = new FunctionExtension();
		PositionX.Parse(node.Attributes["PositionX"].GetStringOrDefault("0"));
		PositionX.SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
		PositionX.SetVariableCallback(GetPerk().OnFunctionPreCallback);
		PositionX.set_Target(this);
	}
}

public class PerkActionMoveModel : PerkAction
{
	public FunctionExtension OffsetX;
	public PerkActionMoveModel() { }
	public PerkActionMoveModel(PerkActionMoveModel source) : base(source) { OffsetX = source.OffsetX; }
	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_MOVE_MODEL);
		OffsetX = new FunctionExtension();
		OffsetX.Parse(node.Attributes["PositionOffsetX"].GetStringOrDefault("0"));
		OffsetX.SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
		OffsetX.SetVariableCallback(GetPerk().OnFunctionPreCallback);
		OffsetX.set_Target(this);
	}
}

public class PerkActionSetMovesVariable : PerkAction
{
	public FunctionExtension Value;
	public PerkActionSetMovesVariable() { }
	public PerkActionSetMovesVariable(PerkActionSetMovesVariable source) : base(source) { Value = source.Value; }
	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Name(node.Attributes["Variable"].GetStringOrDefault(string.Empty));
		set_Type(ActionType.ACTION_SET_MOVES_VARIABLE);
		Value = new FunctionExtension();
		Value.Parse(node.Attributes["Value"].GetStringOrDefault("0"));
		Value.SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
		Value.SetVariableCallback(GetPerk().OnFunctionPreCallback);
		Value.set_Target(this);
	}
}

public class PerkActionStealMagic : PerkActionModificator
{
	public FunctionExtension MagicName;
	public PerkActionStealMagic() { }
	public PerkActionStealMagic(PerkActionStealMagic source) : base(source) { MagicName = source.MagicName; }
	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_STEAL_MAGIC);
		MagicName = new FunctionExtension();
		MagicName.Parse(node.Attributes["MagicName"].GetStringOrDefault(string.Empty));
		MagicName.SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
		MagicName.SetVariableCallback(GetPerk().OnFunctionPreCallback);
		MagicName.set_Target(this);
	}
}
