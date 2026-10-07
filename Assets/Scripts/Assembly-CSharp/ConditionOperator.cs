using System.Collections.Generic;
using System.Xml;

public class ConditionOperator : ConditionCounter
{
	public enum OperatorKind
	{
		TYPE_NONE = 0,
		TYPE_OR = 1,
		TYPE_AND = 2
	}

	public OperatorKind Type;

	private List<ConditionCounter> _conditions = new List<ConditionCounter>();

	public ConditionOperator()
		: base(CounterConditionType.OPERATOR)
	{
		Type = OperatorKind.TYPE_AND;
	}

	public ConditionOperator(XmlNode node)
		: base(CounterConditionType.OPERATOR)
	{
		Parse(node);
	}

	public override bool IsEqual(CounterConditions conditions)
	{
		foreach (ConditionCounter item in _conditions)
		{
			bool flag = item.IsEqual(conditions);
			if (Type == OperatorKind.TYPE_AND && !flag)
			{
				return IsNotCompare(false);
			}
			if (Type == OperatorKind.TYPE_OR && flag)
			{
				return IsNotCompare(true);
			}
		}
		if (Type == OperatorKind.TYPE_AND)
		{
			return IsNotCompare(true);
		}
		if (Type == OperatorKind.TYPE_OR)
		{
			return IsNotCompare(false);
		}
		GameLog.Error(string.Format("ConditionOperator::isEqual - wrong type: %i", Type));
		return false;
	}

	public void ParseConditions(XmlNode EBLIGDMALEA)
	{
		_conditions.Clear();
		foreach (XmlNode childNode in EBLIGDMALEA.ChildNodes)
		{
			ConditionCounter kAJIECHJBNL = CounterConditionsParser.ParseCondition(childNode);
			if (kAJIECHJBNL != null)
			{
				_conditions.Add(kAJIECHJBNL);
			}
		}
	}

	public override void Initialize()
	{
		foreach (ConditionCounter item in _conditions)
		{
			item.Initialize();
		}
	}

	public void AddCondition(ConditionCounter EPJGLECOIBG)
	{
		_conditions.Add(EPJGLECOIBG);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		string text = node.Attributes["Type"].GetStringOrDefault();
		if (text == "OR")
		{
			Type = OperatorKind.TYPE_OR;
		}
		else if (text == "AND")
		{
			Type = OperatorKind.TYPE_AND;
		}
		else
		{
			GameLog.Error("ConditionOperator::ConditionOperator - wrong type: %s", text);
			Type = OperatorKind.TYPE_NONE;
		}
		ParseConditions(node);
	}
}
