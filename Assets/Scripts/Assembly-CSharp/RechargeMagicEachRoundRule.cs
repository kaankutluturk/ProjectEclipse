using System.Xml;

public class RechargeMagicEachRoundRule : InFightRule
{
	public RechargeMagicEachRoundRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleRechargeMagicEachRound, EJPOJJKKICO, node)
	{
	}

	public override void InitRule(object data)
	{
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new RechargeMagicEachRoundRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
