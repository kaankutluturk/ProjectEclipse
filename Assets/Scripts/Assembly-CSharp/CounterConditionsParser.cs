using System.Collections.Generic;
using System.Xml;

public static class CounterConditionsParser
{
	public static List<ConditionCounter> ParseConditions(XmlNode node)
	{
		List<ConditionCounter> list = new List<ConditionCounter>();
		ParseConditions(node, list);
		return list;
	}

	public static void ParseConditions(XmlNode EBLIGDMALEA, List<ConditionCounter> DCJLKCFKCOM)
	{
		foreach (XmlNode childNode in EBLIGDMALEA.ChildNodes)
		{
			DCJLKCFKCOM.Add(ParseCondition(childNode));
		}
	}

	public static ConditionCounter ParseCondition(XmlNode node)
	{
		string name = node.Name;
		if (name == "Battle")
		{
			return new ConditionBattle(node);
		}
		if (name == "Operator")
		{
			return new ConditionOperator(node);
		}
		GameLog.Error(string.Format("CounterConditionsParser::parseCondition - %s", name));
		return null;
	}

	public static void ParseCompletionConditions(XmlNode EBLIGDMALEA, ConditionOperator FMFMOPOJBOH, ConditionOfCompletionInspector GLKOKIOFOMD)
	{
		foreach (XmlNode childNode in EBLIGDMALEA.ChildNodes)
		{
			string name = childNode.Name;
			switch (name)
			{
			case "Battle":
			{
				ConditionBattle ePJGLECOIBG2 = new ConditionBattle(childNode);
				FMFMOPOJBOH.AddCondition(ePJGLECOIBG2);
				break;
			}
			case "Operator":
			{
				ConditionOperator ePJGLECOIBG = new ConditionOperator(childNode);
				FMFMOPOJBOH.AddCondition(ePJGLECOIBG);
				break;
			}
			case "WinBattle":
			{
				ConditionOfCompletionBattle iOFGGOCEIAM = new ConditionOfCompletionBattle(childNode);
				GLKOKIOFOMD.AddCondition(iOFGGOCEIAM);
				break;
			}
			default:
				GameLog.Error(string.Format("CounterConditionsParser::parseCondition - %s", name));
				break;
			}
		}
	}
}
