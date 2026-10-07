public class EventHit : EventAnimation
{
	public EventHit()
		: base(EventAnimationType.EVENT_HIT)
	{
	}

	protected override bool Compare(EventAnimation other)
	{
		bool flag = false;
		EventHit otherHit = other as EventHit;
		if (string.IsNullOrEmpty(AnimationName))
		{
			flag = true;
		}
		else
		{
			Model.StrikeResult strikeResult = (Model.StrikeResult)other.Conditions.StrikeResult;
			Model attacker = strikeResult.AttackerModel;
			IntervalAttack attackInterval = (IntervalAttack)attacker.GetAnimationModule().FindInterval(IntervalAnimation.IntervalType.INTERVAL_ATTACK);
			if (attackInterval != null)
			{
				string text = attackInterval.GetReactionName(attacker.GetReactionFrame());
				flag = text == AnimationName;
			}
		}
		if (flag && !HitType.IsNullOrEmpty())
		{
			bool flag2 = HitType == otherHit.HitType;
			flag = flag && flag2;
		}
		if (flag && !StageName.IsNullOrEmpty())
		{
			bool flag3 = StageName == otherHit.StageName;
			flag = flag && flag3;
		}
		return flag;
	}
}
