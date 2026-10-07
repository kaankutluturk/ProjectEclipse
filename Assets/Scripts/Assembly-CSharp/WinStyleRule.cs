using System.Xml;

public class WinStyleRule : InFightRule
{
	private FightStatistics.FightStyle requiredStyle;

	public WinStyleRule(XmlNode node, RuleAppliance ruleAppliance)
		: base(RuleType.RuleWinStyle, ruleAppliance, node)
	{
		Parse(node);
		applianceLosesOnTrigger = false;
		SubscribeEvent(FightEvent.CrazyEvent);
	}

	protected override bool CompareSingle(object data)
	{
		FightData fightData = (FightData)data;
		return fightData.Style >= requiredStyle;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		requiredStyle = RuleParser.ParseStyleType(node);
	}

	public override InFightRule Copy()
	{
		InFightRule ruleCopy = null;
		RuleAppliance ruleAppliance = GetAppliance();
		XmlNode sourceNode = GetXmlSource().GetNode();
		ruleCopy = new WinStyleRule(sourceNode, ruleAppliance);
		ruleCopy.IsRandom = IsRandom;
		return ruleCopy;
	}
}
