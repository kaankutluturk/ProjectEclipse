using System.Collections.Generic;
using System.Xml;

public class ConditionInterval : ConditionAnimation
{
	private IntervalAnimation.IntervalType _intervalType;

	private string _Name;

	public ConditionInterval(XmlNode node)
		: base(ConditionType.CURRENT_INTERVAL)
	{
		_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		if (node.Attributes["Type"] != null)
		{
			switch (node.Attributes["Type"].GetStringOrDefault(string.Empty))
			{
			case "Attack":
				_intervalType = IntervalAnimation.IntervalType.INTERVAL_ATTACK;
				break;
			case "Block":
				_intervalType = IntervalAnimation.IntervalType.INTERVAL_BLOCK;
				break;
			case "Invulnerable":
				_intervalType = IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE;
				break;
			}
		}
		else
		{
			_intervalType = IntervalAnimation.IntervalType.INTERVAL_NONE;
		}
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		bool flag = false;
		List<IntervalAnimation> cAANBJEPGAA = conditions.Intervals;
		if (cAANBJEPGAA != null)
		{
			foreach (IntervalAnimation item in cAANBJEPGAA)
			{
				if ((_intervalType == IntervalAnimation.IntervalType.INTERVAL_NONE || _intervalType == item.Type) && (_Name == string.Empty || item.Name == _Name))
				{
					flag = true;
					break;
				}
			}
		}
		return (!IsNot) ? flag : (!flag);
	}

	private List<IntervalAnimation> GetIntervals(ModelConditions conditions)
	{
		switch (_targetModelType)
		{
		case ModelType.ModelTargetType.MODEL_THIS:
			return conditions.Intervals;
		case ModelType.ModelTargetType.MODEL_OTHER:
			return conditions.OtherIntervals;
		case ModelType.ModelTargetType.MODEL_PARENT:
			return conditions.ParentIntervals;
		default:
			GameLog.Error("ConditionCurrentAnimation: getAnimationNames - wrong type: {0}", _targetModelType.ToString());
			return conditions.Intervals;
		}
	}
}
