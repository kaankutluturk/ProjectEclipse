using System.Collections.Generic;
using System.Xml;

public class PerkConditionOperator : PerkCondition
{
	private enum PerkConditionOperatorType
	{
		OPERATOR_NONE = 0,
		OPERATOR_OR = 1,
		OPERATOR_AND = 2
	}

	private PerkConditionOperatorType _operator;

	private List<PerkCondition> _conditions = new List<PerkCondition>();

	public PerkConditionOperator()
	{
		set_Type(PerkConditionType.CONDITION_OPERATOR);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_operator = PerkConditionOperatorType.OPERATOR_NONE;
		string text = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		if (text.Equals("Or"))
		{
			_operator = PerkConditionOperatorType.OPERATOR_OR;
		}
		else if (text.Equals("And"))
		{
			_operator = PerkConditionOperatorType.OPERATOR_AND;
		}
		_conditions = PerkCondition.Create(node, GetPerk());
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		Model targetModel = ResolveTargetModel(model);
		if (model == null)
		{
			return false;
		}
		foreach (PerkCondition item in _conditions)
		{
			bool flag = item.IsEqual(model, args);
			bool flag2 = ((!item.IsNot) ? flag : (!flag));
			if (_operator == PerkConditionOperatorType.OPERATOR_AND && !flag2)
			{
				return false;
			}
			if (_operator == PerkConditionOperatorType.OPERATOR_OR && flag2)
			{
				return true;
			}
		}
		return _operator != PerkConditionOperatorType.OPERATOR_OR;
	}
}
