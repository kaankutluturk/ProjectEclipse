using System.Collections.Generic;

public class FunctionResult
{
	public string Value;

	public int ToInt()
	{
		float result;
		if (!float.TryParse(Value, out result))
		{
			Dictionary<string, RpnParser.VariableDelegate> variableDelegates = new Dictionary<string, RpnParser.VariableDelegate>();
			Dictionary<string, RpnParser.ParameterDelegate> parameterDelegates = new Dictionary<string, RpnParser.ParameterDelegate>();
			RpnParser.init(variableDelegates, parameterDelegates);
			RpnParser.Formula formula = new RpnParser.Formula(Value);
			object obj = formula.Calculate();
			if (!float.TryParse(obj.ToString(), out result))
			{
				result = 0f;
			}
		}
		return (int)result;
	}

	public float ToFloat()
	{
		float result;
		if (!float.TryParse(Value, out result))
		{
			Dictionary<string, RpnParser.VariableDelegate> variableDelegates = new Dictionary<string, RpnParser.VariableDelegate>();
			Dictionary<string, RpnParser.ParameterDelegate> parameterDelegates = new Dictionary<string, RpnParser.ParameterDelegate>();
			RpnParser.init(variableDelegates, parameterDelegates);
			RpnParser.Formula formula = new RpnParser.Formula(Value);
			object obj = formula.Calculate();
			if (!float.TryParse(obj.ToString(), out result))
			{
				return 0f;
			}
		}
		return result;
	}
}
