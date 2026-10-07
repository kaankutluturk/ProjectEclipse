using System.Xml;

public class TimeoutWinRule : InFightRule
{
	public TimeoutWinRule(XmlNode node, RuleAppliance ruleAppliance)
		: base(RuleType.RuleTimeoutWin, ruleAppliance, node)
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
		InFightRule copiedRule = null;
		RuleAppliance ruleAppliance = GetAppliance();
		XmlNode ruleNode = GetXmlSource().GetNode();
		copiedRule = new TimeoutWinRule(ruleNode, ruleAppliance);
		copiedRule.IsRandom = IsRandom;
		return copiedRule;
	}
}
