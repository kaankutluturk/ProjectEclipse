using System.Collections.Generic;

public class EventAnimationStart : EventAnimation
{
	public EventAnimationStart()
		: base(EventAnimationType.EVENT_ANIMATION_START)
	{
	}

	protected override bool Compare(EventAnimation other)
	{
		EventAnimationStart otherEvent = other as EventAnimationStart;
		bool flag = IsCompareNames(otherEvent.Conditions.SelfAnimationNames);
		return (!IsNot) ? flag : (!flag);
	}

	private bool IsCompareNames(List<string> names)
	{
		int i = 0;
		for (int count = names.Count; i < count; i++)
		{
			if (names[i] == AnimationName)
			{
				return true;
			}
		}
		return false;
	}
}
