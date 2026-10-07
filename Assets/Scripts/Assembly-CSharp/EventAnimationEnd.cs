using System.Collections.Generic;

public class EventAnimationEnd : EventAnimation
{
	public string Name;

	public EventAnimationEnd()
		: base(EventAnimationType.EVENT_ANIMATION_END)
	{
	}

	protected override bool Compare(EventAnimation other)
	{
		EventAnimationEnd otherEvent = other as EventAnimationEnd;
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
