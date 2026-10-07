public class EventHit : EventAnimation
{
	public EventHit()
		: base(EventAnimationType.EVENT_HIT)
	{
	}

	protected override bool Compare(EventAnimation FOPOKALJIIJ)
	{
		bool flag = false;
		EventHit eLGNDOJMOBH = FOPOKALJIIJ as EventHit;
		if (string.IsNullOrEmpty(AnimationName))
		{
			flag = true;
		}
		else
		{
			Model.StrikeResult jEGHAGLEJCB = (Model.StrikeResult)FOPOKALJIIJ.Conditions.StrikeResult;
			Model gAIBPAGPEGK = jEGHAGLEJCB.AttackerModel;
			IntervalAttack hFIIPNLCIEE = (IntervalAttack)gAIBPAGPEGK.GetAnimationModule().FindInterval(IntervalAnimation.IntervalType.INTERVAL_ATTACK);
			if (hFIIPNLCIEE != null)
			{
				string text = hFIIPNLCIEE.GetReactionName(gAIBPAGPEGK.GetReactionFrame());
				flag = text == AnimationName;
			}
		}
		if (flag && !HitType.IsNullOrEmpty())
		{
			bool flag2 = HitType == eLGNDOJMOBH.HitType;
			flag = flag && flag2;
		}
		if (flag && !StageName.IsNullOrEmpty())
		{
			bool flag3 = StageName == eLGNDOJMOBH.StageName;
			flag = flag && flag3;
		}
		return flag;
	}
}
