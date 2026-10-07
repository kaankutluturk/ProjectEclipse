using System.Diagnostics;
using System.Xml;

public class MatchMinMax
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float minValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float maxValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool minUnbounded;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool maxUnbounded;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FunctionExtension minFunction;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FunctionExtension maxFunction;

	public float MinValue
	{
		get
		{
			return GetMinValue();
		}
		set
		{
			SetMinValue(value);
		}
	}

	public float MaxValue
	{
		get
		{
			return GetMaxValue();
		}
		set
		{
			SetMaxValue(value);
		}
	}

	public bool MinUnbounded
	{
		get
		{
			return GetMinUnbounded();
		}
		set
		{
			SetMinUnbounded(value);
		}
	}

	public bool MaxUnbounded
	{
		get
		{
			return GetMaxUnbounded();
		}
		set
		{
			SetMaxUnbounded(value);
		}
	}

	public FunctionExtension MinFunction
	{
		get
		{
			return GetMinFunction();
		}
		protected set
		{
			SetMinFunction(value);
		}
	}

	public FunctionExtension MaxFunction
	{
		get
		{
			return GetMaxFunction();
		}
		protected set
		{
			SetMaxFunction(value);
		}
	}

	public MatchMinMax()
	{
		SetMinValue(0f);
		SetMaxValue(0f);
		SetMinUnbounded(true);
		SetMaxUnbounded(true);
		SetMinFunction(new FunctionExtension());
		SetMaxFunction(new FunctionExtension());
	}

	public float GetMinValue()
	{
		return minValue;
	}

	public void SetMinValue(float value)
	{
		minValue = value;
	}

	public float GetMaxValue()
	{
		return maxValue;
	}

	public void SetMaxValue(float value)
	{
		maxValue = value;
	}

	public bool GetMinUnbounded()
	{
		return minUnbounded;
	}

	public void SetMinUnbounded(bool value)
	{
		minUnbounded = value;
	}

	public bool GetMaxUnbounded()
	{
		return maxUnbounded;
	}

	public void SetMaxUnbounded(bool value)
	{
		maxUnbounded = value;
	}

	public FunctionExtension GetMinFunction()
	{
		return minFunction;
	}

	protected void SetMinFunction(FunctionExtension value)
	{
		minFunction = value;
	}

	public FunctionExtension GetMaxFunction()
	{
		return maxFunction;
	}

	protected void SetMaxFunction(FunctionExtension value)
	{
		maxFunction = value;
	}

	public void Parse(XmlNode node, PerkCondition condition, PerkInfoItem perk)
	{
		XmlAttribute minAttribute = node.Attributes["Min"];
		if (!minAttribute.Empty())
		{
			string minExpression = minAttribute.GetStringOrDefault(string.Empty);
			GetMinFunction().Parse(minExpression);
			SetMinUnbounded(false);
		}
		XmlAttribute maxAttribute = node.Attributes["Max"];
		if (!maxAttribute.Empty())
		{
			string maxFunctionText = maxAttribute.GetStringOrDefault(string.Empty);
			GetMaxFunction().Parse(maxFunctionText);
			SetMaxUnbounded(false);
		}
		GetMinFunction().SetFunctionCallback(perk.EvaluateFunctionCallback);
		GetMinFunction().SetVariableCallback(perk.OnFunctionPreCallback);
		GetMinFunction().set_Target(condition);
		GetMaxFunction().SetFunctionCallback(perk.EvaluateFunctionCallback);
		GetMaxFunction().SetVariableCallback(perk.OnFunctionPreCallback);
		GetMaxFunction().set_Target(condition);
	}

	public void EvaluateFunctions()
	{
		FunctionResult result = GetMinFunction().Calculate();
		FunctionResult maxResult = GetMaxFunction().Calculate();
		SetMinValue(result.Value.ToFloat());
		SetMaxValue(maxResult.Value.ToFloat());
	}
}
