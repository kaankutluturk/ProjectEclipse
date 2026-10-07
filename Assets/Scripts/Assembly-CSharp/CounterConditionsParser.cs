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

	public static void ParseConditions(XmlNode node, List<ConditionCounter> conditions)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			conditions.Add(ParseCondition(childNode));
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

	public static void ParseCompletionConditions(XmlNode node, ConditionOperator parentOperator, ConditionOfCompletionInspector inspector)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string name = childNode.Name;
			switch (name)
			{
			case "Battle":
			{
				ConditionBattle battleCondition = new ConditionBattle(childNode);
				parentOperator.AddCondition(battleCondition);
				break;
			}
			case "Operator":
			{
				ConditionOperator childOperator = new ConditionOperator(childNode);
				parentOperator.AddCondition(childOperator);
				break;
			}
			case "WinBattle":
			{
				ConditionOfCompletionBattle winCondition = new ConditionOfCompletionBattle(childNode);
				inspector.AddCondition(winCondition);
				break;
			}
			default:
				GameLog.Error(string.Format("CounterConditionsParser::parseCondition - %s", name));
				break;
			}
		}
	}
}
