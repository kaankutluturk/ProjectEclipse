using System.Collections.Generic;
using System.Xml;

public class ConditionList : ConditionAnimation
{
	// best guess for name
	public enum OperatorType
	{
		AND = 0,
		OR = 1
	}

	private OperatorType _operator;

	private List<ConditionAnimation> _conditions = new List<ConditionAnimation>();

	public List<ConditionAnimation> Conditions
	{
		get
		{
			return GetConditions();
		}
	}

	public ConditionList(XmlNode node, List<ConditionAnimation> conditions)
		: base(ConditionType.LIST)
	{
		string text = XmlUtils.ParseString(node.Attributes["Type"]);
		_operator = ((text == "Or") ? OperatorType.OR : OperatorType.AND);
		_conditions = conditions;
	}

	public OperatorType get_Type()
	{
		return _operator;
	}

	// best guess for name
	public List<ConditionAnimation> GetConditions()
	{
		return _conditions;
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		bool flag = EvaluateConditions(conditions);
		return (!IsNot) ? flag : (!flag);
	}

	public bool EvaluateWithModel(ModelConditions conditions, Model model = null, EventAnimation eventAnimation = null)
	{
		bool flag = EvaluateConditions(conditions, model, eventAnimation);
		return (!IsNot) ? flag : (!flag);
	}

	private bool EvaluateConditions(ModelConditions conditions, Model model = null, EventAnimation eventAnimation = null)
	{
		foreach (ConditionAnimation item in _conditions)
		{
			bool flag = false;
			if (item.Type == ConditionType.EVENT && model != null)
			{
				ModelType.ModelTargetType targetType = item.GetTargetModelType();
				Model targetModel = item.ResolveTargetModel(model, targetType);
				ModelConditions targetConditions = targetModel.GetConditions();
				if (eventAnimation != null)
				{
					targetConditions.CurrentEvent = eventAnimation;
					eventAnimation.Conditions = targetConditions;
				}
				flag = item.IsEqual(targetConditions);
			}
			else
			{
				flag = item.IsEqual(conditions);
			}
			if (flag && _operator == OperatorType.OR)
			{
				return true;
			}
			if (!flag && _operator == OperatorType.AND)
			{
				return false;
			}
		}
		return OperatorType.AND == _operator;
	}
}
