using System.Xml;

public class NoHealthBarRule : InFightRule
{
	public NoHealthBarRule(XmlNode node, RuleAppliance appliance)
		: base(RuleType.RuleNoHealthBar, appliance, node)
	{
		Parse(node);
	}

	protected override bool CompareSingle(object data)
	{
		return false;
	}

	public override InFightRule Copy()
	{
		InFightRule copy = null;
		RuleAppliance appliance = GetAppliance();
		XmlNode ruleNode = GetXmlSource().GetNode();
		copy = new NoHealthBarRule(ruleNode, appliance);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
