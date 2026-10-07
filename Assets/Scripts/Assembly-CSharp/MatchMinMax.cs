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

	public void Parse(XmlNode node, PerkCondition IOFGGOCEIAM, PerkInfoItem IOHONODPIIO)
	{
		XmlAttribute cJBEMNNNHDM = node.Attributes["Min"];
		if (!cJBEMNNNHDM.Empty())
		{
			string bLLCOEAOJGF = cJBEMNNNHDM.GetStringOrDefault(string.Empty);
			GetMinFunction().Parse(bLLCOEAOJGF);
			SetMinUnbounded(false);
		}
		XmlAttribute cJBEMNNNHDM2 = node.Attributes["Max"];
		if (!cJBEMNNNHDM2.Empty())
		{
			string bLLCOEAOJGF2 = cJBEMNNNHDM2.GetStringOrDefault(string.Empty);
			GetMaxFunction().Parse(bLLCOEAOJGF2);
			SetMaxUnbounded(false);
		}
		GetMinFunction().SetFunctionCallback(IOHONODPIIO.EvaluateFunctionCallback);
		GetMinFunction().SetVariableCallback(IOHONODPIIO.OnFunctionPreCallback);
		GetMinFunction().set_Target(IOFGGOCEIAM);
		GetMaxFunction().SetFunctionCallback(IOHONODPIIO.EvaluateFunctionCallback);
		GetMaxFunction().SetVariableCallback(IOHONODPIIO.OnFunctionPreCallback);
		GetMaxFunction().set_Target(IOFGGOCEIAM);
	}

	public void EvaluateFunctions()
	{
		FunctionResult dEIHAOLOPLC = GetMinFunction().Calculate();
		FunctionResult dEIHAOLOPLC2 = GetMaxFunction().Calculate();
		SetMinValue(dEIHAOLOPLC.Value.ToFloat());
		SetMaxValue(dEIHAOLOPLC2.Value.ToFloat());
	}
}
