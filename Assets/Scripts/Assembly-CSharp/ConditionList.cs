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

	public bool EvaluateWithModel(ModelConditions conditions, Model ACENLMONNPA = null, EventAnimation DOANBADPBGH = null)
	{
		bool flag = EvaluateConditions(conditions, ACENLMONNPA, DOANBADPBGH);
		return (!IsNot) ? flag : (!flag);
	}

	private bool EvaluateConditions(ModelConditions conditions, Model ACENLMONNPA = null, EventAnimation DOANBADPBGH = null)
	{
		foreach (ConditionAnimation item in _conditions)
		{
			bool flag = false;
			if (item.Type == ConditionType.EVENT && ACENLMONNPA != null)
			{
				ModelType.ModelTargetType lFLGCDNKNJI = item.GetTargetModelType();
				Model fGCODGKLHED = item.ResolveTargetModel(ACENLMONNPA, lFLGCDNKNJI);
				ModelConditions dGJJDPIAEAO = fGCODGKLHED.GetConditions();
				if (DOANBADPBGH != null)
				{
					dGJJDPIAEAO.CurrentEvent = DOANBADPBGH;
					DOANBADPBGH.Conditions = dGJJDPIAEAO;
				}
				flag = item.IsEqual(dGJJDPIAEAO);
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
