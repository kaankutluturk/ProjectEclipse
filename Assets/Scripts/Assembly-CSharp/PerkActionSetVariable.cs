using System.Diagnostics;
using System.Xml;

public class PerkActionSetVariable : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FunctionExtension _value;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool _hasMinValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FunctionExtension _minValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool _hasMaxValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FunctionExtension _maxValue;

	public bool HasMinValue
	{
		get
		{
			return GetHasMinValue();
		}
		protected set
		{
			SetHasMinValue(value);
		}
	}

	public FunctionExtension MinValue
	{
		get
		{
			return GetMinValue();
		}
		protected set
		{
			SetMinValue(value);
		}
	}

	public bool HasMaxValue
	{
		get
		{
			return GetHasMaxValue();
		}
		protected set
		{
			SetHasMaxValue(value);
		}
	}

	public FunctionExtension MaxValue
	{
		get
		{
			return GetMaxValue();
		}
		protected set
		{
			SetMaxValue(value);
		}
	}

	public PerkActionSetVariable()
	{
	}

	public PerkActionSetVariable(PerkActionSetVariable NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_Value(NOLFMPDGCOC.GetValue());
		SetHasMinValue(NOLFMPDGCOC.GetHasMinValue());
		SetMinValue(NOLFMPDGCOC.GetMinValue());
		SetHasMaxValue(NOLFMPDGCOC.GetHasMaxValue());
		SetMaxValue(NOLFMPDGCOC.GetMaxValue());
	}

	public FunctionExtension GetValue()
	{
		return _value;
	}

	protected void set_Value(FunctionExtension value)
	{
		_value = value;
	}

	public bool GetHasMinValue()
	{
		return _hasMinValue;
	}

	protected void SetHasMinValue(bool value)
	{
		_hasMinValue = value;
	}

	public FunctionExtension GetMinValue()
	{
		return _minValue;
	}

	protected void SetMinValue(FunctionExtension value)
	{
		_minValue = value;
	}

	public bool GetHasMaxValue()
	{
		return _hasMaxValue;
	}

	protected void SetHasMaxValue(bool value)
	{
		_hasMaxValue = value;
	}

	public FunctionExtension GetMaxValue()
	{
		return _maxValue;
	}

	protected void SetMaxValue(FunctionExtension value)
	{
		_maxValue = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_SET_VARIABLE);
		string text = node.Attributes["Value"].GetStringOrDefault(string.Empty);
		if (text != null && text != string.Empty)
		{
			set_Value(new FunctionExtension());
			GetValue().Parse(text);
			GetValue().SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
			GetValue().SetVariableCallback(GetPerk().OnFunctionPreCallback);
			GetValue().set_Target(this);
		}
		XmlAttribute xmlAttribute = node.Attributes["MinValue"];
		SetHasMinValue(xmlAttribute != null);
		string text2 = xmlAttribute.GetStringOrDefault(string.Empty);
		if (text2 != null && text2 != string.Empty)
		{
			SetMinValue(new FunctionExtension());
			GetMinValue().Parse(text2);
			GetMinValue().SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
			GetMinValue().SetVariableCallback(GetPerk().OnFunctionPreCallback);
			GetMinValue().set_Target(this);
		}
		XmlAttribute xmlAttribute2 = node.Attributes["MaxValue"];
		SetHasMaxValue(xmlAttribute2 != null);
		string text3 = xmlAttribute2.GetStringOrDefault(string.Empty);
		if (text3 != null && text3 != string.Empty)
		{
			SetMaxValue(new FunctionExtension());
			GetMaxValue().Parse(text3);
			GetMaxValue().SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
			GetMaxValue().SetVariableCallback(GetPerk().OnFunctionPreCallback);
			GetMaxValue().set_Target(this);
		}
	}
}
