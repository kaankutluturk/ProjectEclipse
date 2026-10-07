using System.Xml;

public class CrazyRule : DamageRule
{
	private FightStatistics.FightStyle styleThreshold;

	public CrazyRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(node, EJPOJJKKICO, RuleType.RuleCrazy)
	{
		styleThreshold = FightStatistics.FightStyle.STYLE_TURTLE;
		applianceLosesOnTrigger = false;
		Parse(node);
		SubscribeEvent(FightEvent.CrazyEvent);
	}

	protected override bool CompareSingle(object data)
	{
		FightData hCPJJKMNMCE = (FightData)data;
		if (hCPJJKMNMCE.FightEventType == FightEvent.DamageCheckEvent)
		{
			return false;
		}
		return CheckIsNoDamageChange(hCPJJKMNMCE.Style < styleThreshold);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		styleThreshold = RuleParser.ParseStyleType(node);
	}
}
