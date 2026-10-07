using System.Collections.Generic;
using System.Xml;

public class EventIntervalEnd : EventAnimation
{
	private IntervalAnimation.IntervalType intervalType;

	public EventIntervalEnd()
		: base(EventAnimationType.EVENT_INTERVAL_END)
	{
		intervalType = IntervalAnimation.IntervalType.INTERVAL_NONE;
	}

	protected override bool Compare(EventAnimation FOPOKALJIIJ)
	{
		EventIntervalEnd cNHAJBPAJAF = FOPOKALJIIJ as EventIntervalEnd;
		List<IntervalAnimation> cAANBJEPGAA = cNHAJBPAJAF.Conditions.Intervals;
		bool flag = HasMatchingInterval(cAANBJEPGAA);
		return (!IsNot) ? flag : (!flag);
	}

	protected override void Parse(XmlNode MEEAKLDGLDF)
	{
		switch (MEEAKLDGLDF.Attributes["Type"].GetStringOrDefault(string.Empty))
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

	private bool HasMatchingInterval(List<IntervalAnimation> NFLDEGMEJAK)
	{
		int i = 0;
		for (int count = NFLDEGMEJAK.Count; i < count; i++)
		{
			IntervalAnimation mNOIEOBBCMI = NFLDEGMEJAK[i];
			if ((intervalType == IntervalAnimation.IntervalType.INTERVAL_NONE || intervalType == mNOIEOBBCMI.Type) && (AnimationName == string.Empty || AnimationName == mNOIEOBBCMI.Name))
			{
				return true;
			}
		}
		return false;
	}
}
