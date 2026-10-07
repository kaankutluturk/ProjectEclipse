using System.Xml;

public class RechargeMagicEachRoundRule : InFightRule
{
	public RechargeMagicEachRoundRule(XmlNode node, RuleAppliance appliance)
		: base(RuleType.RuleRechargeMagicEachRound, appliance, node)
	{
	}

	public override void InitRule(object data)
	{
	}

	public override InFightRule Copy()
	{
		InFightRule copy = null;
		RuleAppliance appliance = GetAppliance();
		XmlNode node = GetXmlSource().GetNode();
		copy = new RechargeMagicEachRoundRule(node, appliance);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
