using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class FunctionExtension : global::EventDispatcher<object>
{
	public enum VariableType
	{
		VARIABLE_NONE = 0,
		VARIABLE_STRING = 1,
		VARIABLE_NUMBER = 2
	}

	public enum CompareType
	{
		COMPARE_NONE = 0,
		COMPARE_EQUAL = 1,
		COMPARE_GREATER = 2,
		COMPARE_GREATER_EQUAL = 3,
		COMPARE_LESS = 4,
		COMPARE_LESS_EQUAL = 5
	}

	public enum ObjectType
	{
		TYPE_VALUE = 0,
		TYPE_FUNCTION = 1,
		TYPE_VARIABLE = 2,
		TYPE_SEPARATOR = 3
	}

	public class VariableObject
	{
		public string name = string.Empty;

		public string value = string.Empty;

		public Action<object> callback;
	}

	public class CallbackResult
	{
		public object target;

		public object data;

		public FunctionResult result;
	}

	public class FunctionObject
	{
		public ObjectType type;

		public string body = string.Empty;

		public FunctionResult result = new FunctionResult();
	}

	public class FunctionCall : FunctionObject
	{
		public string functionName = string.Empty;

		public string arguments = string.Empty;

		public string propertyName = string.Empty;

		public string name = string.Empty;

		public List<FunctionObject> argumentValues = new List<FunctionObject>();

		public List<FunctionObject> children = new List<FunctionObject>();

		public FunctionCall()
		{
			type = ObjectType.TYPE_FUNCTION;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Action<CallbackResult> variableCallback;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Action<CallbackResult> functionCallback;

	private List<VariableObject> variables = new List<VariableObject>();

	private List<FunctionObject> valueObjects = new List<FunctionObject>();

	private List<FunctionObject> functionObjects = new List<FunctionObject>();

	private int countFuncName;

	private FunctionCall rootFunction = new FunctionCall();

	private FunctionResult result = new FunctionResult();

	private object target;

	public Action<CallbackResult> VariableCallback
	{
		get
		{
			return GetVariableCallback();
		}
		set
		{
			SetVariableCallback(value);
		}
	}

	public Action<CallbackResult> FunctionCallback
	{
		get
		{
			return GetFunctionCallback();
		}
		set
		{
			SetFunctionCallback(value);
		}
	}

	public Action<CallbackResult> GetVariableCallback()
	{
		return variableCallback;
	}

	public void SetVariableCallback(Action<CallbackResult> value)
	{
		variableCallback = value;
	}

	public Action<CallbackResult> GetFunctionCallback()
	{
		return functionCallback;
	}

	public void SetFunctionCallback(Action<CallbackResult> value)
	{
		functionCallback = value;
	}

	public object GetTarget()
	{
		return target;
	}

	public void set_Target(object value)
	{
		target = value;
	}

	public void Parse(string target)
	{
		string value = ClearGaps(target);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("?Root[");
		stringBuilder.Append(value);
		stringBuilder.Append("]");
		rootFunction = ParseFunction(stringBuilder.ToString());
	}

	public static CompareType ParseCompareType(string LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case "Equal":
			return CompareType.COMPARE_EQUAL;
		case "Greater":
			return CompareType.COMPARE_GREATER;
		case "GreaterEqual":
			return CompareType.COMPARE_GREATER_EQUAL;
		case "Less":
			return CompareType.COMPARE_LESS;
		case "LessEqual":
			return CompareType.COMPARE_LESS_EQUAL;
		default:
			return CompareType.COMPARE_NONE;
		}
	}

	public static VariableType GetVariableType(string value)
	{
		if (value.Length == 0)
		{
			GameLog.Error("Value of QuestConditionVariable is empty");
		}
		Dictionary<string, RpnParser.VariableDelegate> pPEABEJMCPI = new Dictionary<string, RpnParser.VariableDelegate>();
		Dictionary<string, RpnParser.ParameterDelegate> gIOGAJGIGMO = new Dictionary<string, RpnParser.ParameterDelegate>();
		RpnParser.init(pPEABEJMCPI, gIOGAJGIGMO);
		RpnParser.Formula lANLKOHCGEJ = new RpnParser.Formula(value);
		if (lANLKOHCGEJ.GetVariableCount() == 0)
		{
			return VariableType.VARIABLE_NUMBER;
		}
		return VariableType.VARIABLE_STRING;
	}

	public void SetVariable(string name, string value)
	{
		VariableObject cCDGFNHLMCG = new VariableObject();
		cCDGFNHLMCG.name = name;
		cCDGFNHLMCG.value = value;
		variables.Add(cCDGFNHLMCG);
	}

	public VariableObject GetVariable(string name)
	{
		foreach (VariableObject item in variables)
		{
			if (item.name.Equals(name))
			{
				return item;
			}
		}
		return null;
	}

	public List<VariableObject> GetVariables()
	{
		return variables;
	}

	public List<FunctionObject> GetValueObjects()
	{
		return valueObjects;
	}

	public FunctionResult Calculate()
	{
		result.Value = null;
		EvaluateFunction(rootFunction, result);
		result.Value = CalculateResult(result.Value);
		return result;
	}

	public FunctionResult GetResult()
	{
		return result;
	}

	public bool IsResultEmpty()
	{
		return result.Value.Equals(string.Empty);
	}

	public string CalculateResult(string target)
	{
		List<string> list = new List<string>();
		string[] collection = target.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		list.AddRange(collection);
		if (list.Count > 1)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				stringBuilder.Append(CalculateResult(list[i]));
				if (i < list.Count - 1)
				{
					stringBuilder.Append(',');
				}
			}
			return stringBuilder.ToString();
		}
		if (target.Equals(string.Empty))
		{
			return target;
		}
		Dictionary<string, RpnParser.VariableDelegate> pPEABEJMCPI = new Dictionary<string, RpnParser.VariableDelegate>();
		Dictionary<string, RpnParser.ParameterDelegate> gIOGAJGIGMO = new Dictionary<string, RpnParser.ParameterDelegate>();
		RpnParser.init(pPEABEJMCPI, gIOGAJGIGMO);
		RpnParser.Formula lANLKOHCGEJ = new RpnParser.Formula(target);
		if (lANLKOHCGEJ.GetVariableCount() == 0)
		{
			object obj = lANLKOHCGEJ.Calculate();
			string text = ((obj == null) ? string.Empty : obj.ToString());
			double result;
			if (double.TryParse(text, out result) && result >= 0.0)
			{
				return text;
			}
			return string.Format("({0})", text);
		}
		return target;
	}

	private void EvaluateFunction(FunctionCall KJFKPMCPIBH, FunctionResult DCJLKCFKCOM)
	{
		foreach (FunctionObject item in KJFKPMCPIBH.children)
		{
			if (item.type == ObjectType.TYPE_FUNCTION)
			{
				FunctionCall gLBAFLLMOOH = item as FunctionCall;
				if (gLBAFLLMOOH != null)
				{
					EvaluateFunction(gLBAFLLMOOH, item.result);
				}
			}
			else if (item.type == ObjectType.TYPE_VARIABLE)
			{
				ResolveVariable(item.body, item.result);
			}
			else
			{
				item.result.Value = item.body;
			}
		}
		InvokeFunction(KJFKPMCPIBH, DCJLKCFKCOM);
	}

	private void InvokeFunction(FunctionCall KJFKPMCPIBH, FunctionResult DCJLKCFKCOM)
	{
		SubstituteArguments(KJFKPMCPIBH, ref DCJLKCFKCOM);
		SplitArgumentValues(KJFKPMCPIBH, DCJLKCFKCOM);
		DCJLKCFKCOM.Value = CalculateResult(DCJLKCFKCOM.Value);
		if (GetFunctionCallback() != null)
		{
			CallbackResult oMJHHJNIJOL = new CallbackResult();
			oMJHHJNIJOL.data = KJFKPMCPIBH;
			oMJHHJNIJOL.result = DCJLKCFKCOM;
			oMJHHJNIJOL.target = target;
			GetFunctionCallback()(oMJHHJNIJOL);
		}
		KJFKPMCPIBH.argumentValues.Clear();
	}

	private void SplitArgumentValues(FunctionCall KJFKPMCPIBH, FunctionResult DCJLKCFKCOM)
	{
		KJFKPMCPIBH.argumentValues.Clear();
		StringBuilder stringBuilder = new StringBuilder();
		ObjectType pLGKHFNOBCB = ObjectType.TYPE_SEPARATOR;
		char c = ',';
		string dCJLKCFKCOM = DCJLKCFKCOM.Value;
		int i = 0;
		for (int length = dCJLKCFKCOM.Length; i < length; i++)
		{
			char c2 = dCJLKCFKCOM[i];
			bool flag = c2 == ',';
			bool flag2 = c == ',';
			if (pLGKHFNOBCB == ObjectType.TYPE_VALUE && flag)
			{
				if (!stringBuilder.Equals(string.Empty))
				{
					FunctionObject pENDFCHBHIB = new FunctionObject();
					Dictionary<string, RpnParser.VariableDelegate> pPEABEJMCPI = new Dictionary<string, RpnParser.VariableDelegate>();
					Dictionary<string, RpnParser.ParameterDelegate> gIOGAJGIGMO = new Dictionary<string, RpnParser.ParameterDelegate>();
					RpnParser.init(pPEABEJMCPI, gIOGAJGIGMO);
					RpnParser.Formula lANLKOHCGEJ = new RpnParser.Formula(stringBuilder.ToString());
					if (lANLKOHCGEJ.GetVariableCount() == 0)
					{
						pENDFCHBHIB.body = lANLKOHCGEJ.Calculate().ToString();
					}
					else
					{
						pENDFCHBHIB.body = stringBuilder.ToString();
					}
					pENDFCHBHIB.type = ObjectType.TYPE_VALUE;
					KJFKPMCPIBH.argumentValues.Add(pENDFCHBHIB);
					stringBuilder.Clear();
				}
				pLGKHFNOBCB = ObjectType.TYPE_SEPARATOR;
			}
			if (pLGKHFNOBCB == ObjectType.TYPE_SEPARATOR && !flag)
			{
				if (!stringBuilder.Equals(string.Empty))
				{
					stringBuilder.Clear();
				}
				pLGKHFNOBCB = ObjectType.TYPE_VALUE;
			}
			c = c2;
			stringBuilder.Append(c2);
		}
		string text = stringBuilder.ToString();
		if (!text.Equals(string.Empty))
		{
			FunctionObject pENDFCHBHIB2 = new FunctionObject();
			Dictionary<string, RpnParser.VariableDelegate> pPEABEJMCPI2 = new Dictionary<string, RpnParser.VariableDelegate>();
			Dictionary<string, RpnParser.ParameterDelegate> gIOGAJGIGMO2 = new Dictionary<string, RpnParser.ParameterDelegate>();
			RpnParser.init(pPEABEJMCPI2, gIOGAJGIGMO2);
			RpnParser.Formula lANLKOHCGEJ2 = new RpnParser.Formula(text);
			if (lANLKOHCGEJ2.GetVariableCount() == 0)
			{
				pENDFCHBHIB2.body = lANLKOHCGEJ2.Calculate().ToString();
			}
			else
			{
				pENDFCHBHIB2.body = stringBuilder.ToString();
			}
			pENDFCHBHIB2.type = ObjectType.TYPE_VALUE;
			KJFKPMCPIBH.argumentValues.Add(pENDFCHBHIB2);
		}
	}

	private void ResolveVariable(string body, FunctionResult DCJLKCFKCOM)
	{
		if (((body.Length <= 1) ? '_' : body[1]).Equals('$'))
		{
			if (GetVariableCallback() != null)
			{
				CallbackResult oMJHHJNIJOL = new CallbackResult();
				oMJHHJNIJOL.data = body;
				oMJHHJNIJOL.result = DCJLKCFKCOM;
				oMJHHJNIJOL.target = target;
				GetVariableCallback()(oMJHHJNIJOL);
			}
		}
		else
		{
			string text = null;
			text = ((body.Length <= 0 || !body[0].Equals('_')) ? body : body.Substring(1));
			VariableObject cCDGFNHLMCG = GetVariable(text);
			if (cCDGFNHLMCG != null)
			{
				DCJLKCFKCOM.Value = cCDGFNHLMCG.value;
			}
		}
	}

	private void SubstituteArguments(FunctionCall KJFKPMCPIBH, ref FunctionResult DCJLKCFKCOM)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(KJFKPMCPIBH.arguments);
		foreach (FunctionObject item in KJFKPMCPIBH.children)
		{
			stringBuilder.Replace(item.body, item.result.Value);
		}
		DCJLKCFKCOM.Value = stringBuilder.ToString();
	}

	private string ClearGaps(string target)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in target)
		{
			if (!char.IsWhiteSpace(c))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	private FunctionCall ParseFunction(string target)
	{
		int num = target.IndexOf('[');
		int num2 = target.LastIndexOf(']');
		int num3 = target.LastIndexOf('.');
		FunctionCall gLBAFLLMOOH = new FunctionCall();
		gLBAFLLMOOH.functionName = target.Substring(1, num - 1);
		gLBAFLLMOOH.arguments = target.Substring(num + 1, num2 - num - 1);
		gLBAFLLMOOH.propertyName = ((num3 <= num2) ? string.Empty : target.Substring(num3 + 1, target.Length - num3 - 1));
		gLBAFLLMOOH.body = target;
		gLBAFLLMOOH.name = "Function";
		gLBAFLLMOOH.name += countFuncName;
		countFuncName++;
		ParseObjects(gLBAFLLMOOH);
		return gLBAFLLMOOH;
	}

	private void ParseObjects(FunctionCall KJFKPMCPIBH)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ObjectType pLGKHFNOBCB = ObjectType.TYPE_SEPARATOR;
		int num = 0;
		bool flag = false;
		char c = ',';
		string mAABDFKMACJ = KJFKPMCPIBH.arguments;
		int i = 0;
		for (int length = mAABDFKMACJ.Length; i < length; i++)
		{
			char c2 = mAABDFKMACJ[i];
			bool flag2 = (RpnParser.IsOperatorSymbol(c2) || c2 == ',') && c2 != '?' && c2 != '_';
			bool flag3 = RpnParser.IsOperatorSymbol(c) || c == ',';
			if (pLGKHFNOBCB == ObjectType.TYPE_FUNCTION)
			{
				switch (c2)
				{
				case '[':
					num++;
					flag = true;
					break;
				case ']':
					num--;
					if (num < 0)
					{
						GameLog.Error("FunctionExtension::parseObjects error! Brackets not valid. {0}", mAABDFKMACJ);
					}
					break;
				default:
					if (flag2 && flag && num == 0)
					{
						if (!stringBuilder.Equals(string.Empty))
						{
							AddObject(KJFKPMCPIBH, stringBuilder.ToString(), pLGKHFNOBCB);
							stringBuilder.Clear();
						}
						pLGKHFNOBCB = ObjectType.TYPE_SEPARATOR;
					}
					break;
				}
			}
			if (pLGKHFNOBCB == ObjectType.TYPE_VARIABLE && flag2)
			{
				if (!stringBuilder.Equals(string.Empty))
				{
					AddObject(KJFKPMCPIBH, stringBuilder.ToString(), pLGKHFNOBCB);
					stringBuilder.Clear();
				}
				pLGKHFNOBCB = ObjectType.TYPE_SEPARATOR;
			}
			if (pLGKHFNOBCB == ObjectType.TYPE_VALUE && flag2)
			{
				if (!stringBuilder.Equals(string.Empty))
				{
					AddObject(KJFKPMCPIBH, stringBuilder.ToString(), pLGKHFNOBCB);
					stringBuilder.Clear();
				}
				pLGKHFNOBCB = ObjectType.TYPE_SEPARATOR;
			}
			if (pLGKHFNOBCB == ObjectType.TYPE_SEPARATOR)
			{
				switch (c2)
				{
				case '?':
					if (!stringBuilder.Equals(string.Empty))
					{
						AddObject(KJFKPMCPIBH, stringBuilder.ToString(), pLGKHFNOBCB);
						stringBuilder.Clear();
					}
					pLGKHFNOBCB = ObjectType.TYPE_FUNCTION;
					num = 0;
					flag = false;
					break;
				case '_':
					if (!stringBuilder.Equals(string.Empty))
					{
						AddObject(KJFKPMCPIBH, stringBuilder.ToString(), pLGKHFNOBCB);
						stringBuilder.Clear();
					}
					pLGKHFNOBCB = ObjectType.TYPE_VARIABLE;
					break;
				default:
					if (!flag2)
					{
						if (!stringBuilder.Equals(string.Empty))
						{
							AddObject(KJFKPMCPIBH, stringBuilder.ToString(), pLGKHFNOBCB);
							stringBuilder.Clear();
						}
						pLGKHFNOBCB = ObjectType.TYPE_VALUE;
					}
					break;
				}
			}
			c = c2;
			stringBuilder.Append(c2);
		}
		if (!stringBuilder.Equals(string.Empty))
		{
			AddObject(KJFKPMCPIBH, stringBuilder.ToString(), pLGKHFNOBCB);
		}
	}

	private void AddObject(FunctionCall KJFKPMCPIBH, string HGFADEKMPAK, ObjectType LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case ObjectType.TYPE_FUNCTION:
			AddFunction(KJFKPMCPIBH, HGFADEKMPAK);
			break;
		case ObjectType.TYPE_VARIABLE:
			AddValue(KJFKPMCPIBH, HGFADEKMPAK, true);
			break;
		case ObjectType.TYPE_VALUE:
			AddValue(KJFKPMCPIBH, HGFADEKMPAK, false);
			break;
		case ObjectType.TYPE_SEPARATOR:
			AddSeparator(KJFKPMCPIBH, HGFADEKMPAK);
			break;
		}
	}

	private void AddFunction(FunctionCall KJFKPMCPIBH, string target)
	{
		if (!target.Equals(string.Empty))
		{
			FunctionCall gLBAFLLMOOH = ParseFunction(target);
			gLBAFLLMOOH.type = ObjectType.TYPE_FUNCTION;
			KJFKPMCPIBH.children.Add(gLBAFLLMOOH);
			target = string.Empty;
			functionObjects.Add(gLBAFLLMOOH);
		}
	}

	private void AddValue(FunctionCall KJFKPMCPIBH, string target, bool HHEKDBGADGC)
	{
		if (!target.Equals(string.Empty))
		{
			FunctionObject pENDFCHBHIB = new FunctionObject();
			pENDFCHBHIB.body = target;
			pENDFCHBHIB.type = (HHEKDBGADGC ? ObjectType.TYPE_VARIABLE : ObjectType.TYPE_VALUE);
			KJFKPMCPIBH.children.Add(pENDFCHBHIB);
			target = string.Empty;
			valueObjects.Add(pENDFCHBHIB);
		}
	}

	private void AddSeparator(FunctionCall KJFKPMCPIBH, string target)
	{
		if (!target.Equals(string.Empty))
		{
			FunctionObject pENDFCHBHIB = new FunctionObject();
			pENDFCHBHIB.body = target;
			pENDFCHBHIB.type = ObjectType.TYPE_SEPARATOR;
			KJFKPMCPIBH.children.Add(pENDFCHBHIB);
			target = string.Empty;
			valueObjects.Add(pENDFCHBHIB);
		}
	}

	public static bool NumberCompare(float KONPFNHLPJG, float BABJGGEOCBG, CompareType LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case CompareType.COMPARE_EQUAL:
			return KONPFNHLPJG == BABJGGEOCBG;
		case CompareType.COMPARE_GREATER:
			return KONPFNHLPJG > BABJGGEOCBG;
		case CompareType.COMPARE_GREATER_EQUAL:
			return KONPFNHLPJG >= BABJGGEOCBG;
		case CompareType.COMPARE_LESS:
			return KONPFNHLPJG < BABJGGEOCBG;
		case CompareType.COMPARE_LESS_EQUAL:
			return KONPFNHLPJG <= BABJGGEOCBG;
		default:
			return false;
		}
	}

	public static bool IsCompare(string value)
	{
		string[] collection = value.Split(',');
		List<string> list = new List<string>(collection);
		if (list.Count > 2)
		{
			VariableType aFILEBFICDF = GetVariableType(list[0]);
			VariableType aFILEBFICDF2 = GetVariableType(list[1]);
			CompareType lFLGCDNKNJI = ParseCompareType(list[2]);
			if (aFILEBFICDF == aFILEBFICDF2)
			{
				switch (aFILEBFICDF)
				{
				case VariableType.VARIABLE_NUMBER:
				{
					Dictionary<string, RpnParser.VariableDelegate> pPEABEJMCPI = new Dictionary<string, RpnParser.VariableDelegate>();
					Dictionary<string, RpnParser.ParameterDelegate> gIOGAJGIGMO = new Dictionary<string, RpnParser.ParameterDelegate>();
					RpnParser.init(pPEABEJMCPI, gIOGAJGIGMO);
					RpnParser.Formula lANLKOHCGEJ = new RpnParser.Formula(list[0]);
					RpnParser.Formula lANLKOHCGEJ2 = new RpnParser.Formula(list[1]);
					float result;
					if (!float.TryParse(lANLKOHCGEJ.Calculate().ToString(), out result))
					{
						result = 0f;
					}
					float result2;
					if (!float.TryParse(lANLKOHCGEJ2.Calculate().ToString(), out result2))
					{
						result2 = 0f;
					}
					return NumberCompare(result, result2, lFLGCDNKNJI);
				}
				case VariableType.VARIABLE_STRING:
					return list[0].Equals(list[1]);
				}
			}
		}
		return false;
	}
}
