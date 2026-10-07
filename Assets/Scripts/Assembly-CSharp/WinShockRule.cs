using System.Xml;

public class WinShockRule : InFightRule
{
	public WinShockRule(XmlNode node, RuleAppliance ruleAppliance)
		: base(RuleType.RuleWinShock, ruleAppliance, node)
	{
		applianceLosesOnTrigger = false;
		SubscribeEvent(FightEvent.StrikeEvent);
	}

	protected override bool CompareSingle(object data)
	{
		FightData fightData = (FightData)data;
		return fightData.IsOpponentShocked;
	}

	public override InFightRule Copy()
	{
		InFightRule ruleCopy = null;
		RuleAppliance ruleAppliance = GetAppliance();
		XmlNode sourceNode = GetXmlSource().GetNode();
		ruleCopy = new WinShockRule(sourceNode, ruleAppliance);
		ruleCopy.IsRandom = IsRandom;
		return ruleCopy;
	}
}
