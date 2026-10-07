using System.Xml;

public class NoBulletsReplenishmentRule : InFightRule
{
	public NoBulletsReplenishmentRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleNoBulletsReplenishment, EJPOJJKKICO, node)
	{
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new NoBulletsReplenishmentRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
