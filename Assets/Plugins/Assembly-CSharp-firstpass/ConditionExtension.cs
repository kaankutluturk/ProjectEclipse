using System.Collections.Generic;
using System.Text;

public abstract class ConditionExtension
{
	public enum MathFunctionType
	{
		MATH_NONE = 0,
		MATH_SUM = 1,
		MATH_SUB = 2,
		MATH_MULTI = 3,
		MATH_DIVISION = 4,
		MATH_DIVISION_INT = 5,
		MATH_MOD = 6,
		MATH_RAND = 7
	}

	public enum StringFunctionType
	{
		STRING_NONE = 0,
		STRING_CONCAT = 1,
		STRING_SLICE = 2
	}

	public enum QuestVariableType
	{
		QUEST_CONDITION_VARIABLE_NONE = 0,
		QUEST_CONDITION_VARIABLE_STRING = 1,
		QUEST_CONDITION_VARIABLE_NUMBER = 2
	}

	public class FunctionArgument
	{
		public string value = string.Empty;

		public string result = string.Empty;

		public bool Empty()
		{
			return result.Equals(string.Empty);
		}

		public void Reset()
		{
			result = string.Empty;
		}
	}

	public class QuestFunctions : FunctionArgument
	{
		public string functionName = string.Empty;

		public string argumentsText = string.Empty;

		public string property = string.Empty;

		public List<FunctionArgument> arguments = new List<FunctionArgument>();

		public string GetFirstArgument()
		{
			return (arguments.Count <= 0) ? string.Empty : arguments[0].result;
		}
	}

	public class CompareResult
	{
		public string resultSTR;

		public double resultNumber;

		public CompareResult()
		{
			resultSTR = string.Empty;
			resultNumber = 0.0;
		}

		public CompareResult(string value)
		{
			resultSTR = value;
			resultNumber = 0.0;
		}

		public CompareResult(float value)
		{
			resultSTR = string.Empty;
			resultNumber = value;
		}

		public CompareResult(string PGIDABLDOAM, float CCJOKGEFOFP)
		{
			resultSTR = PGIDABLDOAM;
			resultNumber = CCJOKGEFOFP;
		}

		public override string ToString()
		{
			if (IsNumber())
			{
				return resultNumber.ToString();
			}
			return resultSTR;
		}

		public bool IsNumber()
		{
			return resultSTR.Equals(string.Empty);
		}

		public void Clear()
		{
			resultSTR = string.Empty;
			resultNumber = 0.0;
		}
	}

	public ConditionExtension()
	{
	}

	public void SetValue(string value, CompareResult BMDEBHIHIAJ)
	{
		if (!value.Equals(string.Empty))
		{
			char c = value[0];
			if (c.Equals('_'))
			{
				SessionSettings(value, BMDEBHIHIAJ);
			}
			else if (c.Equals('?'))
			{
				ExecuteFunction(value, BMDEBHIHIAJ);
				if (!BMDEBHIHIAJ.resultSTR.Equals(string.Empty))
				{
					string iBBAMMHHBFE = BMDEBHIHIAJ.resultSTR;
					BMDEBHIHIAJ.Clear();
					SetValue(iBBAMMHHBFE, BMDEBHIHIAJ);
				}
			}
			else
			{
				SetLiteralValue(value, BMDEBHIHIAJ);
			}
		}
		else
		{
			GameLog.Write("ConditionExtension::setValue - empty string");
		}
	}

	protected QuestFunctions ParseFunctions(string value)
	{
		if (!value.Equals(string.Empty))
		{
			return ParseFunctionInternal(value);
		}
		GameLog.Write("ConditionExtension.parseFunctions - empty string");
		return null;
	}

	protected QuestFunctions ParseFunctionInternal(string value)
	{
		if (value[0].Equals('?'))
		{
			// Newer gamedata uses ?Function[args] while this quest evaluator was
			// compiled for ?Function(args). Accept both at the parser boundary;
			// combat expressions are handled by the separate FunctionExtension.
			if (value.IndexOf('(') < 0 && value.IndexOf('[') >= 0)
				value = value.Replace('[', '(').Replace(']', ')');
			int num = value.IndexOf('(');
			int num2 = value.LastIndexOf(')');
			int num3 = value.LastIndexOf('.');
			if (num <= 1 || num2 < num)
			{
				GameLog.Error("ConditionExtension::parseFunctions - malformed function: {0}", value);
				return null;
			}
			QuestFunctions bECNHJBBOKO = new QuestFunctions();
			bECNHJBBOKO.argumentsText = value;
			bECNHJBBOKO.functionName = value.Substring(1, num - 1);
			bECNHJBBOKO.argumentsText = value.Substring(num + 1, num2 - num - 1);
			bECNHJBBOKO.property = ((num3 <= num2) ? string.Empty : value.Substring(num3 + 1, value.Length - num3 - 1));
			ParseArguments(bECNHJBBOKO);
			return bECNHJBBOKO;
		}
		return null;
	}

	protected void ParseArguments(QuestFunctions KJFKPMCPIBH)
	{
		KJFKPMCPIBH.arguments.Clear();
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		string text = ClearGaps(KJFKPMCPIBH.argumentsText);
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			char c = text[i];
			char c2 = ((i + 1 >= length) ? '*' : text[i + 1]);
			if (!flag && c == '?')
			{
				flag = true;
			}
			if (!flag)
			{
				if (!c.Equals(',') && !c.Equals('(') && !c.Equals(')') && !c.Equals('?'))
				{
					stringBuilder2.Append(c);
				}
				else if (stringBuilder2.Length > 0)
				{
					FunctionArgument hLCPKKIIBFB = new FunctionArgument();
					hLCPKKIIBFB.result = stringBuilder2.ToString();
					KJFKPMCPIBH.arguments.Add(hLCPKKIIBFB);
					stringBuilder2.Clear();
				}
			}
			if (flag && c.Equals('('))
			{
				num++;
			}
			else if ((flag && c.Equals(')')) || flag2)
			{
				if (c.Equals(')'))
				{
					num--;
				}
				if (num <= 0)
				{
					if (!flag2 && !c2.Equals('*') && !c2.Equals(',') && !c2.Equals(')'))
					{
						flag2 = true;
					}
					if (flag2 && (c2.Equals('*') || c2.Equals(',')))
					{
						flag2 = false;
					}
					if (!flag2)
					{
						flag = false;
						stringBuilder.Append(c);
						QuestFunctions item = ParseFunctions(stringBuilder.ToString());
						KJFKPMCPIBH.arguments.Add(item);
						stringBuilder.Clear();
					}
				}
			}
			if (flag)
			{
				stringBuilder.Append(c);
			}
		}
		if (stringBuilder2.Length > 0)
		{
			FunctionArgument hLCPKKIIBFB2 = new FunctionArgument();
			hLCPKKIIBFB2.result = stringBuilder2.ToString();
			KJFKPMCPIBH.arguments.Add(hLCPKKIIBFB2);
			stringBuilder2.Clear();
		}
	}

	protected void SessionSettings(string value, CompareResult BMDEBHIHIAJ)
	{
		ResolveSessionVariable(value, BMDEBHIHIAJ);
	}

	protected void SetLiteralValue(string value, CompareResult BMDEBHIHIAJ)
	{
		// An unset session/roster variable is a valid state while a quest waits
		// for its first event.  Empty strings used to be classified as numbers
		// and fed to double.Parse, spamming FormatException and interrupting the
		// tutorial action sequence (notably the first Shop button press).
		if (string.IsNullOrEmpty(value))
		{
			BMDEBHIHIAJ.resultSTR = string.Empty;
			return;
		}
		switch (GetVariableType(value))
		{
		case QuestVariableType.QUEST_CONDITION_VARIABLE_STRING:
			BMDEBHIHIAJ.resultSTR = value;
			break;
		case QuestVariableType.QUEST_CONDITION_VARIABLE_NUMBER:
			double result;
			if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result) ||
				double.TryParse(value, out result))
			{
				BMDEBHIHIAJ.resultNumber = result;
			}
			else
			{
				BMDEBHIHIAJ.resultSTR = value;
			}
			break;
		}
	}

	protected QuestVariableType GetVariableType(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return QuestVariableType.QUEST_CONDITION_VARIABLE_STRING;
		}
		foreach (int num in value)
		{
			if (num < 43 || num > 57)
			{
				return QuestVariableType.QUEST_CONDITION_VARIABLE_STRING;
			}
		}
		return QuestVariableType.QUEST_CONDITION_VARIABLE_NUMBER;
	}

	protected void ExecuteFunction(string value, CompareResult BMDEBHIHIAJ)
	{
		QuestFunctions kJFKPMCPIBH = ParseFunctions(value);
		ExecuteFunction(kJFKPMCPIBH, BMDEBHIHIAJ);
		ResetFunction(kJFKPMCPIBH);
	}

	protected void ExecuteFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		if (KJFKPMCPIBH == null)
		{
			return;
		}
		foreach (FunctionArgument item in KJFKPMCPIBH.arguments)
		{
			QuestFunctions bECNHJBBOKO = item as QuestFunctions;
			if (bECNHJBBOKO != null)
			{
				if (bECNHJBBOKO.Empty())
				{
					CompareResult lNIDLHOIHIM = new CompareResult();
					ExecuteFunction(bECNHJBBOKO, lNIDLHOIHIM);
					bECNHJBBOKO.result = lNIDLHOIHIM.ToString();
				}
			}
			else
			{
				CompareResult lNIDLHOIHIM2 = new CompareResult();
				SetValue(item.result, lNIDLHOIHIM2);
				item.result = lNIDLHOIHIM2.ToString();
			}
		}
		FullFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
	}

	protected abstract void FullFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ);

	protected abstract void ResolveSessionVariable(string value, CompareResult BMDEBHIHIAJ);

	protected void ResetFunction(QuestFunctions KJFKPMCPIBH)
	{
		KJFKPMCPIBH.Reset();
		foreach (FunctionArgument item in KJFKPMCPIBH.arguments)
		{
			QuestFunctions bECNHJBBOKO = item as QuestFunctions;
			if (bECNHJBBOKO != null)
			{
				ResetFunction(bECNHJBBOKO);
			}
		}
	}

	protected void MathFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ, MathFunctionType LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case MathFunctionType.MATH_SUM:
		case MathFunctionType.MATH_SUB:
		case MathFunctionType.MATH_MULTI:
		case MathFunctionType.MATH_DIVISION:
		{
			int num5 = 0;
			double num6 = 0.0;
			foreach (FunctionArgument item in KJFKPMCPIBH.arguments)
			{
				double result = 0.0;
				double.TryParse(item.result, out result);
				if (num5 == 0)
				{
					num6 = result;
				}
				else
				{
					switch (LFLGCDNKNJI)
					{
					case MathFunctionType.MATH_SUM:
						num6 += result;
						break;
					case MathFunctionType.MATH_SUB:
						num6 -= result;
						break;
					case MathFunctionType.MATH_MULTI:
						num6 *= result;
						break;
					case MathFunctionType.MATH_DIVISION:
						if (result == 0.0)
						{
							GameLog.Error("ConditionExtension::MathFunction - wrong arguments division 0");
						}
						num6 /= result;
						break;
					default:
						GameLog.Error(string.Format("{0},{1}", "ConditionExtension::mathFunction - unknown type: ", LFLGCDNKNJI));
						break;
					}
				}
				num5++;
			}
			BMDEBHIHIAJ.resultNumber = num6;
			break;
		}
		case MathFunctionType.MATH_DIVISION_INT:
			if (KJFKPMCPIBH.arguments.Count == 2)
			{
				int num3 = int.Parse(KJFKPMCPIBH.arguments[0].result);
				int num4 = int.Parse(KJFKPMCPIBH.arguments[1].result);
				BMDEBHIHIAJ.resultNumber = num3 / num4;
			}
			else
			{
				GameLog.Error(string.Format("{0},{1}", "ConditionExtension::MathFunction - wrong arguments count ", KJFKPMCPIBH.arguments.Count));
			}
			break;
		case MathFunctionType.MATH_MOD:
			if (KJFKPMCPIBH.arguments.Count == 2)
			{
				int num7 = int.Parse(KJFKPMCPIBH.arguments[0].result);
				int num8 = int.Parse(KJFKPMCPIBH.arguments[1].result);
				BMDEBHIHIAJ.resultNumber = num7 % num8;
			}
			else
			{
				GameLog.Error(string.Format("{0},{1}", "ConditionExtension::MathFunction - wrong arguments count ", KJFKPMCPIBH.arguments.Count));
			}
			break;
		case MathFunctionType.MATH_RAND:
			if (KJFKPMCPIBH.arguments.Count == 2)
			{
				float num = float.Parse(KJFKPMCPIBH.arguments[0].result);
				float num2 = float.Parse(KJFKPMCPIBH.arguments[1].result);
				BMDEBHIHIAJ.resultNumber = NekkiMath.randomInt((int)num, (int)num2 + 1);
			}
			else
			{
				GameLog.Error(string.Format("{0},{1}", "ConditionExtension::MathFunction - wrong arguments count ", KJFKPMCPIBH.arguments.Count));
			}
			break;
		default:
			GameLog.Error(string.Format("{0},{1}", "ConditionExtension::mathFunction - unknown type: ", LFLGCDNKNJI));
			break;
		}
	}

	protected void StringFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ, StringFunctionType LFLGCDNKNJI)
	{
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		List<string> list = new List<string>();
		foreach (FunctionArgument item in KJFKPMCPIBH.arguments)
		{
			string dCJLKCFKCOM = item.result;
			list.Add(dCJLKCFKCOM);
			if (num == 0)
			{
				stringBuilder.Clear();
				stringBuilder.Append(dCJLKCFKCOM);
			}
			else if (LFLGCDNKNJI == StringFunctionType.STRING_CONCAT)
			{
				stringBuilder.Append(dCJLKCFKCOM);
			}
			num++;
		}
		if (list.Count >= 3 && LFLGCDNKNJI == StringFunctionType.STRING_SLICE)
		{
			string iGGFGLLIGCG = list[0];
			int iOFHCAAOELD = int.Parse(list[1]);
			int iPMPAMAHLJG = int.Parse(list[2]);
			string value = StringFunctionSlice(iGGFGLLIGCG, iOFHCAAOELD, iPMPAMAHLJG);
			stringBuilder.Clear();
			stringBuilder.Append(value);
		}
		BMDEBHIHIAJ.resultSTR = stringBuilder.ToString();
	}

	protected string ClearGaps(string value)
	{
		if (value == null)
		{
			return string.Empty;
		}
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		for (int length = value.Length; i < length; i++)
		{
			char c = value[i];
			bool flag2 = c.Equals('\'');
			if (!c.Equals(' '))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	protected string StringFunctionSlice(string IGGFGLLIGCG, int IOFHCAAOELD, int IPMPAMAHLJG)
	{
		if (IOFHCAAOELD > IGGFGLLIGCG.Length || IPMPAMAHLJG < IOFHCAAOELD || IOFHCAAOELD < 0 || IPMPAMAHLJG < 0)
		{
			return string.Empty;
		}
		int length = 1 + IPMPAMAHLJG - IOFHCAAOELD;
		if (1 + IPMPAMAHLJG > IGGFGLLIGCG.Length)
		{
			length = IGGFGLLIGCG.Length - IOFHCAAOELD;
		}
		return IGGFGLLIGCG.Substring(IOFHCAAOELD, length);
	}
}
