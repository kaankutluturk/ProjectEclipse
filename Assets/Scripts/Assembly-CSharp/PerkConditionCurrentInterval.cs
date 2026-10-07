using System.Collections.Generic;
using System.Xml;

public class PerkConditionCurrentInterval : PerkCondition
{
	private string _intervalName;

	private string _intervalType;

	public PerkConditionCurrentInterval()
	{
		set_Type(PerkConditionType.CONDITION_CURRENT_INTERVAL);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_intervalName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_intervalType = node.Attributes["Type"].GetStringOrDefault(string.Empty);
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		Model targetModel = ResolveTargetModel(model);
		if (model == null)
		{
			return false;
		}
		List<IntervalAnimation> list = targetModel.GetIntervals();
		IntervalAnimation.IntervalType intervalType = IntervalAnimation.ParseIntervalType(_intervalType);
		bool flag = _intervalName.Equals(string.Empty);
		bool flag2 = intervalType == IntervalAnimation.IntervalType.INTERVAL_NONE;
		foreach (IntervalAnimation item in list)
		{
			if (!flag && _intervalName.Equals(item.Name))
			{
				flag = true;
			}
			if (!flag2 && intervalType == item.Type)
			{
				flag2 = true;
			}
		}
		return flag && flag2;
	}
}
