using System.Xml;

public class TimeoutWinRule : InFightRule
{
	public TimeoutWinRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleTimeoutWin, EJPOJJKKICO, node)
	{
		applianceLosesOnTrigger = false;
		SubscribeEvent(FightEvent.TimeoutEvent);
		Reset();
		appliance = RuleAppliance.AppliancePlayer;
	}

	protected override bool CompareSingle(object data)
	{
		return true;
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new TimeoutWinRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
