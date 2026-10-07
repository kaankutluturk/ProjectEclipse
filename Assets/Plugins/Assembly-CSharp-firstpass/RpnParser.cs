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

	public delegate object ParameterDelegate(List<object> BPLIHEIIBFP);

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

		public OperatorInfo(OperatorPriority DBNEBOIBILM, OperatorDelegate NGOJAJIKFBA, OperatorArgCount MPOEHCOADGE, OperatorDirection PCBCFHJBODO)
		{
			Priority = DBNEBOIBILM;
			Function = NGOJAJIKFBA;
			ArgCount = MPOEHCOADGE;
			Direction = PCBCFHJBODO;
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

		public VariableOperand(VariableDelegate NFDJONMIEFL)
		{
			getter = NFDJONMIEFL;
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

		public FunctionOperand(List<Operand> arguments, ParameterDelegate JKAELOIBLFJ)
		{
			argumentList = arguments;
			function = JKAELOIBLFJ;
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

		public Formula(string HBICLHKEIEI)
		{
			if (!_isInited)
			{
				throw new Exception("RpnParser is not inited!");
			}
			_items = ParseFormula(HBICLHKEIEI);
		}

		public int GetVariableCount()
		{
			return _items.FindAll((FormulaItem DHDMNHCIPEH) => DHDMNHCIPEH.Operand != null && DHDMNHCIPEH.Operand.get_Type() == OperandType.Variable).Count;
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

	public static void init(Dictionary<string, VariableDelegate> PPEABEJMCPI, Dictionary<string, ParameterDelegate> GIOGAJGIGMO)
	{
		if (!_isInited)
		{
			InitOperators();
			_variables = PPEABEJMCPI;
			_functions = GIOGAJGIGMO;
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

	private static List<FormulaItem> ParseFormula(string DPABILBDPFF)
	{
		List<FormulaItem> list = new List<FormulaItem>();
		if (DPABILBDPFF == string.Empty)
		{
			throw new Exception("Formula can not be empty");
		}
		DPABILBDPFF = RemoveSpaces(DPABILBDPFF);
		DPABILBDPFF = InsertUnaryZeros(DPABILBDPFF);
		List<FormulaItem> list2 = Tokenize(DPABILBDPFF);
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

	private static object Calculate(List<FormulaItem> HELFDCAIJNE)
	{
		if (HELFDCAIJNE == null || HELFDCAIJNE.Count == 0)
		{
			// A newer perk can resolve entirely to unsupported runtime parameters.
			// Treat that expression as zero instead of breaking combat every hit.
			return 0.0;
		}
		if (HELFDCAIJNE.Count == 1 && HELFDCAIJNE[0].Kind == ItemKind.Operand)
		{
			return HELFDCAIJNE[0].Operand.GetValue();
		}
		List<object> list = new List<object>(2);
		for (int i = 0; i != HELFDCAIJNE.Count; i++)
		{
			if (HELFDCAIJNE[i].Kind == ItemKind.Operand)
			{
				list.Add(HELFDCAIJNE[i].Operand.GetValue());
			}
			else
			{
				if (HELFDCAIJNE[i].Kind != ItemKind.Operator)
				{
					continue;
				}
				List<double> list2 = new List<double>();
				int num = (int)(list.Count - HELFDCAIJNE[i].Operator.ArgCount);
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
				double num2 = HELFDCAIJNE[i].Operator.Function(list2);
				list.RemoveRange(num, (int)HELFDCAIJNE[i].Operator.ArgCount);
				list.Add(num2);
			}
		}
		return ((list.Count == 0) ? 0.0 : list[0]);
	}

	private static string InsertUnaryZeros(string DPABILBDPFF)
	{
		for (int i = 0; i < DPABILBDPFF.Length; i++)
		{
			if ((DPABILBDPFF[i] == '-' || DPABILBDPFF[i] == '+') && (i == 0 || DPABILBDPFF[i - 1] == '('))
			{
				DPABILBDPFF = DPABILBDPFF.Insert(i, "0");
			}
		}
		return DPABILBDPFF;
	}

	private static string RemoveSpaces(string DPABILBDPFF)
	{
		return DPABILBDPFF.Replace(" ", string.Empty);
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

	private static bool IsOperator(string IGGFGLLIGCG)
	{
		string key = IGGFGLLIGCG.ToLower();
		return _operators.ContainsKey(key);
	}

	private static bool IsFunction(string IGGFGLLIGCG)
	{
		if (IGGFGLLIGCG[0] == '?')
		{
			return true;
		}
		foreach (char c in IGGFGLLIGCG)
		{
			if (c == '.' || c == '[' || c == ']')
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsVariable(string IGGFGLLIGCG)
	{
		if (IGGFGLLIGCG[0] == '$')
		{
			return true;
		}
		return false;
	}

	private static List<FormulaItem> Tokenize(string DPABILBDPFF)
	{
		List<FormulaItem> list = new List<FormulaItem>();
		int i = 0;
		int length = DPABILBDPFF.Length;
		while (i < length)
		{
			FormulaItem bAELOMEILMK = new FormulaItem();
			if (char.IsDigit(DPABILBDPFF[i]))
			{
				string text = string.Empty;
				char? c = null;
				for (; i < length; i++)
				{
					char c2 = DPABILBDPFF[i];
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
				bAELOMEILMK.Kind = ItemKind.Operand;
				bAELOMEILMK.Operand = new ConstantOperand(num);
			}
			else if (!char.IsDigit(DPABILBDPFF[i]))
			{
				if (DPABILBDPFF[i] == ',')
				{
					i++;
					continue;
				}
				string text2 = string.Empty;
				while (i < length && (!IsOperatorSymbol(DPABILBDPFF[i].ToString()) || !AreSquareBracketsBalanced(text2) || text2.Length == 0))
				{
					text2 += DPABILBDPFF[i];
					i++;
					if (IsOperator(text2))
					{
						break;
					}
				}
				if (IsOperator(text2))
				{
					bAELOMEILMK.Kind = ItemKind.Operator;
					string key = text2.ToLower();
					bAELOMEILMK.Operator = _operators[key];
				}
				else
				{
					bAELOMEILMK.Kind = ItemKind.Operand;
					bAELOMEILMK.Operand = ParseOperand(text2);
				}
			}
			list.Add(bAELOMEILMK);
		}
		return list;
	}

	private static bool AreSquareBracketsBalanced(string IGGFGLLIGCG)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < IGGFGLLIGCG.Length; i++)
		{
			switch (IGGFGLLIGCG[i])
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

	private static FunctionOperand ParseFunction(string IGGFGLLIGCG)
	{
		int num = IGGFGLLIGCG.IndexOf("[");
		string text = IGGFGLLIGCG.Substring(0, num);
		string dJIONFCICFC = IGGFGLLIGCG.Substring(num + 1, IGGFGLLIGCG.Length - num - 2);
		string text2 = text.Substring(1, text.Length - 1);
		if (!_functions.ContainsKey(text2))
		{
			throw new Exception("Unknown function name " + text2);
		}
		List<Operand> mAABDFKMACJ = ParseArguments(dJIONFCICFC);
		return new FunctionOperand(mAABDFKMACJ, _functions[text2]);
	}

	private static Operand ParseOperand(string EBDLDPIBIEO)
	{
		Operand fAOBBBMHEBL = null;
		if (IsFunction(EBDLDPIBIEO))
		{
			EBDLDPIBIEO = ExpandMemberAccess(EBDLDPIBIEO);
			fAOBBBMHEBL = ParseFunction(EBDLDPIBIEO);
		}
		else if (char.IsDigit(EBDLDPIBIEO[0]))
		{
			double num = Convert.ToDouble(EBDLDPIBIEO);
			fAOBBBMHEBL = new ConstantOperand(num);
		}
		else if (IsVariable(EBDLDPIBIEO))
		{
			string text = EBDLDPIBIEO.Substring(1, EBDLDPIBIEO.Length - 1);
			if (!_variables.ContainsKey(text))
			{
				throw new Exception("Unknown variable " + text);
			}
			fAOBBBMHEBL = new VariableOperand(_variables[text]);
		}
		else
		{
			fAOBBBMHEBL = new ConstantOperand(EBDLDPIBIEO);
			fAOBBBMHEBL.set_Type(OperandType.Variable);
		}
		return fAOBBBMHEBL;
	}

	private static List<Operand> ParseArguments(string DJIONFCICFC)
	{
		if (DJIONFCICFC.Length == 0)
		{
			List<Operand> list = new List<Operand>();
			list.Add(new Operand());
			return list;
		}
		List<Operand> list2 = new List<Operand>();
		DJIONFCICFC = DJIONFCICFC.Trim();
		string[] array = SplitArguments(DJIONFCICFC, ',');
		string[] array2 = array;
		foreach (string eBDLDPIBIEO in array2)
		{
			Operand item = ParseOperand(eBDLDPIBIEO);
			list2.Add(item);
		}
		return list2;
	}

	private static string[] SplitArguments(string CGJGACJABDF, char EPJDMLMAOII)
	{
		List<string> list = new List<string>();
		int num = 0;
		do
		{
			int num2;
			if (CGJGACJABDF[num] == '?')
			{
				num2 = GetEndOfFunc(CGJGACJABDF, num) + 1;
			}
			else
			{
				num2 = CGJGACJABDF.IndexOf(',', num);
				if (num2 == -1)
				{
					list.Add(CGJGACJABDF.Substring(num));
					break;
				}
			}
			list.Add(CGJGACJABDF.Substring(num, num2 - num));
			num = num2 + 1;
		}
		while (num < CGJGACJABDF.Length);
		return list.ToArray();
	}

	private static int GetEndOfFunc(string CGJGACJABDF, int CAILGDNIKJD)
	{
		int num = CGJGACJABDF.IndexOf('[', CAILGDNIKJD);
		if (num < 0)
		{
			return -1;
		}
		num++;
		int num2 = 1;
		for (; num < CGJGACJABDF.Length; num++)
		{
			if (num2 == 0)
			{
				break;
			}
			if (CGJGACJABDF[num] == '[')
			{
				num2++;
			}
			else if (CGJGACJABDF[num] == ']')
			{
				num2--;
			}
		}
		return num - 1;
	}

	private static string ExpandMemberAccess(string IGGFGLLIGCG)
	{
		while (IGGFGLLIGCG.Contains("."))
		{
			IGGFGLLIGCG = ConvertMemberAccess(IGGFGLLIGCG);
		}
		return IGGFGLLIGCG;
	}

	private static string ConvertMemberAccess(string IGGFGLLIGCG)
	{
		int num = IGGFGLLIGCG.IndexOf('.');
		string text = IGGFGLLIGCG.Substring(0, num);
		string text2 = "?" + IGGFGLLIGCG.Substring(num + 1, IGGFGLLIGCG.Length - num - 1);
		IGGFGLLIGCG = text2 + "[" + text + "]";
		return IGGFGLLIGCG;
	}

	private static bool AreBracketsBalanced(List<FormulaItem> HELFDCAIJNE)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i != HELFDCAIJNE.Count; i++)
		{
			if (HELFDCAIJNE[i].Kind == ItemKind.Operator)
			{
				if (HELFDCAIJNE[i].Operator == _operators["("])
				{
					num++;
				}
				else if (HELFDCAIJNE[i].Operator == _operators[")"])
				{
					num2++;
				}
			}
		}
		return num == num2;
	}

	private static List<FormulaItem> ToPostfix(List<FormulaItem> JEFEGDICJJC)
	{
		List<FormulaItem> list = new List<FormulaItem>();
		List<FormulaItem> list2 = new List<FormulaItem>();
		int num = 0;
		while (num != JEFEGDICJJC.Count)
		{
			if (JEFEGDICJJC[num].Kind == ItemKind.Operand)
			{
				list.Add(JEFEGDICJJC[num]);
				num++;
			}
			else if (JEFEGDICJJC[num].Operator == _operators["("])
			{
				list2.Add(JEFEGDICJJC[num]);
				num++;
			}
			else if (JEFEGDICJJC[num].Operator == _operators[")"])
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
				list2.Add(JEFEGDICJJC[num]);
				num++;
			}
			else if ((JEFEGDICJJC[num].Operator.Direction == OperatorDirection.OperatorAddDirect && list2[list2.Count - 1].Operator.Priority < JEFEGDICJJC[num].Operator.Priority) || (JEFEGDICJJC[num].Operator.Direction == OperatorDirection.OperatorPowDirect && list2[list2.Count - 1].Operator.Priority <= JEFEGDICJJC[num].Operator.Priority))
			{
				list2.Add(JEFEGDICJJC[num]);
				num++;
			}
			else if ((JEFEGDICJJC[num].Operator.Direction == OperatorDirection.OperatorAddDirect && list2[list2.Count - 1].Operator.Priority >= JEFEGDICJJC[num].Operator.Priority) || (JEFEGDICJJC[num].Operator.Direction == OperatorDirection.OperatorPowDirect && list2[list2.Count - 1].Operator.Priority > JEFEGDICJJC[num].Operator.Priority))
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
