using System.Collections.Generic;

public class EventAnimationStart : EventAnimation
{
	public EventAnimationStart()
		: base(EventAnimationType.EVENT_ANIMATION_START)
	{
	}

	protected override bool Compare(EventAnimation FOPOKALJIIJ)
	{
		EventAnimationStart hPCJMBAJKLJ = FOPOKALJIIJ as EventAnimationStart;
		bool flag = IsCompareNames(hPCJMBAJKLJ.Conditions.SelfAnimationNames);
		return (!IsNot) ? flag : (!flag);
	}

	private bool IsCompareNames(List<string> NIKHAICFGNM)
	{
		int i = 0;
		for (int count = NIKHAICFGNM.Count; i < count; i++)
		{
			if (NIKHAICFGNM[i] == AnimationName)
			{
				return true;
			}
		}
		return false;
	}
}
