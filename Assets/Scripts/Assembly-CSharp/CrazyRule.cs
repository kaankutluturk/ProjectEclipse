using System.Xml;

public class CrazyRule : DamageRule
{
	private FightStatistics.FightStyle styleThreshold;

	public CrazyRule(XmlNode node, RuleAppliance ruleAppliance)
		: base(node, ruleAppliance, RuleType.RuleCrazy)
	{
		styleThreshold = FightStatistics.FightStyle.STYLE_TURTLE;
		applianceLosesOnTrigger = false;
		Parse(node);
		SubscribeEvent(FightEvent.CrazyEvent);
	}

	protected override bool CompareSingle(object data)
	{
		FightData fightData = (FightData)data;
		if (fightData.FightEventType == FightEvent.DamageCheckEvent)
		{
			return false;
		}
		return CheckIsNoDamageChange(fightData.Style < styleThreshold);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		styleThreshold = RuleParser.ParseStyleType(node);
	}
}
