using System.Xml;

public class WinStyleRule : InFightRule
{
	private FightStatistics.FightStyle requiredStyle;

	public WinStyleRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleWinStyle, EJPOJJKKICO, node)
	{
		Parse(node);
		applianceLosesOnTrigger = false;
		SubscribeEvent(FightEvent.CrazyEvent);
	}

	protected override bool CompareSingle(object data)
	{
		FightData hCPJJKMNMCE = (FightData)data;
		return hCPJJKMNMCE.Style >= requiredStyle;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		requiredStyle = RuleParser.ParseStyleType(node);
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new WinStyleRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
