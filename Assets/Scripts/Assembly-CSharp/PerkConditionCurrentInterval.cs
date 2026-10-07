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

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		Model fGCODGKLHED = ResolveTargetModel(ACENLMONNPA);
		if (ACENLMONNPA == null)
		{
			return false;
		}
		List<IntervalAnimation> list = fGCODGKLHED.GetIntervals();
		IntervalAnimation.IntervalType nGAJJDIEDGF = IntervalAnimation.ParseIntervalType(_intervalType);
		bool flag = _intervalName.Equals(string.Empty);
		bool flag2 = nGAJJDIEDGF == IntervalAnimation.IntervalType.INTERVAL_NONE;
		foreach (IntervalAnimation item in list)
		{
			if (!flag && _intervalName.Equals(item.Name))
			{
				flag = true;
			}
			if (!flag2 && nGAJJDIEDGF == item.Type)
			{
				flag2 = true;
			}
		}
		return flag && flag2;
	}
}
