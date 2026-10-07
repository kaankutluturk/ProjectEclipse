using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

public class RpnParser
{
	public enum ItemKind
	{
		Operator = 0,
		Operand = 1
	}

	public enum OperandType
	{
		None = 0,
		Constant = 1,
		Variable = 2
	}

	public delegate double OperatorDelegate(List<double> arguments);

	public delegate object VariableDelegate();

	public delegate object ParameterDelegate(List<object> arguments);

	public enum OperatorArgCount
	{
		OperatorAddArgCount = 2,
		OperatorSubArgCount = OperatorAddArgCount,
		OperatorMultArgCount = OperatorAddArgCount,
		OperatorDivArgCount = OperatorAddArgCount,
		OperatorModArgCount = OperatorAddArgCount,
		OperatorPowArgCount = OperatorAddArgCount,
		OperatorRootArgCount = OperatorAddArgCount,
		OperatorBrackLArgCount = 0,
		OperatorBrackRArgCount = OperatorBrackLArgCount,
		OperatorSinArgCount = 1,
		OperatorCosArgCount = OperatorSinArgCount,
		OperatorMaxArgCount = OperatorAddArgCount,
		OperatorMinArgCount = OperatorAddArgCount,
		OperatorSqrtArgCount = OperatorSinArgCount,
		OperatorAbsArgCount = OperatorSinArgCount,
		OperatorExpArgCount = OperatorSinArgCount,
		OperatorLnArgCount = OperatorSinArgCount,
		OperatorLgArgCount = OperatorSinArgCount,
		OperatorLogArgCount = OperatorAddArgCount,
		OperatorComparisonArgCount = OperatorAddArgCount
	}

	public enum OperatorPriority
	{
		OperatorAddPrior = 1,
		OperatorSubPrior = OperatorAddPrior,
		OperatorMultPrior = 2,
		OperatorDivPrior = OperatorMultPrior,
		OperatorModPrior = OperatorMultPrior,
		OperatorPowPrior = 3,
		OperatorSqrtPrior = OperatorPowPrior,
		OperatorRootPrior = OperatorPowPrior,
		OperatorBrackLPrior = 0,
		OperatorBrackRPrior = OperatorBrackLPrior,
		OperatorSinPrior = 4,
		OperatorCosPrior = OperatorSinPrior,
		OperatorMaxPrior = OperatorSinPrior,
		OperatorMinPrior = OperatorSinPrior,
		OperatorAbsPrior = OperatorSinPrior,
		OperatorLnPrior = OperatorSinPrior,
		OperatorLgPrior = OperatorSinPrior,
		OperatorLogPrior = OperatorSinPrior,
		OperatorExpPrior = OperatorSinPrior,
		OperatorComparisonPrior = OperatorSinPrior
	}

	public enum Direction
	{
		DirectionRight = 0,
		DirectionLeft = 1
	}

	public enum OperatorDirection
	{
		OperatorAddDirect = 0,
		OperatorSubDirect = OperatorAddDirect,
		OperatorMultDirect = OperatorAddDirect,
		OperatorDivDirect = OperatorAddDirect,
		OperatorModDirect = OperatorAddDirect,
		OperatorPowDirect = 1,
		OperatorRootDirect = OperatorAddDirect,
		OperatorBrackLDirect = OperatorAddDirect,
		OperatorBrackRDirect = OperatorAddDirect,
		OperatorSinDirect = OperatorPowDirect,
		OperatorCosDirect = OperatorPowDirect,
		OperatorMaxDirect = OperatorPowDirect,
		OperatorMinDirect = OperatorPowDirect,
		OperatorAbsDirect = OperatorPowDirect,
		OperatorSqrtDirect = OperatorPowDirect,
		OperatorExpDirect = OperatorPowDirect,
		OperatorLnDirect = OperatorPowDirect,
		OperatorLgDirect = OperatorPowDirect,
		OperatorLogDirect = OperatorPowDirect,
		OperatorComparisonDirect = OperatorAddDirect
	}

	private class OperatorInfo
	{
		public OperatorPriority Priority;

		public OperatorDelegate Function;

		public OperatorArgCount ArgCount;

		public OperatorDirection Direction;

		public OperatorInfo(OperatorPriority priority, OperatorDelegate operation, OperatorArgCount argCount, OperatorDirection direction)
		{
			Priority = priority;
			Function = operation;
			ArgCount = argCount;
			Direction = direction;
		}

		public static double Add(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return arguments[0] + arguments[1];
		}

		public static double Subtract(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return arguments[0] - arguments[1];
		}

		public static double Multiply(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return arguments[0] * arguments[1];
		}

		public static double Divide(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			if (arguments[1] != 0.0)
			{
				return arguments[0] / arguments[1];
			}
			return 0.0;
		}

		public static double Modulo(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return (int)arguments[0] % (int)arguments[1];
		}

		public static double Power(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return Math.Pow(arguments[0], arguments[1]);
		}

		public static double Or(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return Convert.ToDouble(Convert.ToBoolean(arguments[0]) | Convert.ToBoolean(arguments[1]));
		}

		public static double Sin(List<double> arguments)
		{
			if (arguments.Count != 1)
			{
				return 0.0;
			}
			return Math.Sin(arguments[0]);
		}

		public static double Cos(List<double> arguments)
		{
			if (arguments.Count != 1)
			{
				return 0.0;
			}
			return Math.Cos(arguments[0]);
		}

		public static double Max(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return Math.Max(arguments[0], arguments[1]);
		}

		public static double Min(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return Math.Min(arguments[0], arguments[1]);
		}

		public static double Sqrt(List<double> arguments)
		{
			if (arguments.Count != 1)
			{
				return 0.0;
			}
			return Math.Sqrt(arguments[0]);
		}

		public static double Abs(List<double> arguments)
		{
			if (arguments.Count != 1)
			{
				return 0.0;
			}
			return Math.Abs(arguments[0]);
		}

		public static double Ln(List<double> arguments)
		{
			if (arguments.Count != 1)
			{
				return 0.0;
			}
			return Math.Log(arguments[0], Math.E);
		}

		public static double Lg(List<double> arguments)
		{
			if (arguments.Count != 1)
			{
				return 0.0;
			}
			return Math.Log10(arguments[0]);
		}

		public static double Log(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return Math.Log(arguments[0], arguments[1]);
		}

		public static double Exp(List<double> arguments)
		{
			if (arguments.Count != 1)
			{
				return 0.0;
			}
			return Math.Exp(arguments[0]);
		}

		public static double Greater(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return (arguments[0] > arguments[1]) ? 1 : 0;
		}

		public static double Less(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return (arguments[0] < arguments[1]) ? 1 : 0;
		}

		public static double GreaterOrEqual(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return (arguments[0] >= arguments[1]) ? 1 : 0;
		}

		public static double LessOrEqual(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return (arguments[0] <= arguments[1]) ? 1 : 0;
		}

		public static double Equal(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return (arguments[0] == arguments[1]) ? 1 : 0;
		}

		public static double NotEqual(List<double> arguments)
		{
			if (arguments.Count != 2)
			{
				return 0.0;
			}
			return (arguments[0] != arguments[1]) ? 1 : 0;
		}
	}

	private class Operand
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private OperandType type;

		public Operand()
		{
			set_Type(OperandType.None);
		}

		public OperandType get_Type()
		{
			return type;
		}

		public void set_Type(OperandType value)
		{
			type = value;
		}

		public virtual object GetValue()
		{
			return null;
		}
	}

	private class ConstantOperand : Operand
	{
		private object _value;

		public ConstantOperand(object value)
		{
			_value = value;
			set_Type(OperandType.Constant);
		}

		public override object GetValue()
		{
			return _value;
		}
	}

	private class VariableOperand : Operand
	{
		private VariableDelegate getter;

		public VariableOperand(VariableDelegate variableGetter)
		{
			getter = variableGetter;
			set_Type(OperandType.Variable);
		}

		public override object GetValue()
		{
			return getter();
		}
	}

	private class FunctionOperand : Operand
	{
		private ParameterDelegate function;

		private List<Operand> argumentList;

		public FunctionOperand(List<Operand> arguments, ParameterDelegate functionDelegate)
		{
			argumentList = arguments;
			function = functionDelegate;
		}

		public override object GetValue()
		{
			List<object> list = new List<object>(1);
			foreach (Operand item in argumentList)
			{
				list.Add(item.GetValue());
			}
			return function(list);
		}
	}

	private class FormulaItem
	{
		public ItemKind Kind;

		public OperatorInfo Operator;

		public Operand Operand;
	}

	public class Formula
	{
		private List<FormulaItem> _items = new List<FormulaItem>();

		public int VariableCount
		{
			get
			{
				return GetVariableCount();
			}
		}

		public Formula(string formulaText)
		{
			if (!_isInited)
			{
				throw new Exception("RpnParser is not inited!");
			}
			_items = ParseFormula(formulaText);
		}

		public int GetVariableCount()
		{
			return _items.FindAll((FormulaItem item) => item.Operand != null && item.Operand.get_Type() == OperandType.Variable).Count;
		}

		public object Calculate()
		{
			return RpnParser.Calculate(_items);
		}
	}

	private static Dictionary<string, OperatorInfo> _operators;

	private static Dictionary<string, VariableDelegate> _variables;

	private static Dictionary<string, ParameterDelegate> _functions;

	private static bool _isInited;

	private const string OperatorSymbols = "+-*^()|/&#";

	public const char FunctionPrefix = '?';

	public const char MemberSeparator = '.';

	public const char VariablePrefix = '$';

	public const char ArgumentSeparator = ',';

	public static void init(Dictionary<string, VariableDelegate> variables, Dictionary<string, ParameterDelegate> functions)
	{
		if (!_isInited)
		{
			InitOperators();
			_variables = variables;
			_functions = functions;
			_isInited = true;
		}
	}

	private static void InitOperators()
	{
		if (_operators == null)
		{
			_operators = new Dictionary<string, OperatorInfo>(18);
			_operators["+"] = new OperatorInfo(OperatorPriority.OperatorAddPrior, OperatorInfo.Add, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["-"] = new OperatorInfo(OperatorPriority.OperatorAddPrior, OperatorInfo.Subtract, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["*"] = new OperatorInfo(OperatorPriority.OperatorMultPrior, OperatorInfo.Multiply, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["/"] = new OperatorInfo(OperatorPriority.OperatorMultPrior, OperatorInfo.Divide, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["%"] = new OperatorInfo(OperatorPriority.OperatorMultPrior, OperatorInfo.Modulo, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["^"] = new OperatorInfo(OperatorPriority.OperatorPowPrior, OperatorInfo.Power, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorPowDirect);
			_operators["|"] = new OperatorInfo(OperatorPriority.OperatorPowPrior, OperatorInfo.Or, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["("] = new OperatorInfo(OperatorPriority.OperatorBrackLPrior, null, OperatorArgCount.OperatorBrackLArgCount, OperatorDirection.OperatorAddDirect);
			_operators[")"] = new OperatorInfo(OperatorPriority.OperatorBrackLPrior, null, OperatorArgCount.OperatorBrackLArgCount, OperatorDirection.OperatorAddDirect);
			_operators["sin"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Sin, OperatorArgCount.OperatorSinArgCount, OperatorDirection.OperatorPowDirect);
			_operators["cos"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Cos, OperatorArgCount.OperatorSinArgCount, OperatorDirection.OperatorPowDirect);
			_operators["max"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Max, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorPowDirect);
			_operators["min"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Min, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorPowDirect);
			_operators["pow"] = new OperatorInfo(OperatorPriority.OperatorPowPrior, OperatorInfo.Power, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorPowDirect);
			_operators["sqrt"] = new OperatorInfo(OperatorPriority.OperatorPowPrior, OperatorInfo.Sqrt, OperatorArgCount.OperatorSinArgCount, OperatorDirection.OperatorPowDirect);
			_operators["abs"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Abs, OperatorArgCount.OperatorSinArgCount, OperatorDirection.OperatorPowDirect);
			_operators["ln"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Ln, OperatorArgCount.OperatorSinArgCount, OperatorDirection.OperatorPowDirect);
			_operators["lg"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Lg, OperatorArgCount.OperatorSinArgCount, OperatorDirection.OperatorPowDirect);
			_operators["log"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Log, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorPowDirect);
			_operators["exp"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Exp, OperatorArgCount.OperatorSinArgCount, OperatorDirection.OperatorPowDirect);
			_operators[">"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Greater, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["<"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Less, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["=>"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.GreaterOrEqual, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["=<"] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.LessOrEqual, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["=="] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.Equal, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
			_operators["!="] = new OperatorInfo(OperatorPriority.OperatorSinPrior, OperatorInfo.NotEqual, OperatorArgCount.OperatorAddArgCount, OperatorDirection.OperatorAddDirect);
		}
	}

	private static List<FormulaItem> ParseFormula(string formulaText)
	{
		List<FormulaItem> list = new List<FormulaItem>();
		if (formulaText == string.Empty)
		{
			throw new Exception("Formula can not be empty");
		}
		formulaText = RemoveSpaces(formulaText);
		formulaText = InsertUnaryZeros(formulaText);
		List<FormulaItem> list2 = Tokenize(formulaText);
		if (!AreBracketsBalanced(list2))
		{
			throw new Exception("Formula can not be empty");
		}
		list = ToPostfix(list2);
		if (list.Count == 0)
		{
			throw new Exception("Formula has no items");
		}
		return list;
	}

	private static object Calculate(List<FormulaItem> items)
	{
		if (items == null || items.Count == 0)
		{
			// A newer perk can resolve entirely to unsupported runtime parameters.
			// Treat that expression as zero instead of breaking combat every hit.
			return 0.0;
		}
		if (items.Count == 1 && items[0].Kind == ItemKind.Operand)
		{
			return items[0].Operand.GetValue();
		}
		List<object> list = new List<object>(2);
		for (int i = 0; i != items.Count; i++)
		{
			if (items[i].Kind == ItemKind.Operand)
			{
				list.Add(items[i].Operand.GetValue());
			}
			else
			{
				if (items[i].Kind != ItemKind.Operator)
				{
					continue;
				}
				List<double> list2 = new List<double>();
				int num = (int)(list.Count - items[i].Operator.ArgCount);
				if (num < 0)
				{
					return 0.0;
				}
				for (int j = num; j < list.Count; j++)
				{
					double result = 0.0;
					if (!double.TryParse(list[j].ToString(), out result))
					{
						throw new Exception("Double expected because there are math operators");
					}
					list2.Add(result);
				}
				double num2 = items[i].Operator.Function(list2);
				list.RemoveRange(num, (int)items[i].Operator.ArgCount);
				list.Add(num2);
			}
		}
		return ((list.Count == 0) ? 0.0 : list[0]);
	}

	private static string InsertUnaryZeros(string formulaText)
	{
		for (int i = 0; i < formulaText.Length; i++)
		{
			if ((formulaText[i] == '-' || formulaText[i] == '+') && (i == 0 || formulaText[i - 1] == '('))
			{
				formulaText = formulaText.Insert(i, "0");
			}
		}
		return formulaText;
	}

	private static string RemoveSpaces(string formulaText)
	{
		return formulaText.Replace(" ", string.Empty);
	}

	public static bool IsOperatorSymbol(string symbol)
	{
		if (!"+-*^()|/&#".Contains(symbol))
		{
			return false;
		}
		return true;
	}

	public static bool IsOperatorSymbol(char symbol)
	{
		return "+-*^()|/&#".IndexOf(symbol) != -1;
	}

	private static bool IsOperator(string token)
	{
		string key = token.ToLower();
		return _operators.ContainsKey(key);
	}

	private static bool IsFunction(string token)
	{
		if (token[0] == '?')
		{
			return true;
		}
		foreach (char c in token)
		{
			if (c == '.' || c == '[' || c == ']')
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsVariable(string token)
	{
		if (token[0] == '$')
		{
			return true;
		}
		return false;
	}

	private static List<FormulaItem> Tokenize(string formulaText)
	{
		List<FormulaItem> list = new List<FormulaItem>();
		int i = 0;
		int length = formulaText.Length;
		while (i < length)
		{
			FormulaItem item = new FormulaItem();
			if (char.IsDigit(formulaText[i]))
			{
				string text = string.Empty;
				char? c = null;
				for (; i < length; i++)
				{
					char c2 = formulaText[i];
					if (!char.IsDigit(c2))
					{
						bool flag = c2 != '.';
						bool flag2 = c2 != 'e' && c2 != 'E';
						bool flag3 = !c.HasValue || (((!c.HasValue) ? ((int?)null) : new int?(c.Value)) != 101 && ((!c.HasValue) ? ((int?)null) : new int?(c.Value)) != 69) || c2 != '-';
						if (flag && flag2 && flag3)
						{
							break;
						}
					}
					c = c2;
					text += c2;
				}
				double num = Convert.ToDouble(text, CultureInfo.InvariantCulture);
				item.Kind = ItemKind.Operand;
				item.Operand = new ConstantOperand(num);
			}
			else if (!char.IsDigit(formulaText[i]))
			{
				if (formulaText[i] == ',')
				{
					i++;
					continue;
				}
				string text2 = string.Empty;
				while (i < length && (!IsOperatorSymbol(formulaText[i].ToString()) || !AreSquareBracketsBalanced(text2) || text2.Length == 0))
				{
					text2 += formulaText[i];
					i++;
					if (IsOperator(text2))
					{
						break;
					}
				}
				if (IsOperator(text2))
				{
					item.Kind = ItemKind.Operator;
					string key = text2.ToLower();
					item.Operator = _operators[key];
				}
				else
				{
					item.Kind = ItemKind.Operand;
					item.Operand = ParseOperand(text2);
				}
			}
			list.Add(item);
		}
		return list;
	}

	private static bool AreSquareBracketsBalanced(string text)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < text.Length; i++)
		{
			switch (text[i])
			{
			case '[':
				num++;
				break;
			case ']':
				num2++;
				break;
			}
		}
		return num == num2;
	}

	private static FunctionOperand ParseFunction(string functionCall)
	{
		int num = functionCall.IndexOf("[");
		string text = functionCall.Substring(0, num);
		string argumentsText = functionCall.Substring(num + 1, functionCall.Length - num - 2);
		string text2 = text.Substring(1, text.Length - 1);
		if (!_functions.ContainsKey(text2))
		{
			throw new Exception("Unknown function name " + text2);
		}
		List<Operand> arguments = ParseArguments(argumentsText);
		return new FunctionOperand(arguments, _functions[text2]);
	}

	private static Operand ParseOperand(string operandText)
	{
		Operand operand = null;
		if (IsFunction(operandText))
		{
			operandText = ExpandMemberAccess(operandText);
			operand = ParseFunction(operandText);
		}
		else if (char.IsDigit(operandText[0]))
		{
			double num = Convert.ToDouble(operandText);
			operand = new ConstantOperand(num);
		}
		else if (IsVariable(operandText))
		{
			string text = operandText.Substring(1, operandText.Length - 1);
			if (!_variables.ContainsKey(text))
			{
				throw new Exception("Unknown variable " + text);
			}
			operand = new VariableOperand(_variables[text]);
		}
		else
		{
			operand = new ConstantOperand(operandText);
			operand.set_Type(OperandType.Variable);
		}
		return operand;
	}

	private static List<Operand> ParseArguments(string argumentsText)
	{
		if (argumentsText.Length == 0)
		{
			List<Operand> list = new List<Operand>();
			list.Add(new Operand());
			return list;
		}
		List<Operand> list2 = new List<Operand>();
		argumentsText = argumentsText.Trim();
		string[] array = SplitArguments(argumentsText, ',');
		string[] array2 = array;
		foreach (string argumentText in array2)
		{
			Operand item = ParseOperand(argumentText);
			list2.Add(item);
		}
		return list2;
	}

	private static string[] SplitArguments(string text, char separator)
	{
		List<string> list = new List<string>();
		int num = 0;
		do
		{
			int num2;
			if (text[num] == '?')
			{
				num2 = GetEndOfFunc(text, num) + 1;
			}
			else
			{
				num2 = text.IndexOf(',', num);
				if (num2 == -1)
				{
					list.Add(text.Substring(num));
					break;
				}
			}
			list.Add(text.Substring(num, num2 - num));
			num = num2 + 1;
		}
		while (num < text.Length);
		return list.ToArray();
	}

	private static int GetEndOfFunc(string text, int startIndex)
	{
		int num = text.IndexOf('[', startIndex);
		if (num < 0)
		{
			return -1;
		}
		num++;
		int num2 = 1;
		for (; num < text.Length; num++)
		{
			if (num2 == 0)
			{
				break;
			}
			if (text[num] == '[')
			{
				num2++;
			}
			else if (text[num] == ']')
			{
				num2--;
			}
		}
		return num - 1;
	}

	private static string ExpandMemberAccess(string expression)
	{
		while (expression.Contains("."))
		{
			expression = ConvertMemberAccess(expression);
		}
		return expression;
	}

	private static string ConvertMemberAccess(string expression)
	{
		int num = expression.IndexOf('.');
		string text = expression.Substring(0, num);
		string text2 = "?" + expression.Substring(num + 1, expression.Length - num - 1);
		expression = text2 + "[" + text + "]";
		return expression;
	}

	private static bool AreBracketsBalanced(List<FormulaItem> items)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i != items.Count; i++)
		{
			if (items[i].Kind == ItemKind.Operator)
			{
				if (items[i].Operator == _operators["("])
				{
					num++;
				}
				else if (items[i].Operator == _operators[")"])
				{
					num2++;
				}
			}
		}
		return num == num2;
	}

	private static List<FormulaItem> ToPostfix(List<FormulaItem> infixItems)
	{
		List<FormulaItem> list = new List<FormulaItem>();
		List<FormulaItem> list2 = new List<FormulaItem>();
		int num = 0;
		while (num != infixItems.Count)
		{
			if (infixItems[num].Kind == ItemKind.Operand)
			{
				list.Add(infixItems[num]);
				num++;
			}
			else if (infixItems[num].Operator == _operators["("])
			{
				list2.Add(infixItems[num]);
				num++;
			}
			else if (infixItems[num].Operator == _operators[")"])
			{
				while (list2.Count != 0 && list2[list2.Count - 1].Operator != _operators["("])
				{
					list.Add(list2[list2.Count - 1]);
					list2.RemoveAt(list2.Count - 1);
				}
				if (list2.Count != 0 && list2[list2.Count - 1].Operator == _operators["("])
				{
					list2.RemoveAt(list2.Count - 1);
					if (list2.Count != 0 && list2[list2.Count - 1].Operator.Direction == OperatorDirection.OperatorPowDirect)
					{
						list.Add(list2[list2.Count - 1]);
						list2.RemoveAt(list2.Count - 1);
					}
				}
				num++;
			}
			else if (list2.Count == 0)
			{
				list2.Add(infixItems[num]);
				num++;
			}
			else if ((infixItems[num].Operator.Direction == OperatorDirection.OperatorAddDirect && list2[list2.Count - 1].Operator.Priority < infixItems[num].Operator.Priority) || (infixItems[num].Operator.Direction == OperatorDirection.OperatorPowDirect && list2[list2.Count - 1].Operator.Priority <= infixItems[num].Operator.Priority))
			{
				list2.Add(infixItems[num]);
				num++;
			}
			else if ((infixItems[num].Operator.Direction == OperatorDirection.OperatorAddDirect && list2[list2.Count - 1].Operator.Priority >= infixItems[num].Operator.Priority) || (infixItems[num].Operator.Direction == OperatorDirection.OperatorPowDirect && list2[list2.Count - 1].Operator.Priority > infixItems[num].Operator.Priority))
			{
				list.Add(list2[list2.Count - 1]);
				list2.RemoveAt(list2.Count - 1);
			}
		}
		while (list2.Count != 0)
		{
			list.Add(list2[list2.Count - 1]);
			list2.RemoveAt(list2.Count - 1);
		}
		return list;
	}
}
