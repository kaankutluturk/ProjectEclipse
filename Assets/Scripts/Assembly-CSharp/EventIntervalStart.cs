using System.Collections.Generic;
using System.Xml;

public class EventIntervalStart : EventAnimation
{
	private IntervalAnimation.IntervalType intervalType;

	public EventIntervalStart()
		: base(EventAnimationType.EVENT_INTERVAL_START)
	{
		intervalType = IntervalAnimation.IntervalType.INTERVAL_NONE;
	}

	protected override bool Compare(EventAnimation other)
	{
		EventIntervalStart otherEvent = other as EventIntervalStart;
		List<IntervalAnimation> intervals = otherEvent.Conditions.Intervals;
		bool flag = HasMatchingInterval(intervals);
		return (!IsNot) ? flag : (!flag);
	}

	protected override void Parse(XmlNode node)
	{
		switch (node.Attributes["Type"].GetStringOrDefault(string.Empty))
		{
		case "Attack":
			intervalType = IntervalAnimation.IntervalType.INTERVAL_ATTACK;
			break;
		case "Block":
			intervalType = IntervalAnimation.IntervalType.INTERVAL_BLOCK;
			break;
		case "Invulnerable":
			intervalType = IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE;
			break;
		default:
			intervalType = IntervalAnimation.IntervalType.INTERVAL_NONE;
			break;
		}
	}

	private bool HasMatchingInterval(List<IntervalAnimation> intervals)
	{
		int i = 0;
		for (int count = intervals.Count; i < count; i++)
		{
			IntervalAnimation interval = intervals[i];
			if ((intervalType == IntervalAnimation.IntervalType.INTERVAL_NONE || intervalType == interval.Type) && (AnimationName == string.Empty || AnimationName == interval.Name))
			{
				return true;
			}
		}
		return false;
	}
}
