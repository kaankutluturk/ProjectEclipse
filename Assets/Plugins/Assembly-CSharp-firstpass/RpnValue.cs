using System;
using System.Globalization;
using System.Runtime.CompilerServices;

public class RpnValue<T>
{
	private const string CalculatedPrefix = "?CLC_";

	private const string FunctionPrefix = "?";

	private bool isConst;

	private T value;

	private RpnParser.Formula formula;

	public T Value
	{
		get
		{
			return GetValue();
		}
	}

	public RpnValue(T value)
	{
		this.value = value;
		isConst = true;
	}

	public RpnValue(string formulaText, bool flag = false)
	{
		formula = new RpnParser.Formula(formulaText);
		if (formulaText.Contains("?CLC_") || !formulaText.Contains("?"))
		{
			value = ConvertTo(formula.Calculate().ToString());
			isConst = true;
			formula = null;
		}
	}

	[SpecialName]
	public static T op_Implicit(global::RpnValue<T> rpnValue)
	{
		return rpnValue.GetValue();
	}

	[SpecialName]
	public static global::RpnValue<T> op_Implicit(T value)
	{
		return new global::RpnValue<T>(value);
	}

	[SpecialName]
	public static global::RpnValue<T> op_Implicit(string formulaText)
	{
		return new global::RpnValue<T>(formulaText);
	}

	private static T ConvertTo(string text)
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(int))
		{
			return (T)(object)int.Parse(text);
		}
		if (typeFromHandle == typeof(float))
		{
			return (T)(object)float.Parse(text, CultureInfo.InvariantCulture);
		}
		if (typeFromHandle == typeof(bool))
		{
			return (T)(object)ParseBool(text);
		}
		if (typeFromHandle == typeof(string))
		{
			return (T)(object)text;
		}
		return default(T);
	}

	private static bool ParseBool(string text)
	{
		return text.ToLower() == "true" || text == "1";
	}

	public T GetValue()
	{
		if (isConst)
		{
			return value;
		}
		return ConvertTo(formula.Calculate().ToString());
	}
}
