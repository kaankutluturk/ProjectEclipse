using System.Xml;

public class NoBulletsReplenishmentRule : InFightRule
{
	public NoBulletsReplenishmentRule(XmlNode node, RuleAppliance appliance)
		: base(RuleType.RuleNoBulletsReplenishment, appliance, node)
	{
	}

	public override InFightRule Copy()
	{
		InFightRule copy = null;
		RuleAppliance appliance = GetAppliance();
		XmlNode ruleNode = GetXmlSource().GetNode();
		copy = new NoBulletsReplenishmentRule(ruleNode, appliance);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
