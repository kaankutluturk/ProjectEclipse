using System.Collections.Generic;

public class EventAnimationEnd : EventAnimation
{
	public string Name;

	public EventAnimationEnd()
		: base(EventAnimationType.EVENT_ANIMATION_END)
	{
	}

	protected override bool Compare(EventAnimation FOPOKALJIIJ)
	{
		EventAnimationEnd aFNEGONBIKF = FOPOKALJIIJ as EventAnimationEnd;
		bool flag = IsCompareNames(aFNEGONBIKF.Conditions.SelfAnimationNames);
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
